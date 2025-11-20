using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Northstar.Core.Extensions
{
    public static class SimpleExtensions
    {
        public static int? ToNullableInt32(this string s)
        {
            int i;
            if (Int32.TryParse(s, out i)) return i;
            return null;
        }

        public static decimal? ToNullableDecimal(this string s)
        {
            decimal i;
            if (Decimal.TryParse(s, out i)) return i;
            return null;
        }

        public static bool? ToNullableBool(this string s)
        {
            bool i;
            if (Boolean.TryParse(s, out i)) return i;
            return null;
        }

        public static DateTime? ToNullableDate(this string s)
        {
            DateTime i;
            if (DateTime.TryParse(s, out i)) return i;
            return null;
        }
        public static DateTime? ToNullableDateExplictFormat(this string s, string format)
        {
            DateTime i;

            if (string.IsNullOrEmpty(format))
            {
                return s.ToNullableDate();
            }

            if (DateTime.TryParseExact(s, format, CultureInfo.CurrentCulture, DateTimeStyles.None, out i)) return i;
            return null;
        }
    }
}
