using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using Image = System.Drawing.Image;
using Gatosyocora.VRCPhotoAlbum.Models;
using KoyashiroKohaku.VrcMetaToolSharp;
using System.Diagnostics;
using System.Collections.ObjectModel;
using Gatosyocora.VRCPhotoAlbum.ViewModel;
using Gatosyocora.VRCPhotoAlbum.Views;

namespace Gatosyocora.VRCPhotoAlbum.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel _mainViewModel;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_OnLoaded;
        }

        private void MainWindow_OnLoaded(object sender, EventArgs args)
        {
            _mainViewModel = new MainViewModel();
            DataContext = _mainViewModel;
            PhotoListBox.ItemsSource = _mainViewModel.ShowedPhotoList;
        }

        private void UserListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UserListView.SelectedItem is null) return;

            var selectedUserName = UserListView.SelectedItem as string;
            _mainViewModel.SearchText = selectedUserName;
        }

        private void PhotoListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PhotoListBox.SelectedItem is null) return;

            var selectedPhoto = PhotoListBox.SelectedItem as Photo;
            OpenPhotoPreview(selectedPhoto);
        }

        private void OpenPhotoPreview(Photo photo)
        {
            var photoPreview = new PhotoPreview(photo, this);
            photoPreview.Show();
        }

        public void SearchWithUserName(string userName)
        {
            _mainViewModel.SearchText = userName;
        }
    }
}
