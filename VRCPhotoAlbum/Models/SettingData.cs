using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Gatosyocora.VRCPhotoAlbum.Models
{
    [DataContract]
    public class SettingData
    {
        [DataMember(Name ="folderPath")]
        public String FolderPath { get; set; }
    }
}
