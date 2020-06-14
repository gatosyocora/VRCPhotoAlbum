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

        public ReactiveCommand OnLoadedCommand { get; }
        public ReactiveCommand OnUnLoadedCommand { get; }
        public ReactiveCommand ImageFailedCommand { get; }

        private CancellationTokenSource _loadCancel;

        public Photo(string filePath)
        {
            ThumbnailImage = new ReactiveProperty<BitmapImage>();

            FilePath = filePath;
            var thumbnailImagePath = ImageHelper.GetThumbnailImagePath(FilePath, Cache.Instance.CacheFolderPath);
            ThumbnailImagePath = new ReactiveProperty<string>(thumbnailImagePath);

            OnLoadedCommand = new ReactiveCommand();
            OnUnLoadedCommand = new ReactiveCommand();
            ImageFailedCommand = new ReactiveCommand();

            OnLoadedCommand.Subscribe(async () => await LoadThumnailImage());
            OnUnLoadedCommand.Subscribe(() => _loadCancel.Cancel());
            ImageFailedCommand.Subscribe(() => ThumbnailImage.Value = ImageHelper.GetFailedImage());
        }

        public async Task LoadThumnailImage()
        {
            _loadCancel = new CancellationTokenSource();
            ThumbnailImage.Value = ImageHelper.GetNowLoadingImage();

            await Task.Run(async () =>
            {
                if (!File.Exists(ThumbnailImagePath.Value))
                {
                    if (_loadCancel.Token.IsCancellationRequested) return;
                    await ImageHelper.CreateThumbnailImagePathAsync(FilePath, ThumbnailImagePath.Value);
                }
                if (_loadCancel.Token.IsCancellationRequested) return;
                ThumbnailImage.Value = ImageHelper.LoadBitmapImage(ThumbnailImagePath.Value);
            }, _loadCancel.Token);
        }

        public override string ToString()
        {
            return FilePath;
        }
    }
}
