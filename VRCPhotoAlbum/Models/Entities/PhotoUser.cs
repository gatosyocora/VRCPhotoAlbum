using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Gatosyocora.VRCPhotoAlbum.Models.Entities
{
    [Table("PhotoUsers")]
    public class PhotoUser
    {
        [ForeignKey("FilePath")]
        public virtual Photo Photo { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}
