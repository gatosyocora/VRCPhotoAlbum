using Gatosyocora.VRCPhotoAlbum.Models;
using KoyashiroKohaku.VrcMetaToolSharp;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

namespace Gatosyocora.VRCPhotoAlbum.ViewModel
{
    public class PhotoPreviewViewModel : IDisposable, INotifyPropertyChanged
    {
        public ReactiveProperty<Photo> PreviewPhoto { get; set; } = new ReactiveProperty<Photo>();
        private Photo _previousPhoto;
        private Photo _nextPhoto;

        private List<Photo> _photoList;
        private int _previewPhotoIndex;

        public event PropertyChangedEventHandler PropertyChanged;

        public ReadOnlyReactiveProperty<string> ImageFilePath { get; }
        public ReactiveCollection<User> UserList { get; } = new ReactiveCollection<User>();
        public ReadOnlyReactiveProperty<string> WorldName { get; }
        public ReadOnlyReactiveProperty<string> PhotographerName { get; }
        public ReadOnlyReactiveProperty<string> PhotoDateTime { get; }
        public ReadOnlyReactiveProperty<string> PhotoNumber { get; }

        public ReactiveCommand Previous { get; set; } = new ReactiveCommand();
        public ReactiveCommand Next { get; set; } = new ReactiveCommand();
        public ReactiveCommand<string> SearchWithUser { get; set; } = new ReactiveCommand<string>();

        public PhotoPreviewViewModel(Photo photo, List<Photo> photoList)
        {
            _photoList = photoList;
            PreviewPhoto.Value = photo;
            _previewPhotoIndex = _photoList.IndexOf(photo);
            _previousPhoto = _photoList[PreviousIndex(_previewPhotoIndex, _photoList.Count)];
            _nextPhoto = _photoList[NextIndex(_previewPhotoIndex, _photoList.Count)];

            ImageFilePath = PreviewPhoto.Select(p => p?.FilePath ?? string.Empty).ToReadOnlyReactiveProperty();
            WorldName = PreviewPhoto.Select(p => "World: " + p?.MetaData?.World ?? string.Empty).ToReadOnlyReactiveProperty();
            PhotographerName = PreviewPhoto.Select(p => "Photographer: " + p?.MetaData?.Photographer ?? string.Empty).ToReadOnlyReactiveProperty();
            PhotoDateTime = PreviewPhoto.Select(p => p?.MetaData?.Date?.ToString("yyyy/MM/dd HH:mm:ss") ?? string.Empty).ToReadOnlyReactiveProperty();
            PhotoNumber = PreviewPhoto.Select(_ => $"{_previewPhotoIndex + 1}/{_photoList.Count}").ToReadOnlyReactiveProperty();

            PreviewPhoto.Subscribe(p =>
            {
                UserList.Clear();
                UserList.AddRangeOnScheduler(p?.MetaData?.Users ?? Enumerable.Empty<User>());
            });

            Previous.Subscribe(() => PreviousPreview());
            Next.Subscribe(() => NextPreview());
            //SearchWithUser.Subscribe(userName => )
        }

        private void PreviousPreview()
        {
            _previewPhotoIndex = PreviousIndex(_previewPhotoIndex, _photoList.Count);
            _nextPhoto = PreviewPhoto.Value;
            PreviewPhoto.Value = _previousPhoto;
            _previousPhoto = _photoList[PreviousIndex(_previewPhotoIndex, _photoList.Count)];
        }

        private void NextPreview()
        {
            _previewPhotoIndex = NextIndex(_previewPhotoIndex, _photoList.Count);
            _previousPhoto = PreviewPhoto.Value;
            PreviewPhoto.Value = _nextPhoto;
            _nextPhoto = _photoList[NextIndex(_previewPhotoIndex, _photoList.Count)];
        }

        public static int PreviousIndex(int i, int n)
        {
            return (i + n) % n;
        }

        private int NextIndex(int i, int n)
        {
            return (i + 1) % n;
        }

        public void Dispose()
        {
            PreviewPhoto.Dispose();
            ImageFilePath.Dispose();
            UserList.Dispose();
            WorldName.Dispose();
            PhotographerName.Dispose();
            PhotoNumber.Dispose();
            PhotoDateTime.Dispose();

            Previous.Dispose();
            Next.Dispose();
            SearchWithUser.Dispose();
        }
    }
}
