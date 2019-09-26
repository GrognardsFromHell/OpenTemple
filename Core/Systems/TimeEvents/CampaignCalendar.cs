using SpicyTemple.Core.Time;

namespace SpicyTemple.Core.Systems.TimeEvents
{
    public struct CampaignCalendar
    {
        public const int MonthsPerYear = 13;

        public const int DaysPerMonth = 28;

        public const int DaysPerYear = DaysPerMonth * MonthsPerYear;

        public int Year { get; set; }

        public int Month { get; set; }

        public int Day { get; set; }

        public int Hour { get; set; }

        public int Minute { get; set; }

        public int Second { get; set; }

        public static CampaignCalendar FromElapsedTime(TimePoint elapsed,
            int startingYear,
            int startingDayOfYear,
            int startingHourOfDay)
        {
            // This is the total number of seconds elapsed since the start of the campaign
            var secondsElapsed = elapsed.Time / TimePoint.TicksPerSecond;

            var seconds = secondsElapsed % 60;
            var minutesElapsed = secondsElapsed / 60;
            var minutes = minutesElapsed % 60;
            var hours = startingHourOfDay + minutesElapsed / 60;
            var hourOfDay = hours % 24;

            var days = startingDayOfYear + hours / 24;
            var dayOfYear = days % DaysPerYear;
            var monthOfYear = dayOfYear % DaysPerMonth;

            var year = days / DaysPerYear;

            return new CampaignCalendar
            {
                Year = (int) year,
                Month = (int) monthOfYear,
                Day = (int) dayOfYear,
                Hour = (int) hourOfDay,
                Minute = (int) minutes,
                Second = (int) seconds
            };
        }
    }
}