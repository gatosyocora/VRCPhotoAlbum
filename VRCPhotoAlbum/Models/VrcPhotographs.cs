using Gatosyocora.VRCPhotoAlbum.Helpers;
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

        public VrcPhotographs()
        {
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

                    var tasks = filePaths.Select(fp =>
                        new Task(async () =>
                        {
                            Collection.AddOnScheduler(
                                new Photo(fp)
                                {
                                    MetaData = await GetVrcMetaDataAsync(fp, cancelToken)
                                });
                        }, cancelToken));

                    foreach (var task in tasks)
                    {
                        if (cancelToken.IsCancellationRequested) return;
                        task.Start();
                    }
                }, cancelToken);
            }
            catch (Exception e)
            {
                Debug.Print($"{e.GetType().Name}: {e.Message}");
            }
        }

        private VrcMetaData GetVrcMetaData(string filePath)
        {
            //return new VrcMetaData();
            if (!VrcMetaDataReader.TryRead(filePath, out VrcMetaData meta))
            {
                var vrcPhotoMatch = Regex.Match(filePath,
                        @".*(VRChat|screen)_[0-9]+x[0-9]+_(?<datetime>[0-9]{4}-[0-9]{2}-[0-9]{2}_[0-9]{2}-[0-9]{2}-[0-9]{2}.[0-9]{3}).png$");
                if (vrcPhotoMatch.Success)
                {
                    if (DateTime.TryParseExact($"{vrcPhotoMatch.Groups["datetime"]}",
                                                "yyyy-MM-dd_HH-mm-ss.fff",
                                                new CultureInfo("en", false),
                                                DateTimeStyles.None,
                                                out DateTime date))
                    {
                        meta = new VrcMetaData
                        {
                            Date = date
                        };
                    }
                }
            }
            return meta;
        }

        private Task<VrcMetaData> GetVrcMetaDataAsync(string filePath, CancellationToken cancelToken) => Task.Run(() => GetVrcMetaData(filePath), cancelToken);
    }
}
