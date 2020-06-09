using Gatosyocora.VRCPhotoAlbum.Helpers;
using Gatosyocora.VRCPhotoAlbum.Models;
using Gatosyocora.VRCPhotoAlbum.Views;
using KoyashiroKohaku.VrcMetaToolSharp;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Media.Imaging;
using User = KoyashiroKohaku.VrcMetaToolSharp.User;

namespace Gatosyocora.VRCPhotoAlbum.ViewModels
{
    public class PhotoPreviewViewModel : ViewModelBase
    {
        public ReactiveProperty<Photo> PreviewPhoto { get; set; }

        private List<Photo> _photoList;
        private int _previewPhotoIndex;

        private PhotoPreview _photoPreviewWindow;

        public ReactiveProperty<BitmapImage> Image { get; }
        public ReactiveCollection<User> UserList { get; }
        public ReadOnlyReactiveProperty<string> WorldName { get; }
        public ReadOnlyReactiveProperty<string> PhotographerName { get; }
        public ReadOnlyReactiveProperty<string> PhotoDateTime { get; }
        public ReadOnlyReactiveProperty<string> PhotoNumber { get; }
        public ReactiveProperty<bool> UseTestFunction { get; }

        public ReactiveCommand Previous { get; }
        public ReactiveCommand Next { get; }
        public ReactiveCommand<string> SearchWithUser { get; }
        public ReactiveCommand<string> OpenTwitter { get; }
        public ReactiveCommand RotateL90 { get; }
        public ReactiveCommand RotateR90 { get; }
        public ReactiveCommand FlipHorizontal { get; }
        public ReactiveCommand ShareToTwitter { get; }
        public ReactiveCommand<User> UserSelectCommand { get; }
        public ReactiveCommand<PhotoPreview> WindowCloseCommand { get; }

        public PhotoPreviewViewModel(PhotoPreview photoPreviewWindow, Photo photo, List<Photo> photoList, SearchResult searchResult)
        {
            _photoPreviewWindow = photoPreviewWindow;

            PreviewPhoto = new ReactiveProperty<Photo>().AddTo(Disposable);

            _photoList = photoList;
            PreviewPhoto.Value = photo;
            _previewPhotoIndex = _photoList.IndexOf(photo);

            Image = PreviewPhoto.Select(p =>
            {
                var filePath = p?.FilePath ?? string.Empty;
                return ImageHelper.LoadBitmapImage(filePath);
            })
            .ToReactiveProperty().AddTo(Disposable);
            UserList = new ReactiveCollection<User>().AddTo(Disposable);
            WorldName = PreviewPhoto.Select(p => "World: " + p?.MetaData?.World ?? string.Empty).ToReadOnlyReactiveProperty().AddTo(Disposable);
            PhotographerName = PreviewPhoto.Select(p => "Photographer: " + p?.MetaData?.Photographer ?? string.Empty).ToReadOnlyReactiveProperty().AddTo(Disposable);
            PhotoDateTime = PreviewPhoto.Select(p => p?.MetaData?.Date?.ToString("yyyy/MM/dd HH:mm:ss") ?? string.Empty).ToReadOnlyReactiveProperty().AddTo(Disposable);
            PhotoNumber = PreviewPhoto.Select(_ => $"{_previewPhotoIndex + 1}/{_photoList.Count}").ToReadOnlyReactiveProperty().AddTo(Disposable);
            UseTestFunction = new ReactiveProperty<bool>().AddTo(Disposable);

            UseTestFunction.Value = Setting.Instance.Data.UseTestFunction;

            PreviewPhoto.Subscribe(p =>
            {
                UserList.Clear();
                UserList.AddRangeOnScheduler(p?.MetaData?.Users ?? Enumerable.Empty<User>());
            });

            Previous = new ReactiveCommand().AddTo(Disposable);
            Next = new ReactiveCommand().AddTo(Disposable);
            SearchWithUser = new ReactiveCommand<string>().AddTo(Disposable);
            OpenTwitter = new ReactiveCommand<string>().AddTo(Disposable);
            RotateL90 = new ReactiveCommand().AddTo(Disposable);
            RotateR90 = new ReactiveCommand().AddTo(Disposable);
            FlipHorizontal = new ReactiveCommand().AddTo(Disposable);
            ShareToTwitter = new ReactiveCommand().AddTo(Disposable);
            UserSelectCommand = new ReactiveCommand<User>().AddTo(Disposable);
            WindowCloseCommand = new ReactiveCommand<PhotoPreview>().AddTo(Disposable);

            Previous.Subscribe(PreviousPreview);
            Next.Subscribe(NextPreview);
            OpenTwitter.Subscribe(OpenTwitterWithScreenName);
            RotateL90.Subscribe(() => ImageProcessing(PreviewPhoto.Value.FilePath, PreviewPhoto.Value.MetaData, ImageHelper.RotateLeft90AndSave));
            RotateR90.Subscribe(() => ImageProcessing(PreviewPhoto.Value.FilePath, PreviewPhoto.Value.MetaData, ImageHelper.RotateRight90AndSave));
            FlipHorizontal.Subscribe(() => ImageProcessing(PreviewPhoto.Value.FilePath, PreviewPhoto.Value.MetaData, ImageHelper.FilpHorizontalAndSave));
            ShareToTwitter.Subscribe(() => WindowHelper.OpenShareDialog(PreviewPhoto.Value, _photoPreviewWindow));
            UserSelectCommand.Subscribe(u =>
            {
                searchResult.SearchedUserName.Value = u.UserName;
                Debug.Print(u.UserName);
            });
            WindowCloseCommand.Subscribe(w => w.Close());
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

        private void ImageProcessing(string filePath, VrcMetaData meta, Action<string, VrcMetaData> imageProcessFunction)
        {
            imageProcessFunction(filePath, meta);
            Cache.Instance.DeleteCacheFile(filePath);
            PreviewPhoto.Value.ThumbnailImage = ImageHelper.GetThumbnailImage(filePath, Cache.Instance.CacheFolderPath);
            Image.Value = ImageHelper.LoadBitmapImage(filePath);
            MainWindow.Instance.Reload();
        }
    }
}
