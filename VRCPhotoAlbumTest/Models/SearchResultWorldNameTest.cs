using Gatosyocora.VRCPhotoAlbum.Models;
using KoyashiroKohaku.VrcMetaTool;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VRCPhotoAlbumTest.Models
{
    [TestClass]
    public class SearchResultWorldNameTest
    {
        /// <summary>
        /// SearchedWorldNameが変化
        /// → SearchWithWorldName()が呼ばれ, 所定フォーマットでSearchTextに追記される
        /// → SearchTextの変化によってShowedPhotoListを更新(全削除→SearchPhoto()の検索結果を追加)
        /// </summary>

        #region Mock
        private IEnumerable<Photo> _photoList;
        #endregion

        public SearchResultWorldNameTest()
        {
            _photoList = new int[] { 3, 2, 5, 10 } // 各ワールドが含まれる写真の数
                            .SelectMany((i, offset) =>
                                Enumerable.Range(0, i)
                                    .Select(_ =>
                                    {
                                        var meta = new VrcMetaData
                                        {
                                            World = $"{(char)('a' + offset)}"
                                        };
                                        // ワールド検索だけどUserがnullだと自動的に検索から除外されてしまう
                                        meta.Users.Add(new KoyashiroKohaku.VrcMetaTool.User("userName"));
                                        return new Photo("hoge.png")
                                        {
                                            MetaData = meta
                                        };
                                    }));
        }

        [TestMethod("SearchedWorldNameで検索時にSearchTextに正しい文字列が入力されるか")]
        [DataTestMethod]
        [DataRow("a")]
        [DataRow("b")]
        [DataRow("c")]
        [DataRow("d")]
        public void CanSetCorrectSearchTextWhenSearchWithWorldName(string worldName)
        {
            var searchResultModel = new SearchResult(new ReactiveCollection<Photo>());

            searchResultModel.SearchedWorldName.Value = worldName;
            searchResultModel.SearchText.Subscribe(_ => Assert.AreEqual(@$"world:""{worldName}""", searchResultModel.SearchText.Value));
        }

        [TestMethod("ワールド名の所定フォーマットから内部メソッドが正しく取得できているか")]
        [DataTestMethod]
        [DataRow("a")]
        [DataRow("b")]
        [DataRow("c")]
        [DataRow("d")]
        public void CanGetCorrectPhotoByBaseMethodWithFormattedWorldName(string worldName)
        {
            var photoList = new ReactiveCollection<Photo>();
            var searchResultModel = new SearchResult(photoList);

            foreach (var photo in _photoList)
            {
                photoList.Add(photo);
            }

            var privateObject = new PrivateObject(searchResultModel);

            var searchedPhotoList = privateObject.Invoke("SearchPhoto", @$"world:""{worldName}""") as IEnumerable<Photo>;
            Assert.IsTrue(searchedPhotoList.All(p => p.MetaData.World == worldName));
        }

        [TestMethod("ワールド名の所定フォーマットから内部メソッドが正しい個数取得できているか")]
        [DataTestMethod]
        [DataRow("a", 3)]
        [DataRow("b", 2)]
        [DataRow("c", 5)]
        [DataRow("d", 10)]
        public void CanGetCorrectPhotoCountByBaseMethodWithFormattedWorldName(string worldName, int hitCount)
        {
            var photoList = new ReactiveCollection<Photo>();
            var searchResultModel = new SearchResult(photoList);

            foreach (var photo in _photoList)
            {
                photoList.Add(photo);
            }

            var privateObject = new PrivateObject(searchResultModel);

            var searchedPhotoList = (IEnumerable<Photo>)privateObject.Invoke("SearchPhoto", @$"world:""{worldName}""");
            Assert.AreEqual(hitCount, searchedPhotoList.Count());
        }

        [TestMethod("SearchTestに検索するワールド名の所定フォーマットを入力して正しく取得できているか")]
        [DataTestMethod]
        [DataRow("a", 3)]
        [DataRow("b", 2)]
        [DataRow("c", 5)]
        [DataRow("d", 10)]
        public void CanGetCorrectPhotoWhenInputFormattedWorldNameToSearchText(string worldName, int hitCount)
        {
            var photoList = new ReactiveCollection<Photo>();
            var searchResultModel = new SearchResult(photoList);

            foreach (var photo in _photoList)
            {
                photoList.Add(photo);
            }

            searchResultModel.SearchText.Value = @$"world:""{worldName}""";
            searchResultModel.ShowedPhotoList.ObserveAddChangedItems()
                .Subscribe(_ => Assert.AreEqual(hitCount, searchResultModel.ShowedPhotoList.Count));
        }

        [TestMethod("SearchTestに検索するワールド名の所定フォーマットを入力して正しく取得できているか")]
        [DataTestMethod]
        [DataRow("a", 3)]
        [DataRow("b", 2)]
        [DataRow("c", 5)]
        [DataRow("d", 10)]
        public void CanGetCorrectPhotoWhenSearchWithWorldName(string worldName, int hitCount)
        {
            var photoList = new ReactiveCollection<Photo>();
            var searchResultModel = new SearchResult(photoList);

            foreach (var photo in _photoList)
            {
                photoList.Add(photo);
            }

            searchResultModel.SearchedWorldName.Value = worldName;

            searchResultModel.ShowedPhotoList.ObserveAddChangedItems()
                .Subscribe(_ => Assert.AreEqual(hitCount, searchResultModel.ShowedPhotoList.Count));
        }
    }
}
