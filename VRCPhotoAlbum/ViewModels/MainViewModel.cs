using Gatosyocora.VRCPhotoAlbum.Helpers;
using Gatosyocora.VRCPhotoAlbum.Models;
using Gatosyocora.VRCPhotoAlbum.Views;
using KoyashiroKohaku.VrcMetaToolSharp;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Gatosyocora.VRCPhotoAlbum.ViewModels
{
    public class MainViewModel
    {
        public ObservableCollection<Photo> ShowedPhotoList = new ObservableCollection<Photo>();
        private List<Photo> _photoList { get; }

        public List<string> UserList { get; }

        public ReactiveProperty<string> SearchText { get; set; } = new ReactiveProperty<string>(string.Empty);
        public ReactiveProperty<DateTime> SearchDate { get; set; } = new ReactiveProperty<DateTime>(DateTime.Now);
        public ReactiveProperty<bool> SearchWithDateTime { get; set; } = new ReactiveProperty<bool>(false);

        public ReactiveCommand ClearSearchText { get; set; } = new ReactiveCommand();
        public ReactiveCommand<Photo> ShowPreview { get; set; } = new ReactiveCommand<Photo>();
        public ReactiveCommand<string> SearchWithUser { get; set; } = new ReactiveCommand<string>();
        public ReactiveCommand OpenSettingCommand { get; set; } = new ReactiveCommand();

        private MainWindow _mainWindow;

        private SettingData _settingData;

        public MainViewModel(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;

            var jsonFilePath = JsonHelper.GetJsonFilePath();
            if (File.Exists(jsonFilePath))
            {
                _settingData = JsonHelper.ImportJsonFile<SettingData>(jsonFilePath);
            }
            else
            {
                _settingData = WindowHelper.OpenSettingDialog(_settingData, _mainWindow);
            }

            Cache.Instance.Create();

            try
            {
                _photoList = LoadVRCPhotoList(_settingData.FolderPath);
                UserList = GetSortedUserList(_photoList);

                foreach (var photo in _photoList)
                {
                    ShowedPhotoList.Add(photo);
                }
            }
            catch (Exception e)
            {
                Debug.Print($"{e.GetType().Name}: {e.Message}");
            }

            SearchText.Subscribe(searchText => SearchPhoto(searchText, SearchDate.Value, SearchWithDateTime.Value));
            SearchDate.Subscribe(searchDate => SearchPhoto(SearchText.Value, searchDate, SearchWithDateTime.Value));
            SearchWithDateTime.Subscribe(useDateTime => SearchPhoto(SearchText.Value, SearchDate.Value, useDateTime));

            ClearSearchText.Subscribe(() => SearchText.Value = string.Empty);
            ShowPreview.Subscribe(photo => { if (!(photo is null)) WindowHelper.OpenPhotoPreviewWindow(photo, ShowedPhotoList.ToList(), _mainWindow); });
            SearchWithUser.Subscribe(SearchWithUserName);
            OpenSettingCommand.Subscribe(() =>
            {
                _settingData = WindowHelper.OpenSettingDialog(_settingData, _mainWindow);
            });
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
                        new Photo
                        {
                            FilePath = x,
                            ThumbnailImage = ImageHelper.GetThumbnailImage(x, Cache.Instance.CacheFolderPath),
                            MetaData = VrcMetaDataReader.Read(x)
                        })
                        .ToList();
        }

        private void SearchPhoto(string searchText, DateTime searchedDate, bool useDate)
        {
            ShowedPhotoList.Clear();

            string searchUserName, searchWorldName;
            var userMatch = Regex.Match(searchText, @".*user:""(?<userName>.*?)"".*");
            var worldMatch = Regex.Match(searchText, @".*world:""(?<worldName>.*?)"".*");
            if (userMatch.Success)
            {
                searchUserName = $"{userMatch.Groups["userName"]}";
            }
            else
            {
                searchUserName = Regex.Replace(searchText, @"\s*world:"".*""\s*", string.Empty);
            }

            if (worldMatch.Success)
            {
                searchWorldName = $"{worldMatch.Groups["worldName"]}";
            }
            else
            {
                searchWorldName = Regex.Replace(searchText, @"\s*user:"".*""\s*", string.Empty);
            }

            var searchedPhotoList = _photoList
                    .Where(x => (!useDate || x.MetaData.Date?.Date.CompareTo(searchedDate.Date) == 0));

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

            foreach (var photo in searchedPhotoList.ToList())
            {
                ShowedPhotoList.Add(photo);
            }
        }

        public void UpdatePhotoList()
        {
            ShowedPhotoList.Clear();
            foreach (var photo in _photoList)
            {
                ShowedPhotoList.Add(photo);
            }
        }

        public void SearchWithUserName(string userName)
        {
            var userMatch = Regex.Match(SearchText.Value, @"(?<prefix>.*user:"")(?<userName>.*)(?<suffix>"".*)");

            if (userMatch.Success)
            {
                SearchText.Value = $"{userMatch.Groups["prefix"]}{userName}{userMatch.Groups["suffix"]}";
            }
            else
            {
                if (!string.IsNullOrEmpty(SearchText.Value))
                {
                    SearchText.Value += " ";
                }
                SearchText.Value += $@"user:""{userName}""";
            }
        }

        public void SearchWithWorldName(string worldName)
        {
            var worldMatch = Regex.Match(SearchText.Value, @"(?<prefix>.*world:"")(?<userName>.*)(?<suffix>"".*)");

            if (worldMatch.Success)
            {
                SearchText.Value = $"{worldMatch.Groups["prefix"]}{worldName}{worldMatch.Groups["suffix"]}";
            }
            else
            {
                if (!string.IsNullOrEmpty(SearchText.Value))
                {
                    SearchText.Value += " ";
                }
                SearchText.Value += $@"world:""{worldName}""";
            }
        }

        private List<string> GetSortedUserList(List<Photo> photoList)
        {
            return photoList
                        .SelectMany(x => x.MetaData.Users)
                        .Select(u => u.UserName)
                        .Distinct()
                        .OrderBy(x => x)
                        .ToList();
        }
    }
}
