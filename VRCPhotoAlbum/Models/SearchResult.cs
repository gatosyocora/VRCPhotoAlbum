using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public ReactiveCommand ResetSearchKeywordsCommand { get; }

        public ReactiveProperty<string> OnChangedSearchKeyword { get; }

        private DateTime _defaultDate;

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
            ResetSearchKeywordsCommand = new ReactiveCommand().AddTo(Disposable);

            ResetSearchKeywordsCommand.Subscribe(() =>
            {
                SearchedUserName.Value = string.Empty;
                SearchedWorldName.Value = string.Empty;
                SearchedDate.Value = _defaultDate;
                SearchedSinceDate.Value = _defaultDate;
                SearchedUntilDate.Value = _defaultDate;
            });

            SearchText.Subscribe(t =>
            {
                ExtractKeywords(t);
            });

            _photoList = photoList.ObserveAddChanged()
                            .Select(p => p)
                            .ToReadOnlyReactiveCollection(
                                onReset:ResetCommand.Select(_ => Unit.Default)
                            )
                            .AddTo(Disposable);

            OnChangedSearchKeyword = Observable.Merge(
                                        Observable.Merge(SearchedUserName, SearchedWorldName).Select(x => x),
                                        Observable.Merge(SearchedDate, SearchedSinceDate, SearchedUntilDate).Select(d => d.ToString()))
                                    .ToReactiveProperty().AddTo(Disposable);

            // 読み込み時は_photoList.ObserveAddChanged()で1つずつPhotoが追加されていく
            ShowedPhotoList = Observable.Merge(
                                    _photoList.ObserveAddChanged(),
                                    OnChangedSearchKeyword,
                                    ResearchCommand)
                                .SelectMany(x =>
                                {
                                    if (x is Photo photo)
                                    {
                                        return new Photo[] { photo };
                                    }
                                    else
                                    {
                                        return SearchPhoto( SearchedUserName.Value, 
                                                            SearchedWorldName.Value, 
                                                            SearchedDate.Value, 
                                                            SearchedSinceDate.Value, 
                                                            SearchedUntilDate.Value);
                                    }
                                })
                                .ToReadOnlyReactiveCollection(
                                    onReset: Observable.Merge(
                                                OnChangedSearchKeyword, 
                                                ResearchCommand, 
                                                ResetCommand)
                                            .Select(_ => Unit.Default))
                                .AddTo(Disposable);
        }

        private IEnumerable<Photo> SearchPhoto(string user, string world, DateTime date, DateTime sinceDate, DateTime untilDate)
        {
            var useUser = user.Any();
            var userWorld = world.Any();
            var useDate = date.Date.CompareTo(_defaultDate.Date) != 0;
            var useSinceDate = sinceDate.Date.CompareTo(_defaultDate.Date) != 0;
            var useUntilDate = untilDate.Date.CompareTo(_defaultDate.Date) != 0;

            if (!useUser && !userWorld && !useDate && !useSinceDate && !useUntilDate) return _photoList;

            var searchedPhotoList = _photoList.Select(x => x);

            // 日付検索と期間検索は排他的であり, 期間検索のほうが上位
            if (useDate && (!useSinceDate && !useUntilDate))
            {
                searchedPhotoList = searchedPhotoList.Where(x => (x.MetaData?.Date?.Date.CompareTo(date) ?? 1) == 0);
            }
            else
            {
                if (useSinceDate) searchedPhotoList = searchedPhotoList.Where(x => (x.MetaData?.Date?.Date.CompareTo(sinceDate) ?? -1) >= 0);

                if (useUntilDate) searchedPhotoList = searchedPhotoList.Where(x => (x.MetaData?.Date?.Date.CompareTo(untilDate) ?? 1) <= 0);
            }

            if (!useUser && !userWorld)
            {
                // TODO:入力されている文字で検索できるか再度検討が必要
                // どちらも使わない場合, 入力されている文字でどちらにも一致するか調べてみる
                searchedPhotoList = searchedPhotoList.Where(x => 
                                        (x?.MetaData?.Users?.Any(u => u.UserName.ToLower().StartsWith(user.ToLower())) ?? false) ||
                                        (x?.MetaData?.World?.ToLower().Contains(world.ToLower()) ?? false));
            }
            else
            {
                // 一方がnullの場合, 不正なデータとして必ず検索からは除外する
                searchedPhotoList = searchedPhotoList.Where(x => 
                                        (x?.MetaData?.Users?.Any(u => u.UserName.ToLower().StartsWith(user.ToLower())) ?? false) &&
                                        (x?.MetaData?.World?.ToLower().Contains(world.ToLower()) ?? false));
            }

            return searchedPhotoList;
        }

        private void ExtractKeywords(string text)
        {
            // 最初と最後にスペースが連なっているものはString.Emptyと同じ扱いにする
            text = Regex.Replace(text, @"^\s*", string.Empty);
            text = Regex.Replace(text, @"\s*$", string.Empty);

            if (!text.Any()) return;

            var userMatch = Regex.Match(text, @".*user:""(?<userName>.*?)"".*");
            var worldMatch = Regex.Match(text, @".*world:""(?<worldName>.*?)"".*");
            var dateMatch = Regex.Match(text, @".*date:""(?<dateString>.*?)"".*");
            var sinceDateMatch = Regex.Match(text, @".*since:""(?<dateString>.*?)"".*");
            var untilDateMatch = Regex.Match(text, @".*until:""(?<dateString>.*?)"".*");

            if (userMatch.Success) SearchedUserName.Value = $"{userMatch.Groups["userName"]}";
            else SearchedUserName.Value = Regex.Replace(text, @"\s*(world|date|since|until):"".*?""\s*", string.Empty);

            if (worldMatch.Success) SearchedUserName.Value = $"{worldMatch.Groups["worldName"]}";
            else SearchedUserName.Value = Regex.Replace(text, @"\s*(user|date|since|until):"".*?""\s*", string.Empty);

            if (dateMatch.Success) SearchedDate.Value = DateTime.Parse($"{dateMatch.Groups["dateString"]}");
            else SearchedDate.Value = _defaultDate;

            if (sinceDateMatch.Success) SearchedSinceDate.Value = DateTime.Parse($"{sinceDateMatch.Groups["dateString"]}");
            else SearchedSinceDate.Value = _defaultDate;

            if (untilDateMatch.Success) SearchedUntilDate.Value = DateTime.Parse($"{untilDateMatch.Groups["dateString"]}");
            else SearchedUntilDate.Value = _defaultDate;
        }
    }
}
