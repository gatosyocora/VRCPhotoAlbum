using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Gatosyocora.VRCPhotoAlbum.Models
{
    [DataContract]
    public class SettingData
    {
        [DataMember(Name = "photoFolders")]
        public List<PhotoFolder> PhotoFolders { get; set; }

        [DataMember(Name = "useTestFunction")]
        public bool UseTestFunction { get; set; }
    }
}
