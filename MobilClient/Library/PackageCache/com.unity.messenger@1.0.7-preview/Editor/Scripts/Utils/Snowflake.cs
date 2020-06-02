using System;
using System.Globalization;

namespace Unity.Messenger
{
    public partial class Utils
    {
        private static DateTime _epoch = new DateTime(
            year: 2016,
            month: 1,
            day: 1,
            hour: 0,
            minute: 0,
            second: 0,
            DateTimeKind.Utc
        );
        
        public static DateTime Epoch = new DateTime(
            1970,
            1,
            1,
            0,
            0,
            0,
            DateTimeKind.Utc
        );

        public static string CreateNonce()
        {
            var ticks = (long) DateTime.UtcNow.Subtract(_epoch).TotalMilliseconds;
            ++ticks;
            ticks <<= 22;
            --ticks;
            var nonce = Convert.ToString(ticks, 16);
            while (nonce.Length < 16)
            {
                nonce = $"0{nonce}";
            }

            return nonce;
        }

        public static DateTime ExtractTimeFromSnowflakeId(string snowflakeId)
        {
            var value = long.Parse(snowflakeId, NumberStyles.HexNumber);
            value >>= 22;
            return _epoch.Add(new TimeSpan(value * 10000L));
        }
    }
}