using Gatosyocora.VRCPhotoAlbum.Helpers;
using Gatosyocora.VRCPhotoAlbum.Models;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
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

        public ReactiveCommand DeleteCacheCommand { get; }
        public ReactiveCommand SelectCacheFolderCommand { get; }


        public SettingViewModel(SettingData settingData)
        {
            if (settingData is null)
            {
                _settingData = new SettingData
                {
                    FolderPath = string.Empty
                };
            }
            else
            {
                _settingData = settingData;
            }

            PhotoFolderName.Value = _settingData.FolderPath;
            CacheDataSize.Value = FileHelper.DataSize2String(FileHelper.CalcDataSize(Cache.Instance.CacheFolderPath));

            PhotoFolderName = new ReactiveProperty<string>().AddTo(disposes);
            CacheDataSize = new ReactiveProperty<string>().AddTo(disposes);
            CanEnter = PhotoFolderName.Select(_ => !string.IsNullOrEmpty(_)).ToReactiveProperty().AddTo(disposes);

            DeleteCacheCommand = new ReactiveCommand().AddTo(disposes);
            SelectCacheFolderCommand = new ReactiveCommand().AddTo(disposes);

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

        public SettingData CreateSettingData()
        {
            JsonHelper.ExportJsonFile(_settingData, JsonHelper.GetJsonFilePath());
            return _settingData;
        }
    }
}
