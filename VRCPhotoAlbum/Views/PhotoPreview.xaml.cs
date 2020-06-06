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
    public partial class PhotoPreview : Window
    {
        private PhotoPreviewViewModel _photoPreviewViewModel;
        private MainWindow _mainWindow;

        public PhotoPreview(Photo photo, List<Photo> photoList, MainWindow mainWindow)
        {
            InitializeComponent();
            _photoPreviewViewModel = new PhotoPreviewViewModel(photo, photoList);
            DataContext = _photoPreviewViewModel;
            _mainWindow = mainWindow;
        }

        private void UserListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedUser = UserListView.SelectedItem as User;
            _mainWindow.SearchWithUserName(selectedUser.UserName);
            Close();
        }

        private void TwitterButton_Click(object sender, RoutedEventArgs e)
        {
            var displayName = (sender as Button)?.CommandParameter as string ?? string.Empty;
            _photoPreviewViewModel.OpenTwitter.Execute(displayName);
        }
    }
}
