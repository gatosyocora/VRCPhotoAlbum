using Gatosyocora.VRCPhotoAlbum.Models;
using Gatosyocora.VRCPhotoAlbum.ViewModel;
using System;
using System.Collections.Generic;
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
    /// SettingWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingWindow : Window
    {
        protected SettingViewModel _settingViewModel;

        public SettingData SettingData { get; set; }
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

        private void EnterButton_Click(object sender, RoutedEventArgs e)
        {
            SettingData = _settingViewModel.CreateSettingData();
            Close();
        }
    }
}
