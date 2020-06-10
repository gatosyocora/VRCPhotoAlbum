using Gatosyocora.VRCPhotoAlbum.Helpers;
using Gatosyocora.VRCPhotoAlbum.Models;
using Gatosyocora.VRCPhotoAlbum.Views;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Reactive.Linq;
using System.Windows.Forms;

namespace Gatosyocora.VRCPhotoAlbum.ViewModels
{
    public class SettingViewModel : ViewModelBase
    {
        private SettingData _settingData;

        public ReactiveProperty<string> PhotoFolderName { get; }
        public ReactiveProperty<string> CacheDataSize { get; }
        public ReactiveProperty<bool> CanEnter { get; }
        public ReactiveProperty<bool> UseTestFunction { get; }

        public ReactiveCommand DeleteCacheCommand { get; }
        public ReactiveCommand SelectCacheFolderCommand { get; }
        public ReactiveCommand ApplyCommand { get; }


        public SettingViewModel()
        {
            if (Setting.Instance.Data is null)
            {
                _settingData = new SettingData
                {
                    FolderPath = string.Empty,
                    UseTestFunction = false
                };
            }
            else
            {
                _settingData = Setting.Instance.Copy();
            }

            PhotoFolderName = new ReactiveProperty<string>(_settingData.FolderPath).AddTo(Disposable);
            CacheDataSize = new ReactiveProperty<string>().AddTo(Disposable);
            CanEnter = PhotoFolderName.Select(_ => !string.IsNullOrEmpty(_)).ToReactiveProperty().AddTo(Disposable);
            UseTestFunction = new ReactiveProperty<bool>(_settingData.UseTestFunction).AddTo(Disposable);

            DeleteCacheCommand = new ReactiveCommand().AddTo(Disposable);
            SelectCacheFolderCommand = new ReactiveCommand().AddTo(Disposable);

            PhotoFolderName.Value = _settingData.FolderPath;
            CacheDataSize.Value = FileHelper.DataSize2String(FileHelper.CalcDataSize(Cache.Instance.CacheFolderPath));

            PhotoFolderName.Subscribe(f => _settingData.FolderPath = f);
            UseTestFunction.Subscribe(b => _settingData.UseTestFunction = b);

            DeleteCacheCommand.Subscribe(() =>
            {
                Cache.Instance.DeleteCacheFileAll();
                CacheDataSize.Value = FileHelper.DataSize2String(FileHelper.CalcDataSize(Cache.Instance.CacheFolderPath));
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
                PhotoFolderName.Value = dialog.SelectedPath;
            });
        }

        public void ApplySettingData()
        {
            JsonHelper.ExportJsonFile(_settingData, JsonHelper.GetJsonFilePath());

            var isChangedPhotoFolder = (Setting.Instance.Data?.FolderPath ?? string.Empty) != _settingData.FolderPath;

            Setting.Instance.Data = _settingData;

            if (isChangedPhotoFolder)
            {
                Cache.Instance.DeleteCacheFileAll();
                MainWindow.Instance.Reboot();
            }

        }
    }
}
