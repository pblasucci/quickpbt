using FsCheck;
using FsCheck.Xunit;

namespace quickpbt
{
  /* domain under test */
  using Date = System.DateTimeOffset;
  using Zone = System.TimeZoneInfo;

  /// <summary>
  /// demonstrates labelling properties for diagnostic purposes
  /// </summary>
  [Properties(Arbitrary = new []{ typeof(Generator) })]
  public sealed class Labelled
  {
    /// <summary>
    /// compound property tests can lead to obtuse failures... which propery is at fault?
    /// </summary>
    [Property]
    public bool zone_conversion_is_not_affected_by_detours_naive(Date anyDate, Zone zone1, Zone zone2)
    { 
      var viaZone1 = Zone.ConvertTime(Zone.ConvertTime(anyDate, zone1), zone2);
      var directly = Zone.ConvertTime(anyDate, zone2);

      return (viaZone1 == directly)                     // same date
          && (directly.Offset == zone2.BaseUtcOffset);  // same shift
    }

    /// <summary>
    /// FsCheck provides tools for labelling individual properties... see the .Label method
    /// </summary>
    [Property]
    public Property zone_conversion_is_not_affected_by_detours_labelled(Date anyDate, Zone zone1, Zone zone2)
    { 
      var viaZone1 = Zone.ConvertTime(Zone.ConvertTime(anyDate, zone1), zone2);
      var directly = Zone.ConvertTime(anyDate, zone2);
      
      bool sameDate () => (viaZone1 == directly);
      bool sameShift() => (directly.Offset == zone2.BaseUtcOffset);
      
      return sameDate ().Label($"Same Date?  ({viaZone1} = {directly})")
      // if either of these properties (^^^/vvv) does *not* hold, its label will be printed
        .And(sameShift().Label($"Same Shift? ({zone2.BaseUtcOffset} = {directly.Offset})"));
    }

    /// <summary>
    ///  based on the knowledge gained by labelling properties, we can fix the test
    /// </summary>
    [Property]
    public Property zone_conversion_is_not_affected_by_detours_corrected(Date anyDate, Zone zone1, Zone zone2)
    { 
      var viaZone1 = Zone.ConvertTime(Zone.ConvertTime(anyDate, zone1), zone2);
      var directly = Zone.ConvertTime(anyDate, zone2);
      
      bool sameDate () => (viaZone1 == directly);
      bool sameShift() => (directly.Offset == zone2.GetUtcOffset(directly)); // note that we had the wrong check previously
      
      return sameDate ().Label($"Same Date?  ({viaZone1} = {directly})")
        .And(sameShift().Label($"Same Shift? ({zone2.BaseUtcOffset} = {directly.Offset})"));
    }
  }
}
