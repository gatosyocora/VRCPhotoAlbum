using Gatosyocora.VRCPhotoAlbum.Helpers;
using Gatosyocora.VRCPhotoAlbum.Models;
using Gatosyocora.VRCPhotoAlbum.Views;
using KoyashiroKohaku.VrcMetaTool;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using User = KoyashiroKohaku.VrcMetaTool.User;

namespace Gatosyocora.VRCPhotoAlbum.ViewModels
{
    public class PhotoPreviewViewModel : ViewModelBase
    {
        public ReactiveProperty<Photo> PreviewPhoto { get; set; }

        private List<Photo> _photoList;
        private int _previewPhotoIndex;

        public ReactiveProperty<BitmapImage> Image { get; }
        public ReadOnlyReactiveCollection<User> UserList { get; }
        public ReadOnlyReactiveProperty<string> WorldName { get; }
        public ReadOnlyReactiveProperty<string> PhotographerName { get; }
        public ReadOnlyReactiveProperty<string> PhotoDateTime { get; }
        public ReadOnlyReactiveProperty<string> PhotoNumber { get; }
        public ReactiveProperty<bool> UseTestFunction { get; }

        public ReactiveCommand PreviousCommand { get; }
        public ReactiveCommand NextCommand { get; }
        public ReactiveCommand<string> OpenTwitterCommand { get; }
        public ReactiveCommand OpenExplorerCommand { get; }
        public ReactiveCommand RotateL90Command { get; }
        public ReactiveCommand RotateR90Command { get; }
        public ReactiveCommand FlipHorizontalCommand { get; }
        public ReactiveCommand ShareToTwitterCommand { get; }
        public ReactiveCommand<User> UserSelectCommand { get; }
        public ReactiveCommand<string> WorldSelectCommand { get; }
        public ReactiveCommand<DateTime> DateSelectCommand { get; }
        public ReactiveCommand<PhotoPreview> WindowCloseCommand { get; }


        public PhotoPreviewViewModel(PhotoPreview photoPreviewWindow, Photo photo, List<Photo> photoList, SearchResult searchResult)
        {
            if (photoList is null)
            {
                throw new ArgumentNullException($"{photoList} is null");
            }

            PreviewPhoto = new ReactiveProperty<Photo>(photo).AddTo(Disposable);

            _photoList = photoList;
            _previewPhotoIndex = _photoList.IndexOf(photo);

            Image = new ReactiveProperty<BitmapImage>(ImageHelper.GetNowLoadingImage()).AddTo(Disposable);
            PreviewPhoto.Subscribe(async p =>
            {
                var task = new Task(() =>
                {
                    var filePath = p?.FilePath ?? string.Empty;
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        try
                        {
                            Image.Value = ImageHelper.LoadBitmapImage(p.FilePath);
                        }
                        catch (IOException e)
                        {
                            Image.Value = ImageHelper.GetFailedImage();
                        }
                    }
                });
                task.Start(TaskScheduler.FromCurrentSynchronizationContext());
            });

            PreviousCommand = new ReactiveCommand().AddTo(Disposable);
            NextCommand = new ReactiveCommand().AddTo(Disposable);

            UserList = PreviewPhoto.SelectMany(p => p?.MetaData?.Users ?? Enumerable.Empty<User>())
                            .ToReadOnlyReactiveCollection(
                                onReset: Observable
                                            .Merge(
                                                PreviousCommand,
                                                NextCommand)
                                            .Select(_ => Unit.Default))
                            .AddTo(Disposable);
            WorldName = PreviewPhoto.Select(p => "World: " + p?.MetaData?.World ?? string.Empty).ToReadOnlyReactiveProperty().AddTo(Disposable);
            PhotographerName = PreviewPhoto.Select(p => "Photographer: " + p?.MetaData?.Photographer ?? string.Empty).ToReadOnlyReactiveProperty().AddTo(Disposable);
            PhotoDateTime = PreviewPhoto.Select(p => p?.MetaData?.Date?.ToString("yyyy/MM/dd HH:mm:ss", new CultureInfo("en-US")) ?? string.Empty).ToReadOnlyReactiveProperty().AddTo(Disposable);
            PhotoNumber = PreviewPhoto.Select(_ => $"{_previewPhotoIndex + 1}/{_photoList.Count}").ToReadOnlyReactiveProperty().AddTo(Disposable);
            UseTestFunction = new ReactiveProperty<bool>().AddTo(Disposable);

