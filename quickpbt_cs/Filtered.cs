using System;
using System.Linq;
using Xunit;
using FsCheck;
using FsCheck.Xunit;

using date = System.DateTimeOffset;
using time = System.TimeSpan;
using zone = System.TimeZoneInfo;

namespace QuickPBT.CS
{
  /// <summary>
  /// demonstrates a few different ways to filter inputs to a test
  /// </summary>
  public static class Filtered
  {
    /* naive test fails (because the range of inputs is too broad) */
    [Property, Trait("section", "filtered")]
    public static Boolean DaylightSavings_TestOracle_Naive (date civil)
    {
      /**
        NOTE: this test also demonstrates the common pattern of the 
              "test oracle" ... using a known-good implementation of 
              something to test out an alternate, equivalent implementation
      */
      var eastern   = zone.FindSystemTimeZoneById("Eastern Standard Time");
      var eastDate  = zone.ConvertTime(civil, eastern);
      return (Zone.InUnitedStatesDaylightTime(eastDate) 
              == 
              eastern.IsDaylightSavingTime(eastDate));
    }

    /* uses a conditional property to ensure only valid inputs are used */
    [Property, Trait("section", "filtered")]
    public static Property DaylightSavings_TestOracle_Conditional (date civil)
    {
      var eastern   = zone.FindSystemTimeZoneById("Eastern Standard Time");
      var eastDate  = zone.ConvertTime(civil, eastern);

      var filter = civil.Year >= 2007 && eastern.IsDaylightSavingTime(eastDate);
      Func<Boolean> check = () => Zone.InUnitedStatesDaylightTime(eastDate);
      return check.When(filter);
    }

    /* instead of a condtional property, here we use a IArbitrary with a "universal quantifier" */
    [Property, Trait ("section", "filtered")]
    public static Property TimeZoneInfo_IsUnchanged_RoundTripSerialization ()
    {
      // arbitrary generators can be easily defined
      var zones = Gen.Elements(from z in zone.GetSystemTimeZones() select z)
                     .ToArbitrary();
      
      // "for all" zones, run a test...
      return Prop.ForAll(zones, z =>
      {
        var deflated = z.ToSerializedString();
        var inflated = zone.FromSerializedString(deflated); 
        return z.Equals(inflated);
      });
      /**
        NOTE: this test also demonstrates another variation on the "inversion" pattern
      */
    }
  }
}
