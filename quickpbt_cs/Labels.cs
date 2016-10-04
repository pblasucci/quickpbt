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
  /// demonstrates labelling properties for diagnostic purposes
  /// </summary>
  public static class Labels
  {
    /* compound property tests can lead to obtuse failures... which propery is at fault? */
    [Property(Arbitrary = new []{ typeof(Generator) }), Trait("section", "labelled")]
    public static Boolean ZoneConversion_NotAffectedByDetours_Naive (date civil, zone zone1, zone zone2)
    {
      var viaZone1 = zone.ConvertTime(zone.ConvertTime(civil, zone1), zone2);
      var directly = zone.ConvertTime(civil, zone2);

      return  (viaZone1 == directly) 
              && 
              (directly.Offset == zone2.BaseUtcOffset);
    }
    
    /* FsCheck propvides tools for labelling individual properties */
    [Property(Arbitrary = new []{ typeof(Generator) }), Trait("section", "labelled")]
    public static Property ZoneConversion_NotAffectedByDetours_Labelled (date civil, zone zone1, zone zone2)
    {
      var viaZone1 = zone.ConvertTime(zone.ConvertTime(civil, zone1), zone2);
      var directly = zone.ConvertTime(civil, zone2);
      
      Func<bool> sameDate  = (() => viaZone1 == directly);
      Func<bool> sameShift = (() => directly.Offset == zone2.BaseUtcOffset);
      
      // if either of the following properties does not hold, its label will be printed
      return  sameDate.Label($"Same Date? {viaZone1} = {directly}")
              .And(
              sameShift.Label($"Same Date? {zone2.BaseUtcOffset} = {directly.Offset}"));
    }

    /* based on the knowledge gained by labelling properties, we can fix the test */
    [Property(Arbitrary = new []{ typeof(Generator) }), Trait("section", "labelled")]
    public static Property ZoneConversion_NotAffectedByDetours_Corrected (date civil, zone zone1, zone zone2)
    {
      var viaZone1 = zone.ConvertTime(zone.ConvertTime(civil, zone1), zone2);
      var directly = zone.ConvertTime(civil, zone2);

      Func<bool> sameDate  = (() => viaZone1 == directly);
      Func<bool> sameShift = (() => directly.Offset == zone2.GetUtcOffset(directly));
      // note that we had the wrong check previously

      return sameDate.ToProperty().And(sameShift);
    }
  }
}
