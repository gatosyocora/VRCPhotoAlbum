using Gatosyocora.VRCPhotoAlbum.Helpers;
using Gatosyocora.VRCPhotoAlbum.Models;
using Gatosyocora.VRCPhotoAlbum.Views;
using KoyashiroKohaku.VrcMetaToolSharp;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Media.Imaging;

namespace Gatosyocora.VRCPhotoAlbum.ViewModels
{
    public class PhotoPreviewViewModel : IDisposable, INotifyPropertyChanged
    {
        public ReactiveProperty<Photo> PreviewPhoto { get; set; } = new ReactiveProperty<Photo>();

        private List<Photo> _photoList;
        private int _previewPhotoIndex;

        private PhotoPreview _photoPreviewWindow;

        public event PropertyChangedEventHandler PropertyChanged;

        public ReadOnlyReactiveProperty<BitmapImage> Image { get; }
        public ReactiveCollection<User> UserList { get; } = new ReactiveCollection<User>();
        public ReadOnlyReactiveProperty<string> WorldName { get; }
        public ReadOnlyReactiveProperty<string> PhotographerName { get; }
        public ReadOnlyReactiveProperty<string> PhotoDateTime { get; }
        public ReadOnlyReactiveProperty<string> PhotoNumber { get; }

        public ReactiveCommand Previous { get; set; } = new ReactiveCommand();
        public ReactiveCommand Next { get; set; } = new ReactiveCommand();
        public ReactiveCommand<string> SearchWithUser { get; set; } = new ReactiveCommand<string>();
        public ReactiveCommand<string> OpenTwitter { get; set; } = new ReactiveCommand<string>();
        public ReactiveCommand RotateL90 { get; set; } = new ReactiveCommand();
        public ReactiveCommand RotateR90 { get; set; } = new ReactiveCommand();
        public ReactiveCommand ShareToTwitter { get; set; } = new ReactiveCommand();

        public PhotoPreviewViewModel(PhotoPreview photoPreviewWindow, Photo photo, List<Photo> photoList)
        {
            _photoPreviewWindow = photoPreviewWindow;

            _photoList = photoList;
            PreviewPhoto.Value = photo;
            _previewPhotoIndex = _photoList.IndexOf(photo);

            Image = PreviewPhoto.Select(p => 
            {
                var filePath = p?.FilePath ?? string.Empty;
                return ImageHelper.LoadBitmapImage(filePath);
            })
            .ToReadOnlyReactiveProperty();
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
            OpenTwitter.Subscribe(userName => OpenTwitterWithScreenName(userName));
            // 回転させるとメタデータが消えてしまうので一旦保留
            //RotateL90.Subscribe(() =>
            //{
            //    ImageHelper.RotateLeft90AndSave(PreviewPhoto.Value.FilePath);
            //});
            //RotateR90.Subscribe(() =>
            //{
            //    ImageHelper.RotateRight90AndSave(PreviewPhoto.Value.FilePath);
            //});
            ShareToTwitter.Subscribe(() => OpenShareWindow(PreviewPhoto.Value));
        }

        private void PreviousPreview()
        {
            _previewPhotoIndex = (_previewPhotoIndex - 1 + _photoList.Count) % _photoList.Count;
            PreviewPhoto.Value = _photoList[_previewPhotoIndex];
        }

        private void NextPreview()
        {
            _previewPhotoIndex = (_previewPhotoIndex + 1) % _photoList.Count;
            PreviewPhoto.Value = _photoList[_previewPhotoIndex];
        }

        private void OpenTwitterWithScreenName(string twitterScreenName)
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

        private void OpenShareWindow(Photo photo)
        {
            var shareWindow = new ShareWindow(photo);
            shareWindow.Owner = _photoPreviewWindow;
            shareWindow.ShowDialog();
        }

        public void Dispose()
        {
            PreviewPhoto.Dispose();
            Image.Dispose();
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
