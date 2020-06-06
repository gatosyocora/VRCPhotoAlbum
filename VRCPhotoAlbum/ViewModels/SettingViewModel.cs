using Gatosyocora.VRCPhotoAlbum.Helpers;
using Gatosyocora.VRCPhotoAlbum.Models;

namespace Gatosyocora.VRCPhotoAlbum.ViewModels
{
    public class SettingViewModel
    {
        private SettingData _settingData;

        public string FolderName { get; set; } = @"D:\VRTools\vrc_meta_tool\meta_pic";

        public SettingViewModel(SettingData settingData)
        {
            if (settingData is null)
            {
                _settingData = new SettingData
                {
                    FolderPath = FolderName
                };
            }
            else
            {
                _settingData = settingData;
            }

            FolderName = _settingData.FolderPath;
        }

        public SettingData CreateSettingData()
        {
            JsonHelper.ExportJsonFile(_settingData, JsonHelper.GetJsonFilePath());
            return _settingData;
        }
    }
}
