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
        IEnumerable<Photo> _photoListForAlphabet;
        IEnumerable<Photo> _photoListForCount;

        public UsersTest()
        {
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

        // TODO: Subscribeが呼ばれていない
        [TestMethod("写真のリストからそれに含まれるユーザーリストを作成する")]
        public void CreateUserListFromPhotoList()
        {
            var photoList = new ReactiveCollection<Photo>();
            var usersModel = new Users(photoList);

            var privateObject = new PrivateObject(usersModel);

            var _photoList = Enumerable.Range(0, 5)
                .Select(offset =>
                {
                    var meta = new VrcMetaData();
                    meta.Users.Add(new KoyashiroKohaku.VrcMetaTool.User($"{(char)('a' + offset)}"));
                    return new Photo($"file{offset}.png")
                    {
                        MetaData = meta
                    };
                });

            photoList.AddRangeOnScheduler(_photoList);

            photoList.ObserveAddChangedItems()
                .Subscribe(_ =>
                {
                    var userlist = privateObject.GetInvokeMember("_userList") as ReadOnlyReactiveCollection<string>;
                    Assert.AreEqual(5, userlist.Count);
                });
        }

        // TODO: Subscribeが呼ばれていない
        [TestMethod("ユーザーリストのユーザーが重複していないか")]
        public void DontDuplicateUserInUserList()
        {
            var photoList = new ReactiveCollection<Photo>();
            var usersModel = new Users(photoList);

            var privateObject = new PrivateObject(usersModel);

            var _photoList = Enumerable.Range(0, 5)
                .Select(offset =>
                {
                    var meta = new VrcMetaData();
                    foreach (var userOffset in Enumerable.Range(0, offset + 1))
                    {
                        meta.Users.Add(new KoyashiroKohaku.VrcMetaTool.User($"{(char)('a' + userOffset)}"));
                    }
                    return new Photo($"file{offset}.png")
                    {
                        MetaData = meta
                    };
                });

            photoList.AddRangeOnScheduler(_photoList);

            photoList.ObserveAddChangedItems()
                .Subscribe(_ =>
                {
                    var userlist = privateObject.GetInvokeMember("_userList") as ReadOnlyReactiveCollection<string>;
                    Assert.AreEqual(5*5, userlist.Count);
                });
        }

        // TODO: Subscribeが呼ばれていない
        [TestMethod("ユーザーリストのUserに含まれる写真の枚数があっているか")]
        [DataTestMethod()]
        [DataRow("a", 5)]
        [DataRow("b", 4)]
        [DataRow("c", 3)]
        [DataRow("d", 2)]
        [DataRow("e", 1)]
        public void CorrectPhotoCountInUserList(string userName, int count)
        {
            var photoList = new ReactiveCollection<Photo>();
            var usersModel = new Users(photoList);

            var privateObject = new PrivateObject(usersModel);

            var _photoList = Enumerable.Range(0, 5)
                .Select(offset =>
                {
                    var meta = new VrcMetaData();
                    foreach (var userOffset in Enumerable.Range(0, offset + 1))
                    {
                        meta.Users.Add(new KoyashiroKohaku.VrcMetaTool.User($"{(char)('a' + userOffset)}"));
                    }
                    return new Photo($"file{offset}.png")
                    {
                        MetaData = meta
                    };
                });

            photoList.AddRangeOnScheduler(_photoList);

            photoList.ObserveAddChangedItems()
                .Subscribe(_ =>
                {
                    var userlist = privateObject.GetInvokeMember("_userList") as ReadOnlyReactiveCollection<string>; ;
                    Assert.AreEqual(5, usersModel.SortedUserList.Count);
                });
        }

        [TestMethod("写真のリストのメタ情報に含まれるユーザー数とユーザーリストの要素数が一致するか(各画像に異なる人が一人ずつ)")]
        public void EqualsPhotoListUserCountToUserListCount()
        {
            var photoList = new ReactiveCollection<Photo>();
            var usersModel = new Users(photoList);

            var _photoList = Enumerable.Range(0, 5)
                .Select(i =>
                {
                    var meta = new VrcMetaData();
                    meta.Users.Add(
                        new KoyashiroKohaku.VrcMetaTool.User(('a' + i).ToString()));
                    return new Photo("hoge.png")
                    {
                        MetaData = meta
                    };
                });

            photoList.AddRangeOnScheduler(_photoList);

            Assert.AreEqual(5, usersModel.SortedUserList.Count);
        }

        [TestMethod("写真のリストのメタ情報に含まれるユーザー数とユーザーリストの要素数が一致するか(各画像に異なる人が2人ずつ)")]
        public void EqualsPhotoListUserCountToUserListCount2()
        {
            var photoList = new ReactiveCollection<Photo>();
            var usersModel = new Users(photoList);

            var _photoList = Enumerable.Range(0, 5)
                .Select(i =>
                {
                    var meta = new VrcMetaData();
                    meta.Users.AddRange(
                        new KoyashiroKohaku.VrcMetaTool.User[]
                        {
                            new KoyashiroKohaku.VrcMetaTool.User(('a' + i).ToString()),
                            new KoyashiroKohaku.VrcMetaTool.User(('z' - i).ToString())
                        });
                    return new Photo("hoge.png")
                    {
                        MetaData = meta
                    };
                });

            photoList.AddRangeOnScheduler(_photoList);
            Assert.AreEqual(10, usersModel.SortedUserList.Count);
        }

        [TestMethod("写真のリストのメタ情報に含まれるユーザー数とユーザーリストの要素数が一致するか（各画像に同じ人が2人ずつ）")]
        public void EqualsPhotoListUserCountToUserListCount3()
        {
            var photoList = new ReactiveCollection<Photo>();
            var usersModel = new Users(photoList);

            var _photoList = Enumerable.Range(0, 5)
                .Select(i =>
                {
                    var meta = new VrcMetaData();
                    meta.Users.AddRange(
                        new KoyashiroKohaku.VrcMetaTool.User[] 
                        {
                            new KoyashiroKohaku.VrcMetaTool.User(('a' + i).ToString()),
                            new KoyashiroKohaku.VrcMetaTool.User(('a' + i).ToString())
                        });
                    return new Photo("hoge.png")
                    {
                        MetaData = meta
                    };
                });

            photoList.AddRangeOnScheduler(_photoList);
            Assert.AreEqual(5, usersModel.SortedUserList.Count);
        }

        [TestMethod("写真のリストのメタ情報に含まれるユーザー数とユーザーリストの要素数が一致するか（各画像にi人, n人がそれぞれ入る）")]
        public void EqualsPhotoListUserCountToUserListCount4()
        {
            var photoList = new ReactiveCollection<Photo>();
            var usersModel = new Users(photoList);

            var _photoList = Enumerable.Range(1, 5)
                .Select(i =>
                {
                    var meta = new VrcMetaData();
                    meta.Users.AddRange(
                        Enumerable.Range(0, i)
                            .Select(ii =>
                                new KoyashiroKohaku.VrcMetaTool.User(('a' + ii).ToString())));
                    return new Photo("hoge.png")
                    {
                        MetaData = meta
                    };
                });

            photoList.AddRangeOnScheduler(_photoList);
            Assert.AreEqual(5, usersModel.SortedUserList.Count);
        }

        [TestMethod("ユーザーリストの全要素がResetCommand発行で削除されるか")]
        public void IsZeroInUserListWhenExecuteResetCommand()
        {
            var photoList = new ReactiveCollection<Photo>();
            var usersModel = new Users(photoList);

            var _photoList = Enumerable.Range(1, 5)
                .Select(i =>
                {
                    var meta = new VrcMetaData();
                    meta.Users.AddRange(
                        Enumerable.Range(0, i)
                            .Select(ii =>
                                new KoyashiroKohaku.VrcMetaTool.User(('a' + ii).ToString())));
                    return new Photo("hoge.png")
                    {
                        MetaData = meta
                    };
                });

            photoList.AddRangeOnScheduler(_photoList);

            usersModel.ResetCommand.Execute();

            Assert.AreEqual(0, usersModel.SortedUserList.Count);
        }

        [TestMethod("ユーザーリストの要素数がアルファベット順ソートで変化しないか")]
        public void SaneUserListCountAfterAlphabetSort()
        {
            var photoList = new ReactiveCollection<Photo>();
            var usersModel = new Users(photoList);

            foreach (var photo in _photoListForAlphabet)
            {
                photoList.Add(photo);
            }

            usersModel.SortType.Value = UserSortType.Alphabet;

            usersModel.SortedUserList.ObserveAddChangedItems()
                .Subscribe(_ => Assert.AreEqual(5, usersModel.SortedUserList.Count));
        }

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

            var list = usersModel.SortedUserList;

            usersModel.SortedUserList.ObserveAddChangedItems()
                .Subscribe(_ => 
                {
                    for (int i = 0; i < 4; i++)
                    {
                        Assert.IsTrue(list[i].Name.CompareTo(list[i + 1].Name) <= 0);
                    }
                });
            
        }

        [TestMethod("ユーザーリストの要素数が枚数順ソートで変化しないか")]
        public void SameUserListCountAfterPhotoCountSort()
        {
            var photoList = new ReactiveCollection<Photo>();
            var usersModel = new Users(photoList);

            photoList.AddRangeOnScheduler(_photoListForCount);

            usersModel.SortType.Value = UserSortType.Count;

            usersModel.SortedUserList.ObserveAddChangedItems()
                .Subscribe(_ => Assert.AreEqual(5, usersModel.SortedUserList.Count));
        }

        [TestMethod("ユーザーリストが枚数順にソートされるか")]
        public void CanSortOrderToCount()
        {
            var photoList = new ReactiveCollection<Photo>();
            var usersModel = new Users(photoList);

            foreach (var photo in _photoListForAlphabet)
            {
                photoList.Add(photo);
            }

            usersModel.SortType.Value = UserSortType.Count;

            var list = usersModel.SortedUserList;

            usersModel.SortedUserList.ObserveAddChangedItems()
                .Subscribe(_ =>
                {
                    for (int i = 0; i < 4; i++)
                    {
                        Assert.IsTrue(list[i].PhotoCount.CompareTo(list[i + 1].PhotoCount) <= 0);
                    }
                });

        }
    }
}
