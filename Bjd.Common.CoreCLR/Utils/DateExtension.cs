using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bjd.Utils
{
    public static class DateExtension
    {
        
        public static string ToDateTimeString(this DateTime val)
        {
            //return dt.ToShortDateString() + " " + dt.ToLongTimeString();
            var formatDate = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
            var formatTime = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.LongTimePattern;
            return val.ToString(formatDate) + " " + val.ToString(formatTime);
        }


    }
}
