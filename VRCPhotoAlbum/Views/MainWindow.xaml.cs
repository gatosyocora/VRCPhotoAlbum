using Gatosyocora.VRCPhotoAlbum.ViewModels;
using MahApps.Metro.Controls;
using System;
using System.Windows.Controls;

namespace Gatosyocora.VRCPhotoAlbum.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow, IDisposable
    {
        public static MainWindow Instance { get; private set; }

        private MainViewModel _mainViewModel;

        static MainWindow()
        {
            Instance = new MainWindow();
        }

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

        public async void Reboot()
        {
            await _mainViewModel.LoadResourcesAsync();
        }

        private void PhotoListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 選択状態を保たないために選択されると未選択状態に切り替える
            PhotoListBox.SelectedItem = null;
        }

        private void UserListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 選択状態を保たないために選択されると未選択状態に切り替える
            UserListView.SelectedItem = null;
        }

        public void Dispose()
        {
            _mainViewModel.Dispose();
        }
    }
}
