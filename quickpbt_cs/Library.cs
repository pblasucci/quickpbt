using System;
using System.Globalization;
using System.Linq;

using static Microsoft.VisualBasic.DateAndTime;

namespace QuickPBT.CS
{
  /// <summary>
  /// represent a time value which is always greater then zero (> 0)
  /// (note: only meant for use with FsCheck's generation functionality)
  /// </summary>
  public sealed class PositiveTime
  {
    public TimeSpan Value { get; }

    public PositiveTime(TimeSpan value)
    {
      // PositiveTime encapsulates the following domain rules
      if (value <= TimeSpan.Zero)
      { throw new ArgumentException("value must be greater than 0", nameof(value)); }

      this.Value = value;
    }

    public override Boolean Equals(object obj)
    {
      var that = obj as PositiveTime;
      if (that == null) return false;

      return this.Value.Equals(that.Value);
    }

    public override Int32 GetHashCode() => -1640531527 ^ this.Value.GetHashCode();

    public override String ToString() => $"PositiveTime({Value})";
  }

  /// <summary>
  /// contains functions for approximating System.TimeZoneInfo behavior
  /// </summary>
  internal static class Zone
  {
    private readonly static Calendar cal = CultureInfo.CurrentCulture.Calendar;

    /// <summary>
    /// determines the Nth weekday for a given month of a given year (e.g. 2nd Sunday in March 2016)
    /// </summary>
    public static DateTime NthDay(Int32 numDays, DayOfWeek weekDay, Int32 year, Int32 month)
    {
      var monthDays = cal.GetDaysInMonth(year, month);
      var nthBounds = numDays - 1;

      var days = from dayNum in Enumerable.Range(1, monthDays)
                 select new DateTime(year, month, dayNum);
      var weeks = from day in days
                  group day by day.DayOfWeek into dayOfWeek
                  where dayOfWeek.Key == weekDay
                  select dayOfWeek.Skip(nthBounds).First();

      return weeks.Single();
    }

    /// <summary>
    /// determines (approximately) if a DateTimeOffset is within Daylight Saving Time
    /// (note: only applies post-2007 rules and only for years 2007 or later)
    /// </summary>
    public static Boolean InUnitedStatesDaylightTime(DateTimeOffset civil)
    {
      if (civil.Year < 2007) { return false; }
      // algorithm only applies rules in effect since 2007
      switch (civil.Month)
      {
        // very near boundaries
        case 3:
          var secondSunday = NthDay(2, DayOfWeek.Sunday, civil.Year, 3);
          return civil.Day >= secondSunday.Day;
        case 11:
          var firstSunday = NthDay(1, DayOfWeek.Sunday, civil.Year, 11);
          return civil.Day <= firstSunday.Day;
        // away from boundaries
        default:
          if (civil.Month < 3 || civil.Month > 11) { return false; }
          return true;
      }
    }
  }

  /// <summary>
  /// contains miscellaneous functions for working 
  /// with DateTime, DateTimeOffset, and TimeSpan instances
  /// </summary>
  public static class DateAndTimeExtensions
  {
    /// <summary>
    /// /// Gets the textual value of the day of the week for a given date
    public static String DayOfWeekName(this DateTimeOffset value)
    {
      return WeekdayName((Int32) value.Date.DayOfWeek);
    }
  }
}
