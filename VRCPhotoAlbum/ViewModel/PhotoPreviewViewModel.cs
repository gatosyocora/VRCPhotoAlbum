using Gatosyocora.VRCPhotoAlbum.Models;
using KoyashiroKohaku.VrcMetaToolSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace Gatosyocora.VRCPhotoAlbum.ViewModel
{
    public class PhotoPreviewViewModel : INotifyPropertyChanged
    {
        private Photo _previewPhoto;

        public BitmapImage Image => _previewPhoto.OriginalImage;

        public List<User> UserList => _previewPhoto?.MetaData?.Users ?? Enumerable.Empty<User>().ToList();
        public string WorldName => "World: " + _previewPhoto?.MetaData?.World ?? string.Empty;
        public string PhotographerName => "Photographer: " + _previewPhoto?.MetaData?.Photographer ?? string.Empty;
        public string PhotoDateTime => _previewPhoto?.MetaData?.Date?.ToString("yyyy/MM/dd HH:mm:ss") ?? string.Empty;

        public event PropertyChangedEventHandler PropertyChanged;

        public PhotoPreviewViewModel(Photo photo)
        {
            _previewPhoto = photo;
            OnPropertyChanged(nameof(Image));
            OnPropertyChanged(nameof(UserList));
            OnPropertyChanged(nameof(WorldName));
            OnPropertyChanged(nameof(PhotographerName));
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged is null) return;
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
