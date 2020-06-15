using Gatosyocora.VRCPhotoAlbum.Views;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
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

        private static readonly KeyValuePair<string, DatePeriod>[] _specialSearchs = new KeyValuePair<string, DatePeriod>[]
        {
            new KeyValuePair<string, DatePeriod>(@".*\s*(vket1|VKET1|Vket1)\s*.*", new DatePeriod(@"2018/08/26 00:00:00", @"2018/08/28 23:59:59")),
            new KeyValuePair<string, DatePeriod>(@".*\s*(vket2|VKET2|Vket2)\s*.*", new DatePeriod(@"2019/03/08 00:00:00", @"2019/03/10 23:59:59")),
            new KeyValuePair<string, DatePeriod>(@".*\s*(vket3|VKET3|Vket3)\s*.*", new DatePeriod(@"2019/09/21 00:00:00", @"2019/09/28 23:00:00")),
            new KeyValuePair<string, DatePeriod>(@".*\s*(vket4|VKET4|Vket4)\s*.*", new DatePeriod(@"2020/04/29 11:00:00", @"2020/05/10 23:00:00"))
        };

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
                                        if (IsSearchingPhoto(photo))
                                        {
                                            return new Photo[] { photo };
                                        }
                                        else
                                        {
                                            return Enumerable.Empty<Photo>();
                                        }
                                    }
                                    else
                                    {
                                        MainWindow.Instance.ScrollToTopInPhotoList();
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

            bool useSpecialSearch = false;
            foreach (var specialSearch in _specialSearchs)
            {
                if (Regex.IsMatch(searchText, specialSearch.Key))
                {
                    SearchedSinceDate.Value = specialSearch.Value.SinceDate;
                    SearchedUntilDate.Value = specialSearch.Value.UntilDate;
                    useSpecialSearch = true;
                    break;
                }
            }

            string searchUserName, searchWorldName, searchDateString, searchSinceDateString, searchUntilDateString;
            var userMatch = Regex.Match(searchText, @".*user:""(?<userName>.*?)"".*");
            var worldMatch = Regex.Match(searchText, @".*world:""(?<worldName>.*?)"".*");
            var dateMatch = Regex.Match(searchText, @".*date:""(?<dateString>.*?)"".*");
            var sinceDateMatch = Regex.Match(searchText, @".*since:""(?<dateString>.*?)"".*");
            var untilDateMatch = Regex.Match(searchText, @".*until:""(?<dateString>.*?)"".*");
            if (userMatch.Success)
            {
                SearchedUserName.Value = $"{userMatch.Groups["userName"]}";
            }
            else if (!useSpecialSearch)
            {
                searchUserName = Regex.Replace(searchText, @"\s*world:"".*?""\s*", string.Empty);
                searchUserName = Regex.Replace(searchUserName, @"\s*date:"".*?""\s*", string.Empty);
                searchUserName = Regex.Replace(searchUserName, @"\s*since:"".*?""\s*", string.Empty);
                searchUserName = Regex.Replace(searchUserName, @"\s*until:"".*?""\s*", string.Empty);

                SearchedUserName.Value = searchUserName;
            }

            if (worldMatch.Success)
            {
                SearchedWorldName.Value = $"{worldMatch.Groups["worldName"]}";
            }
            else if (!useSpecialSearch)
            {
                searchWorldName = Regex.Replace(searchText, @"\s*user:"".*?""\s*", string.Empty);
                searchWorldName = Regex.Replace(searchWorldName, @"\s*date:"".*?""\s*", string.Empty);
                searchWorldName = Regex.Replace(searchWorldName, @"\s*since:"".*?""\s*", string.Empty);
                searchWorldName = Regex.Replace(searchWorldName, @"\s*until:"".*?""\s*", string.Empty);

                SearchedWorldName.Value = searchWorldName;
            }

            if (dateMatch.Success)
            {
                if (DateTime.TryParse($"{dateMatch.Groups["dateString"]}", out var searchDate))
                {
                    SearchedSinceDate.Value = searchDate;
                }
            }
            else
            {
                SearchedDate.Value = _defaultDate;
            }

            if (sinceDateMatch.Success)
            {
                if (DateTime.TryParse($"{sinceDateMatch.Groups["dateString"]}", out var searchDate))
                {
                    SearchedSinceDate.Value = searchDate;
                }
            }
            else if (!useSpecialSearch)
            {
                SearchedSinceDate.Value = _defaultDate;
            }

            if (untilDateMatch.Success)
            {
                if (DateTime.TryParse($"{untilDateMatch.Groups["dateString"]}", out var searchDate))
                {
                    SearchedUntilDate.Value = searchDate;
                }
            }
            else if (!useSpecialSearch)
            {
                SearchedUntilDate.Value = _defaultDate;
            }

            var searchedPhotoList = _photoList.Select(x => x).Where(x => IsSearchingPhoto(x));

            _IsSearching = false;

            return searchedPhotoList;
        }

        private bool IsSearchingPhoto(Photo photo)
        {
            var useUser = !string.IsNullOrEmpty(SearchedUserName.Value);
            var useWorld = !string.IsNullOrEmpty(SearchedWorldName.Value);
            var useDate = SearchedDate.Value.Date.CompareTo(_defaultDate.Date) != 0;
            var useSinceDate = SearchedSinceDate.Value.Date.CompareTo(_defaultDate.Date) != 0;
            var useUntilDate = SearchedUntilDate.Value.Date.CompareTo(_defaultDate.Date) != 0;

            if (useDate && (!useSinceDate && !useUntilDate))
            {
                if ((photo?.MetaData?.Date?.Date.CompareTo(SearchedDate.Value.Date) ?? 1) != 0) return false;
            }
            else
            {
                if (useSinceDate)
                {
                    if ((photo?.MetaData?.Date?.Date.CompareTo(SearchedSinceDate.Value.Date) ?? -1) < 0) return false;
                }

                if (useUntilDate)
                {
                    if ((photo?.MetaData?.Date?.Date.CompareTo(SearchedUntilDate.Value.Date) ?? 1) > 0) return false;
                }
            }

            // UsersとWorldがnullでDateがnullでない写真はファイル名Dateな写真なので無条件で通す
            if (photo?.MetaData?.Users?.Count > 0 || !(photo?.MetaData?.World is null))
            {
                // ユーザーでもワールドでも検索していない場合
                if (!useUser && !useWorld)
                {
                    if ((!photo?.MetaData?.Users?.Any(u => u.UserName.ToLower(new CultureInfo("en-US")).StartsWith(SearchedUserName.Value.ToLower(new CultureInfo("en-US")), StringComparison.Ordinal)) ?? true) &&
                        (!photo?.MetaData?.World?.ToLower(new CultureInfo("en-US")).Contains(SearchedWorldName.Value.ToLower(new CultureInfo("en-US")), StringComparison.Ordinal) ?? true)) return false;
                }
                else
                {
                    if ((!photo?.MetaData?.Users?.Any(u => u.UserName.ToLower(new CultureInfo("en-US")).StartsWith(SearchedUserName.Value.ToLower(new CultureInfo("en-US")), StringComparison.Ordinal)) ?? true) ||
                        (!photo?.MetaData?.World?.ToLower(new CultureInfo("en-US")).Contains(SearchedWorldName.Value.ToLower(new CultureInfo("en-US")), StringComparison.Ordinal) ?? true)) return false;
                }
            }

            return true;
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
                    searchText = searchText.Replace(freeMatch.Value, string.Empty, StringComparison.Ordinal);
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

            var dateString = dateTime.Date.ToString("yyyy-MM-dd", new CultureInfo("en-US"));

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

            sinceDateString = DateTime.Parse(sinceDateString, new CultureInfo("en-US")).Date.ToString("yyyy-MM-dd", new CultureInfo("en-US"));
            untilDateString = DateTime.Parse(untilDateString, new CultureInfo("en-US")).Date.ToString("yyyy-MM-dd", new CultureInfo("en-US"));

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

            var sinceDateString = sinceDate.Date.ToString("yyyy-MM-dd", new CultureInfo("en-US"));
            var untilDateString = untilDate.Date.ToString("yyyy-MM-dd", new CultureInfo("en-US"));

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
    }
}
