﻿using Gatosyocora.VRCPhotoAlbum.Helpers;
using System.IO;

namespace Gatosyocora.VRCPhotoAlbum.Models
{
    public class Setting
    {
        public static Setting Instance { get; }

        public SettingData Data { get; set; }
        static Setting()
        {
            Instance = new Setting();
        }

        public Setting()
        {
            var jsonFilePath = JsonHelper.GetJsonFilePath();
            if (File.Exists(jsonFilePath))
            {
                Data = JsonHelper.ImportJsonFile<SettingData>(jsonFilePath);
            }
            else
            {
                Data = new SettingData
                {
                    FolderPath = string.Empty,
                    UseTestFunction = false
                };
            }
        }

        public SettingData Copy()
        {
            return new SettingData()
            {
                FolderPath = Data.FolderPath,
                UseTestFunction = Data.UseTestFunction
            };
        }
    }
}
