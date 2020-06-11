using Gatosyocora.VRCPhotoAlbum.Models;
using KoyashiroKohaku.VrcMetaTool;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VRCPhotoAlbumTest.Models
{
    [TestClass]
    public class SearchResultUserNameTest
    {
        /// <summary>
        /// SearchedUserNameが変化
        /// → SearchWithUserName()が呼ばれ, 所定フォーマットでSearchTextに追記される
        /// → SearchTextの変化によってShowedPhotoListを更新(全削除→SearchPhoto()の検索結果を追加)
        /// </summary>

        private IEnumerable<Photo> _photoList;

        public SearchResultUserNameTest()
        {
            _photoList = new int[] { 3, 2, 5, 10 } // 各人が含まれる写真の数
                    .SelectMany((i, offset) =>
                    {
                        var meta = new VrcMetaData 
                        {
                            World = "worldName"
                        };
                        meta.Users.Add(
                            new KoyashiroKohaku.VrcMetaTool.User
                            {
                                UserName = $"{(char)('a' + offset)}"
                            });
                        return Enumerable.Range(0, i)
                        .Select(_ =>
                            new Photo
                            {
                                MetaData = meta,
                            });
                    });
        }

        [TestMethod("SearchedUserNameで検索時にSearchTextに正しい文字列が入力されるか")]
        [DataTestMethod]
        [DataRow("a")]
        [DataRow("b")]
        [DataRow("c")]
        [DataRow("d")]
        public void CanSetCorrectSearchTextWhenSearchWithUserName(string userName)
        {
            var searchResultModel = new SearchResult(new ReactiveCollection<Photo>());

            searchResultModel.SearchedUserName.Value = userName;
            searchResultModel.SearchText.Subscribe(_ => Assert.AreEqual(@$"user:""{userName}""", searchResultModel.SearchText.Value));
        }

        [TestMethod("ユーザー名の所定フォーマットから内部メソッドが正しく取得できているか")]
        [DataTestMethod]
        [DataRow("a")]
        [DataRow("b")]
        [DataRow("c")]
        [DataRow("d")]
        public void CanGetCorrectPhotoByBaseMethodWithFormattedUserName(string userName)
        {
            var photoList = new ReactiveCollection<Photo>();
            var searchResultModel = new SearchResult(photoList);

            foreach (var photo in _photoList)
            {
                photoList.Add(photo);
            }

            var privateObject = new PrivateObject(searchResultModel);

            var searchedPhotoList = privateObject.Invoke("SearchPhoto", @$"user:""{userName}""") as IEnumerable<Photo>;
            Assert.IsTrue(searchedPhotoList.All(p => p.MetaData.Users.Any(u => u.UserName == userName)));
        }

        [TestMethod("ユーザー名の所定フォーマットから内部メソッドが正しい個数取得できているか")]
        [DataTestMethod]
        [DataRow("a", 3)]
        [DataRow("b", 2)]
        [DataRow("c", 5)]
        [DataRow("d", 10)]
        public void CanGetCorrectPhotoCountByBaseMethodWithFormattedUserName(string userName, int hitCount)
        {
            var photoList = new ReactiveCollection<Photo>();
            var searchResultModel = new SearchResult(photoList);

            foreach (var photo in _photoList)
            {
                photoList.Add(photo);
            }

            var privateObject = new PrivateObject(searchResultModel);

            var searchedPhotoList = (IEnumerable<Photo>)privateObject.Invoke("SearchPhoto", @$"user:""{userName}""");
            Assert.AreEqual(hitCount, searchedPhotoList.Count());
        }

        [TestMethod("SearchTextに検索するユーザー名の所定フォーマットを入力して正しく取得できているか")]
        [DataTestMethod]
        [DataRow("a", 3)]
        [DataRow("b", 2)]
        [DataRow("c", 5)]
        [DataRow("d", 10)]
        public void CanGetCorrectPhotoWhenInputFormattedUserNameToSearchText(string userName, int hitCount)
        {
            var photoList = new ReactiveCollection<Photo>();
            var searchResultModel = new SearchResult(photoList);

            foreach (var photo in _photoList)
            {
                photoList.Add(photo);
            }

            searchResultModel.SearchText.Value = @$"user:""{userName}""";
            searchResultModel.ShowedPhotoList.ObserveAddChangedItems()
                .Subscribe(_ => Assert.AreEqual(hitCount, searchResultModel.ShowedPhotoList.Count));
        }

        [TestMethod("SearchTextに検索するユーザー名の所定フォーマットを入力して正しく取得できているか")]
        [DataTestMethod]
        [DataRow("a", 3)]
        [DataRow("b", 2)]
        [DataRow("c", 5)]
        [DataRow("d", 10)]
        public void CanGetCorrectPhotoWhenSearchWithUserName(string userName, int hitCount)
        {
            var photoList = new ReactiveCollection<Photo>();
            var searchResultModel = new SearchResult(photoList);

            foreach (var photo in _photoList)
            {
                photoList.Add(photo);
            }

            searchResultModel.SearchedUserName.Value = userName;

            searchResultModel.ShowedPhotoList.ObserveAddChangedItems()
                .Subscribe(_ => Assert.AreEqual(hitCount, searchResultModel.ShowedPhotoList.Count));
        }
    }
}
