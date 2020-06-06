using Gatosyocora.VRCPhotoAlbum.Helpers;
using Gatosyocora.VRCPhotoAlbum.Models;

namespace Gatosyocora.VRCPhotoAlbum.ViewModel
{
    public class SettingViewModel
    {
        public string FolderName { get; set; } = @"D:\VRTools\vrc_meta_tool\meta_pic";

        public SettingViewModel()
        {

        }

        public SettingData CreateSettingData()
        {
            var settingData = new SettingData
            {
                FolderPath = FolderName
            };

            var jsonFilePath = JsonHelper.GetJsonFilePath();
            JsonHelper.ExportJsonFile(settingData, jsonFilePath);

            return settingData;
        }
    }
}
