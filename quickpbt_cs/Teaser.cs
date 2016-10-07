using System;
using Xunit;
using FsCheck;
using FsCheck.Xunit;

using date = System.DateTimeOffset;
using time = System.TimeSpan;
using zone = System.TimeZoneInfo;

namespace QuickPBT.CS
{
  /// <summary>
  /// Contrasts a unit test with a property-based test
  /// </summary>
  public static class Teaser
  {
    private static readonly Int32 daysInAWeek   = 7;
    private static readonly Int32 hoursInAWeek  = daysInAWeek * 24; // hours in a day

    [Fact, Trait("section", "teaser")]
    public static void DaysShouldEqualHours ()
    {
      var days  = time.FromDays(daysInAWeek);
      var hours = time.FromHours(hoursInAWeek);
      var civil = date.Now; //NOTE: single, hard-coded date value
      
      Assert.Equal(civil + days, civil + hours);
    }

    [Property, Trait("section", "teaser")]
    public static Boolean UnitOfTime_ShouldNot_EffectAddition (date civil)
    {
      var days  = time.FromDays(daysInAWeek);
      var hours = time.FromHours(hoursInAWeek);
      //NOTE: lots of different, random date values

      return (civil + days == civil + hours);
    }
  }
}
