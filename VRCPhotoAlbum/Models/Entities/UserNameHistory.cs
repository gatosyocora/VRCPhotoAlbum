using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Gatosyocora.VRCPhotoAlbum.Models.Entities
{
    [Table("UserNameHistories")]
    public class UserNameHistory
    {
        [Column(nameof(UserNameHistoryId))]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserNameHistoryId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        public string UserName { get; set; }
    }
}
