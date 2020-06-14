using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Gatosyocora.VRCPhotoAlbum.Models.Entities
{
    [Table("Users")]
    public class User
    {
        [Column(nameof(UserId))]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }

        [Column(nameof(UserName))]
        public string UserName { get; set; }

        public virtual List<UserNameHistory> DisplayNameHistories { get; set; }

        [Column(nameof(TwitterScreenName))]
        public string TwitterScreenName { get; set; }
    }
}
