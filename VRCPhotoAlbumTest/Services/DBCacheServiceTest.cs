using Microsoft.VisualStudio.TestTools.UnitTesting;
using Gatosyocora.VRCPhotoAlbum.Servisies;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Diagnostics;
using System.Linq;

namespace VRCPhotoAlbumTest.Services
{
    [TestClass]
    public class DBCacheServiceTest
    {
        private static readonly string _dbFilePath = "Cache/cache.db";

        [TestMethod("コンストラクタ")]
        public async Task TestAsync()
        {
            /*if (File.Exists(_dbFilePath))
            {
                File.Delete(_dbFilePath);
            }*/

            var dbCecheService = new DBCacheService();

            Assert.IsTrue(File.Exists(_dbFilePath));

            var filePaths = Directory.GetFiles(@"D:\Pictures\VRChat", "*.png", SearchOption.AllDirectories);

            await dbCecheService.CreateDBCacheIfNeededAsync(filePaths);

            var photos = await dbCecheService.GetAllPhotosAsync();

            foreach (var photo in photos)
            {
                Console.WriteLine(photo.FilePath);
            }

            //Assert.IsTrue(photos.Count == 66);
        }

        [TestMethod("DB取得の時間計測")]
        public void MeasureTimeOfFinishedLoadingFromDB()
        {
            var dbCecheService = new DBCacheService();
            var filePaths = Directory.GetFiles(@"D:\Pictures\VRChat", "*.png", SearchOption.AllDirectories);

            var stopwatch1 = new Stopwatch();
            stopwatch1.Start();
            var photos = dbCecheService.Photos
                            .Select(p =>
                                new
                                {
                                    p.FilePath,
                                    p.World,
                                    p.Photographer,
                                    p.Date,
                                    p.PhotoUsers
                                }).ToList();
            stopwatch1.Stop();
            Debug.Print($"{photos.Count}, Time:{stopwatch1.ElapsedMilliseconds} ms");

            return;

            var stopwatch2 = new Stopwatch();
            stopwatch2.Start();
            int count = 0;
            foreach (var filePath in filePaths)
            {
                var meta = dbCecheService.GetVrcMetaDataIfExists(filePath);
                if (meta is null)
                {
                    //Debug.Print($"{filePath} is null");
                    count++;
                }
            }
            stopwatch2.Stop();
            Debug.Print($"PhotoCount:{filePaths.Length-count}/{filePaths.Length}, Time:{stopwatch2.ElapsedMilliseconds} ms");
        }
    }
}
