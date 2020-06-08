using Gatosyocora.VRCPhotoAlbum.Models;
using KoyashiroKohaku.VrcMetaToolSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gatosyocora.VRCPhotoAlbum.Helpers
{
    public class MetaDataHelper
    {
        public enum UserSortType
        {
            Alphabet, Count
        }

        public static List<string> GetSortedUserList(IList<Photo> photoList, UserSortType sortType)
        {
            var sortedList = photoList
                                .SelectMany(x => x.MetaData?.Users ?? Enumerable.Empty<User>())
                                .Select(u => u.UserName);

            switch (sortType)
            {
                case UserSortType.Alphabet:
                    sortedList = sortedList.Distinct()
                                    .OrderBy(x => x);
                    break;
                case UserSortType.Count:
                    sortedList = sortedList
                                    .GroupBy(x => x)
                                    .OrderByDescending(x => x.Count())
                                    .Select(x => x.Key);
                    break;
                default:
                    break;
            }

            return sortedList.ToList();
            
        }
    }
}
