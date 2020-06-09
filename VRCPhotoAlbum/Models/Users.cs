using Gatosyocora.VRCPhotoAlbum.Helpers;
using KoyashiroKohaku.VrcMetaToolSharp;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using User = KoyashiroKohaku.VrcMetaToolSharp.User;

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
        public ReactiveCommand<string> SortCommand { get; }

        public Users(ReactiveCollection<Photo> photoList)
        {
            _userList = photoList.ObserveAddChanged()
                            .SelectMany(p => p?.MetaData?.Users ?? Enumerable.Empty<KoyashiroKohaku.VrcMetaToolSharp.User>())
                            .Select(u => u.UserName)
                            .ToReadOnlyReactiveCollection()
                            .AddTo(Disposable);
            SortCommand = new ReactiveCommand<string>();
            SortedUserList = SortCommand
                                .SelectMany(type => SortWith(type))
                                .ToReadOnlyReactiveCollection(
                                    onReset: SortCommand.Select(_ => Unit.Default))
                                .AddTo(Disposable);
        }

        /// <summary>
        /// いろいろなソートをする
        /// </summary>
        /// <param name="type">ソートの種類</param>
        /// <returns></returns>
        public IEnumerable<User> SortWith(string type)
        {
            if (type == nameof(MetaDataHelper.UserSortType.Alphabet)) return SortWithAlphabet();
            else if (type == nameof(MetaDataHelper.UserSortType.Count)) return SortWithCount();
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
