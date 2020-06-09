using Gatosyocora.VRCPhotoAlbum.Models;
using Gatosyocora.VRCPhotoAlbum.ViewModels;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Gatosyocora.VRCPhotoAlbum.Views
{
    /// <summary>
    /// PhotoPreview.xaml の相互作用ロジック
    /// </summary>
    public partial class PhotoPreview : MetroWindow, IDisposable
    {
        private PhotoPreviewViewModel _photoPreviewViewModel;

        public PhotoPreview(Photo photo, List<Photo> photoList, SearchResult searchResult)
        {
            InitializeComponent();
            _photoPreviewViewModel = new PhotoPreviewViewModel(this, photo, photoList, searchResult);
            DataContext = _photoPreviewViewModel;
        }

        // TODO: コマンド実行化できなかったので保留
        private void TwitterButton_Click(object sender, RoutedEventArgs e)
        {
            var displayName = (sender as Button)?.CommandParameter as string ?? string.Empty;
            _photoPreviewViewModel.OpenTwitterCommand.Execute(displayName);
        }

        public void Dispose()
        {
            _photoPreviewViewModel.Dispose();
        }
    }
}
