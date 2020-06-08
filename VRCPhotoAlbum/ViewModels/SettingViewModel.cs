using Gatosyocora.VRCPhotoAlbum.Helpers;
using Gatosyocora.VRCPhotoAlbum.Models;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Diagnostics;
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


        public SettingViewModel()
        {
            _settingData = Setting.Instance.Data;

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

        public void CreateSettingData()
        {
            JsonHelper.ExportJsonFile(_settingData, JsonHelper.GetJsonFilePath());
            Setting.Instance.Data = _settingData;
        }
    }
}
