using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using KoyashiroKohaku.VrcMetaToolSharp;

namespace Gatosyocora.VRCPhotoAlbum.Models
{
    class Photo
    {
        public string FilePath { get; set; }
        public Image OriginalImage { get; set; }

        public VrcMetaData MetaData { get; set; }

        public override string ToString()
        {
            return FilePath;
        }
    }
}
