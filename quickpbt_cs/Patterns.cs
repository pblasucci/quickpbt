using FsCheck;
using FsCheck.Xunit;
using System;

namespace quickpbt
{
  /* domain under test */
  using Date = System.DateTimeOffset;
  using Time = System.TimeSpan;
  using Zone = System.TimeZoneInfo;

  /// <summary>
  /// demonstrates testing for very common properties
  /// </summary>
  public sealed class Patterns
  {
    static readonly string CentralEuroTime = Platform.As(
        win:  () => "Central Europe Standard Time",
        osx:  () => "Europe/Amsterdam",
        unix: () => throw new PlatformNotSupportedException()
    );

    /// <summary>
    /// inversion ... the property by which one action “undoes” the work of another action
    /// </summary>
    [Property]
    public bool adding_and_subtracting_days_are_inverses(Date anyDate, PositiveInt total)
    {
      var days = Time.FromDays((int) total);

      return (anyDate + days) - days == anyDate;
    }

    /// <summary>
    /// interchange ... the property by which the order of two or more actions does not affect the outcome
    /// </summary>
    [Property]
    public bool adding_and_changing_zone_can_be_reordered(Date anyDate, PositiveInt total)
    {
      var days    = Time.FromDays((int) total);
      
      var addThenShift = Zone.ConvertTimeBySystemTimeZoneId(anyDate + days, CentralEuroTime);
      var shiftThenAdd = Zone.ConvertTimeBySystemTimeZoneId(anyDate, CentralEuroTime) + days;

      return addThenShift == shiftThenAdd;
    }

    /// <summary>
    /// invariance ... the property by which something remains constant, despite action being taken
    /// </summary>
    [Property]
    public bool adding_does_not_change_the_date_offset(Date anyDate, PositiveInt months)
    {
      var offset  = anyDate.Offset;
      var shifted = anyDate.AddMonths((int) months);

      return shifted.Offset == offset;
    }

    /// <summary>
    /// idempotence ... the property of an action having the same effect no matter how many times it occurs
    /// </summary>
    [Property]
    public bool taking_a_time_duration_is_idempotent(Time anyTime)
    {
      var once  = anyTime.Duration();
      var twice = anyTime.Duration().Duration();

      return once == twice;
    }
  }
}