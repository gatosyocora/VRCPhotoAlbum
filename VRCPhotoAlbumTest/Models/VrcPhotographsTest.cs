using Gatosyocora.VRCPhotoAlbum.Models;
using Gatosyocora.VRCPhotoAlbum.Servisies;
using KoyashiroKohaku.VrcMetaTool;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using User = KoyashiroKohaku.VrcMetaTool.User;

namespace VRCPhotoAlbumTest.Models
{
    public class DBCacheServiceMockNotExistInDB : IDBCacheService
    {
        public void CreateDBCacheIfNeeded() { }

        public List<(string filePath, VrcMetaData vrcMetaData)> GetVrcMetaDataIfExists(IEnumerable<string> filePaths)
        {
            return Enumerable.Empty<(string, VrcMetaData)>().ToList();
        }

        public Task InsertAsync(string filePath, VrcMetaData metaData, CancellationToken cancelToken)
        {
            return Task.Delay(100);
        }

        public void SaveChanges() { }
    }

    public class DBCacheServiceMockExistInDB : IDBCacheService
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
            var vrcPhotographs = new VrcPhotographs(new DBCacheServiceMockNotExistInDB());
            Assert.IsNotNull(vrcPhotographs);
        }

        [TestMethod("通常の写真の読み込みができる(すべてDBへの登録なし)")]
        public async Task CanLoadNormalPhotographsNotExistsDB()
        {
            var vrcPhotographs = new VrcPhotographs(new DBCacheServiceMockNotExistInDB());
            var filePaths = Directory.GetFiles(@"../../../Resources/NormalPhotos", "*.png");
            vrcPhotographs.Collection.ObserveAddChanged()
                .Select(x => x.FilePath)
                .Subscribe(f =>
                {
                    Assert.IsTrue(filePaths.Contains(f));
                });
            await vrcPhotographs.LoadVRCPhotoListAsync(@"../../../Resources/NormalPhotos", new CancellationToken());
        }

        [TestMethod("通常の写真の読み込みができる(すべてDBに登録済み)")]
        public async Task CanLoadNormalPhotographsExistsDB()
        {
            var vrcPhotographs = new VrcPhotographs(new DBCacheServiceMockExistInDB());
            var filePaths = Directory.GetFiles(@"../../../Resources/NormalPhotos", "*.png");
            vrcPhotographs.Collection.ObserveAddChanged()
                .Select(x => x.FilePath)
                .Subscribe(f =>
                {
                    Assert.IsTrue(filePaths.Contains(f));
                });
            await vrcPhotographs.LoadVRCPhotoListAsync(@"../../../Resources/NormalPhotos", new CancellationToken());
        }
    }
}
