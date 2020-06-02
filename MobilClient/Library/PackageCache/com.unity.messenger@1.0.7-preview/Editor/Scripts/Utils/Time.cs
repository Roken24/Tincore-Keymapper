using System;

namespace Unity.Messenger
{
    public partial class Utils
    {
        private static string PrependZero(int digit)
        {
            var sz = $"0{digit}";
            return sz.Substring(sz.Length - 2);
        }
        public static string FormatDateTime(DateTime dateTime)
        {
            var now = DateTime.Now;
            if (now.Year == dateTime.Year && now.Month == dateTime.Month && now.Day == dateTime.Day)
            {
                return $"{PrependZero(dateTime.Hour)}:{PrependZero(dateTime.Minute)}";
            } else if (dateTime.Date == DateTime.Today - TimeSpan.FromDays(1)) {
                return $"昨天 {PrependZero(dateTime.Hour)}:{PrependZero(dateTime.Minute)}";
            }
            return $"{dateTime.Month}月{dateTime.Day}日";
        } 
        
        public static string DateTimeString(DateTime time, bool showTimeNotToday = true) {
            var localtime = time.ToLocalTime();
            if (showTimeNotToday) {
                return localtime.Date == DateTime.Today
                    ? localtime.ToString("HH:mm")
                    : localtime.Date == DateTime.Today - TimeSpan.FromDays(1)
                        ? $"昨天 {localtime:HH:mm}"
                        : localtime.Year == DateTime.Today.Year
                            ? localtime.ToString("M月d日 HH:mm")
                            : localtime.ToString("yyyy年M月d日 HH:mm");
            }
            else {
                return localtime.Date == DateTime.Today
                    ? localtime.ToString("HH:mm")
                    : localtime.Date == DateTime.Today - TimeSpan.FromDays(1)
                        ? "昨天"
                        : localtime.Year == DateTime.Today.Year
                            ? localtime.ToString("M月d日")
                            : localtime.ToString("yyyy年M月d日");
            }
        }
    }
}