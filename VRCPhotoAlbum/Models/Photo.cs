using Gatosyocora.VRCPhotoAlbum.Helpers;
using KoyashiroKohaku.VrcMetaTool;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Diagnostics;
using System.IO;
using System.Reactive.Linq;
using System.Windows.Media.Imaging;

namespace Gatosyocora.VRCPhotoAlbum.Models
{
    public class Photo : ModelBase
    {
        private static readonly string NOW_LOADING_IMAGE_PATH = @"pack://application:,,,/Resources/nowloading.jpg";
        private static readonly string FAILED_IMAGE_PATH = @"pack://application:,,,/Resources/noloading.png";

        public string FilePath { get; set; }
        public ReactiveProperty<BitmapImage> ThumbnailImage { get; set; }
        public ReactiveProperty<string> ThumbnailImagePath { get; }

        public VrcMetaData MetaData { get; set; }

        public ReactiveCommand CreateThumbnailCommand { get; }

        public Photo()
        {
            ThumbnailImage = new ReactiveProperty<BitmapImage>().AddTo(Disposable);

            var thumbnailImagePath = ImageHelper.GetThumbnailImagePath(FilePath, Cache.Instance.CacheFolderPath);
            if (File.Exists(thumbnailImagePath))
            {
                ThumbnailImagePath = new ReactiveProperty<string>(thumbnailImagePath).AddTo(Disposable);
            }
            else
            {
                ThumbnailImagePath = new ReactiveProperty<string>(NOW_LOADING_IMAGE_PATH).AddTo(Disposable);
            }
            CreateThumbnailCommand = new ReactiveCommand().AddTo(Disposable);

            CreateThumbnailCommand.Subscribe(async () =>
            {
                ThumbnailImage.Value = ImageHelper.LoadBitmapImage(ThumbnailImagePath.Value);
                if (!File.Exists(ThumbnailImagePath.Value))
                {
                    await ImageHelper.CreateThumbnailImagePathAsync(FilePath, Cache.Instance.CacheFolderPath);
                }
                ThumbnailImagePath.Value = ImageHelper.GetThumbnailImagePath(FilePath, Cache.Instance.CacheFolderPath);
                ThumbnailImage.Value = ImageHelper.LoadBitmapImage(ThumbnailImagePath.Value);
            }).AddTo(Disposable);
        }

        public override string ToString()
        {
            return FilePath;
        }
    }
}