            UseTestFunction.Value = Setting.Instance.Data.UseTestFunction;

            OpenTwitterCommand = new ReactiveCommand<string>().AddTo(Disposable);
            OpenExplorerCommand = new ReactiveCommand().AddTo(Disposable);
            RotateL90Command = new ReactiveCommand().AddTo(Disposable);
            RotateR90Command = new ReactiveCommand().AddTo(Disposable);
            FlipHorizontalCommand = new ReactiveCommand().AddTo(Disposable);
            ShareToTwitterCommand = new ReactiveCommand().AddTo(Disposable);
            UserSelectCommand = new ReactiveCommand<User>().AddTo(Disposable);
            WorldSelectCommand = new ReactiveCommand<string>().AddTo(Disposable);
            DateSelectCommand = new ReactiveCommand<DateTime>().AddTo(Disposable);
            WindowCloseCommand = new ReactiveCommand<PhotoPreview>().AddTo(Disposable);

            PreviousCommand.Subscribe(() =>
            {
                Image.Value = ImageHelper.GetNowLoadingImage();
                _previewPhotoIndex = (_previewPhotoIndex - 1 + _photoList.Count) % _photoList.Count;
                PreviewPhoto.Value = _photoList[_previewPhotoIndex];
            }).AddTo(Disposable);
            NextCommand.Subscribe(() =>
            {
                Image.Value = ImageHelper.GetNowLoadingImage();
                _previewPhotoIndex = (_previewPhotoIndex + 1) % _photoList.Count;
                PreviewPhoto.Value = _photoList[_previewPhotoIndex];
            }).AddTo(Disposable);
            OpenTwitterCommand.Subscribe(WindowHelper.OpenTwitterWithScreenName).AddTo(Disposable);
            OpenExplorerCommand.Subscribe(() => WindowHelper.OpenFileExplorer(PreviewPhoto.Value.FilePath)).AddTo(Disposable);
            RotateL90Command.Subscribe(() => ImageProcessing(PreviewPhoto.Value.FilePath, PreviewPhoto.Value.MetaData, searchResult, ImageHelper.RotateLeft90AndSave)).AddTo(Disposable);
            RotateR90Command.Subscribe(() => ImageProcessing(PreviewPhoto.Value.FilePath, PreviewPhoto.Value.MetaData, searchResult, ImageHelper.RotateRight90AndSave)).AddTo(Disposable);
            FlipHorizontalCommand.Subscribe(() => ImageProcessing(PreviewPhoto.Value.FilePath, PreviewPhoto.Value.MetaData, searchResult, ImageHelper.FilpHorizontalAndSave)).AddTo(Disposable);
            ShareToTwitterCommand.Subscribe(() => WindowHelper.OpenShareDialog(PreviewPhoto.Value, photoPreviewWindow)).AddTo(Disposable);
            UserSelectCommand.Subscribe(u => searchResult.SearchedUserName.Value = u.UserName).AddTo(Disposable);
            WorldSelectCommand.Subscribe(w => searchResult.SearchedWorldName.Value = w).AddTo(Disposable);
            DateSelectCommand.Subscribe(d => searchResult.SearchedDate.Value = d).AddTo(Disposable);
            WindowCloseCommand.Subscribe(w => w.Close()).AddTo(Disposable);
        }

        // TODO: 命名をどうにかする
        private void ImageProcessing(string filePath, VrcMetaData meta, SearchResult searchResult, Action<string, VrcMetaData> imageProcessFunction)
        {
            imageProcessFunction(filePath, meta);
            AppCache.Instance.DeleteCacheFile(filePath);
            //TODO: ここでサムネイルの読み直しをおこなう
            Image.Value = ImageHelper.LoadBitmapImage(filePath);
            searchResult.ResearchCommand.Execute();
        }
    }
}
