using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Media.Imaging;
using KoyashiroKohaku.VrcMetaToolSharp;

namespace Gatosyocora.VRCPhotoAlbum.Models
{
    public class Photo
    {
        public string FilePath { get; set; }
        public BitmapImage OriginalImage { get; set; }

        public VrcMetaData MetaData { get; set; }

        public override string ToString()
        {
            return FilePath;
        }
    }
}
