using System;
using System.Globalization;
using System.Linq;

namespace quickpbt
{
  using Date = System.DateTimeOffset;

  /// <summary>
  /// contains functions for approximating System.TimeZoneInfo behavior
  /// </summary>
  public static class ZoneUtils
  {
    private static Calendar cal = CultureInfo.CurrentCulture.Calendar;

    // determines the Nth weekday for a given month of a given year (e.g. 2nd Sunday in March 2016)
    private static DateTime NthDay((int NumDays, DayOfWeek WeekDay) range, (int Year, int Month) date)
    {
      var monthDays = cal.GetDaysInMonth(date.Year, date.Month);
      var nthBounds = range.NumDays - 1;
      return Enumerable.Range(1, monthDays)
        .Select (i => new DateTime(date.Year, date.Month, i))
        .GroupBy(d => d.DayOfWeek)
        .Where  (g => g.Key == range.WeekDay)
        .Select (g => 
        {
          var values = g.ToArray();
          return values[nthBounds]; 
        })
        .First();
    }

    /// <summary>
    /// determines (approximately) if a DateTimeOffset is within Daylight Saving Time
    /// (note: only applies post-2007 rules and only for years 2007 or later)
    /// </summary>
    public static bool InUnitedStatesDaylightTime(Date value)
    {
      var year = value.Year;
      // algorithm only applies rules in effect since 2007
      if (year < 2007) { return false; }
      switch (value.Month)
      {
        // very near boundaries
        case var mar when (mar == 3):
          var secondSunday = NthDay((2, DayOfWeek.Sunday), (year, mar));
          return value.Day >= secondSunday.Day;

        case var nov when (nov == 11):
          var firstSunday = NthDay((1, DayOfWeek.Sunday), (year, nov));
          return value.Day <= firstSunday.Day;

        // away from boundaries
        case var month when (month < 3 || month > 11):
          return false;
        
        default:
          return true;
      }
    }
  }
}