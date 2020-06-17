using Gatosyocora.VRCPhotoAlbum.Models;
using Gatosyocora.VRCPhotoAlbum.ViewModels;
using MahApps.Metro.Controls;
using System;
using System.Windows;

namespace Gatosyocora.VRCPhotoAlbum.Views
{
    /// <summary>
    /// ShareWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ShareWindow : WindowBase
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
            Disposable.Add(_shareViewModel);
            DataContext = _shareViewModel;
        }
    }
}
