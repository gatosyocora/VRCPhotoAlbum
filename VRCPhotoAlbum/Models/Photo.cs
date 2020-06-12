using Gatosyocora.VRCPhotoAlbum.Helpers;
using KoyashiroKohaku.VrcMetaTool;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Diagnostics;
using System.IO;
using System.Windows.Media.Imaging;

namespace Gatosyocora.VRCPhotoAlbum.Models
{
    public class Photo : ModelBase
    {
        private static readonly string NOW_LOADING_IMAGE_PATH = @"pack://application:,,,/Resources/nowloading.jpg";
        private static readonly string FAILED_IMAGE_PATH = @"pack://application:,,,/Resources/noloading.png";

        public string FilePath { get; set; }
        public BitmapImage ThumbnailImage { get; set; }
        public ReactiveProperty<string> ThumbnailImagePath { get; set; }

        public VrcMetaData MetaData { get; set; }

        public ReactiveCommand CreateThumbnailCommand { get; }

        public Photo()
        {
            ThumbnailImagePath = new ReactiveProperty<string>(NOW_LOADING_IMAGE_PATH).AddTo(Disposable);
            CreateThumbnailCommand = new ReactiveCommand().AddTo(Disposable);
            CreateThumbnailCommand.Subscribe(async () =>
            {
                if (!File.Exists(ThumbnailImagePath.Value))
                {
                    await ImageHelper.CreateThumbnailImagePathAsync(FilePath, Cache.Instance.CacheFolderPath);
                }
                ThumbnailImagePath.Value = ImageHelper.GetThumbnailImagePath(FilePath, Cache.Instance.CacheFolderPath);
            }).AddTo(Disposable);
        }

        public override string ToString()
        {
            return FilePath;
        }
    }
}
