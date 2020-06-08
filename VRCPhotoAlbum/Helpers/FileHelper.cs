using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Gatosyocora.VRCPhotoAlbum.Helpers
{
    public class FileHelper
    {
        private static readonly string[] _unit = new string[] { "B", "KB", "MB", "GB", "TB" };

        public static long CalcDataSize(string folderPath) => CalcDirectoryDataSize(new DirectoryInfo(folderPath));

        private static long CalcDirectoryDataSize(DirectoryInfo directory) => directory.GetFiles().Sum(f => f.Length) + directory.GetDirectories().Sum(CalcDirectoryDataSize);

        public static string DataSize2String(long dataSize)
        {
            int divCount = 0;
            
            while (dataSize >= 1024)
            {
                dataSize /= 1024;
                divCount++;
            }

            return $"{dataSize} {_unit[divCount]}";
        }
    }
}
