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
            var thumbnailImagePath = ImageHelper.GetThumbnailImagePath(FilePath, AppCache.Instance.CacheFolderPath);
            ThumbnailImagePath = new ReactiveProperty<string>(thumbnailImagePath);

            OnLoadedCommand = new ReactiveCommand();
            OnUnLoadedCommand = new ReactiveCommand();
            ImageFailedCommand = new ReactiveCommand();

            OnLoadedCommand.Subscribe(async () =>
            {
                // TODO: エラーで落ちる
                try
                {
                    LoadThumnailImage();
                }
                // 例外がスローされました: 'System.IO.IOException'
                // ストリームから読み取ることができません。
                catch (IOException e)
                {
                    FileHelper.OutputErrorLogFile(e);
                    ThumbnailImage.Value = ImageHelper.GetFailedImage();
                }
            });
            OnUnLoadedCommand.Subscribe(() => _loadCancel.Cancel());
            ImageFailedCommand.Subscribe(() => ThumbnailImage.Value = ImageHelper.GetFailedImage());
        }

        public async void LoadThumnailImage()
        {
            _loadCancel = new CancellationTokenSource();
            ThumbnailImage.Value = ImageHelper.GetNowLoadingImage();

            var image = await Task.Run(async () =>
            {
                try
                {
                    return await ImageHelper.LoadThumbnailBitmapImageAsync(FilePath, 120).ConfigureAwait(false);
                }
                catch (IOException e)
                {
                    return ImageHelper.GetFailedImage();
                }

            }, _loadCancel.Token).ConfigureAwait(false);

            ThumbnailImage.Value = image;
        }

        public override string ToString()
        {
            return FilePath;
        }
    }
}
