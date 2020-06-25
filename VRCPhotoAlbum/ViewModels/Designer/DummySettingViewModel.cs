using Gatosyocora.VRCPhotoAlbum.Models;
using Gatosyocora.VRCPhotoAlbum.ViewModels.Interfaces;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gatosyocora.VRCPhotoAlbum.ViewModels.Designer
{
    [Obsolete("Designer only", true)]
    public class DummySettingViewModel : ViewModelBase, ISettingViewModel
    {
        public ReactiveCollection<PhotoFolder> PhotoFolders { get; }
        public ReactiveProperty<string> CacheDataSize { get; }
        public ReactiveProperty<string> CacheFolderPath { get; }
        public ReactiveProperty<bool> CanEnter { get; }
        public ReactiveProperty<string> MessageText { get; }

        public ReactiveProperty<bool> UseTestFunction { get; }

        public ReactiveCommand SelectVRChatFolderCommand { get; }

        public ReactiveCommand DeleteCacheCommand { get; }

        public ReactiveCommand SelectCacheFolderCommand { get; }

        public ReactiveCommand ApplyCommand { get; }

        public DummySettingViewModel()
        {
            PhotoFolders = new ReactiveCollection<PhotoFolder>().AddTo(Disposable);
            PhotoFolders.Add(new PhotoFolder { FolderPath=@"C:\\Test", ContainsSubFolder=false});
            PhotoFolders.Add(new PhotoFolder { FolderPath=@"C:\\Hoge\\VRChat", ContainsSubFolder=true});
            CacheDataSize = new ReactiveProperty<string>("00 MB").AddTo(Disposable);
            CacheFolderPath = new ReactiveProperty<string>(@"C:\\Hoge\\Piyo").AddTo(Disposable);
            CanEnter = new ReactiveProperty<bool>(false).AddTo(Disposable);
            MessageText = new ReactiveProperty<string>("Testメッセージです").AddTo(Disposable);
        }
    }
}
