using Gatosyocora.VRCPhotoAlbum.Models;
using Gatosyocora.VRCPhotoAlbum.ViewModels;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Gatosyocora.VRCPhotoAlbum.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MahApps.Metro.Controls.MetroWindow
    {
        private MainViewModel _mainViewModel;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_OnLoaded;
        }

        private void MainWindow_OnLoaded(object sender, EventArgs args)
        {
            _mainViewModel = new MainViewModel(this);
            DataContext = _mainViewModel;
            PhotoListBox.ItemsSource = _mainViewModel.ShowedPhotoList;
        }

        public void SearchWithUserName(string userName)
        {
            _mainViewModel.SearchUserText.Value = userName;
        }
    }
}
