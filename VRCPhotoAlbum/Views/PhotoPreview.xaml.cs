using Gatosyocora.VRCPhotoAlbum.Models;
using Gatosyocora.VRCPhotoAlbum.ViewModel;
using KoyashiroKohaku.VrcMetaToolSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (e is null) return;
            var twitterScreenName = (sender as Button)?.CommandParameter as string;
            var uri = $@"https://twitter.com/{twitterScreenName.Replace("@", string.Empty)}";
            try
            {
                var startInfo = new ProcessStartInfo(uri)
                {
                    UseShellExecute = true
                };
                Process.Start(startInfo);
            }
            catch (Exception exception)
            {
                Debug.Print($"{exception.GetType()}: {exception.Message} {uri}");
            }
        }

        private void UserListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var  selectedUser = UserListView.SelectedItem as User;
            _mainWindow.SearchWithUserName(selectedUser.UserName);
            Close();
        }

        private void PreviousPhotoButton_Click(object sender, RoutedEventArgs e)
        {
            _photoPreviewViewModel.PreviousPreview();
        }

        private void NextPhotoButton_Click(object sender, RoutedEventArgs e)
        {
            _photoPreviewViewModel.NextPreview();
        }
    }
}
