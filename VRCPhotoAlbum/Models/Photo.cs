using Gatosyocora.VRCPhotoAlbum.Helpers;
using KoyashiroKohaku.VrcMetaTool;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Diagnostics;
using System.IO;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Gatosyocora.VRCPhotoAlbum.Models
{
    public class Photo : ModelBase
    {
        public string FilePath { get; set; }
        public ReactiveProperty<BitmapImage> ThumbnailImage { get; set; }
        public ReactiveProperty<string> ThumbnailImagePath { get; }

        public VrcMetaData MetaData { get; set; }

        public ReactiveCommand CreateThumbnailCommand { get; }

        public Photo(string filePath)
        {
            ThumbnailImage = new ReactiveProperty<BitmapImage>().AddTo(Disposable);

            FilePath = filePath;
            var thumbnailImagePath = ImageHelper.GetThumbnailImagePath(FilePath, Cache.Instance.CacheFolderPath);
            ThumbnailImagePath = new ReactiveProperty<string>(thumbnailImagePath).AddTo(Disposable);

            CreateThumbnailCommand = new ReactiveCommand().AddTo(Disposable);

            CreateThumbnailCommand.Subscribe(async () =>
            {
                ThumbnailImage.Value = ImageHelper.GetNowLoadingImage();

                await Task.Run(async () =>
                {
                    if (!File.Exists(ThumbnailImagePath.Value))
                    {
                        await ImageHelper.CreateThumbnailImagePathAsync(FilePath, ThumbnailImagePath.Value);
                    }
                    ThumbnailImage.Value = ImageHelper.LoadBitmapImage(ThumbnailImagePath.Value);
                });
            }).AddTo(Disposable);
        }

        public override string ToString()
        {
            return FilePath;
        }
    }
}
