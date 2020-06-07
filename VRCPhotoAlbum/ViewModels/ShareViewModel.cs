using Gatosyocora.VRCPhotoAlbum.Helpers;
using Gatosyocora.VRCPhotoAlbum.Models;
using Reactive.Bindings;
using System.Reactive.Linq;
using System.Windows.Media.Imaging;

namespace Gatosyocora.VRCPhotoAlbum.ViewModels
{
    public class ShareViewModel
    {
        public ReactiveProperty<Photo> SharePhoto { get; set; } = new ReactiveProperty<Photo>();
        public ReactiveProperty<BitmapImage> SharePhotoBitmapImage { get; set; } = new ReactiveProperty<BitmapImage>();
        public ReactiveProperty<string> TweetContent { get; set; }

        public ReactiveCommand Login { get; set; } = new ReactiveCommand();
        public ReactiveCommand Send { get; set; } = new ReactiveCommand();

        public ShareViewModel(Photo photo)
        {
            TweetContent = SharePhoto.Select(p => $"Taken by {p?.MetaData?.Photographer ?? string.Empty} in {p?.MetaData?.World ?? string.Empty} #VRCPhotoAlbum").ToReactiveProperty();
            SharePhotoBitmapImage = SharePhoto.Select(p =>
                                    {
                                        if (p != null)
                                        {
                                            return ImageHelper.LoadBitmapImage(p.FilePath);
                                        }
                                        else
                                        {
                                            return new BitmapImage();
                                        }
                                    }).ToReactiveProperty();
            SharePhoto.Value = photo;

            Login.Subscribe(() => LoginToTwitter());
            Send.Subscribe(() => SendToTwitter(SharePhoto.Value.FilePath, TweetContent.Value));
        }

        private void LoginToTwitter()
        {
        }

        private void SendToTwitter(string FilePath, string message)
        {

        }
    }
}
