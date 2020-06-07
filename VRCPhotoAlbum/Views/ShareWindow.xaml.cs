using Gatosyocora.VRCPhotoAlbum.Models;
using Gatosyocora.VRCPhotoAlbum.ViewModels;
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
    /// ShareWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ShareWindow : MahApps.Metro.Controls.MetroWindow
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
