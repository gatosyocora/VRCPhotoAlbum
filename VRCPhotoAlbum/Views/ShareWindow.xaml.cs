using Gatosyocora.VRCPhotoAlbum.Models;
using Gatosyocora.VRCPhotoAlbum.ViewModels;
using System.Windows;
using MahApps.Metro.Controls;

namespace Gatosyocora.VRCPhotoAlbum.Views
{
    /// <summary>
    /// ShareWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ShareWindow : MetroWindow
    {
        private ShareViewModel _shareViewModel;

        private Photo _photo;

        public ShareWindow(Photo photo)
        {
            _photo = photo;
            InitializeComponent();
            Loaded += ShareWindow_OnLoaded;
        }

        private void ShareWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            _shareViewModel = new ShareViewModel(_photo);
            DataContext = _shareViewModel;
        }
    }
}
