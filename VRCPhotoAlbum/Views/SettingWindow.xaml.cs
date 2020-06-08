using Gatosyocora.VRCPhotoAlbum.Models;
using Gatosyocora.VRCPhotoAlbum.ViewModels;
using System.Windows;
using MahApps.Metro.Controls;
using System;

namespace Gatosyocora.VRCPhotoAlbum.Views
{
    /// <summary>
    /// SettingWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingWindow : MetroWindow, IDisposable
    {
        protected SettingViewModel _settingViewModel;

        public SettingWindow()
        {
            InitializeComponent();
            Loaded += SettingWindow_OnLoaded;
        }

        private void SettingWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            _settingViewModel = new SettingViewModel();
            DataContext = _settingViewModel;
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            _settingViewModel.ApplySettingData();
            Close();
        }

        public void Dispose()
        {
            _settingViewModel.Dispose();
        }
    }
}
