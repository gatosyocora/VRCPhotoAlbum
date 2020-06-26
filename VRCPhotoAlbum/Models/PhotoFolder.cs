using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Gatosyocora.VRCPhotoAlbum.Models
{
    [DataContract]
    public class PhotoFolder
    {
        [DataMember(Name = "folderPath")]
        public string FolderPath { get; set; }

        [DataMember(Name = "containsSubFolder")]
        public bool ContainsSubFolder { get; set; }
    }
}
