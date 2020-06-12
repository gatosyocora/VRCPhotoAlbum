using Gatosyocora.VRCPhotoAlbum.Models;
using Gatosyocora.VRCPhotoAlbum.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

        internal static void OpenTwitterWithScreenName(string twitterScreenName)
        {
            var uri = $@"https://twitter.com/{twitterScreenName.Replace("@", string.Empty)}";
            try
            {
                var startInfo = new ProcessStartInfo(uri)
                {
                    UseShellExecute = true
                };
                Process.Start(startInfo);
            }
            catch (Exception exception)
            {
                Debug.Print($"{exception.GetType()}: {exception.Message} {uri}");
            }
        }

        internal static void OpenFileExplorer(string filePath)
        {
            var uri = @"explorer.exe";
            try
            {
                var startInfo = new ProcessStartInfo(uri)
                {
                    UseShellExecute = true,
                    Arguments = $@"/select,{filePath}"
                };
                Process.Start(startInfo);
            }
            catch (Exception exception)
            {
                Debug.Print($"{exception.GetType()}: {exception.Message} {uri}");
            }
        }
    }
}
