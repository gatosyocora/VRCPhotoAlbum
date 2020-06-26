using Gatosyocora.VRCPhotoAlbum.Models;
using KoyashiroKohaku.VrcMetaTool;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace VRCPhotoAlbumTest.Models
{
    [TestClass]
    public class SearchResultTest
    {
        IEnumerable<Photo> _photoList;

        public SearchResultTest()
        {
            _photoList = Enumerable.Range(0, 5).Select(_ => new Photo("hoge.png"));
        }

        [TestMethod("SearchTextの初期値が空文字であるか")]
        public void IsEmptyDafalutStateOfSearchText()
        {
            var photoList = new ReactiveCollection<Photo>();
            var searchResultModel = new SearchResult(photoList);

            Assert.AreEqual(string.Empty, searchResultModel.SearchText.Value);
        }

        [TestMethod("SearchedUserNameの初期値が空文字であるか")]
        public void IsEmptyDefalutStateOfSearchedUserName()
        {
            var photoList = new ReactiveCollection<Photo>();
            var searchResultModel = new SearchResult(photoList);

            Assert.AreEqual(string.Empty, searchResultModel.SearchedUserName.Value);
        }

        [TestMethod("SearchedWorldNameの初期値が空文字であるか")]
        public void IsEmptyDefalutStateOfSearchedWorldName()
        {
            var photoList = new ReactiveCollection<Photo>();
            var searchResultModel = new SearchResult(photoList);

            Assert.AreEqual(string.Empty, searchResultModel.SearchedWorldName.Value);
        }

        [TestMethod("SearchedDateの初期値が0001/01/01であるか")]
        public void IsOnesDefalutStateOfSearchedDate()
        {
            var photoList = new ReactiveCollection<Photo>();
            var searchResultModel = new SearchResult(photoList);

            Assert.AreEqual(new DateTime(), searchResultModel.SearchedDate.Value);
        }

        [TestMethod("SearchedSinceDateの初期値が0001/01/01であるか")]
        public void IsOnesDefalutStateOfSearchedSinceDate()
        {
            var photoList = new ReactiveCollection<Photo>();
            var searchResultModel = new SearchResult(photoList);

            Assert.AreEqual(new DateTime(), searchResultModel.SearchedSinceDate.Value);
        }

        [TestMethod("SearchedUntilDateの初期値が0001/01/01であるか")]
        public void IsOnesDefalutStateOfSearchedUntilDate()
        {
            var photoList = new ReactiveCollection<Photo>();
            var searchResultModel = new SearchResult(photoList);

            Assert.AreEqual(new DateTime(), searchResultModel.SearchedUntilDate.Value);
        }

        [TestMethod("読み込み時は読み込まれた写真がすべて表示用写真リストに追加されているか")]
        public void AddedAllPhotoToShowedPhotoListWhenLoading()
        {
            var photoList = new ReactiveCollection<Photo>();
            var searchResultModel = new SearchResult(photoList);

            foreach (var photo in _photoList)
            {
                photoList.Add(photo);
            }

            Assert.AreEqual(5, searchResultModel.ShowedPhotoList.Count);
        }

        [TestMethod("表示用写真リストの全要素がResetCommand実行時に削除されているか")]
        public void IsZeroInShowedPhotoListWhenExecuteResetCommand()
        {
            var photoList = new ReactiveCollection<Photo>();
            var searchResultModel = new SearchResult(photoList);

            foreach (var photo in _photoList)
            {
                photoList.Add(photo);
            }

            Assert.AreEqual(5, searchResultModel.ShowedPhotoList.Count);

            searchResultModel.ResetCommand.Execute();

            Assert.AreEqual(0, searchResultModel.ShowedPhotoList.Count);
        }

        [TestMethod("表示用写真リストの要素数がResearchCommand実行後に変化していないか")]
        public void SameShowedPhotoListCountAfterExecuteResearchCommand()
        {
            var photoList = new ReactiveCollection<Photo>();
            var searchResultModel = new SearchResult(photoList);

            foreach (var photo in _photoList)
            {
                photoList.Add(photo);
            }

            var photoCount = searchResultModel.ShowedPhotoList.Count;

            searchResultModel.ResearchCommand.Execute();

            Assert.AreEqual(photoCount, searchResultModel.ShowedPhotoList.Count);
        }

        [TestMethod("検索キーワードがない状態のとき内部メソッドがすべての写真リストを取得できているか")]
        public void CanGetAllPhotoCountByBaseMethodWithNothingKeyword()
        {
            var photoList = new ReactiveCollection<Photo>();
            var searchResultModel = new SearchResult(photoList);

            foreach (var photo in _photoList)
            {
                photoList.Add(photo);
            }

            var privateObject = new PrivateObject(searchResultModel);

            var searchedPhotoList = (IEnumerable<Photo>)(privateObject.Invoke("SearchPhoto", string.Empty));
            Assert.AreEqual(5, searchedPhotoList.Count());
        }
    }
}
