using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Gatosyocora.VRCPhotoAlbum.Models
{
    public class SearchResult : ModelBase
    {
        public ReadOnlyReactiveCollection<Photo> ShowedPhotoList { get; }
        public ReactiveProperty<string> SearchText { get; }

        private ReadOnlyReactiveCollection<Photo> _photoList;

        public ReactiveProperty<string> SearchedUserName { get; }
        public ReactiveProperty<string> SearchedWorldName { get; }
        public ReactiveProperty<DateTime> SearchedDate { get; }
        public ReactiveProperty<DateTime> SearchedSinceDate { get; }
        public ReactiveProperty<DateTime> SearchedUntilDate{ get; }

        public SearchResult(ReactiveCollection<Photo> photoList)
        {
            SearchText = new ReactiveProperty<string>(string.Empty).AddTo(Disposable);

            SearchedUserName = new ReactiveProperty<string>().AddTo(Disposable);
            SearchedWorldName = new ReactiveProperty<string>().AddTo(Disposable);
            SearchedDate = new ReactiveProperty<DateTime>().AddTo(Disposable);
            SearchedSinceDate = new ReactiveProperty<DateTime>().AddTo(Disposable);
            SearchedUntilDate = new ReactiveProperty<DateTime>().AddTo(Disposable);

            SearchedUserName.Subscribe(SearchWithUserName).AddTo(Disposable);
            SearchedWorldName.Subscribe(SearchWithWorldName).AddTo(Disposable);
            SearchedDate.Subscribe(SearchWithDate).AddTo(Disposable);
            SearchedSinceDate.Subscribe(d => SearchWithDatePeriod(d, SearchedUntilDate.Value)).AddTo(Disposable);
            SearchedUntilDate.Subscribe(d => SearchWithDatePeriod(SearchedUntilDate.Value, d)).AddTo(Disposable);

            _photoList = photoList.ObserveAddChanged()
                            .Select(p => p)
                            .ToReadOnlyReactiveCollection()
                            .AddTo(Disposable);

            ShowedPhotoList = SearchText
                                .SelectMany(_ => SearchPhoto(SearchText?.Value ?? string.Empty))
                                .ToReadOnlyReactiveCollection(onReset: SearchText.Select(_ => Unit.Default))
                                .AddTo(Disposable);

            SearchedDate.Subscribe(d => SearchWithDateString(d.ToString("yyyy/MM/dd HH:mm:ss")));
        }

        private IEnumerable<Photo> SearchPhoto(string searchText)
        {
            string searchUserName, searchWorldName, searchDateString, searchSinceDateString, searchUntilDateString;
            var userMatch = Regex.Match(searchText, @".*user:""(?<userName>.*?)"".*");
            var worldMatch = Regex.Match(searchText, @".*world:""(?<worldName>.*?)"".*");
            var dateMatch = Regex.Match(searchText, @".*date:""(?<dateString>.*?)"".*");
            var sinceDateMatch = Regex.Match(searchText, @".*since:""(?<dateString>.*?)"".*");
            var untilDateMatch = Regex.Match(searchText, @".*until:""(?<dateString>.*?)"".*");
            if (userMatch.Success)
            {
                searchUserName = $"{userMatch.Groups["userName"]}";
            }
            else
            {
                searchUserName = Regex.Replace(searchText, @"\s*world:"".*?""\s*", string.Empty);
                searchUserName = Regex.Replace(searchUserName, @"\s*date:"".*?""\s*", string.Empty);
                searchUserName = Regex.Replace(searchUserName, @"\s*since:"".*?""\s*", string.Empty);
                searchUserName = Regex.Replace(searchUserName, @"\s*until:"".*?""\s*", string.Empty);
            }

            if (worldMatch.Success)
            {
                searchWorldName = $"{worldMatch.Groups["worldName"]}";
            }
            else
            {
                searchWorldName = Regex.Replace(searchText, @"\s*user:"".*?""\s*", string.Empty);
                searchWorldName = Regex.Replace(searchWorldName, @"\s*date:"".*?""\s*", string.Empty);
                searchWorldName = Regex.Replace(searchWorldName, @"\s*since:"".*?""\s*", string.Empty);
                searchWorldName = Regex.Replace(searchWorldName, @"\s*until:"".*?""\s*", string.Empty);
            }

            if (dateMatch.Success)
            {
                searchDateString = $"{dateMatch.Groups["dateString"]}";
            }
            else
            {
                searchDateString = string.Empty;
            }

            if (sinceDateMatch.Success)
            {
                searchSinceDateString = $"{sinceDateMatch.Groups["dateString"]}";
            }
            else
            {
                searchSinceDateString = string.Empty;
            }

            if (untilDateMatch.Success)
            {
                searchUntilDateString = $"{untilDateMatch.Groups["dateString"]}";
            }
            else
            {
                searchUntilDateString = string.Empty;
            }

            var searchedPhotoList = _photoList.Select(x => x);

            if (dateMatch.Success && (!sinceDateMatch.Success && !untilDateMatch.Success))
            {
                var searchDate = DateTime.Parse(searchDateString).Date;
                searchedPhotoList = searchedPhotoList
                                        .Where(x => (x.MetaData?.Date?.Date.CompareTo(searchDate) ?? 1) == 0);
            }
            else
            {
                if (sinceDateMatch.Success)
                {
                    var searchSinceDate = DateTime.Parse(searchSinceDateString).Date;
                    searchedPhotoList = searchedPhotoList
                                            .Where(x => (x.MetaData?.Date?.Date.CompareTo(searchSinceDate) ?? -1) >= 0);
                }

                if (untilDateMatch.Success)
                {
                    var searchUntilDate = DateTime.Parse(searchUntilDateString).Date;
                    searchedPhotoList = searchedPhotoList
                                            .Where(x => (x.MetaData?.Date?.Date.CompareTo(searchUntilDate) ?? 1) <= 0);
                }
            }

            if (!userMatch.Success && !worldMatch.Success)
            {
                searchedPhotoList = searchedPhotoList
                                        .Where(x => (x?.MetaData?.Users?.Any(u => u.UserName.ToLower().StartsWith(searchUserName.ToLower())) ?? false) ||
                                                    (x?.MetaData?.World?.ToLower().Contains(searchWorldName.ToLower()) ?? false));
            }
            else
            {
                searchedPhotoList = searchedPhotoList
                                        .Where(x => (x?.MetaData?.Users?.Any(u => u.UserName.ToLower().StartsWith(searchUserName.ToLower())) ?? false) &&
                                                    (x?.MetaData?.World?.ToLower().Contains(searchWorldName.ToLower()) ?? false));
            }

            return searchedPhotoList;
        }

        public void SearchWithUserName(string userName)
        {
            if (string.IsNullOrEmpty(userName)) return;

            var userMatch = Regex.Match(SearchText.Value, @"(?<prefix>.*user:"")(?<userName>.*?)(?<suffix>"".*)");

            if (userMatch.Success)
            {
                SearchText.Value = $"{userMatch.Groups["prefix"]}{userName}{userMatch.Groups["suffix"]}";
            }
            else
            {
                var space = string.Empty;
                if (!string.IsNullOrEmpty(SearchText.Value))
                {
                    space = " ";
                }
                SearchText.Value += $@"{space}user:""{userName}""";
            }
        }

        public void SearchWithWorldName(string worldName)
        {
            if (string.IsNullOrEmpty(worldName)) return;

            var worldMatch = Regex.Match(SearchText.Value, @"(?<prefix>.*world:"")(?<userName>.*?)(?<suffix>"".*)");

            if (worldMatch.Success)
            {
                SearchText.Value = $"{worldMatch.Groups["prefix"]}{worldName}{worldMatch.Groups["suffix"]}";
            }
            else
            {
                var space = string.Empty;
                if (!string.IsNullOrEmpty(SearchText.Value))
                {
                    space = " ";
                }
                SearchText.Value += $@"{space}world:""{worldName}""";
            }
        }

        public void SearchWithDateString(string dateString)
        {
            if (string.IsNullOrEmpty(dateString) || dateString == "0001/01/01 00:00:00") return;

            SearchedDate.Value = DateTime.Parse(dateString).Date;

            dateString = SearchedDate.Value.ToString("yyyy-MM-dd");

            var searchText = Regex.Replace(SearchText?.Value ?? string.Empty, @"\s*since:"".*?""\s*", string.Empty);
            searchText = Regex.Replace(searchText, @"\s*until:"".*?""\s*", string.Empty);

            var dateMatch = Regex.Match(searchText, @"(?<prefix>.*date:"")(?<dateString>.*?)(?<suffix>"".*)");

            if (dateMatch.Success)
            {
                SearchText.Value = $"{dateMatch.Groups["prefix"]}{dateString}{dateMatch.Groups["suffix"]}";
            }
            else
            {
                var space = string.Empty;
                if (!string.IsNullOrEmpty(searchText))
                {
                    space = " ";
                }
                SearchText.Value = $@"{searchText}{space}date:""{dateString}""";
            }
        }

        public void SearchWithDate(DateTime dateTime)
        {
            if (dateTime.Date.CompareTo(new DateTime().Date) == 0) return;

            var dateString = dateTime.Date.ToString("yyyy-MM-dd");

            var searchText = Regex.Replace(SearchText?.Value ?? string.Empty, @"\s*since:"".*?""\s*", string.Empty);
            searchText = Regex.Replace(searchText, @"\s*until:"".*?""\s*", string.Empty);

            var dateMatch = Regex.Match(searchText, @"(?<prefix>.*date:"")(?<dateString>.*?)(?<suffix>"".*)");

            if (dateMatch.Success)
            {
                SearchText.Value = $"{dateMatch.Groups["prefix"]}{dateString}{dateMatch.Groups["suffix"]}";
            }
            else
            {
                var space = string.Empty;
                if (!string.IsNullOrEmpty(searchText))
                {
                    space = " ";
                }
                SearchText.Value = $@"{searchText}{space}date:""{dateString}""";
            }
        }

        public void SearchWithDatePeriodString(string sinceDateString, string untilDateString)
        {
            if (string.IsNullOrEmpty(sinceDateString) || string.IsNullOrEmpty(untilDateString) ||
                sinceDateString == "0001/01/01 00:00:00" || untilDateString == "0001/01/01 00:00:00") return;

            sinceDateString = DateTime.Parse(sinceDateString).Date.ToString("yyyy-MM-dd");
            untilDateString = DateTime.Parse(untilDateString).Date.ToString("yyyy-MM-dd");

            var searchText = Regex.Replace(SearchText.Value, @"\s*date:"".*?""\s*", string.Empty);

            var sinceDateMatch = Regex.Match(searchText, @"(?<prefix>.*since:"")(?<dateString>.*?)(?<suffix>"".*)");

            string searchTextWithSinceDate;
            if (sinceDateMatch.Success)
            {
                searchTextWithSinceDate = $"{sinceDateMatch.Groups["prefix"]}{sinceDateString}{sinceDateMatch.Groups["suffix"]}";
            }
            else
            {
                var space = string.Empty;
                if (!string.IsNullOrEmpty(searchText))
                {
                    space = " ";
                }
                searchTextWithSinceDate = $@"{searchText}{space}since:""{sinceDateString}""";
            }

            var untilDateMatch = Regex.Match(searchTextWithSinceDate, @"(?<prefix>.*until:"")(?<dateString>.*?)(?<suffix>"".*)");

            if (untilDateMatch.Success)
            {
                SearchText.Value = $"{untilDateMatch.Groups["prefix"]}{untilDateString}{untilDateMatch.Groups["suffix"]}";
            }
            else
            {
                var space = string.Empty;
                if (!string.IsNullOrEmpty(searchTextWithSinceDate))
                {
                    space = " ";
                }
                SearchText.Value = $@"{searchTextWithSinceDate}{space}until:""{untilDateString}""";
            }
        }

        public void SearchWithDatePeriod(DateTime sinceDate, DateTime untilDate)
        {
            var shouldSearchSinceDate = (sinceDate.Date.CompareTo(new DateTime().Date) != 0);
            var shouldSearchUntilDate = (untilDate.Date.CompareTo(new DateTime().Date) != 0);

            if (!shouldSearchSinceDate && !shouldSearchUntilDate) return;

            var sinceDateString = sinceDate.Date.ToString("yyyy-MM-dd");
            var untilDateString = untilDate.Date.ToString("yyyy-MM-dd");

            var searchText = Regex.Replace(SearchText.Value, @"\s*date:"".*?""\s*", string.Empty);

            var sinceDateMatch = Regex.Match(searchText, @"(?<prefix>.*since:"")(?<dateString>.*?)(?<suffix>"".*)");

            string searchTextWithSinceDate = string.Empty;
            if (sinceDateMatch.Success)
            {
                searchTextWithSinceDate = $"{sinceDateMatch.Groups["prefix"]}{sinceDateString}{sinceDateMatch.Groups["suffix"]}";
            }
            else
            {
                var space = string.Empty;
                if (!string.IsNullOrEmpty(searchText))
                {
                    space = " ";
                }
                searchTextWithSinceDate = $@"{searchText}{space}since:""{sinceDateString}""";
            }

            var untilDateMatch = Regex.Match(searchTextWithSinceDate, @"(?<prefix>.*until:"")(?<dateString>.*?)(?<suffix>"".*)");

            if (untilDateMatch.Success)
            {
                SearchText.Value = $"{untilDateMatch.Groups["prefix"]}{untilDateString}{untilDateMatch.Groups["suffix"]}";
            }
            else
            {
                var space = string.Empty;
                if (!string.IsNullOrEmpty(searchTextWithSinceDate))
                {
                    space = " ";
                }
                SearchText.Value = $@"{searchTextWithSinceDate}{space}until:""{untilDateString}""";
            }
        }
    }
}
