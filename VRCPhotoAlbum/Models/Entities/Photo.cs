using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Gatosyocora.VRCPhotoAlbum.Models.Entities
{
    [Table("Photos")]
    public class Photo
    {
        [Column(nameof(FilePath))]
        public string FilePath { get; set; }

        [Column(nameof(Date))]
        public DateTime? Date { get; set; }

        [ForeignKey("PhotographerUserId")]
        public virtual User? Photographer { get; set; }

        [ForeignKey("WorldId")]
        public World? World { get; set; }

        public virtual List<PhotoUser> PhotoUsers { get; }

        [NotMapped]
        public IEnumerable<User> Users => PhotoUsers.Select(p => p.User);

        [Column(nameof(Thumbnail))]
        public byte[]? Thumbnail { get; set; }
    }
}
