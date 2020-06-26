using Gatosyocora.VRCPhotoAlbum.Helpers;
using Gatosyocora.VRCPhotoAlbum.Models;
using Gatosyocora.VRCPhotoAlbum.ViewModels.Interfaces;
using Gatosyocora.VRCPhotoAlbum.Views;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Forms;

namespace Gatosyocora.VRCPhotoAlbum.ViewModels
{
    public class SettingViewModel : ViewModelBase, ISettingViewModel
    {
        public ReactiveCollection<PhotoFolder> PhotoFolders { get; }
        public ReactiveProperty<string> CacheDataSize { get; }
        public ReactiveProperty<string> CacheFolderPath { get; }
        public ReactiveProperty<bool> CanEnter { get; }
        public ReactiveProperty<DateTime> InternationalDateLine { get; }
        public ReactiveProperty<bool> UseTestFunction { get; }
        public ReactiveProperty<string> MessageText { get; }

        public ReactiveCommand SelectVRChatFolderCommand { get; }
        public ReactiveCommand DeleteCacheCommand { get; }
        public ReactiveCommand SelectCacheFolderCommand { get; }
        public ReactiveCommand<string> RemoveCacheFolderCommand { get; }
        public ReactiveCommand ApplyCommand { get; }

        public CultureInfo DateTimeCulture { get; } = new CultureInfo("en-In");

        public SettingViewModel()
        {
            PhotoFolders = new ReactiveCollection<PhotoFolder>().AddTo(Disposable);
            CacheDataSize = new ReactiveProperty<string>().AddTo(Disposable);
            CacheFolderPath = new ReactiveProperty<string>().AddTo(Disposable);
            CanEnter = new ReactiveProperty<bool>().AddTo(Disposable);
            Observable.Merge(
                    PhotoFolders.ObserveAddChanged(),
                    PhotoFolders.ObserveRemoveChanged())
                .Subscribe(_ => CanEnter.Value = PhotoFolders.Any(f => !string.IsNullOrEmpty(f.FolderPath)))
                .AddTo(Disposable);
            InternationalDateLine = new ReactiveProperty<DateTime>(Setting.Instance.Data?.InternationalDateLine ?? default).AddTo(Disposable);
            UseTestFunction = new ReactiveProperty<bool>(Setting.Instance.Data?.UseTestFunction ?? false).AddTo(Disposable);

            DeleteCacheCommand = new ReactiveCommand().AddTo(Disposable);
            SelectCacheFolderCommand = new ReactiveCommand().AddTo(Disposable);
            SelectVRChatFolderCommand = new ReactiveCommand().AddTo(Disposable);
            RemoveCacheFolderCommand = new ReactiveCommand<string>().AddTo(Disposable);

            foreach (var folder in Setting.Instance.Data?.PhotoFolders ?? Enumerable.Empty<PhotoFolder>())
            {
                PhotoFolders.Add(folder);
            }
            CacheDataSize.Value = FileHelper.DataSize2String(FileHelper.CalcDataSize(AppCache.Instance.CacheFolderPath));
            CacheFolderPath.Value = AppCache.Instance.CacheFolderPath;

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

            RemoveCacheFolderCommand.Subscribe(folderPath =>
            {
                var removeFolder = PhotoFolders
                                    .Where(f => f.FolderPath == folderPath)
                                    .Single();
                PhotoFolders.Remove(removeFolder);
            });
        }

        public void ApplySettingData()
        {
            if (Setting.Instance.Data is null)
            {
                Setting.Instance.Data = new SettingData();
            }
            else
            {
                Setting.Instance.Data.PhotoFolders.Clear();
            }

            foreach (var folder in PhotoFolders)
            {
                Setting.Instance.Data.PhotoFolders.Add(folder);
            }
            Setting.Instance.Data.InternationalDateLine = InternationalDateLine.Value;
            Setting.Instance.Data.UseTestFunction = UseTestFunction.Value;

            JsonHelper.ExportJsonFile(Setting.Instance.Data, JsonHelper.GetJsonFilePath());

            MainWindow.Instance.Reboot();
        }
    }
}
