using Xunit;
using FsCheck;
using FsCheck.Xunit;

namespace quickpbt
{
  /* domain under test */
  using Date = System.DateTimeOffset;
  using Time = System.TimeSpan;
  using Zone = System.TimeZoneInfo;

  /// <summary>
  /// Contrasts a unit test with a property test
  /// </summary>
  public sealed class Teaser
  {
    static readonly int DaysInAWeek   =  7;
    static readonly int HoursInAWeek  = 24 * DaysInAWeek;

    [Fact]
    public void days_should_equal_hours()
    {
      var today = Date.Now; //NOTE: single, hard-coded value

      var days  = today + Time.FromDays(DaysInAWeek);
      var hours = today + Time.FromHours(HoursInAWeek);

      Assert.Equal(days, hours);
    }

    [Property]
    public bool unit_of_time_should_not_effect_addition(Date anyDate)
    {
      //NOTE: lots of different, random values
      var days  = anyDate + Time.FromDays(DaysInAWeek);
      var hours = anyDate + Time.FromHours(HoursInAWeek);

      return days == hours;
    }
  }
}
