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
        /// 非同期でデータを読み込む
        /// </summary>
        /// <returns></returns>
        public async Task LoadResourcesAsync(string folderPath)
        {
            Collection.ClearOnScheduler();
            try
            {
                foreach (var photo in LoadVRCPhotoListAsync(folderPath))
                {
                    Collection.AddOnScheduler(await photo);
                }
            }
            catch (Exception e)
            {
                Debug.Print($"{e.GetType().Name}: {e.Message}");
            }
        }

        /// <summary>
        /// 非同期で画像を読み込む
        /// </summary>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        private IEnumerable<Task<Photo>> LoadVRCPhotoListAsync(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                throw new ArgumentException($"{folderPath} is not exist.");
            }

            return Directory.GetFiles(folderPath, "*.png", SearchOption.AllDirectories)
                .Where(x => !x.StartsWith(Cache.Instance.CacheFolderPath))
                .Select(async filePath =>
                    new Photo
                    {
                        FilePath = filePath,
                        MetaData = await GetVrcMetaDataAsync(filePath)
                    });
        }

        private async Task<VrcMetaData> GetVrcMetaDataAsync(string filePath)
        {
            return await Task.Run(() =>
            {
                if (!VrcMetaDataReader.TryRead(filePath, out VrcMetaData meta))
                {
                    var vrcPhotoMatch = Regex.Match(filePath,
                            @".*VRChat_[0-9]+x[0-9]+_(?<datetime>[0-9]{4}-[0-9]{2}-[0-9]{2}_[0-9]{2}-[0-9]{2}-[0-9]{2}.[0-9]{3}).png$");
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
            });
        }
    }
}
