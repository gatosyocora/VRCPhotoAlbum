using Gatosyocora.VRCPhotoAlbum.Models;
using KoyashiroKohaku.VrcMetaToolSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace Gatosyocora.VRCPhotoAlbum.ViewModel
{
    public class PhotoPreviewViewModel : INotifyPropertyChanged
    {
        private Photo _previewPhoto;
        private Photo _previousPhoto;
        private Photo _nextPhoto;

        private List<Photo> _photoList;
        private int _previewPhotoIndex;

        public string ImageFilePath => _previewPhoto.FilePath;

        public List<User> UserList => _previewPhoto?.MetaData?.Users ?? Enumerable.Empty<User>().ToList();
        public string WorldName => "World: " + _previewPhoto?.MetaData?.World ?? string.Empty;
        public string PhotographerName => "Photographer: " + _previewPhoto?.MetaData?.Photographer ?? string.Empty;
        public string PhotoDateTime => _previewPhoto?.MetaData?.Date?.ToString("yyyy/MM/dd HH:mm:ss") ?? string.Empty;
        public string PhotoNumber => $"{_previewPhotoIndex+1}/{_photoList.Count}";

        public event PropertyChangedEventHandler PropertyChanged;

        public PhotoPreviewViewModel(Photo photo, List<Photo> photoList)
        {
            _photoList = photoList;
            _previewPhoto = photo;
            _previewPhotoIndex = _photoList.IndexOf(photo);
            _previousPhoto = _photoList[PreviousIndex(_previewPhotoIndex, _photoList.Count)];
            _nextPhoto = _photoList[NextIndex(_previewPhotoIndex, _photoList.Count)];

            OnPropertyChanged(nameof(ImageFilePath));
            OnPropertyChanged(nameof(UserList));
            OnPropertyChanged(nameof(WorldName));
            OnPropertyChanged(nameof(PhotographerName));
            OnPropertyChanged(nameof(PhotoDateTime));
            OnPropertyChanged(nameof(PhotoNumber));
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged is null) return;
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public void PreviousPreview()
        {
            _previewPhotoIndex = PreviousIndex(_previewPhotoIndex, _photoList.Count);
            _nextPhoto = _previewPhoto;
            _previewPhoto = _previousPhoto;
            _previousPhoto = _photoList[PreviousIndex(_previewPhotoIndex, _photoList.Count)];

            OnPropertyChanged(nameof(ImageFilePath));
            OnPropertyChanged(nameof(UserList));
            OnPropertyChanged(nameof(WorldName));
            OnPropertyChanged(nameof(PhotographerName));
            OnPropertyChanged(nameof(PhotoDateTime));
            OnPropertyChanged(nameof(PhotoNumber));
        }

        public void NextPreview()
        {
            _previewPhotoIndex = NextIndex(_previewPhotoIndex, _photoList.Count);
            _previousPhoto = _previewPhoto;
            _previewPhoto = _nextPhoto;
            _nextPhoto = _photoList[NextIndex(_previewPhotoIndex, _photoList.Count)];

            OnPropertyChanged(nameof(ImageFilePath));
            OnPropertyChanged(nameof(UserList));
            OnPropertyChanged(nameof(WorldName));
            OnPropertyChanged(nameof(PhotographerName));
            OnPropertyChanged(nameof(PhotoDateTime));
            OnPropertyChanged(nameof(PhotoNumber));
        }

        public static int PreviousIndex(int i, int n)
        {
            return i - 1 + n * (1 - (i + (n - 1)) / n);
        }

        private int NextIndex(int i, int n)
        {
            return (i + 1) % n;
        }
    }
}
