using Gatosyocora.VRCPhotoAlbum.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

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
            return new SettingData
            {
                FolderPath = FolderName
            };
        }
    }
}
