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
                await Task.Run(async () =>
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
                                            var meta = await MetaDataHelper.GetVrcMetaDataAsync(fp, cancelToken).ConfigureAwait(true);
                                            if (cancelToken.IsCancellationRequested) return;
                                            Collection.AddOnScheduler(
                                                new Photo(fp)
                                                {
                                                    MetaData = meta
                                                });

                                            try
                                            {
                                                await _db.InsertAsync(fp, meta).ConfigureAwait(false);
                                            }
                                            catch (InvalidOperationException e)
                                            {
                                                // A second operation started on this context before a previous operation completed. This is usually caused by different threads using the same instance of DbContext. For more information on how to avoid threading issues with DbContext, see https://go.microsoft.com/fwlink/?linkid=2097913.
                                                // The instance of entity type 'Photo' cannot be tracked because another instance with the same key value for {'FilePath'} is already being tracked. When attaching existing entities, ensure that only one entity instance with a given key value is attached. Consider using 'DbContextOptionsBuilder.EnableSensitiveDataLogging' to see the conflicting key values.
                                                FileHelper.OutputErrorLogFile(e);
                                            }
                                            catch (ArgumentException e)
                                            {
                                                FileHelper.OutputErrorLogFile(e);
                                            }
                                            catch (Microsoft.EntityFrameworkCore.DbUpdateException e)
                                            {
                                                // An error occurred while updating the entries. See the inner exception for details.
                                                FileHelper.OutputErrorLogFile(e);
                                            }
                                        }, cancelToken));

                    Debug.Print($"{tasks.Count()}/{filePaths.Count}");

                    foreach (var task in tasks)
                    {
                        if (cancelToken.IsCancellationRequested) return;
                        task.Start();
                    }

                }, cancelToken).ConfigureAwait(true);

                _db.SaveChanges();
            }
            catch (Exception e)
            {
                Debug.Print($"{e.GetType().Name}: {e.Message}");
            }
        }
    }
}
