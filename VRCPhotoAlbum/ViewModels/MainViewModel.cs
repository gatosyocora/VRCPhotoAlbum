using Gatosyocora.VRCPhotoAlbum.Helpers;
using Gatosyocora.VRCPhotoAlbum.Models;
using Gatosyocora.VRCPhotoAlbum.Views;
using KoyashiroKohaku.VrcMetaToolSharp;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Gatosyocora.VRCPhotoAlbum.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        #region View
        private MainWindow _mainWindow;
        #endregion

        #region Model
        private SearchResult _searchResult;
        private Users _users;
        #endregion

        #region Photo
        private ReactiveCollection<Photo> _photoList { get; }
        public ReadOnlyReactiveCollection<Photo> ShowedPhotoList { get; }
        public ReactiveProperty<bool> HaveNoShowedPhoto { get; }
        #endregion

        #region Search
        public ReactiveProperty<string> SearchText { get; }
        public ReactiveProperty<DateTime> SearchDate { get; }

        public ReactiveCommand<string> SearchWithUserNameCommand { get; }
        public ReactiveCommand<string> SearchWithWorldNameCommand { get; }
        public ReactiveCommand<string> SearchWithDateCommand { get; }
        public ReactiveCommand<string> SearchWithDateTypeCommand { get; }

        public ReactiveCommand ClearSearchText { get; }
        #endregion

        #region UserList
        public ReadOnlyReactiveCollection<string> UserList { get; }

        public ReactiveProperty<UserSortType> CurrentUserSortType { get; }
        public ReactiveCommand SortUserWithAlphabetCommand { get; }
        public ReactiveCommand SortUserWithCountCommand { get; }
        #endregion

        #region Window
        public ReactiveCommand<Photo> OpenPhotoPreviewCommand { get; }
        public ReactiveCommand OpenSettingCommand { get; }
        #endregion

        public MainViewModel(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;

            if (Setting.Instance.Data is null)
            {
                WindowHelper.OpenSettingDialog(_mainWindow);
            }

            Cache.Instance.Create();

            _photoList = new ReactiveCollection<Photo>();
            _searchResult = new SearchResult(_photoList);
            _users = new Users(_photoList);

            SearchText = _searchResult.SearchText.ToReactivePropertyAsSynchronized(x => x.Value).AddTo(Disposable);

            ShowedPhotoList = _searchResult.ShowedPhotoList
                                .ObserveAddChangedItems()
                                .SelectMany(p => p)
                                .ToReadOnlyReactiveCollection(
                                    onReset: SearchText.Select(_ => Unit.Default))
                                .AddTo(Disposable);
            HaveNoShowedPhoto = ShowedPhotoList.ObserveAddChanged().Select(_ => !ShowedPhotoList.Any()).ToReactiveProperty().AddTo(Disposable);

            SearchDate = _searchResult.SearchedDate.ToReactivePropertyAsSynchronized(x => x.Value).AddTo(Disposable);
            SearchWithUserNameCommand = new ReactiveCommand<string>().AddTo(Disposable);
            SearchWithUserNameCommand.Subscribe(_searchResult.SearchWithUserName).AddTo(Disposable);
            SearchWithWorldNameCommand = new ReactiveCommand<string>().AddTo(Disposable);
            SearchWithWorldNameCommand.Subscribe(_searchResult.SearchWithWorldName).AddTo(Disposable);
            SearchWithDateCommand = new ReactiveCommand<string>().AddTo(Disposable);
            SearchWithDateCommand.Subscribe(_searchResult.SearchWithDateString).AddTo(Disposable);
            SearchWithDateTypeCommand = new ReactiveCommand<string>().AddTo(Disposable);
            SearchWithDateTypeCommand.Subscribe(type =>
            {
                var now = DateTime.Now;

                if (type == "today")
                {
                    SearchDate.Value = now;
                }
                else if (type == "week")
                {
                    _searchResult.SearchWithDatePeriodString(now.AddDays(-7).ToString("yyyy/MM/dd HH:mm:ss"), now.ToString("yyyy/MM/dd HH:mm:ss"));
                }
                else if (type == "month")
                {
                    _searchResult.SearchWithDatePeriodString(now.AddMonths(-1).ToString("yyyy/MM/dd HH:mm:ss"), now.ToString("yyyy/MM/dd HH:mm:ss"));
                }
            });
            ClearSearchText = new ReactiveCommand().AddTo(Disposable);
            ClearSearchText.Subscribe(() => SearchText.Value = string.Empty).AddTo(Disposable);

            CurrentUserSortType = new ReactiveProperty<UserSortType>().AddTo(Disposable);
            CurrentUserSortType.Subscribe(type =>
            {
                _users.SortCommand.Execute(Enum.GetName(typeof(UserSortType), CurrentUserSortType.Value));
            }).AddTo(Disposable);
            UserList = _users.SortedUserList
                            .ObserveAddChanged()
                            .Select(u => u.Name)
                            .ToReadOnlyReactiveCollection(
                                onReset: CurrentUserSortType.Select(_ => Unit.Default))
                            .AddTo(Disposable);

            SortUserWithAlphabetCommand = new ReactiveCommand().AddTo(Disposable);
            SortUserWithCountCommand = new ReactiveCommand().AddTo(Disposable);
            SortUserWithAlphabetCommand.Subscribe(() => CurrentUserSortType.Value = UserSortType.Alphabet).AddTo(Disposable);
            SortUserWithCountCommand.Subscribe(() => CurrentUserSortType.Value = UserSortType.Count).AddTo(Disposable);

            OpenPhotoPreviewCommand = new ReactiveCommand<Photo>().AddTo(Disposable);
            OpenPhotoPreviewCommand.Subscribe(photo => { if (!(photo is null)) WindowHelper.OpenPhotoPreviewWindow(photo, ShowedPhotoList.ToList(), _searchResult, _mainWindow); }).AddTo(Disposable);
            OpenSettingCommand = new ReactiveCommand().AddTo(Disposable);
            OpenSettingCommand.Subscribe(() => WindowHelper.OpenSettingDialog(_mainWindow)).AddTo(Disposable);

            _ = LoadResourcesAsync();
        }

        /// <summary>
        /// 非同期でデータを読み込む
        /// </summary>
        /// <returns></returns>
        public async Task LoadResourcesAsync()
        {
            try
            {
                _photoList.AddRangeOnScheduler(await LoadVRCPhotoListAsync(Setting.Instance.Data.FolderPath));
            }
            catch (Exception e)
            {
                Debug.Print($"{e.GetType().Name}: {e.Message}");
            }
        }

        /// <summary>
        /// 非同期で画像を読み込む
        /// </summary>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        private Task<Photo[]> LoadVRCPhotoListAsync(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                throw new ArgumentException($"{folderPath} is not exist.");
            }

            return Task.WhenAll(Directory.GetFiles(folderPath, "*.png", SearchOption.AllDirectories)
                        .Where(x => !x.StartsWith(Cache.Instance.CacheFolderPath))
                        .Select(async x =>
                        {
                            VrcMetaData meta;
                            try
                            {
                                meta = VrcMetaDataReader.Read(x);
                            }
                            catch (Exception)
                            {
                                meta = null;
                            }

                            BitmapImage image;
                            try
                            {
                                image = await ImageHelper.GetThumbnailImageAsync(x, Cache.Instance.CacheFolderPath);
                            }
                            catch (Exception)
                            {
                                image = new BitmapImage(new Uri(@"pack://application:,,,/Resources/noloading.png"));
                            }

                            return new Photo
                            {
                                FilePath = x,
                                ThumbnailImage = image,
                                MetaData = meta
                            };
                        })
                        .ToList());
        }

        public void UpdatePhotoList()
        {
            //SearchPhoto(_photoList, SearchText.Value);
        }
    }
}
