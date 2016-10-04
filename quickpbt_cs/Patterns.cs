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
  /// demonstrates testing for very common properties
  /// </summary>
  public static class Patterns
  {
    /* inversion ... the property by which an action and its inverse cancel out */
    [Property, Trait("section", "patterns")]    
    public static Boolean Adding_SubtractingDays_AreInverses (date civil, PositiveInt total)
    {
      var days = time.FromDays(total.Item);
      return ((civil + days) - days == civil);
    }

    /* interchange ... the property by which the order of two or more actions does not affect the outcome */
    [Property, Trait("section", "patterns")]
    public static Boolean Adding_ChangingZone_CanBeReordered (date civil, PositiveInt total)
    {
      var pacStd = "Pacific Standard Time";
      var days   = time.FromDays(total.Item);

      var addThenShift = zone.ConvertTimeBySystemTimeZoneId(civil + days, pacStd);
      var shiftThenAdd = zone.ConvertTimeBySystemTimeZoneId(civil, pacStd) + days;
    
      return (addThenShift == shiftThenAdd);
    }

    /* invariance ... the property by which something remains constant, despite action being taken */
    [Property, Trait("section", "patterns")]
    public static Boolean Adding_DoesNotChange_DatesOffset (date civil, PositiveInt months)
    {
      var offset  = civil.Offset;
      var shifted = civil.AddMonths(months.Item);
    
      return (shifted.Offset == offset);
    }

    /* idempotence ... the property of an action having the same effect no matter how many times it occurs */
    [Property, Trait ("section", "patterns")]
    public static Boolean Taking_TimeDuration_IsIdempotent (time value)
    {
      var once  = value.Duration();
      var twice = value.Duration().Duration();
    
      return (once == twice);
    } 
  }
}
