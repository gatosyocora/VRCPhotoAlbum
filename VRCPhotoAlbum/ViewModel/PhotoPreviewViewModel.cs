using Gatosyocora.VRCPhotoAlbum.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Gatosyocora.VRCPhotoAlbum.ViewModel
{
    public class PhotoPreviewViewModel : INotifyPropertyChanged
    {
        private Photo _previewPhoto;
        public Photo PreviewPhoto { get => _previewPhoto; 
                        set
                        {
                            _previewPhoto = value;
                            OnPropertyChanged(nameof(PreviewPhoto));
                        }}

        public event PropertyChangedEventHandler PropertyChanged;

        public PhotoPreviewViewModel(Photo photo)
        {
            PreviewPhoto = photo;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged is null) return;
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
