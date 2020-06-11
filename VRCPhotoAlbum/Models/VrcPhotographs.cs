using Gatosyocora.VRCPhotoAlbum.Helpers;
using KoyashiroKohaku.VrcMetaTool;
using Reactive.Bindings;
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
    public class VrcPhotographs
    {
        public ReactiveCollection<Photo> Collection { get; }

        private static BitmapImage _failedImage => new BitmapImage(new Uri(@"pack://application:,,,/Resources/noloading.png"));

        public VrcPhotographs()
        {
            Collection = new ReactiveCollection<Photo>();
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
                Collection.AddRangeOnScheduler(await LoadVRCPhotoListAsync(folderPath));
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
        private Task<Photo[]> LoadVRCPhotoListAsync(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                throw new ArgumentException($"{folderPath} is not exist.");
            }

            return Task.WhenAll(Directory.GetFiles(folderPath, "*.png", SearchOption.AllDirectories)
                        .Where(x => !x.StartsWith(Cache.Instance.CacheFolderPath))
                        .Select(async filePath =>
                        new Photo
                        {
                            FilePath = filePath,
                            ThumbnailImage = await GetThumbnailImage(filePath),
                            MetaData = GetVrcMetaData(filePath)
                        })
                        .ToList());
        }

        private async Task<BitmapImage> GetThumbnailImage(string filePath)
        {
            BitmapImage image;
            try
            {
                image = await ImageHelper.GetThumbnailImageAsync(filePath, Cache.Instance.CacheFolderPath);
            }
            catch (Exception)
            {
                image = _failedImage;
            }
            return image;
        }

        private VrcMetaData GetVrcMetaData(string filePath)
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
        }
    }
}
