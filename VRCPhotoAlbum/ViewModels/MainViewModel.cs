using Gatosyocora.VRCPhotoAlbum.Helpers;
using Gatosyocora.VRCPhotoAlbum.Models;
using Gatosyocora.VRCPhotoAlbum.Views;
using KoyashiroKohaku.VrcMetaTool;
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
        private VrcPhotographs _vrcPhotographs;
        #endregion

        #region Photo
        public ReadOnlyReactiveCollection<Photo> ShowedPhotoList { get; }
        public ReactiveProperty<bool> HaveNoShowedPhoto { get; }

        public ReactiveCommand ChangePreviousPageCommand { get; }
        public ReactiveCommand ChangeNextPageCommand { get; }
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

        #region System
        public ReactiveCommand RebootCommand { get; }
        public ReactiveProperty<bool> ActiveProgressRing { get; }
        #endregion

        public ReactiveCommand LoadResourcesCommand { get; }

        public MainViewModel(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;

            Setting.Instance.Create();
            Cache.Instance.Create();

            _vrcPhotographs = new VrcPhotographs();
            _searchResult = new SearchResult(_vrcPhotographs.Collection);
            _users = new Users(_vrcPhotographs.Collection);

            SearchText = _searchResult.SearchText.ToReactivePropertyAsSynchronized(x => x.Value).AddTo(Disposable);

            ShowedPhotoList = _searchResult.ShowedPhotoList.ObserveAddChanged()
                                .ToReadOnlyReactiveCollection(
                                    onReset: Observable.Merge(
                                                    _searchResult.SearchText,
                                                    _searchResult.ResearchCommand,
                                                    _searchResult.ResetCommand)
                                            .Select(_ => Unit.Default))
                                .AddTo(Disposable);
            HaveNoShowedPhoto = ShowedPhotoList.ObserveAddChanged().Select(_ => !ShowedPhotoList.Any()).ToReactiveProperty().AddTo(Disposable);

            SearchDate = _searchResult.SearchedDate.ToReactivePropertyAsSynchronized(x => x.Value).AddTo(Disposable);
            SearchWithUserNameCommand = new ReactiveCommand<string>().AddTo(Disposable);
            SearchWithUserNameCommand.Subscribe(_searchResult.SearchWithUserName).AddTo(Disposable);
            SearchWithWorldNameCommand = new ReactiveCommand<string>().AddTo(Disposable);
            SearchWithWorldNameCommand.Subscribe(_searchResult.SearchWithWorldName).AddTo(Disposable);
            SearchWithDateCommand = new ReactiveCommand<string>().AddTo(Disposable);
            SearchWithDateCommand.Subscribe(dateString => _searchResult.SearchWithDate(DateTime.Parse(dateString))).AddTo(Disposable);
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

            CurrentUserSortType = new ReactiveProperty<UserSortType>(UserSortType.None).AddTo(Disposable);
            CurrentUserSortType.Subscribe(type => _users.SortType.Value = CurrentUserSortType.Value).AddTo(Disposable);
            UserList = _users.SortedUserList.ObserveAddChanged()
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
                _ = _vrcPhotographs.LoadResourcesAsync(Setting.Instance.Data.FolderPath);
            }).AddTo(Disposable);

            ActiveProgressRing = new ReactiveProperty<bool>(true).AddTo(Disposable);
            _vrcPhotographs.Collection.ObserveAddChangedItems().Subscribe(_ => ActiveProgressRing.Value = false).AddTo(Disposable);
            _vrcPhotographs.Collection.ObserveResetChanged().Subscribe(_ => ActiveProgressRing.Value = true).AddTo(Disposable);

            ChangePreviousPageCommand = new ReactiveCommand().AddTo(Disposable);
            ChangePreviousPageCommand.Subscribe(() =>
            {
                _vrcPhotographs.PreviousLoadingCommand.Execute();
                RebootCommand.Execute();
            }).AddTo(Disposable);
            ChangeNextPageCommand = new ReactiveCommand().AddTo(Disposable);
            ChangeNextPageCommand.Subscribe(() =>
            {
                _vrcPhotographs.NextLoadingCommand.Execute();
                RebootCommand.Execute();
            }).AddTo(Disposable);

            LoadResourcesCommand = new ReactiveCommand().AddTo(Disposable);
            LoadResourcesCommand.Subscribe(() =>
            {
                _ = _vrcPhotographs.LoadResourcesAsync(Setting.Instance.Data.FolderPath);
            }).AddTo(Disposable);
        }
    }
}
