using Gatosyocora.VRCPhotoAlbum.Models;
using KoyashiroKohaku.VrcMetaTool;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VRCPhotoAlbumTest.Models
{
    [TestClass]
    public class UsersTest
    {
        IEnumerable<Photo> _photoListWithOneUser; // 一人のUserだけ入っている写真のリスト
        IEnumerable<Photo> _photoListWithManyUsers; // 複数のUserが入っている写真のリスト
        IEnumerable<Photo> _photoListForAlphabet;
        IEnumerable<Photo> _photoListForCount;

        public UsersTest()
        {
            _photoListWithOneUser = Enumerable.Range(0, 5)
                                    .Select(offset =>
                                    {
                                        var meta = new VrcMetaData();
                                        meta.Users.Add(new KoyashiroKohaku.VrcMetaTool.User($"{(char)('a' + offset)}"));
                                        return new Photo($"file{offset}.png")
                                        {
                                            MetaData = meta
                                        };
                                    });

            _photoListWithManyUsers = Enumerable.Range(0, 5)
                                        .Select(offset =>
                                        {
                                            var meta = new VrcMetaData();
                                            foreach (var userOffset in Enumerable.Range(0, 5))
                                            {
                                                meta.Users.Add(new KoyashiroKohaku.VrcMetaTool.User($"{(char)('a' + userOffset)}"));
                                            }
                                            return new Photo($"file{offset}.png")
                                            {
                                                MetaData = meta
                                            };
                                        });

            _photoListForAlphabet = new int[] { 0, 3, 1, 2, 4 } // 各写真のユーザーの文字offset
                                        .Select(i =>
                                        {
                                            var meta = new VrcMetaData
                                            {
                                                World = "worldName"
                                            };
                                            meta.Users.Add(
                                                new KoyashiroKohaku.VrcMetaTool.User($"{(char)('a' + i)}"));
                                            return new Photo("hoge.png")
                                            {
                                                MetaData = meta
                                            };
                                        });

            _photoListForCount = new int[] { 3, 5, 1, 4, 2 } // 各写真のユーザー数
                                        .Select(i =>
                                        {
                                            var meta = new VrcMetaData
                                            {
                                                World = "worldName"
                                            };
                                            meta.Users.AddRange(
                                                Enumerable.Range(0, i)
                                                    .Select(ii =>
                                                        new KoyashiroKohaku.VrcMetaTool.User($"{(char)('a' + i)}")));
                                            return new Photo("hoge.png")
                                            {
                                                MetaData = meta
                                            };
                                        });
        }

        [TestMethod("始めのソートタイプがNoneである")]
        public void IsNoneWhenDefaultStateOfSortType()
        {
            var usersModel = new Users(new ReactiveCollection<Photo>());
            Assert.AreEqual(UserSortType.None, usersModel.SortType.Value);
        }

        [TestMethod("写真のリストからそれに含まれるユーザーリストを作成する(ユーザーに重複なし)")]
        public void CreateUserListFromPhotoList()
        {
            var photoList = new ReactiveCollection<Photo>();
            var usersModel = new Users(photoList);

            var privateObject = new PrivateObject(usersModel);
            var userlist = privateObject.GetProperty("_userList") as ReadOnlyReactiveCollection<string>;
            Assert.AreEqual(0, userlist.Count);

            foreach (var photo in _photoListWithOneUser)
            {
                photoList.Add(photo);
            }

            Assert.AreEqual(5, userlist.Count);
        }


        [TestMethod("写真のリストからそれに含まれるユーザーリストを作成する(ユーザーに重複あり)")]
        public void CreateUserListFromPhotoListContainsDuplicateUser()
        {
            var photoList = new ReactiveCollection<Photo>();
            var usersModel = new Users(photoList);

            var privateObject = new PrivateObject(usersModel);
            var userlist = privateObject.GetProperty("_userList") as ReadOnlyReactiveCollection<string>;
            Assert.AreEqual(0, userlist.Count);

            foreach (var photo in _photoListWithManyUsers)
            {
                photoList.Add(photo);
            }

            Assert.AreEqual(5*5, userlist.Count);
        }

        [TestMethod("内部メソッドが重複をまとめたユーザーリストを作成できる")]
        [DataTestMethod()]
        [DataRow("a", 5)]
        [DataRow("b", 5)]
        [DataRow("c", 5)]
        [DataRow("d", 5)]
        [DataRow("e", 5)]
        public void CanCreateNoDuplicatedUserList(string userName, int photoCount)
        {
            var photoList = new ReactiveCollection<Photo>();
            var usersModel = new Users(photoList);
            var privateObject = new PrivateObject(usersModel);

            foreach (var photo in _photoListWithManyUsers)
            {
                photoList.Add(photo);
            }

            var duplicatedUserList = privateObject.Invoke("CreateUserList", null) as IEnumerable<Gatosyocora.VRCPhotoAlbum.Models.User>;
            Assert.AreEqual(5, duplicatedUserList.Count());
            Assert.AreEqual(photoCount, duplicatedUserList.Where(u => u.Name == userName).Single().PhotoCount);
        }

        [TestMethod("表示用ユーザーリストのユーザーが重複していないか")]
        public void DontDuplicateUserInUserList()
        {
            var photoList = new ReactiveCollection<Photo>();
            var usersModel = new Users(photoList);

            foreach (var photo in _photoListWithManyUsers)
            {
                photoList.Add(photo);
            }

            // 無ソート状態の初期状態では枚数1のUserの重複なしリストが作られる
            Assert.AreEqual(5, usersModel.SortedUserList.Count);
        }

        [TestMethod("ユーザーリストの全要素がResetCommand発行で削除されるか")]
        public void IsZeroInUserListWhenExecuteResetCommand()
        {
            var photoList = new ReactiveCollection<Photo>();
            var usersModel = new Users(photoList);

            foreach (var photo in _photoListWithOneUser)
            {
                photoList.Add(photo);
            }
            Assert.AreEqual(5, usersModel.SortedUserList.Count);

            usersModel.ResetCommand.Execute();

            Assert.AreEqual(0, usersModel.SortedUserList.Count);
        }

        [TestMethod("ユーザーリストの要素数がアルファベット順ソートで変化しないか")]
        public void SameUserListCountAfterAlphabetSort()
        {
            var photoList = new ReactiveCollection<Photo>();
            var usersModel = new Users(photoList);

            foreach (var photo in _photoListForAlphabet)
            {
                photoList.Add(photo);
            }

            var userCount = usersModel.SortedUserList.Count;

            usersModel.SortType.Value = UserSortType.Alphabet;
            Assert.AreEqual(userCount, usersModel.SortedUserList.Count);
        }

        // TODO : 検査部分が未完成
        [TestMethod("ユーザーリストがアルファベット順にソートされるか")]
        public void CanSortOrderToAlphabet()
        {
            var photoList = new ReactiveCollection<Photo>();
            var usersModel = new Users(photoList);

            foreach (var photo in _photoListForAlphabet)
            {
                photoList.Add(photo);
            }

            usersModel.SortType.Value = UserSortType.Alphabet;

            throw new NotImplementedException();
        }

        [TestMethod("ユーザーリストの要素数が枚数順ソートで変化しないか")]
        public void SameUserListCountAfterPhotoCountSort()
        {
            var photoList = new ReactiveCollection<Photo>();
            var usersModel = new Users(photoList);

            foreach (var photo in _photoListForCount)
            {
                photoList.Add(photo);
            }

            var userCount = usersModel.SortedUserList.Count;

            usersModel.SortType.Value = UserSortType.Count;

            Assert.AreEqual(userCount, usersModel.SortedUserList.Count);
        }

        // TODO : 検査部分が未完成
        [TestMethod("ユーザーリストが枚数順にソートされるか")]
        public void CanSortOrderToCount()
        {
            var photoList = new ReactiveCollection<Photo>();
            var usersModel = new Users(photoList);

            foreach (var photo in _photoListForCount)
            {
                photoList.Add(photo);
            }

            usersModel.SortType.Value = UserSortType.Count;

            throw new NotImplementedException();
        }
    }
}
