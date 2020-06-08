using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;

namespace Gatosyocora.VRCPhotoAlbum.Models
{
    public class Cache
    {
        public static Cache Instance { get; }

        public string CacheFolderPath { get; }

        static Cache()
        {
            Instance = new Cache();
        }
        public Cache()
        {
            CacheFolderPath = GetCacheFolderPath();

            if (!Directory.Exists(CacheFolderPath))
            {
                Directory.CreateDirectory(CacheFolderPath);
            }
        }

        public void Create() { }

        private string GetCacheFolderPath()
        {
            return $"{Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}{Path.DirectorySeparatorChar}Cache";
        }

        public void DeleteCacheFile(string originalFilePath)
        {
            var cacheFilePath = CacheFolderPath + Path.DirectorySeparatorChar + Path.GetFileName(originalFilePath);
            File.Delete(cacheFilePath);
        }

        public void DeleteCacheFileAll()
        {
            foreach (var file in new DirectoryInfo(CacheFolderPath).GetFiles())
            {
                File.Delete(file.FullName);
            }
        }
    }
}
