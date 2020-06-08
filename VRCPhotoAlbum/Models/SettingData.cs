using System;
using System.Runtime.Serialization;

namespace Gatosyocora.VRCPhotoAlbum.Models
{
    [DataContract]
    public class SettingData
    {
        [DataMember(Name = "folderPath")]
        public String FolderPath { get; set; }
        
        [DataMember(Name = "useTestFunction")]
        public bool UseTestFunction { get; set; } 
    }
}
