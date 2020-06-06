using Gatosyocora.VRCPhotoAlbum.Models;
using Gatosyocora.VRCPhotoAlbum.ViewModel;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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
            UserListView.SelectedItem = null;
            _mainViewModel.SearchText = selectedUserName;
        }

        private void PhotoListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PhotoListBox.SelectedItem is null) return;

            var selectedPhoto = PhotoListBox.SelectedItem as Photo;
            PhotoListBox.SelectedItem = null;
            OpenPhotoPreview(selectedPhoto);
        }

        private void OpenPhotoPreview(Photo photo)
        {
            var photoPreview = new PhotoPreview(photo, _mainViewModel.ShowedPhotoList.ToList(), this);
            photoPreview.Show();
        }

        public void SearchWithUserName(string userName)
        {
            _mainViewModel.SearchText = userName;
        }

        private void SearchDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SearchDatePicker.SelectedDate is null) return;

            var selectedDate = SearchDatePicker.SelectedDate ?? new DateTime();
            _mainViewModel.SearchDate = selectedDate;
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            _mainViewModel.SearchText = string.Empty;
        }
    }
}
