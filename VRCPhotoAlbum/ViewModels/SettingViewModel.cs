using Gatosyocora.VRCPhotoAlbum.Helpers;
using Gatosyocora.VRCPhotoAlbum.Models;
using Reactive.Bindings;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Windows.Forms;

namespace Gatosyocora.VRCPhotoAlbum.ViewModels
{
    public class SettingViewModel
    {
        private SettingData _settingData;

        public ReactiveProperty<string> PhotoFolderName { get; set; } = new ReactiveProperty<string>();
        public ReactiveProperty<string> CacheDataSize { get; set; } = new ReactiveProperty<string>();
        public ReactiveProperty<bool> CanEnter { get; set; }

        public ReactiveCommand DeleteCacheCommand { get; set; } = new ReactiveCommand();
        public ReactiveCommand SelectCacheFolderCommand { get; set; } = new ReactiveCommand();


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

            CanEnter = PhotoFolderName.Select(_ => !string.IsNullOrEmpty(_)).ToReactiveProperty();

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
