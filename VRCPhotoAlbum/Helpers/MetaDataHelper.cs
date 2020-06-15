using KoyashiroKohaku.VrcMetaTool;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Gatosyocora.VRCPhotoAlbum.Helpers
{
    public static class MetaDataHelper
    {
        public static DateTime? GetDateTimeFromPhotoName(string photoName)
        {
            var vrcPhotoMatch = Regex.Match(photoName,
                                    @".*(VRChat|screen|vrchat)_[0-9]+x[0-9]+_(?<datetime>[0-9]{4}-[0-9]{2}-[0-9]{2}_[0-9]{2}-[0-9]{2}-[0-9]{2}.[0-9]{3}).png$");

            if(DateTime.TryParseExact(
                $"{vrcPhotoMatch.Groups["datetime"]}",
                "yyyy-MM-dd_HH-mm-ss.fff",
                new CultureInfo("en", false),
                DateTimeStyles.None,
                out DateTime date))
            {
                return date;
            }
            else 
            {
                return null;
            }
        }

        public static VrcMetaData GetVrcMetaData(string filePath)
        {
            //return new VrcMetaData();
            if (!VrcMetaDataReader.TryRead(filePath, out VrcMetaData meta))
            {
                meta = new VrcMetaData
                {
                    Date = GetDateTimeFromPhotoName(filePath)
                };
            }
            return meta;
        }

        public static Task<VrcMetaData> GetVrcMetaDataAsync(string filePath, CancellationToken cancelToken) => Task.Run(() => GetVrcMetaData(filePath), cancelToken);
    }
}
