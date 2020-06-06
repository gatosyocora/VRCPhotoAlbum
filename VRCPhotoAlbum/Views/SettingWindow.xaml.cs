using Gatosyocora.VRCPhotoAlbum.Models;
using Gatosyocora.VRCPhotoAlbum.ViewModels;
using System.Windows;

namespace Gatosyocora.VRCPhotoAlbum.Views
{
    /// <summary>
    /// SettingWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingWindow : MahApps.Metro.Controls.MetroWindow
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
