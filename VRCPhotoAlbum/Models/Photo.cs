using KoyashiroKohaku.VrcMetaToolSharp;
using System.Windows.Media.Imaging;

namespace Gatosyocora.VRCPhotoAlbum.Models
{
    public class Photo
    {
        public string FilePath { get; set; }
        public BitmapImage ThumbnailImage { get; set; }

        public VrcMetaData MetaData { get; set; }

        public override string ToString()
        {
            return FilePath;
        }
    }
}
