using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Gatosyocora.VRCPhotoAlbum.Models
{
    public class DatePeriod
    {
        public DateTime SinceDate { get; }
        public DateTime UntilDate { get; }

        public DatePeriod(string since, string until)
        {
            SinceDate = DateTime.Parse(since, new CultureInfo("en-US"));
            UntilDate = DateTime.Parse(until, new CultureInfo("en-US"));
        }

    }
}
