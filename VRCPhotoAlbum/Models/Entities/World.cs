using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Gatosyocora.VRCPhotoAlbum.Models.Entities
{
    [Table("Worlds")]
    public class World
    {
        [Column(nameof(WorldId))]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int WorldId { get; set; }

        [Column(nameof(WorldName))]
        public string WorldName { get; set; }

        public virtual List<WorldNameHistory> WorldNameHistories { get; }
    }
}
