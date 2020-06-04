using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Gatosyocora.VRCPhotoAlbum.Models
{
    class Photo
    {
        public string FilePath { get; set; }
        public Image OriginalImage { get; set; }

        public override string ToString()
        {
            return FilePath;
        }
    }
}
