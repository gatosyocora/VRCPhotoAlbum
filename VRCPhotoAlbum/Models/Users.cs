using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

namespace Gatosyocora.VRCPhotoAlbum.Models
{
    public class Users : ModelBase
    {
        /// <summary>
        /// ソート済みユーザー一覧
        /// </summary>
        public ReadOnlyReactiveCollection<User> SortedUserList { get; }

        /// <summary>
        /// ユーザー一覧（重複あり）
        /// </summary>
        private ReadOnlyReactiveCollection<string> _userList { get; }

        /// <summary>
        /// ソートを実行させるコマンド
        /// </summary>
        public ReactiveCommand SortCommand { get; }
        public ReactiveProperty<UserSortType> SortType { get; }

        public Users(ReactiveCollection<Photo> photoList)
        {
            _userList = photoList.ObserveAddChanged()
                            .SelectMany(p => p?.MetaData?.Users ?? Enumerable.Empty<KoyashiroKohaku.VrcMetaToolSharp.User>())
                            .Select(u => u.UserName)
                            .ToReadOnlyReactiveCollection()
                            .AddTo(Disposable);
            SortCommand = new ReactiveCommand();
            SortType = new ReactiveProperty<UserSortType>(UserSortType.None);
            SortedUserList = Observable.Merge(
                                    _userList.ObserveAddChanged(),
                                    SortCommand)
                                .SelectMany(x =>
                                {
                                    if (x is string userName)
                                    {
                                        // 既に同じuserNameの要素があったら追加しない
                                        if (SortedUserList
                                            .Select((u, i) => new { u.Name, i })
                                            .Where(x => x.Name == userName)
                                            .Any()) 
                                        {
                                            return Enumerable.Empty<User>();
                                        }
                                        else
                                        {
                                            return new User[] { new User { Name = userName, PhotoCount = 1 } };
                                        }
                                    }
                                    else
                                    {
                                        return SortWith(SortType.Value);
                                    }
                                })
                                .ToReadOnlyReactiveCollection(
                                    onReset: SortCommand.Select(_ => Unit.Default))
                                .AddTo(Disposable);
        }

        /// <summary>
        /// いろいろなソートをする
        /// </summary>
        /// <param name="type">ソートの種類</param>
        /// <returns></returns>
        public IEnumerable<User> SortWith(UserSortType type)
        {
            if (type == UserSortType.Alphabet) return SortWithAlphabet();
            else if (type == UserSortType.Count) return SortWithCount();
            else return CreateUserList();
        }

        private IEnumerable<User> CreateUserList() =>
            _userList
                .GroupBy(u => u)
                .Select(g =>
                    new User
                    {
                        Name = g.Key,
                        PhotoCount = g.Count()
                    });

        public IEnumerable<User> SortWithAlphabet() => CreateUserList().OrderBy(u => u.Name);

        public IEnumerable<User> SortWithCount() => CreateUserList().OrderByDescending(u => u.PhotoCount);
    }
}
