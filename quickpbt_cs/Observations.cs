using Xunit;
using FsCheck;
using FsCheck.Xunit;

using date = System.DateTimeOffset;
using time = System.TimeSpan;
using zone = System.TimeZoneInfo;

using static QuickPBT.CS.Divisibility;

namespace QuickPBT.CS
{
  /// <summary>
  /// contains examples of gathering diagnostics abour generated data
  /// </summary>
  public static class Observations
  {
    /* a trival observation partions data into one of two buckets */
    [Property (Arbitrary = new []{ typeof(Generator) }), Trait("group", "observations")]
    public static Property SupportsDaylightSavings_Trivial(date civil, zone target, NonNegativeInt total)
    {
      var days = time.FromDays(total.Item);

      var addThenShift = zone.ConvertTime(civil + days, target);
      var shiftThenAdd = zone.ConvertTime(civil, target) + days;
    
      return (addThenShift == shiftThenAdd).Trivial(target.SupportsDaylightSavingTime);
    }

    /* a classification partions data into one of N, labelled buckets */
    [Property (Arbitrary = new []{ typeof(Generator) }), Trait("group", "observations")]
    public static Property RelativeMeridianPosition_Classify(date civil, zone target, NonNegativeInt total)
    {
      var days = time.FromDays(total.Item);

      var addThenShift = zone.ConvertTime(civil + days, target);
      var shiftThenAdd = zone.ConvertTime(civil, target) + days;
    
      return (addThenShift == shiftThenAdd).Classify(civil.Offset <  time.Zero, "West of Greenwich")
                                           .Classify(civil.Offset == time.Zero, "Within Greenwich" )
                                           .Classify(civil.Offset  > time.Zero, "East of Greenwich");
    }

    /* rather than using a boolean observation, collect reports any value */
    [Property (Arbitrary = new []{ typeof(Generator) }), Trait("group", "observations")]
    public static Property DivisibilityOfAddedDays_Collect(date civil, zone target, NonNegativeInt total)
    {
      var days = time.FromDays(total.Item);

      var addThenShift = zone.ConvertTime(civil + days, target);
      var shiftThenAdd = zone.ConvertTime(civil, target) + days;
    
      return (addThenShift == shiftThenAdd).Collect((total.Item % 2 == 0) ? Even : Odd);
    }

    /* observations may be combined as mush as is desired */
    [Property (Arbitrary = new []{ typeof(Generator) }), Trait("group", "observations")]
    public static Property ManyObservationsCombined(date civil, zone target, NonNegativeInt total)
    {
      var days = time.FromDays(total.Item);

      var addThenShift = zone.ConvertTime(civil + days, target);
      var shiftThenAdd = zone.ConvertTime(civil, target) + days;
    
      return (addThenShift == shiftThenAdd).Trivial(target.SupportsDaylightSavingTime)
                                           .Classify(civil.Offset <  time.Zero, "West of Greenwich")
                                           .Classify(civil.Offset == time.Zero, "Within Greenwich" )
                                           .Classify(civil.Offset  > time.Zero, "East of Greenwich")
                                           .Collect((total.Item % 2 == 0) ? Even : Odd);
    }
  }
}
