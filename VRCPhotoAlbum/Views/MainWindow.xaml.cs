using Gatosyocora.VRCPhotoAlbum.Helpers;
using Gatosyocora.VRCPhotoAlbum.Models;
using Gatosyocora.VRCPhotoAlbum.ViewModels;
using MahApps.Metro.Controls;
using System;
using System.Diagnostics;
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
            ContentRendered += MainWindow_OnContentRendered;
        }

        private void MainWindow_OnLoaded(object sender, EventArgs args)
        {
            Title += $" {GetApplicationVersion()}";

            try
            {
                _mainViewModel = new MainViewModel(this);
                DataContext = _mainViewModel;
            }
            catch (Exception e)
            {
                FileHelper.OutputErrorLogFile(e);
            }
        }

        private void MainWindow_OnContentRendered(object sender, EventArgs args)
        {
            if (Setting.Instance.Data is null)
            {
                WindowHelper.OpenSettingDialog(this);
            }

            _mainViewModel.LoadResourcesCommand.Execute();
        }

        public void Reboot()
        {
            _mainViewModel.RebootCommand.Execute();
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

        private string GetApplicationVersion()
        {
            return FileVersionInfo.GetVersionInfo(typeof(App).Assembly.Location).FileVersion;
        }

        public void Dispose()
        {
            _mainViewModel.Dispose();
        }
    }
}
