using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.RegularExpressions;

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
        public ReactiveProperty<DateTime> SearchedUntilDate { get; }

        public ReactiveCommand ResearchCommand { get; }
        public ReactiveCommand ResetCommand { get; }

        private DateTime _defaultDate;

        private bool _IsSearching = false;

        public SearchResult(ReactiveCollection<Photo> photoList)
        {
            _defaultDate = new DateTime();

            SearchText = new ReactiveProperty<string>(string.Empty).AddTo(Disposable);

            SearchedUserName = new ReactiveProperty<string>(string.Empty).AddTo(Disposable);
            SearchedWorldName = new ReactiveProperty<string>(string.Empty).AddTo(Disposable);
            SearchedDate = new ReactiveProperty<DateTime>(_defaultDate).AddTo(Disposable);
            SearchedSinceDate = new ReactiveProperty<DateTime>(_defaultDate).AddTo(Disposable);
            SearchedUntilDate = new ReactiveProperty<DateTime>(_defaultDate).AddTo(Disposable);

            ResearchCommand = new ReactiveCommand().AddTo(Disposable);
            ResetCommand = new ReactiveCommand().AddTo(Disposable);

            SearchedUserName.Subscribe(u => { if (!_IsSearching) { SearchWithUserName(u); } }).AddTo(Disposable);
            SearchedWorldName.Subscribe(w => { if (!_IsSearching) { SearchWithWorldName(w); } }).AddTo(Disposable);
            SearchedDate.Subscribe(d => { if (!_IsSearching) { SearchWithDate(d); } }).AddTo(Disposable);
            SearchedSinceDate.Subscribe(d => { if (!_IsSearching) { SearchWithDatePeriod(d, SearchedUntilDate.Value); } }).AddTo(Disposable);
            SearchedUntilDate.Subscribe(d => { if (!_IsSearching) { SearchWithDatePeriod(SearchedUntilDate.Value, d); } }).AddTo(Disposable);

            _photoList = photoList.ObserveAddChanged()
                            .Select(p => p)
                            .ToReadOnlyReactiveCollection(
                                onReset: ResetCommand.Select(_ => Unit.Default)
                            )
                            .AddTo(Disposable);

            // 読み込み時は_photoList.ObserveAddChanged()で1つずつPhotoが追加されていく
            ShowedPhotoList = Observable.Merge(
                                    _photoList.ObserveAddChanged(),
                                    SearchText,
                                    ResearchCommand)
                                .SelectMany(x =>
                                {
                                    if (x is Photo photo)
                                    {
                                        return new Photo[] { photo };
                                    }
                                    else
                                    {
                                        return SearchPhoto(SearchText?.Value ?? string.Empty);
                                    }
                                })
                                .ToReadOnlyReactiveCollection(
                                    onReset: Observable.Merge(
                                                SearchText,
                                                ResearchCommand,
                                                ResetCommand)
                                            .Select(_ => Unit.Default))
                                .AddTo(Disposable);
        }

        private IEnumerable<Photo> SearchPhoto(string searchText)
        {
            // 最初と最後にスペースが連なっているものはString.Emptyと同じ扱いにする
            searchText = Regex.Replace(searchText, @"^\s*", string.Empty);
            searchText = Regex.Replace(searchText, @"\s*$", string.Empty);

            if (!searchText.Any()) return _photoList;

            _IsSearching = true;

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

                SearchedUserName.Value = searchUserName;
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

                SearchedWorldName.Value = searchWorldName;
            }

            if (dateMatch.Success)
            {
                searchDateString = $"{dateMatch.Groups["dateString"]}";
            }
            else
            {
                searchDateString = string.Empty;
                SearchedDate.Value = _defaultDate;
            }

            if (sinceDateMatch.Success)
            {
                searchSinceDateString = $"{sinceDateMatch.Groups["dateString"]}";
            }
            else
            {
                searchSinceDateString = string.Empty;
                SearchedSinceDate.Value = _defaultDate;
            }

            if (untilDateMatch.Success)
            {
                searchUntilDateString = $"{untilDateMatch.Groups["dateString"]}";
            }
            else
            {
                searchUntilDateString = string.Empty;
                SearchedUntilDate.Value = _defaultDate;
            }

            var searchedPhotoList = _photoList.Select(x => x);

            if (dateMatch.Success && (!sinceDateMatch.Success && !untilDateMatch.Success))
            {
                if (DateTime.TryParse(searchDateString, out var searchDate))
                {
                    searchedPhotoList = searchedPhotoList
                        .Where(x => (x.MetaData?.Date?.Date.CompareTo(searchDate) ?? 1) == 0);
                }
            }
            else
            {
                if (sinceDateMatch.Success)
                {
                    if (DateTime.TryParse(searchSinceDateString, out var searchSinceDate))
                    {
                        searchedPhotoList = searchedPhotoList
                            .Where(x => (x.MetaData?.Date?.Date.CompareTo(searchSinceDate.Date) ?? -1) >= 0);
                    }
                }

                if (untilDateMatch.Success)
                {
                    if (DateTime.TryParse(searchUntilDateString, out var searchUntilDate))
                    {
                        searchedPhotoList = searchedPhotoList
                            .Where(x => (x.MetaData?.Date?.Date.CompareTo(searchUntilDate.Date) ?? 1) <= 0);
                    }
                }
            }

            // ユーザーでもワールドでも検索していない場合
            if (!userMatch.Success && !worldMatch.Success)
            {
                // UsersとWorldがnullでDateがnullでない写真はファイル名Dateな写真なので無条件で通す
                searchedPhotoList = searchedPhotoList
                                        .Where(x => (x?.MetaData?.Users?.Count() <= 0 && x?.MetaData?.World is null && !(x?.MetaData?.Date is null)) ||
                                                    (x?.MetaData?.Users?.Any(u => u.UserName.ToLower().StartsWith(searchUserName.ToLower())) ?? false) ||
                                                    (x?.MetaData?.World?.ToLower().Contains(searchWorldName.ToLower()) ?? false));
            }
            else
            {
                // UsersとWorldがnullでDateがnullでない写真はファイル名Dateな写真なので無条件で通す
                searchedPhotoList = searchedPhotoList
                                        .Where(x => (x?.MetaData?.Users?.Count() <= 0 && x?.MetaData?.World is null && !(x?.MetaData?.Date is null)) ||
                                                    ((x?.MetaData?.Users?.Any(u => u.UserName.ToLower().StartsWith(searchUserName.ToLower())) ?? false) &&
                                                    (x?.MetaData?.World?.ToLower().Contains(searchWorldName.ToLower()) ?? false)));
            }

            _IsSearching = false;

            return searchedPhotoList;
        }

        public void SearchWithTemplate(string name, string type)
        {
            if (string.IsNullOrEmpty(name)) return;

            // プレフィックスつきで既に入力されているか調べる
            var prefixMatch = Regex.Match(SearchText.Value, @$"(?<prefix>.*{type}:"")(?<name>.*?)(?<suffix>"".*)");
            if (prefixMatch.Success)
            {
                // 内側だけ差し替える
                SearchText.Value = $"{prefixMatch.Groups["prefix"]}{name}{prefixMatch.Groups["suffix"]}";
            }
            else
            {
                var searchText = SearchText.Value;

                // プレフィックスつき以外で入力されているか調べる
                var freeMatch = Regex.Match(searchText, @$"\s*{name}\s*");
                if (freeMatch.Success)
                {
                    searchText = searchText.Replace(freeMatch.Value, string.Empty);
                }

                // 既に何か入力されていたらスペースをいれて入力する
                SearchText.Value = $@"{searchText}{(searchText.Any() ? " " : string.Empty)}{type}:""{name}""";
            }
        }

        public void SearchWithUserName(string userName) => SearchWithTemplate(userName, "user");
        public void SearchWithWorldName(string worldName) => SearchWithTemplate(worldName, "world");

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
            // TODO: ここの判定が甘い. 0001/01/01 00:00:00でないほうがあればそれだけで検索をしたい
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

            // TODO: ここの判定が甘い. 0001/01/01 00:00:00でないほうがあればそれだけで検索をしたい
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
