using System;

namespace ProjectDaydream.Common.Extensions
{
    public static partial class Extensions
    {
        public static TimeSpan CalculateDifference(this TimeSpan from, TimeSpan to)
        {
            var difference = to - from;
            return difference.TotalHours < 0 ? difference + TimeSpan.FromDays(1) : difference;
        }
    }
}