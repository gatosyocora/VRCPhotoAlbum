using Gatosyocora.VRCPhotoAlbum.Helpers;
using Gatosyocora.VRCPhotoAlbum.Models;
using Gatosyocora.VRCPhotoAlbum.Views;
using KoyashiroKohaku.VrcMetaToolSharp;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;

namespace Gatosyocora.VRCPhotoAlbum.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public ReactiveCollection<Photo> ShowedPhotoList;
        private List<Photo> _photoList { get; set; }

        public ReactiveCollection<string> UserList { get; }

        public ReactiveProperty<string> SearchText { get; }
        public ReactiveProperty<DateTime> SearchDate { get; }
        public ReactiveProperty<bool> HaveNoShowedPhoto { get; }

        public ReactiveCommand ClearSearchText { get; }
        public ReactiveCommand<Photo> ShowPreview { get; }
        public ReactiveCommand<string> SearchWithUser { get; }
        public ReactiveCommand<string> SearchWithDate { get; }
        public ReactiveCommand OpenSettingCommand { get; }
        public ReactiveCommand SortUserWithAlphabetCommand { get; }
        public ReactiveCommand SortUserWithCountCommand { get; }

        private MainWindow _mainWindow;

        public MainViewModel(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;

            if (Setting.Instance.Data is null)
            {
                WindowHelper.OpenSettingDialog(_mainWindow);
            }

            Cache.Instance.Create();

            ShowedPhotoList = new ReactiveCollection<Photo>().AddTo(Disposable);
            UserList = new ReactiveCollection<string>().AddTo(Disposable);
            SearchText = new ReactiveProperty<string>(string.Empty).AddTo(Disposable);
            SearchDate = new ReactiveProperty<DateTime>().AddTo(Disposable);
            HaveNoShowedPhoto = new ReactiveProperty<bool>(true).AddTo(Disposable);

            ClearSearchText = new ReactiveCommand().AddTo(Disposable);
            ShowPreview = new ReactiveCommand<Photo>().AddTo(Disposable);
            SearchWithUser = new ReactiveCommand<string>().AddTo(Disposable);
            SearchWithDate = new ReactiveCommand<string>().AddTo(Disposable);
            OpenSettingCommand = new ReactiveCommand().AddTo(Disposable);
            SortUserWithAlphabetCommand = new ReactiveCommand().AddTo(Disposable);
            SortUserWithCountCommand = new ReactiveCommand().AddTo(Disposable);

            ShowedPhotoList.CollectionChanged += PhotoList_OnChanged;

            LoadResources();

            SearchText.Subscribe(SearchPhoto);
            SearchDate.Subscribe(d => SearchWithDateString(d.ToString("yyyy/MM/dd HH:mm:ss")));

            ClearSearchText.Subscribe(() => SearchText.Value = string.Empty);
            ShowPreview.Subscribe(photo => { if (!(photo is null)) WindowHelper.OpenPhotoPreviewWindow(photo, ShowedPhotoList.ToList(), _mainWindow); });
            SearchWithUser.Subscribe(SearchWithUserName);
            SearchWithDate.Subscribe(type => 
            {
                var now = DateTime.Now;

                if (type == "today")
                {
                    SearchWithDateString(now.ToString("yyyy/MM/dd HH:mm:ss"));
                }
                else if (type == "week")
                {
                    SearchWithDatePeriodString(now.AddDays(-7).ToString("yyyy/MM/dd HH:mm:ss"), now.ToString("yyyy/MM/dd HH:mm:ss"));
                }
                else if (type == "month")
                {
                    SearchWithDatePeriodString(now.AddMonths(-1).ToString("yyyy/MM/dd HH:mm:ss"), now.ToString("yyyy/MM/dd HH:mm:ss"));
                }
            });
            OpenSettingCommand.Subscribe(() =>
            {
                WindowHelper.OpenSettingDialog(_mainWindow);
            });
            SortUserWithAlphabetCommand.Subscribe(() =>
            {
                UserList.ClearOnScheduler();
                UserList.AddRangeOnScheduler(MetaDataHelper.GetSortedUserList(_photoList, MetaDataHelper.UserSortType.Alphabet));
            });
            SortUserWithCountCommand.Subscribe(() =>
            {
                UserList.ClearOnScheduler();
                UserList.AddRangeOnScheduler(MetaDataHelper.GetSortedUserList(_photoList, MetaDataHelper.UserSortType.Count));
            });
        }

        public void LoadResources()
        {
            try
            {
                _photoList = LoadVRCPhotoList(Setting.Instance.Data.FolderPath);
                UserList.ClearOnScheduler();
                UserList.AddRangeOnScheduler(MetaDataHelper.GetSortedUserList(_photoList, MetaDataHelper.UserSortType.Alphabet));

                ShowedPhotoList.ClearOnScheduler();
                ShowedPhotoList.AddRangeOnScheduler(_photoList);
            }
            catch (Exception e)
            {
                Debug.Print($"{e.GetType().Name}: {e.Message}");
            }
        }

        private void PhotoList_OnChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            HaveNoShowedPhoto.Value = !ShowedPhotoList.Any();
        }

        private List<Photo> LoadVRCPhotoList(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                throw new ArgumentException($"{folderPath} is not exist.");
            }

            return Directory.GetFiles(folderPath, "*.png", SearchOption.AllDirectories)
                        .Where(x => !x.StartsWith(Cache.Instance.CacheFolderPath))
                        .Select(x =>
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
                                image = ImageHelper.GetThumbnailImage(x, Cache.Instance.CacheFolderPath);
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
                        .ToList();
        }

        private void SearchPhoto(string searchText)
        {
            string searchUserName, searchWorldName, searchDateString, searchSinceDateString, searchUntilDateString;
            var userMatch = Regex.Match(searchText, @".*user:""(?<userName>.*?)"".*");
            var worldMatch = Regex.Match(searchText, @".*world:""(?<worldName>.*?)"".*");
            var dateMatch = Regex.Match(searchText, @".*date:""(?<dateString>.*?)"".*");
            var sinceDateMatch = Regex.Match(searchText, @".*since:""(?<dateString>.*?)"".*");
            var untilDateMatch = Regex.Match(searchText, @".*until:""(?<dateString>.*?)"".*");
            if (userMatch.Success)
            {
                searchUserName = $"{userMatch.Groups["userName"]}";
            }
            else
            {
                searchUserName = Regex.Replace(searchText, @"\s*world:"".*?""\s*", string.Empty);
                searchUserName = Regex.Replace(searchUserName, @"\s*date:"".*?""\s*", string.Empty);
                searchUserName = Regex.Replace(searchUserName, @"\s*since:"".*?""\s*", string.Empty);
                searchUserName = Regex.Replace(searchUserName, @"\s*until:"".*?""\s*", string.Empty);
            }

            if (worldMatch.Success)
            {
                searchWorldName = $"{worldMatch.Groups["worldName"]}";
            }
            else
            {
                searchWorldName = Regex.Replace(searchText, @"\s*user:"".*?""\s*", string.Empty);
                searchWorldName = Regex.Replace(searchWorldName, @"\s*date:"".*?""\s*", string.Empty);
                searchWorldName = Regex.Replace(searchWorldName, @"\s*since:"".*?""\s*", string.Empty);
                searchWorldName = Regex.Replace(searchWorldName, @"\s*until:"".*?""\s*", string.Empty);
            }

            if (dateMatch.Success)
            {
                searchDateString = $"{dateMatch.Groups["dateString"]}";
            }
            else
            {
                searchDateString = string.Empty;
            }

            if (sinceDateMatch.Success)
            {
                searchSinceDateString = $"{sinceDateMatch.Groups["dateString"]}";
            }
            else
            {
                searchSinceDateString = string.Empty;
            }

            if (untilDateMatch.Success)
            {
                searchUntilDateString = $"{untilDateMatch.Groups["dateString"]}";
            }
            else
            {
                searchUntilDateString = string.Empty;
            }

            var searchedPhotoList = _photoList.Select(x => x);

            if (dateMatch.Success && (!sinceDateMatch.Success && !untilDateMatch.Success))
            {
                var searchDate = DateTime.Parse(searchDateString).Date;
                searchedPhotoList = searchedPhotoList
                                        .Where(x => (x.MetaData?.Date?.Date.CompareTo(searchDate) ?? 1) == 0);
            }
            else 
            {
                if (sinceDateMatch.Success)
                {
                    var searchSinceDate = DateTime.Parse(searchSinceDateString).Date;
                    searchedPhotoList = searchedPhotoList
                                            .Where(x => (x.MetaData?.Date?.Date.CompareTo(searchSinceDate) ?? -1) >= 0);
                }

                if (untilDateMatch.Success)
                {
                    var searchUntilDate = DateTime.Parse(searchUntilDateString).Date;
                    searchedPhotoList = searchedPhotoList
                                            .Where(x => (x.MetaData?.Date?.Date.CompareTo(searchUntilDate) ?? 1) <= 0);
                }
            }

            if (!userMatch.Success && !worldMatch.Success)
            {
                searchedPhotoList = searchedPhotoList
                                        .Where(x => (x?.MetaData?.Users?.Any(u => u.UserName.ToLower().StartsWith(searchUserName.ToLower())) ?? false) ||
                                                    (x?.MetaData?.World?.ToLower().Contains(searchWorldName.ToLower()) ?? false));
            }
            else
            {
                searchedPhotoList = searchedPhotoList
                                        .Where(x => (x?.MetaData?.Users?.Any(u => u.UserName.ToLower().StartsWith(searchUserName.ToLower())) ?? false) &&
                                                    (x?.MetaData?.World?.ToLower().Contains(searchWorldName.ToLower()) ?? false));
            }

            ShowedPhotoList.Clear();
            foreach (var photo in searchedPhotoList.ToList())
            {
                ShowedPhotoList.AddOnScheduler(photo);
            }
        }

        public void UpdatePhotoList()
        {
            SearchPhoto(SearchText.Value);
        }

        public void SearchWithUserName(string userName)
        {
            if (string.IsNullOrEmpty(userName)) return;

            var userMatch = Regex.Match(SearchText.Value, @"(?<prefix>.*user:"")(?<userName>.*?)(?<suffix>"".*)");

            if (userMatch.Success)
            {
                SearchText.Value = $"{userMatch.Groups["prefix"]}{userName}{userMatch.Groups["suffix"]}";
            }
            else
            {
                var space = string.Empty;
                if (!string.IsNullOrEmpty(SearchText.Value))
                {
                    space  = " ";
                }
                SearchText.Value += $@"{space}user:""{userName}""";
            }
        }

        public void SearchWithWorldName(string worldName)
        {
            if (string.IsNullOrEmpty(worldName)) return;

            var worldMatch = Regex.Match(SearchText.Value, @"(?<prefix>.*world:"")(?<userName>.*?)(?<suffix>"".*)");

            if (worldMatch.Success)
            {
                SearchText.Value = $"{worldMatch.Groups["prefix"]}{worldName}{worldMatch.Groups["suffix"]}";
            }
            else
            {
                var space = string.Empty;
                if (!string.IsNullOrEmpty(SearchText.Value))
                {
                    space = " ";
                }
                SearchText.Value += $@"{space}world:""{worldName}""";
            }
        }

        public void SearchWithDateString(string dateString)
        {
            if (string.IsNullOrEmpty(dateString) || dateString == "0001/01/01 00:00:00") return;

            SearchDate.Value = DateTime.Parse(dateString).Date;

            dateString = SearchDate.Value.ToString("yyyy-MM-dd");

            var searchText = Regex.Replace(SearchText.Value, @"\s*since:"".*?""\s*", string.Empty);
            searchText = Regex.Replace(searchText, @"\s*until:"".*?""\s*", string.Empty);

            var dateMatch = Regex.Match(searchText, @"(?<prefix>.*date:"")(?<dateString>.*?)(?<suffix>"".*)");

            if (dateMatch.Success)
            {
                SearchText.Value = $"{dateMatch.Groups["prefix"]}{dateString}{dateMatch.Groups["suffix"]}";
            }
            else
            {
                var space = string.Empty;
                if (!string.IsNullOrEmpty(searchText))
                {
                    space = " ";
                }
                SearchText.Value = $@"{searchText}{space}date:""{dateString}""";
            }
        }

        public void SearchWithDatePeriodString(string sinceDateString, string untilDateString)
        {
            if (string.IsNullOrEmpty(sinceDateString) || string.IsNullOrEmpty(untilDateString) ||
                sinceDateString == "0001/01/01 00:00:00" || untilDateString == "0001/01/01 00:00:00") return;

            sinceDateString = DateTime.Parse(sinceDateString).Date.ToString("yyyy-MM-dd");
            untilDateString = DateTime.Parse(untilDateString).Date.ToString("yyyy-MM-dd");

            var searchText = Regex.Replace(SearchText.Value, @"\s*date:"".*?""\s*", string.Empty);

            var sinceDateMatch = Regex.Match(searchText, @"(?<prefix>.*since:"")(?<dateString>.*?)(?<suffix>"".*)");

            string searchTextWithSinceDate;
            if (sinceDateMatch.Success)
            {
                searchTextWithSinceDate = $"{sinceDateMatch.Groups["prefix"]}{sinceDateString}{sinceDateMatch.Groups["suffix"]}";
            }
            else
            {
                var space = string.Empty;
                if (!string.IsNullOrEmpty(searchText))
                {
                    space = " ";
                }
                searchTextWithSinceDate = $@"{searchText}{space}since:""{sinceDateString}""";
            }

            var untilDateMatch = Regex.Match(searchTextWithSinceDate, @"(?<prefix>.*until:"")(?<dateString>.*?)(?<suffix>"".*)");

            if (untilDateMatch.Success)
            {
                SearchText.Value = $"{untilDateMatch.Groups["prefix"]}{untilDateString}{untilDateMatch.Groups["suffix"]}";
            }
            else
            {
                var space = string.Empty;
                if (!string.IsNullOrEmpty(searchTextWithSinceDate))
                {
                    space = " ";
                }
                SearchText.Value = $@"{searchTextWithSinceDate}{space}until:""{untilDateString}""";
            }
        }
    }
}
