using Gatosyocora.VRCPhotoAlbum.Helpers;
using Gatosyocora.VRCPhotoAlbum.Models;
using Gatosyocora.VRCPhotoAlbum.ViewModels.Interfaces;
using Gatosyocora.VRCPhotoAlbum.Views;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Forms;

namespace Gatosyocora.VRCPhotoAlbum.ViewModels
{
    public class SettingViewModel : ViewModelBase, ISettingViewModel
    {
        private SettingData _settingData;

        public ReactiveCollection<PhotoFolder> PhotoFolders { get; }
        public ReactiveProperty<string> CacheDataSize { get; }
        public ReactiveProperty<string> CacheFolderPath { get; }
        public ReactiveProperty<bool> CanEnter { get; }
        public ReactiveProperty<bool> UseTestFunction { get; }
        public ReactiveProperty<string> MessageText { get; }

        public ReactiveCommand SelectVRChatFolderCommand { get; }
        public ReactiveCommand DeleteCacheCommand { get; }
        public ReactiveCommand SelectCacheFolderCommand { get; }
        public ReactiveCommand ApplyCommand { get; }


        public SettingViewModel()
        {
            if (Setting.Instance.Data is null)
            {
                _settingData = new SettingData
                {
                    PhotoFolders = new List<PhotoFolder>(),
                    UseTestFunction = false
                };
            }
            else
            {
                _settingData = Setting.Instance.Copy();
            }

            if (_settingData.PhotoFolders is null)
            {
                _settingData.PhotoFolders = new List<PhotoFolder>();
            }

            PhotoFolders = new ReactiveCollection<PhotoFolder>().AddTo(Disposable);
            CacheDataSize = new ReactiveProperty<string>().AddTo(Disposable);
            CacheFolderPath = new ReactiveProperty<string>().AddTo(Disposable);
            CanEnter = new ReactiveProperty<bool>().AddTo(Disposable);
            Observable.Merge(
                    PhotoFolders.ObserveAddChanged(),
                    PhotoFolders.ObserveRemoveChanged())
                .Subscribe(_ => CanEnter.Value = PhotoFolders.Any(f => !string.IsNullOrEmpty(f.FolderPath)))
                .AddTo(Disposable);
            UseTestFunction = new ReactiveProperty<bool>(_settingData.UseTestFunction).AddTo(Disposable);

            DeleteCacheCommand = new ReactiveCommand().AddTo(Disposable);
            SelectCacheFolderCommand = new ReactiveCommand().AddTo(Disposable);
            SelectVRChatFolderCommand = new ReactiveCommand().AddTo(Disposable);

            foreach (var folder in _settingData.PhotoFolders)
            {
                PhotoFolders.Add(folder);
            }
            CacheDataSize.Value = FileHelper.DataSize2String(FileHelper.CalcDataSize(AppCache.Instance.CacheFolderPath));
            CacheFolderPath.Value = AppCache.Instance.CacheFolderPath;

            PhotoFolders.ObserveAddChanged().Subscribe(f => _settingData.PhotoFolders.Add(f)).AddTo(Disposable);
            PhotoFolders.ObserveRemoveChanged().Subscribe(f => _settingData.PhotoFolders.Remove(f)).AddTo(Disposable);
            UseTestFunction.Subscribe(b => _settingData.UseTestFunction = b);

            DeleteCacheCommand.Subscribe(() =>
            {
                MainWindow.Instance.DeleteCache();
                CacheDataSize.Value = FileHelper.DataSize2String(FileHelper.CalcDataSize(AppCache.Instance.CacheFolderPath));
                MessageText.Value = "正しく動作させるにはアプリケーションの再起動が必要です";
            });
            SelectCacheFolderCommand.Subscribe(() =>
            {
                var dialog = new FolderBrowserDialog
                {
                    Description = "画像フォルダを選択してください",
                    RootFolder = System.Environment.SpecialFolder.MyPictures,
                    ShowNewFolderButton = false,
                };
                dialog.ShowDialog();
                var newFolderPath = dialog.SelectedPath;

                // すでに存在したら追加しない
                if (PhotoFolders.Any(f => f.FolderPath == newFolderPath)) return;

                PhotoFolders.Add(
                    new PhotoFolder
                    {
                        FolderPath = newFolderPath,
                        ContainsSubFolder = true
                    });
            });

            SelectVRChatFolderCommand.Subscribe(() =>
            {
                var vrcPictureFolderPath = VRChatHelper.GetVRChatPictureFolderPath();
                if (!string.IsNullOrEmpty(vrcPictureFolderPath) && Directory.Exists(vrcPictureFolderPath))
                {
                    // すでに存在したら追加しない
                    if (PhotoFolders.Any(f => f.FolderPath == vrcPictureFolderPath)) return;

                    PhotoFolders.Add(
                        new PhotoFolder
                        {
                            FolderPath = vrcPictureFolderPath,
                            ContainsSubFolder = true
                        });
                }
                else
                {
                    MessageText.Value = "VRChatの写真が入ったフォルダを見つけるのに失敗しました";
                }
            });
        }

        public void ApplySettingData()
        {
            JsonHelper.ExportJsonFile(_settingData, JsonHelper.GetJsonFilePath());
            
            // 写真のフォルダリストに変化があったかどうか
            var isChangedPhotoFolder = (Setting.Instance.Data?.PhotoFolders ?? Enumerable.Empty<PhotoFolder>())
                                            .Except(
                                                _settingData.PhotoFolders ?? Enumerable.Empty<PhotoFolder>())
                                            .Any();

            Setting.Instance.Data = _settingData;

            if (isChangedPhotoFolder)
            {
                MainWindow.Instance.Reboot();
            }

        }
    }
}
