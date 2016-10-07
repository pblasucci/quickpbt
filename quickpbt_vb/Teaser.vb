Imports Dated = System.DateTimeOffset
Imports Time  = System.TimeSpan
Imports Zone  = System.TimeZoneInfo

''' Contrasts a unit test with a property-based test
Public NotInheritable Class Teaser
  Dim DaysInAWeek   = 7
  Dim HoursInAWeek  = DaysInAWeek * 24 ' hours in a day

  <Fact, Trait("group", "teaser")>
  Public Sub DaysShouldEqualHours ()
    Dim days  = Time.FromDays(DaysInAWeek)
    Dim hours = Time.FromHours(HoursInAWeek)
    Dim civil = Dated.Now 'NOTE: single, hard-coded date value
        
    Assert.Equal (civil + days,civil + hours)
  End Sub

  <[Property], Trait("group", "teaser")>
  Public Function UnitOfTime_ShouldNot_EffectAddition (civil As Dated) As Boolean
    Dim days  = Time.FromDays(DaysInAWeek)
    Dim hours = time.FromHours(HoursInAWeek)
    ' NOTE: lots of different, random date values
    Return civil + days = civil + hours
  End Function

End Class