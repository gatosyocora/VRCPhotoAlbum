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

        public ReactiveProperty<string> SearchText { get; set; } = new ReactiveProperty<string>();
        public ReactiveProperty<DateTime> SearchDate { get; set; } = new ReactiveProperty<DateTime>();
        public ReactiveProperty<bool> SearchWithDateTime { get; set; } = new ReactiveProperty<bool>();

        public ReactiveCommand ClearSearchText { get; set; } = new ReactiveCommand();
        public ReactiveCommand<Photo> ShowPreview { get; set; } = new ReactiveCommand<Photo>();
        public ReactiveCommand<string> SearchWithUser { get; set; } = new ReactiveCommand<string>();
        public ReactiveCommand OpenSettingCommand { get; set; } = new ReactiveCommand();

        private string _cashFolderPath;

        private MainWindow _mainWindow;

        public MainViewModel(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;

            var jsonFilePath = JsonHelper.GetJsonFilePath();
            SettingData settingData;
            if (File.Exists(jsonFilePath))
            {
                settingData = JsonHelper.ImportJsonFile<SettingData>(jsonFilePath);
            }
            else
            {
                settingData = OpenSetting();
            }

            _cashFolderPath = settingData.FolderPath + Path.DirectorySeparatorChar + "Cash";

            try
            {
                _photoList = LoadVRCPhotoList(settingData.FolderPath);
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

            SearchText.Value = string.Empty;
            SearchDate.Value = DateTime.Now;
            SearchWithDateTime.Value = false;
            SearchText.Subscribe(searchText => SearchPhotoWithUserNameAndDateTime(searchText, SearchDate.Value, SearchWithDateTime.Value));
            SearchDate.Subscribe(searchDate => SearchPhotoWithUserNameAndDateTime(SearchText.Value, searchDate, SearchWithDateTime.Value));
            SearchWithDateTime.Subscribe(useDateTime => SearchPhotoWithUserNameAndDateTime(SearchText.Value, SearchDate.Value, useDateTime));

            ClearSearchText.Subscribe(_ => SearchText.Value = string.Empty);
            ShowPreview.Subscribe(photo => { if (!(photo is null)) OpenPhotoPreview(photo); });
            SearchWithUser.Subscribe(userName => SearchText.Value = userName);
            OpenSettingCommand.Subscribe(() => 
            {
                settingData = OpenSetting();
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
                            ThumbnailImage = GetThumbnailImage(x),
                            MetaData = VrcMetaDataReader.Read(x)
                        })
                        .ToList();
        }

        public void SearchPhotoWithUserNameAndDateTime(string searchedUserName, DateTime searchedDate, bool useDate)
        {
            ShowedPhotoList.Clear();
            var searchedPhotoList = _photoList
                    .Where(x => x.MetaData.Users.Any(u => u.UserName.StartsWith(searchedUserName)) &&
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

        private SettingData OpenSetting()
        {
            var settingWindow = new SettingWindow();
            settingWindow.Owner = _mainWindow;
            settingWindow.ShowDialog();
            JsonHelper.ExportJsonFile(settingWindow.SettingData, JsonHelper.GetJsonFilePath());
            return settingWindow.SettingData;
        }

        private BitmapImage GetThumbnailImage(string filePath)
        {
            if (!Directory.Exists(_cashFolderPath))
            {
                Directory.CreateDirectory(_cashFolderPath);
            }

            var thumbnailImageFilePath = $"{_cashFolderPath}/tn_" + Path.GetFileName(filePath);

            if (!File.Exists(thumbnailImageFilePath))
            {
                using (var stream = File.OpenRead(filePath))
                {
                    var originalImage = Image.FromStream(stream, false, false);
                    var thumbnailImage = originalImage.GetThumbnailImage(originalImage.Width / 4, originalImage.Height / 4, () => { return false; }, IntPtr.Zero);
                    thumbnailImage.Save(thumbnailImageFilePath, ImageFormat.Png);
                    originalImage.Dispose();
                    thumbnailImage.Dispose();
                }
            }
            var thumbnailBimapImage = new BitmapImage();
            thumbnailBimapImage.BeginInit();
            thumbnailBimapImage.UriSource = new Uri(thumbnailImageFilePath);
            thumbnailBimapImage.EndInit();
            return thumbnailBimapImage;
        }

        private void OpenPhotoPreview(Photo photo)
        {
            var photoPreview = new PhotoPreview(photo, ShowedPhotoList.ToList(), _mainWindow);
            photoPreview.Owner = _mainWindow;
            photoPreview.Show();
        }
    }
}
