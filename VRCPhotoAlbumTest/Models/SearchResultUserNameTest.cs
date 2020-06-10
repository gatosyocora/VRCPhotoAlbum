using Gatosyocora.VRCPhotoAlbum.Models;
using KoyashiroKohaku.VrcMetaToolSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reactive.Bindings;
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

        [TestMethod("SearchedUserNameで検索時にSearchTextに正しい文字列が入力されるか")]
        public void CanSetCorrectSearchTextWhenSearchWithUserName()
        {
            var photoList = new ReactiveCollection<Photo>();
            var searchResultModel = new SearchResult(photoList);

            var _photoList = new int[] { 3, 2, 5, 10 } // 各人が含まれる写真の数
                                .SelectMany((i, offset) =>
                                {
                                    var meta = new VrcMetaData();
                                    meta.Users.Add(
                                        new KoyashiroKohaku.VrcMetaToolSharp.User
                                        {
                                            UserName = ('a' + offset).ToString()
                                        });
                                    return Enumerable.Range(0, i)
                                    .Select(_ =>
                                        new Photo
                                        {
                                            MetaData = meta
                                        });
                                });

            photoList.AddRangeOnScheduler(_photoList);

            searchResultModel.SearchedUserName.Value = "a";
            Assert.AreEqual(@"user:""a""", searchResultModel.SearchText.Value);

            searchResultModel.SearchedUserName.Value = "b";
            Assert.AreEqual(@"user:""b""", searchResultModel.SearchText.Value);

            searchResultModel.SearchedUserName.Value = "c";
            Assert.AreEqual(@"user:""c""", searchResultModel.SearchText.Value);

            searchResultModel.SearchedUserName.Value = "d";
            Assert.AreEqual(@"user:""d""", searchResultModel.SearchText.Value);
        }

        [TestMethod("SearchedUserNameに検索するユーザー名を入力して正しく取得できているか")]
        public void CanGetCorrectPhotoWhenSearchWithUserName()
        {
            var photoList = new ReactiveCollection<Photo>();
            var searchResultModel = new SearchResult(photoList);

            var _photoList = new int[] { 3, 2, 5, 10 } // 各人が含まれる写真の数
                                .SelectMany((i, offset) =>
                                {
                                    var meta = new VrcMetaData();
                                    meta.Users.Add(
                                        new KoyashiroKohaku.VrcMetaToolSharp.User
                                        {
                                            UserName = ('a' + offset).ToString()
                                        });
                                    return Enumerable.Range(0, i)
                                    .Select(_ =>
                                        new Photo
                                        {
                                            MetaData = meta
                                        });
                                });

            photoList.AddRangeOnScheduler(_photoList);

            searchResultModel.SearchedUserName.Value = "a";
            Assert.AreEqual(3, searchResultModel.ShowedPhotoList.Count);

            searchResultModel.SearchedUserName.Value = "b";
            Assert.AreEqual(2, searchResultModel.ShowedPhotoList.Count);

            searchResultModel.SearchedUserName.Value = "c";
            Assert.AreEqual(5, searchResultModel.ShowedPhotoList.Count);

            searchResultModel.SearchedUserName.Value = "d";
            Assert.AreEqual(10, searchResultModel.ShowedPhotoList.Count);
        }
    }
}
