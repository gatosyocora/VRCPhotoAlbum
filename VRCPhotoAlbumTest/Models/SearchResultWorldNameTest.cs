using Gatosyocora.VRCPhotoAlbum.Models;
using KoyashiroKohaku.VrcMetaToolSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        [TestMethod("SearchedWorldNameで検索時にSearchTextに正しい文字列が入力されるか")]
        public void CanSetCorrectSearchTextWhenSearchWithWorldName()
        {
            var searchResultModel = new SearchResult(new ReactiveCollection<Photo>());

            searchResultModel.SearchedWorldName.Value = "a";
            Assert.AreEqual(@"world:""a""", searchResultModel.SearchText.Value);

            searchResultModel.SearchedWorldName.Value = "b";
            Assert.AreEqual(@"world:""b""", searchResultModel.SearchText.Value);

            searchResultModel.SearchedWorldName.Value = "c";
            Assert.AreEqual(@"world:""c""", searchResultModel.SearchText.Value);

            searchResultModel.SearchedWorldName.Value = "d";
            Assert.AreEqual(@"world:""d""", searchResultModel.SearchText.Value);
        }

        [TestMethod("ワールド名の所定フォーマットから内部メソッドが正しく取得できているか")]
        public void CanGetCorrectPhotoByBaseMethodWithFormattedWorldName()
        {
            var photoList = new ReactiveCollection<Photo>();
            var searchResultModel = new SearchResult(photoList);

            var _photoList = new int[] { 3, 2, 5, 10 } // 各ワールドが含まれる写真の数
                                .SelectMany((i, offset) =>
                                    Enumerable.Range(0, i)
                                        .Select(_ =>
                                            new Photo
                                            {
                                                MetaData = new VrcMetaData
                                                {
                                                    World = $"{(char)('a' + offset)}"
                                                }
                                            }));

            foreach (var photo in _photoList)
            {
                photoList.Add(photo);
            }

            var privateObject = new PrivateObject(searchResultModel);

            var searchedPhotoList = privateObject.Invoke("SearchPhoto", @"world:""a""") as IEnumerable<Photo>;
            Assert.IsTrue(searchedPhotoList.All(p => p.MetaData.World == "a"));

            searchedPhotoList = privateObject.Invoke("SearchPhoto", @"world:""b""") as IEnumerable<Photo>;
            Assert.IsTrue(searchedPhotoList.All(p => p.MetaData.World == "b"));

            searchedPhotoList = privateObject.Invoke("SearchPhoto", @"world:""c""") as IEnumerable<Photo>;
            Assert.IsTrue(searchedPhotoList.All(p => p.MetaData.World == "c"));

            searchedPhotoList = privateObject.Invoke("SearchPhoto", @"world:""d""") as IEnumerable<Photo>;
            Assert.IsTrue(searchedPhotoList.All(p => p.MetaData.World == "d"));
        }

        [TestMethod("ワールド名の所定フォーマットから内部メソッドが正しい個数取得できているか")]
        public void CanGetCorrectPhotoCountByBaseMethodWithFormattedWorldName()
        {
            var photoList = new ReactiveCollection<Photo>();
            var searchResultModel = new SearchResult(photoList);

            var _photoList = new int[] { 3, 2, 5, 10 } // 各ワールドが含まれる写真の数
                                .SelectMany((i, offset) =>
                                    Enumerable.Range(0, i)
                                        .Select(_ =>
                                            new Photo
                                            {
                                                MetaData = new VrcMetaData
                                                {
                                                    World = $"{(char)('a' + offset)}"
                                                }
                                            }));

            foreach (var photo in _photoList)
            {
                photoList.Add(photo);
            }

            var privateObject = new PrivateObject(searchResultModel);

            var searchedPhotoList = privateObject.Invoke("SearchPhoto", @"world:""a""") as IEnumerable<Photo>;
            Assert.AreEqual(3, searchedPhotoList.Count());

            searchedPhotoList = privateObject.Invoke("SearchPhoto", @"world:""b""") as IEnumerable<Photo>;
            Assert.AreEqual(2, searchedPhotoList.Count());

            searchedPhotoList = privateObject.Invoke("SearchPhoto", @"world:""c""") as IEnumerable<Photo>;
            Assert.AreEqual(5, searchedPhotoList.Count());

            searchedPhotoList = privateObject.Invoke("SearchPhoto", @"world:""d""") as IEnumerable<Photo>;
            Assert.AreEqual(10, searchedPhotoList.Count());
        }

        [TestMethod("SearchTestに検索するワールド名の所定フォーマットを入力して正しく取得できているか")]
        public void CanGetCorrectPhotoWhenInputFormattedWorldNameToSearchText()
        {
            var photoList = new ReactiveCollection<Photo>();
            var searchResultModel = new SearchResult(photoList);

            var _photoList = new int[] { 3, 2, 5, 10 } // 各ワールドが含まれる写真の数
                                .SelectMany((i, offset) =>
                                    Enumerable.Range(0, i)
                                        .Select(_ =>
                                            new Photo
                                            {
                                                MetaData = new VrcMetaData
                                                {
                                                    World = $"{(char)('a' + offset)}"
                                                }
                                            }));

            photoList.AddRangeOnScheduler(_photoList);

            searchResultModel.SearchText.Value = @"world:""a""";
            Assert.AreEqual(3, searchResultModel.ShowedPhotoList.Count);

            searchResultModel.SearchText.Value = @"world:""b""";
            Assert.AreEqual(2, searchResultModel.ShowedPhotoList.Count);

            searchResultModel.SearchText.Value = @"world:""c""";
            Assert.AreEqual(5, searchResultModel.ShowedPhotoList.Count);

            searchResultModel.SearchText.Value = @"world:""d""";
            Assert.AreEqual(10, searchResultModel.ShowedPhotoList.Count);
        }

        [TestMethod("SearchedWorldNameに検索するワールド名を入力して正しく取得できているか")]
        public void CanGetCorrectPhotoWhenSearchWithWorldName()
        {
            var photoList = new ReactiveCollection<Photo>();
            var searchResultModel = new SearchResult(photoList);

            var _photoList = new int[] { 3, 2, 5, 10 } // 各ワールドが含まれる写真の数
                                .SelectMany((i, offset) =>
                                    Enumerable.Range(0, i)
                                        .Select(_ =>
                                            new Photo
                                            {
                                                MetaData = new VrcMetaData
                                                {
                                                    World = $"{(char)('a' + offset)}"
                                                }
                                            }));

            foreach (var photo in _photoList)
            {
                photoList.Add(photo);
            }

            searchResultModel.SearchedWorldName.Value = "a";
            Assert.AreEqual(3, searchResultModel.ShowedPhotoList.Count);

            searchResultModel.SearchedWorldName.Value = "b";
            Assert.AreEqual(2, searchResultModel.ShowedPhotoList.Count);

            searchResultModel.SearchedWorldName.Value = "c";
            Assert.AreEqual(5, searchResultModel.ShowedPhotoList.Count);

            searchResultModel.SearchedWorldName.Value = "d";
            Assert.AreEqual(10, searchResultModel.ShowedPhotoList.Count);
        }
    }
}
