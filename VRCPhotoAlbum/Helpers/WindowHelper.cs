using Gatosyocora.VRCPhotoAlbum.Models;
using Gatosyocora.VRCPhotoAlbum.Views;
using System.Collections.Generic;
using System.Windows;

namespace Gatosyocora.VRCPhotoAlbum.Helpers
{
    public class WindowHelper
    {
        internal static void OpenSettingDialog(Window ownerWindow)
        {
            var settingWindow = new SettingWindow()
            {
                Owner = ownerWindow
            };
            settingWindow.ShowDialog();
        }

        internal static void OpenPhotoPreviewWindow(Photo photo, List<Photo> photoList, SearchResult searchResult, Window ownerWindow)
        {
            var photoPreview = new PhotoPreview(photo, photoList, searchResult)
            {
                Owner = ownerWindow
            };
            photoPreview.Show();
        }

        internal static void OpenShareDialog(Photo photo, Window ownerWindow)
        {
            var shareWindow = new ShareWindow(photo)
            {
                Owner = ownerWindow
            };
            shareWindow.ShowDialog();
        }
    }
}
