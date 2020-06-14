using Microsoft.VisualStudio.TestTools.UnitTesting;
using Gatosyocora.VRCPhotoAlbum.Servisies;
using System.IO;
using System.Threading.Tasks;
using System;

namespace VRCPhotoAlbumTest.Services
{
    [TestClass]
    public class DBCacheServiceTest
    {
        private static readonly string _dbFilePath = "Cache/cache.db";

        [TestMethod("コンストラクタ")]
        public async Task TestAsync()
        {
            if (File.Exists(_dbFilePath))
            {
                File.Delete(_dbFilePath);
            }

            var dbCecheService = new DBCacheService();

            Assert.IsTrue(File.Exists(_dbFilePath));

            /*var filePaths = Directory.GetFiles(@"D:\Documents\image", "*.png", SearchOption.AllDirectories);

            await dbCecheService.CreateDBCacheIfNeededAsync(filePaths);

            var photos = await dbCecheService.GetAllPhotosAsync();

            foreach (var photo in photos)
            {
                Console.WriteLine(photo.FilePath);
            }

            Assert.IsTrue(photos.Count == 66);*/
        }
    }
}
