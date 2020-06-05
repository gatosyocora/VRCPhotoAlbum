using Gatosyocora.VRCPhotoAlbum.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Linq;
using System.Drawing;
using KoyashiroKohaku.VrcMetaToolSharp;
using System.Windows.Controls;
using Image = System.Drawing.Image;
using System.Windows.Media.Imaging;
using System.Text.Json;
using System.Reflection;
using System.Windows;
using Gatosyocora.VRCPhotoAlbum.Views;
using Gatosyocora.VRCPhotoAlbum.Helpers;
using System.Drawing.Imaging;

namespace Gatosyocora.VRCPhotoAlbum.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Photo> ShowedPhotoList = new ObservableCollection<Photo>();
        private List<Photo> _photoList { get; }

        public List<string> UserList { get; }

        private string _searchText = string.Empty;
        public string SearchText { get { return _searchText; }
            set
            {
                _searchText = value;
                SearchTextPropertyChanged(nameof(SearchText));
                SearchPhotoWithUserName(_searchText);
            }
        }
        private void SearchTextPropertyChanged(string propertyName) => PropertyChanged(this, new PropertyChangedEventArgs(propertyName));

        public event PropertyChangedEventHandler PropertyChanged;

        private string _cashFolderPath;

        public MainViewModel()
        {
            var jsonFilePath = JsonHelper.GetJsonFilePath();
            SettingData settingData;
            if (File.Exists(jsonFilePath))
            {
                settingData = JsonHelper.ImportJsonFile<SettingData>(jsonFilePath);            }
            else
            {
                settingData = OpenSetting();
            }

            _cashFolderPath = settingData.FolderPath + Path.DirectorySeparatorChar+"Cash";

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

        public void SearchPhotoWithUserName(string searchedUserName)
        {
            ShowedPhotoList.Clear();
            var searchedPhotoList = _photoList
                    .Where(x => x.MetaData.Users.Any(u => u.UserName.StartsWith(searchedUserName)))
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
            settingWindow.ShowDialog();
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
    }
}
