using System;
using System.Collections.Generic;
using System.Text;

namespace CsgoDiscordRich
{
    public static class DateTimeExtension
    {
        public static DateTime FromUnixTime(long unixTime)
        {
            return epoch.AddSeconds(unixTime);
        }
        public static long UnixTime(this DateTime dateTime)
        {
            return (long)(dateTime - epoch).TotalSeconds;
        }
        private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    }
}
