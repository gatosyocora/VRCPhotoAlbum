using Gatosyocora.VRCPhotoAlbum.Helpers;
using Gatosyocora.VRCPhotoAlbum.Models;
using Gatosyocora.VRCPhotoAlbum.Views;
using KoyashiroKohaku.VrcMetaToolSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using Image = System.Drawing.Image;
using Reactive.Bindings;
using System.Windows;

namespace Gatosyocora.VRCPhotoAlbum.ViewModels
{
    public class MainViewModel
    {
        public ObservableCollection<Photo> ShowedPhotoList = new ObservableCollection<Photo>();
        private List<Photo> _photoList { get; }

        public List<string> UserList { get; }

        public ReactiveProperty<string> SearchUserText { get; set; } = new ReactiveProperty<string>();
        public ReactiveProperty<string> SearchWorldText { get; set; } = new ReactiveProperty<string>();
        public ReactiveProperty<DateTime> SearchDate { get; set; } = new ReactiveProperty<DateTime>();
        public ReactiveProperty<bool> SearchWithDateTime { get; set; } = new ReactiveProperty<bool>();

        public ReactiveCommand ClearSearchUserText { get; set; } = new ReactiveCommand();
        public ReactiveCommand ClearSearchWorldText { get; set; } = new ReactiveCommand();
        public ReactiveCommand<Photo> ShowPreview { get; set; } = new ReactiveCommand<Photo>();
        public ReactiveCommand<string> SearchWithUser { get; set; } = new ReactiveCommand<string>();
        public ReactiveCommand OpenSettingCommand { get; set; } = new ReactiveCommand();

        private string _cashFolderPath;

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
                _settingData = OpenSetting(_settingData);
            }

            _cashFolderPath = _settingData.FolderPath + Path.DirectorySeparatorChar + "Cash";

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

            SearchUserText.Value = string.Empty;
            SearchWorldText.Value = string.Empty;
            SearchDate.Value = DateTime.Now;
            SearchWithDateTime.Value = false;
            SearchUserText.Subscribe(searchText => SearchPhoto(searchText, SearchWorldText.Value, SearchDate.Value, SearchWithDateTime.Value));
            SearchWorldText.Subscribe(searchText => SearchPhoto(SearchUserText.Value, searchText, SearchDate.Value, SearchWithDateTime.Value));
            SearchDate.Subscribe(searchDate => SearchPhoto(SearchUserText.Value, SearchWorldText.Value, searchDate, SearchWithDateTime.Value));
            SearchWithDateTime.Subscribe(useDateTime => SearchPhoto(SearchUserText.Value, SearchWorldText.Value, SearchDate.Value, useDateTime));

            ClearSearchUserText.Subscribe(_ => SearchUserText.Value = string.Empty);
            ClearSearchWorldText.Subscribe(_ => SearchWorldText.Value = string.Empty);
            ShowPreview.Subscribe(photo => { if (!(photo is null)) OpenPhotoPreview(photo); });
            SearchWithUser.Subscribe(userName => SearchUserText.Value = userName);
            OpenSettingCommand.Subscribe(() => 
            {
                _settingData = OpenSetting(_settingData);
            });
        }

        private List<Photo> LoadVRCPhotoList(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                throw new ArgumentException($"{folderPath} is not exist.");
            }

            return Directory.GetFiles(folderPath, "*.png", SearchOption.AllDirectories)
                        .Where(x => !x.StartsWith(_cashFolderPath))
                        .Select(x =>
                        new Photo
                        {
                            FilePath = x,
                            ThumbnailImage = ImageHelper.GetThumbnailImage(x, _cashFolderPath),
                            MetaData = VrcMetaDataReader.Read(x)
                        })
                        .ToList();
        }

        private void SearchPhoto(string searchedUserName, string searchWorldName, DateTime searchedDate, bool useDate)
        {
            ShowedPhotoList.Clear();
            var searchedPhotoList = _photoList
                    .Where(x => x.MetaData.Users.Any(u => u.UserName.ToLower().StartsWith(searchedUserName.ToLower())) &&
                                x.MetaData.World.ToLower().Contains(searchWorldName.ToLower()) &&
                                (!useDate || x.MetaData.Date?.Date.CompareTo(searchedDate.Date) == 0))
                    .ToList();

            foreach (var photo in searchedPhotoList)
            {
                ShowedPhotoList.Add(photo);
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

        private SettingData OpenSetting(SettingData settingData)
        {
            var settingWindow = new SettingWindow(settingData);
            settingWindow.Owner = _mainWindow;
            settingWindow.ShowDialog();
            return settingWindow.SettingData;
        }

        private void OpenPhotoPreview(Photo photo)
        {
            var photoPreview = new PhotoPreview(photo, ShowedPhotoList.ToList(), _mainWindow);
            photoPreview.Owner = _mainWindow;
            photoPreview.Show();
        }
    }
}
