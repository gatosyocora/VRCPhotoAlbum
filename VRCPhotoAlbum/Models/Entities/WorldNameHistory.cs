using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Gatosyocora.VRCPhotoAlbum.Models.Entities
{
    [Table("WorldNameHistories")]
    public class WorldNameHistory
    {
        [Column(nameof(WorldNameHistoryId))]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int WorldNameHistoryId { get; set; }

        [ForeignKey("WorldId")]
        public virtual World World { get; set; }

        public string WorldName { get; set; }
    }
}
