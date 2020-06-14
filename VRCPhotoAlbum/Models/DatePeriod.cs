using System;
using System.Collections.Generic;
using System.Text;

namespace Gatosyocora.VRCPhotoAlbum.Models
{
    public class DatePeriod
    {
        public DateTime SinceDate { get; }
        public DateTime UntilDate { get; }

        public DatePeriod(DateTime since, DateTime until)
        {
            SinceDate = since;
            UntilDate = until;
        }

    }
}
