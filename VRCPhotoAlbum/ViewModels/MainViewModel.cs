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
using System.Text;
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

        public ReactiveCommand RebootCommand { get; }

        public MainViewModel(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;

            Setting.Instance.Create();

            Cache.Instance.Create();

            _photoList = new ReactiveCollection<Photo>();
            _searchResult = new SearchResult(_photoList);
            _users = new Users(_photoList);

            SearchText = _searchResult.OnChangedSearchKeyword
                            .Select(_ => ToSearchText(_searchResult))
                            .ToReactiveProperty().AddTo(Disposable);
            SearchText.Subscribe(t =>
            {
                _searchResult.SearchText.Value = t;
                Debug.Print("viewModel:SearchText");
            }).AddTo(Disposable);

            ShowedPhotoList = _searchResult.ShowedPhotoList
                                .ObserveAddChanged()
                                .ToReadOnlyReactiveCollection(
                                    onReset: Observable.Merge(
                                                    _searchResult.SearchText,
                                                    _searchResult.ResearchCommand,
                                                    _searchResult.ResetCommand,
                                                    _searchResult.ResetSearchKeywordsCommand)
                                            .Select(_ => Unit.Default))
                                .AddTo(Disposable);
            HaveNoShowedPhoto = ShowedPhotoList.ObserveAddChanged().Select(_ => !ShowedPhotoList.Any()).ToReactiveProperty().AddTo(Disposable);

            SearchDate = _searchResult.SearchedDate.ToReactivePropertyAsSynchronized(x => x.Value).AddTo(Disposable);
            SearchWithUserNameCommand = new ReactiveCommand<string>().AddTo(Disposable);
            SearchWithUserNameCommand.Subscribe(u => _searchResult.SearchedUserName.Value = u).AddTo(Disposable);
            SearchWithWorldNameCommand = new ReactiveCommand<string>().AddTo(Disposable);
            SearchWithWorldNameCommand.Subscribe(w => _searchResult.SearchedWorldName.Value = w).AddTo(Disposable);
            SearchWithDateCommand = new ReactiveCommand<string>().AddTo(Disposable);
            SearchWithDateCommand.Subscribe(dateString =>  _searchResult.SearchedDate.Value = DateTime.Parse(dateString)).AddTo(Disposable);
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
                    _searchResult.SearchedSinceDate.Value = now.AddDays(-7);
                    _searchResult.SearchedUntilDate.Value = now;
                }
                else if (type == "month")
                {
                    _searchResult.SearchedSinceDate.Value = now.AddMonths(-1);
                    _searchResult.SearchedUntilDate.Value = now;
                }
            });
            ClearSearchText = new ReactiveCommand().AddTo(Disposable);
            ClearSearchText.Subscribe(() =>
            {
                _searchResult.ResetSearchKeywordsCommand.Execute();
            })
            .AddTo(Disposable);

            CurrentUserSortType = new ReactiveProperty<UserSortType>(UserSortType.None).AddTo(Disposable);
            CurrentUserSortType.Subscribe(type => _users.SortType.Value = CurrentUserSortType.Value).AddTo(Disposable);
            UserList = _users.SortedUserList
                            .ObserveAddChanged()
                            .Select(u => u.Name)
                            .ToReadOnlyReactiveCollection(
                                onReset: Observable.Merge(_users.SortCommand, _users.ResetCommand).Select(_ => Unit.Default))
                            .AddTo(Disposable);

            SortUserWithAlphabetCommand = new ReactiveCommand().AddTo(Disposable);
            SortUserWithCountCommand = new ReactiveCommand().AddTo(Disposable);
            SortUserWithAlphabetCommand.Subscribe(() => CurrentUserSortType.Value = UserSortType.Alphabet).AddTo(Disposable);
            SortUserWithCountCommand.Subscribe(() => CurrentUserSortType.Value = UserSortType.Count).AddTo(Disposable);

            OpenPhotoPreviewCommand = new ReactiveCommand<Photo>().AddTo(Disposable);
            OpenPhotoPreviewCommand.Subscribe(photo => { if (!(photo is null)) WindowHelper.OpenPhotoPreviewWindow(photo, ShowedPhotoList.ToList(), _searchResult, _mainWindow); }).AddTo(Disposable);
            OpenSettingCommand = new ReactiveCommand().AddTo(Disposable);
            OpenSettingCommand.Subscribe(() => WindowHelper.OpenSettingDialog(_mainWindow)).AddTo(Disposable);

            RebootCommand = new ReactiveCommand().AddTo(Disposable);
            RebootCommand.Subscribe(() =>
            {
                _searchResult.ResetCommand.Execute();
                _users.ResetCommand.Execute();
                _ = LoadResourcesAsync();
            });

            if (!(Setting.Instance.Data is null))
            {
                _ = LoadResourcesAsync();
            }
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

        private string ToSearchText(SearchResult result)
        {
            var useUser = result.SearchedUserName.Value.Any();
            var userWorld = result.SearchedWorldName.Value.Any();
            var useDate = result.SearchedDate.Value.Date.CompareTo(new DateTime().Date) != 0;
            var useSinceDate = result.SearchedSinceDate.Value.Date.CompareTo(new DateTime().Date) != 0;
            var useUntilDate = result.SearchedUntilDate.Value.Date.CompareTo(new DateTime().Date) != 0;

            var stringBuilder = new StringBuilder();
            if (useUser)
                stringBuilder.Append(@$"user:""{result.SearchedUserName.Value}"" ");
            if (userWorld)
                stringBuilder.Append(@$"world:""{result.SearchedWorldName.Value}"" ");

            if (useDate && !useSinceDate && !useUntilDate)
                stringBuilder.Append(@$"date:""{result.SearchedDate.Value.Date.ToString("yyyy-MM-dd")}"" ");
            if (useSinceDate)
                stringBuilder.Append(@$"since:""{result.SearchedSinceDate.Value.Date.ToString("yyyy-MM-dd")}"" ");
            if (useUntilDate)
                stringBuilder.Append(@$"until:""{result.SearchedUntilDate.Value.Date.ToString("yyyy-MM-dd")}""");

            return stringBuilder.ToString();
        }
    }
}
