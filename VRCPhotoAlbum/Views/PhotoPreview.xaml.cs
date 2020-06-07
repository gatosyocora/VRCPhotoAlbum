using Gatosyocora.VRCPhotoAlbum.Models;
using Gatosyocora.VRCPhotoAlbum.ViewModels;
using KoyashiroKohaku.VrcMetaToolSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Gatosyocora.VRCPhotoAlbum.Views
{
    /// <summary>
    /// PhotoPreview.xaml の相互作用ロジック
    /// </summary>
    public partial class PhotoPreview : MahApps.Metro.Controls.MetroWindow
    {
        private PhotoPreviewViewModel _photoPreviewViewModel;

        public PhotoPreview(Photo photo, List<Photo> photoList)
        {
            InitializeComponent();
            _photoPreviewViewModel = new PhotoPreviewViewModel(this, photo, photoList);
            DataContext = _photoPreviewViewModel;
        }

        private void UserListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedUser = UserListView.SelectedItem as User;
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
    }
}
