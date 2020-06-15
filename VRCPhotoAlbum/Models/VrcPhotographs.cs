using Gatosyocora.VRCPhotoAlbum.Helpers;
using Gatosyocora.VRCPhotoAlbum.Servisies;
using KoyashiroKohaku.VrcMetaTool;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Gatosyocora.VRCPhotoAlbum.Models
{
    public class VrcPhotographs : ModelBase
    {
        public ReactiveCollection<Photo> Collection { get; }

        private DBCacheService _db;

        public VrcPhotographs(DBCacheService db)
        {
            _db = db;
            Collection = new ReactiveCollection<Photo>().AddTo(Disposable);
        }

        /// <summary>
        /// 非同期で画像を読み込む
        /// </summary>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        public async Task LoadVRCPhotoListAsync(string folderPath, CancellationToken cancelToken)
        {
            if (!Directory.Exists(folderPath))
            {
                throw new DirectoryNotFoundException($"{folderPath} is not exist.");
            }

            Collection.Clear();

            try
            {
                // UIスレッドと分離させる
                await Task.Run(() =>
                {
                    var filePaths = Directory.GetFiles(folderPath, "*.png", SearchOption.AllDirectories)
                                        .Where(x => !x.StartsWith(AppCache.Instance.CacheFolderPath, StringComparison.Ordinal))
                                        .ToList();

                    var metaSets = _db.GetVrcMetaDataIfExists(filePaths);

                    var photos = metaSets
                                    .Select(m =>
                                    new Photo(m.filePath)
                                    {
                                        MetaData = m.vrcMetaData
                                    })
                                    .ToList();

                    Collection.AddRangeOnScheduler(photos);
           
                    var tasks = filePaths
                                    .Except(metaSets.Select(m => m.filePath))
                                    .Select(fp =>
                                        new Task(async () =>
                                        {
                                            var meta = await GetVrcMetaDataAsync(fp, cancelToken).ConfigureAwait(true);

                                            Collection.AddOnScheduler(
                                                new Photo(fp)
                                                {
                                                    MetaData = meta
                                                });

                                            await _db.InsertAsync(fp, meta).ConfigureAwait(false);

                                        }, cancelToken));

                    foreach (var task in tasks)
                    {
                        if (cancelToken.IsCancellationRequested) return;
                        task.Start();
                    }
                }, cancelToken).ConfigureAwait(true);
            }
            catch (Exception e)
            {
                Debug.Print($"{e.GetType().Name}: {e.Message}");
            }
        }

        private static VrcMetaData GetVrcMetaData(string filePath)
        {
            //return new VrcMetaData();
            if (!VrcMetaDataReader.TryRead(filePath, out VrcMetaData meta))
            {
                meta = new VrcMetaData
                {
                    Date = MetaDataHelper.GetDateTimeFromPhotoName(filePath)
                };
            }
            return meta;
        }

        private static Task<VrcMetaData> GetVrcMetaDataAsync(string filePath, CancellationToken cancelToken) => Task.Run(() => GetVrcMetaData(filePath), cancelToken);
    }
}
