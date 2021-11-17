using System;

namespace Blog.Foundation.Extensions
{
    public static class DateTimeExtension
    {
        public static string FormatDateTime_DDMMYYYY(this DateTime datetime)
        {
            return datetime.ToString("dd-MMM-yyyy");
        }

        public static string FormatDateTime_DDMMYYYYHHMM(this DateTime datetime)
        {
            return datetime.ToString("dd-MMM-yyyy HH MM");
        }

        public static string FormatDateTime_DDMMYYYY(this DateTimeOffset datetime)
        {
            return datetime.ToString("dd-MMM-yyyy");
        }

        public static string FormatDateTime_DDMMYYYYHHMM(this DateTimeOffset datetime)
        {
            return datetime.ToString("dd-MMM-yyyy HH MM");
        }

        public static string FormatDateTime_DDMMYYYYHH(this DateTimeOffset datetime)
        {
            return datetime.ToString("dd-MMM-yyyy");
        }
    }
}
