Imports Dated = System.DateTimeOffset
Imports Time  = System.TimeSpan
Imports Zone  = System.TimeZoneInfo

''' demonstrates a few different ways to filter inputs to a test
Public NotInheritable Class Filtered
  ' naive test fails (because the range of inputs is too broad)
  <[Property], Trait("group", "filtered")>
  Public Function DaylightSavings_TestOracle_Naive (civil As Dated) As Boolean
    ' NOTE: this test also demonstrates the common pattern of the 
    '       "test oracle" ... using a known-good implementation of 
    '       something to test out an alternate, equivalent implementation
    Dim eastern   = zone.FindSystemTimeZoneById("Eastern Standard Time")
    Dim eastDate  = zone.ConvertTime(civil, eastern)
    Return InUnitedStatesDaylightTime(eastDate) = eastern.IsDaylightSavingTime(eastDate)
  End Function

  ' uses a conditional property to ensure only valid inputs are used
  <[Property], Trait("group", "filtered")>
  Public Function DaylightSavings_TestOracle_Conditional (civil AS Dated) As [Property]
    Dim eastern   = Zone.FindSystemTimeZoneById("Eastern Standard Time")
    Dim eastDate  = Zone.ConvertTime(civil, eastern)
    
    Dim filter = civil.Year >= 2007 AndAlso eastern.IsDaylightSavingTime(eastDate)
    Dim check As Func(Of Boolean) = Function () InUnitedStatesDaylightTime(eastDate)

    Return check.When(filter)      
  End Function

  ' instead of a conditional property, here we use a IArbitrary with a "universal quantifier"
  <[Property], Trait("group", "filtered")>
  Public Function TimeZoneInfo_IsUnchanged_RoundTripSerialization () As [Property]
    ' arbitrary generators can be easily defined
    Dim zones =
      Gen.Elements(from zone in Zone.GetSystemTimeZones() select zone) _
         .ToArbitrary()
    ' "for all" zones, run a test...
    return Prop.ForAll(zones, 
      Function (z)
        Dim deflated = z.ToSerializedString()
        Dim inflated = zone.FromSerializedString(deflated)
        Return z.Equals(inflated)
      End Function)
  
    'NOTE: this test also demonstrates another variation on the "inversion" pattern
  End Function
End Class
