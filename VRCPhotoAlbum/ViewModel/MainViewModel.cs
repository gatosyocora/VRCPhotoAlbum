using Gatosyocora.VRCPhotoAlbum.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Linq;
using System.Drawing;
using KoyashiroKohaku.VrcMetaToolSharp;
using System.Windows.Controls;
using Image = System.Drawing.Image;

namespace Gatosyocora.VRCPhotoAlbum.ViewModel
{
    public class MainViewModel
    {
        public ObservableCollection<Photo> ShowedPhotoList = new ObservableCollection<Photo>();
        private List<Photo> _photoList;

        public string SearchText { get; set; }

        public MainViewModel()
        {
            try
            {
                _photoList = LoadVRCPhotoList(@"D:\VRTools\vrc_meta_tool\meta_pic");

                foreach (var photo in _photoList)
                {
                    ShowedPhotoList.Add(photo);
                }
            }
            catch (Exception e)
            {
                Debug.Print($"{e.GetType().Name}: {e.Message}");
            }
        }

        private List<Photo> LoadVRCPhotoList(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                throw new ArgumentException($"{folderPath} is not exist.");
            }

            return Directory.GetFiles(folderPath, "*.png", SearchOption.AllDirectories)
                        .Select(x =>
                        new Photo
                        {
                            FilePath = x,
                            OriginalImage = Image.FromFile(x),
                            MetaData = VrcMetaDataReader.Read(File.ReadAllBytes(x))
                        })
                        .ToList();
        }

        public void SearchPhotoWithUserName(string searchedUserName)
        {
            ShowedPhotoList.Clear();
            var searchedPhotoList = _photoList
                    .Where(x => x.MetaData.Users.Any(u => u.StartsWith(searchedUserName)))
                    .ToList();

            foreach (var photo in searchedPhotoList)
            {
                ShowedPhotoList.Add(photo);
            }
        }
    }
}
