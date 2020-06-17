using Gatosyocora.VRCPhotoAlbum.Models;
using Gatosyocora.VRCPhotoAlbum.Servisies;
using KoyashiroKohaku.VrcMetaTool;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using User = KoyashiroKohaku.VrcMetaTool.User;

namespace VRCPhotoAlbumTest.Models
{
    public class DBCacheServiceMock : IDBCacheService
    {
        public void CreateDBCacheIfNeeded(){}

        public List<(string filePath, VrcMetaData vrcMetaData)> GetVrcMetaDataIfExists(IEnumerable<string> filePaths)
        {
            return filePaths
                    .Select((filePath, offset) => 
                    {
                        var meta = new VrcMetaData
                        {
                            Photographer = $"user{offset}",
                            Date = DateTime.Now.AddDays(-offset),
                            World = $"world{offset}"
                        };
                        meta.Users.AddRange(
                            Enumerable.Range(0, offset + 1)
                                .Select(i => new User($"user{i}")
                                {
                                    TwitterScreenName = "@user{i}"
                                }));

                        return (filePath, meta);
                    })
                    .ToList();
        }

        public Task InsertAsync(string filePath, VrcMetaData metaData, CancellationToken cancelToken)
        {
            return Task.Delay(100);
        }

        public void SaveChanges(){}
    }

    [TestClass]
    public class VrcPhotographsTest
    {
        [TestMethod("インスタンスが作成できる")]
        public void CanCreateInstance()
        {
            var vrcPhotographs = new VrcPhotographs(new DBCacheServiceMock());
            Assert.IsNotNull(vrcPhotographs);
        }

        [TestMethod("通常の写真の読み込みができる")]
        public async Task CanLoadNormalPhotographs()
        {
            var vrcPhotographs = new VrcPhotographs(new DBCacheServiceMock());
            var filePaths = Directory.GetFiles(@"../../../Resources/NormalPhotos", "*.png");
            await vrcPhotographs.LoadVRCPhotoListAsync(@"../../../Resources/NormalPhotos", new CancellationToken());

            var loadedFilePaths = vrcPhotographs.Collection.Select(x => x.FilePath);
            foreach (var filePath in filePaths){
                Assert.IsTrue(loadedFilePaths.Contains(filePath));
            }
        }
    }
}
