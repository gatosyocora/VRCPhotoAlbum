using Gatosyocora.VRCPhotoAlbum.Models;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gatosyocora.VRCPhotoAlbum.ViewModels.Interfaces
{
    public interface ISettingViewModel
    {
        public ReactiveCollection<PhotoFolder> PhotoFolders { get; }
        public ReactiveProperty<string> CacheDataSize { get; }
        public ReactiveProperty<string> CacheFolderPath { get; }
        public ReactiveProperty<bool> CanEnter { get; }
        public ReactiveProperty<bool> UseTestFunction { get; }
        public ReactiveProperty<string> MessageText { get; }

        public ReactiveCommand SelectVRChatFolderCommand { get; }
        public ReactiveCommand DeleteCacheCommand { get; }
        public ReactiveCommand SelectCacheFolderCommand { get; }
        public ReactiveCommand<string> RemoveCacheFolderCommand { get; }
        public ReactiveCommand ApplyCommand { get; }
    }
}
