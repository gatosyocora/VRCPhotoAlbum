using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Gatosyocora.VRCPhotoAlbum.Models
{
    public class AppCache
    {
        public static AppCache Instance { get; }

        public string CacheFolderPath { get; }

        static AppCache()
        {
            Instance = new AppCache();
        }
        public AppCache()
        {
            CacheFolderPath = GetCacheFolderPath();

            if (!Directory.Exists(CacheFolderPath))
            {
                Directory.CreateDirectory(CacheFolderPath);
            }
        }

        private string GetCacheFolderPath()
        {
            return $"{Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}{Path.DirectorySeparatorChar}Cache";
        }

        public void DeleteCacheFile(string originalFilePath)
        {
            var cacheFilePath = $"{CacheFolderPath}{Path.DirectorySeparatorChar}{Path.GetFileNameWithoutExtension(originalFilePath)}.jpg";
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
