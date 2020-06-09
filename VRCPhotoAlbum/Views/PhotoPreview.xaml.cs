using Gatosyocora.VRCPhotoAlbum.Models;
using Gatosyocora.VRCPhotoAlbum.ViewModels;
using KoyashiroKohaku.VrcMetaToolSharp;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using System;

namespace Gatosyocora.VRCPhotoAlbum.Views
{
    /// <summary>
    /// PhotoPreview.xaml の相互作用ロジック
    /// </summary>
    public partial class PhotoPreview : MetroWindow, IDisposable
    {
        private PhotoPreviewViewModel _photoPreviewViewModel;

        public PhotoPreview(Photo photo, List<Photo> photoList, SearchResult searchResult)
        {
            InitializeComponent();
            _photoPreviewViewModel = new PhotoPreviewViewModel(this, photo, photoList, searchResult);
            DataContext = _photoPreviewViewModel;
        }

        private void UserListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedUser = UserListView.SelectedItem as KoyashiroKohaku.VrcMetaToolSharp.User;
            MainWindow.Instance.SearchWithUserName(selectedUser.UserName);
            Close();
        }

        private void TwitterButton_Click(object sender, RoutedEventArgs e)
        {
            var displayName = (sender as Button)?.CommandParameter as string ?? string.Empty;
            _photoPreviewViewModel.OpenTwitter.Execute(displayName);
        }

        private void SearchWithWorldButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedWorld = (sender as Button)?.CommandParameter as string ?? string.Empty;
            MainWindow.Instance.SearchWithWorldName(selectedWorld);
            Close();
        }

        private void SearchWithDateButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedDate = (sender as Button)?.CommandParameter as string ?? string.Empty;
            MainWindow.Instance.SearchWithDate(selectedDate);
            Close();
        }

        public void Dispose()
        {
            _photoPreviewViewModel.Dispose();
        }
    }
}
