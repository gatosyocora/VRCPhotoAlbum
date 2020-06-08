using Gatosyocora.VRCPhotoAlbum.Helpers;
using Gatosyocora.VRCPhotoAlbum.Models;
using Reactive.Bindings;

namespace Gatosyocora.VRCPhotoAlbum.ViewModels
{
    public class SettingViewModel
    {
        private SettingData _settingData;

        public ReactiveProperty<string> PhotoFolderName { get; set; } = new ReactiveProperty<string>();
        public ReactiveProperty<string> CacheDataSize { get; set; } = new ReactiveProperty<string>();

        public ReactiveCommand DeleteCacheCommand { get; set; } = new ReactiveCommand();


        public SettingViewModel(SettingData settingData)
        {
            if (settingData is null)
            {
                _settingData = new SettingData
                {
                    FolderPath = @"D:\VRTools\vrc_meta_tool\meta_pic"
                };
            }
            else
            {
                _settingData = settingData;
            }

            PhotoFolderName.Value = _settingData.FolderPath;
            CacheDataSize.Value = FileHelper.DataSize2String(FileHelper.CalcDataSize(Cache.Instance.CacheFolderPath));

            DeleteCacheCommand.Subscribe(() =>
            {
                Cache.Instance.DeleteCacheFileAll();
                CacheDataSize.Value = FileHelper.DataSize2String(FileHelper.CalcDataSize(Cache.Instance.CacheFolderPath));
            });
        }

        public SettingData CreateSettingData()
        {
            JsonHelper.ExportJsonFile(_settingData, JsonHelper.GetJsonFilePath());
            return _settingData;
        }
    }
}
