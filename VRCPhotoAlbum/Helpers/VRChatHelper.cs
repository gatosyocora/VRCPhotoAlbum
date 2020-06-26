using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Gatosyocora.VRCPhotoAlbum.Helpers
{
    public static class VRChatHelper
    {
        public static string GetVRChatPictureFolderPath()
            => $"{Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)}{Path.DirectorySeparatorChar}VRChat";
    }
}
