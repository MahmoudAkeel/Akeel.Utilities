    using System;

namespace Akeel.Utilities.ExtensionMethods
{
    public static class DateTimeExtension
    {
        public static DateTime ToLastDayTime(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 23, 59, 59);
        }

        public static DateTime ToStartDayTime(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0);
        }

        public static string ToArabicDayName(this DateTime dateTime)
        {
            switch (dateTime.DayOfWeek)
            {

                case DayOfWeek.Saturday: return "السبت";
                case DayOfWeek.Sunday: return "الأحد";
                case DayOfWeek.Monday: return "الاثنين";
                case DayOfWeek.Tuesday: return "الثلاثاء";
                case DayOfWeek.Wednesday: return "الأربعاء";
                case DayOfWeek.Thursday: return "الخميس";
                case DayOfWeek.Friday: return "الجمعة";
            }

            return string.Empty;



        }
    }
}
