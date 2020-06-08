using Gatosyocora.VRCPhotoAlbum.Helpers;
using Gatosyocora.VRCPhotoAlbum.Models;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Reactive.Linq;
using System.Windows.Media.Imaging;

namespace Gatosyocora.VRCPhotoAlbum.ViewModels
{
    public class ShareViewModel : ViewModelBase
    {
        public ReactiveProperty<Photo> SharePhoto { get; }
        public ReactiveProperty<BitmapImage> SharePhotoBitmapImage { get; }
        public ReactiveProperty<string> TweetContent { get; }

        public ReactiveCommand Login { get; }
        public ReactiveCommand Send { get; } 

        public ShareViewModel(Photo photo)
        {
            SharePhoto = new ReactiveProperty<Photo>().AddTo(disposes);
            TweetContent = SharePhoto.Select(p => $"Taken by {p?.MetaData?.Photographer ?? string.Empty} in {p?.MetaData?.World ?? string.Empty} #VRCPhotoAlbum")
                                    .ToReactiveProperty().AddTo(disposes);
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
                                    }).ToReactiveProperty().AddTo(disposes);
            SharePhoto.Value = photo;

            Login = new ReactiveCommand().AddTo(disposes);
            Send = new ReactiveCommand().AddTo(disposes);

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
