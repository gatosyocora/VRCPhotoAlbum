using Gatosyocora.VRCPhotoAlbum.Helpers;
using Gatosyocora.VRCPhotoAlbum.Models;
using Gatosyocora.VRCPhotoAlbum.Servisies;
using Gatosyocora.VRCPhotoAlbum.Views;
using KoyashiroKohaku.VrcMetaTool;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
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
        private DBCacheService _db;
        #endregion

        #region Photo
        public ReadOnlyReactiveCollection<Photo> ShowedPhotoList { get; }
        public ReactiveProperty<bool> HaveNoShowedPhoto { get; }
        public ReactiveProperty<string> PhotoCount { get; }

        public ReactiveCommand ChangePreviousPageCommand { get; }
        public ReactiveCommand ChangeNextPageCommand { get; }

        private Task _loadingTask;
        private CancellationTokenSource _loadingCancel;
        #endregion

        #region Search
        public ReactiveProperty<string> SearchText { get; }
        public ReactiveProperty<DateTime> SearchDate { get; }

        public ReactiveCommand<string> SearchWithUserNameCommand { get; }
        public ReactiveCommand<string> SearchWithWorldNameCommand { get; }
        public ReactiveCommand<string> SearchWithDateCommand { get; }
        public ReactiveCommand<DateSearchType> SearchWithDateTypeCommand { get; }

        public ReactiveCommand ClearSearchText { get; }
        #endregion

        #region UserList
        public ReadOnlyReactiveCollection<string> UserList { get; }

        public ReactiveProperty<UserSortType> CurrentUserSortType { get; }
        public ReactiveCommand SortUserWithAlphabetCommand { get; }
        public ReactiveCommand SortUserWithCountCommand { get; }

        public ReactiveProperty<bool> CanUseToSorting { get; }
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
        public ReactiveCommand CancelLoadingCommand { get; }
        public ReactiveCommand DeleteCacheCommand { get; }

        public MainViewModel(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;

            _db = new DBCacheService($"{AppCache.Instance.CacheFolderPath}{Path.DirectorySeparatorChar}cache.db");

            _vrcPhotographs = new VrcPhotographs(_db).AddTo(Disposable);
            _searchResult = new SearchResult(_vrcPhotographs.Collection).AddTo(Disposable);
            _users = new Users(_vrcPhotographs.Collection).AddTo(Disposable);

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
            PhotoCount = Observable.Merge(
                            ShowedPhotoList.ObserveAddChanged().Select(_ => Unit.Default),
                            ShowedPhotoList.ObserveRemoveChanged().Select(_ => Unit.Default),
                            ShowedPhotoList.ObserveResetChanged().Select(_ => Unit.Default)
                         ).Select(_ => $"{ShowedPhotoList.Count} 枚").ToReactiveProperty().AddTo(Disposable);

            SearchDate = _searchResult.SearchedDate.ToReactivePropertyAsSynchronized(x => x.Value).AddTo(Disposable);
            SearchWithUserNameCommand = new ReactiveCommand<string>().AddTo(Disposable);
            SearchWithUserNameCommand.Subscribe(_searchResult.SearchWithUserName).AddTo(Disposable);
            SearchWithWorldNameCommand = new ReactiveCommand<string>().AddTo(Disposable);
            SearchWithWorldNameCommand.Subscribe(_searchResult.SearchWithWorldName).AddTo(Disposable);
            SearchWithDateCommand = new ReactiveCommand<string>().AddTo(Disposable);
            SearchWithDateCommand.Subscribe(dateString => _searchResult.SearchWithDate(DateTime.Parse(dateString, new CultureInfo("en-US")))).AddTo(Disposable);
            SearchWithDateTypeCommand = new ReactiveCommand<DateSearchType>().AddTo(Disposable);
            SearchWithDateTypeCommand.Subscribe(type =>
            {
                var now = DateTime.Now;

                if (type == DateSearchType.TODAY)
                {
                    SearchDate.Value = now;
                }
                else if (type == DateSearchType.WEEK)
                {
                    _searchResult.SearchWithDatePeriodString(now.AddDays(-7).ToString("yyyy/MM/dd HH:mm:ss", new CultureInfo("en-US")), now.ToString("yyyy/MM/dd HH:mm:ss", new CultureInfo("en-US")));
                }
                else if (type == DateSearchType.MONTH)
                {
                    _searchResult.SearchWithDatePeriodString(now.AddMonths(-1).ToString("yyyy/MM/dd HH:mm:ss", new CultureInfo("en-US")), now.ToString("yyyy/MM/dd HH:mm:ss", new CultureInfo("en-US")));
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
            CanUseToSorting = _users.SortedUserList.CollectionChangedAsObservable()
                                    .Select(_ => _users.SortedUserList.Any()).ToReactiveProperty(false).AddTo(Disposable);

            OpenPhotoPreviewCommand = new ReactiveCommand<Photo>().AddTo(Disposable);
            OpenPhotoPreviewCommand.Subscribe(photo => { if (!(photo is null)) WindowHelper.OpenPhotoPreviewWindow(photo, ShowedPhotoList.ToList(), _searchResult, _mainWindow); }).AddTo(Disposable);
            OpenSettingCommand = new ReactiveCommand().AddTo(Disposable);
            OpenSettingCommand.Subscribe(() => WindowHelper.OpenSettingDialog(_mainWindow)).AddTo(Disposable);

            LoadResourcesCommand = new ReactiveCommand().AddTo(Disposable);
            LoadResourcesCommand.Subscribe(() =>
            {
                _loadingCancel = new CancellationTokenSource().AddTo(Disposable);
                var folderPaths = GetPhotoFolders(Setting.Instance.Data.PhotoFolders);
                _loadingTask = _vrcPhotographs.LoadVRCPhotoListAsync(folderPaths, _loadingCancel.Token);
            }).AddTo(Disposable);

            CancelLoadingCommand = new ReactiveCommand().AddTo(Disposable);
            CancelLoadingCommand.Subscribe(() =>
            {
                if (!(_loadingCancel is null))
                {
                    try
                    {
                        _loadingCancel.Cancel();
                    }
                    catch (TaskCanceledException e) { }
                }
            });

            RebootCommand = new ReactiveCommand().AddTo(Disposable);
            RebootCommand.Subscribe(async () =>
            {
                CancelLoadingCommand.Execute();
                // キャンセルにラグがあるので少し待つ
                await Task.Delay(2000).ConfigureAwait(true);
                _searchResult.ResetCommand.Execute();
                _users.ResetCommand.Execute();
                LoadResourcesCommand.Execute();
            }).AddTo(Disposable);

            DeleteCacheCommand = new ReactiveCommand().AddTo(Disposable);
            DeleteCacheCommand.Subscribe(async () =>
            {
                CancelLoadingCommand.Execute();
                await _db.DeleteAll().ConfigureAwait(true);
                AppCache.Instance.DeleteCacheFileAll();
            });

            ActiveProgressRing = new ReactiveProperty<bool>(true).AddTo(Disposable);
            _vrcPhotographs.Collection.ObserveAddChangedItems().Subscribe(_ => ActiveProgressRing.Value = false).AddTo(Disposable);
            _vrcPhotographs.Collection.ObserveResetChanged().Subscribe(_ => ActiveProgressRing.Value = true).AddTo(Disposable);
        }

        private string[] GetPhotoFolders(IList<PhotoFolder> photoFolderList)
        {
            if (photoFolderList is null) return Array.Empty<string>();

            return photoFolderList.SelectMany(f =>
            {
                if (f.ContainsSubFolder)
                {
                    return Directory.GetDirectories(f.FolderPath);
                }
                else
                {
                    return new string[] { f.FolderPath };
                }
            })
            .Distinct()
            .ToArray();
        }
    }
}
