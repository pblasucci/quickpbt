' Domain Under Test
Imports Dated = System.DateTimeOffset
Imports Timed = System.TimeSpan

''' <summary>
''' contrasts a unit test with a property test
''' </summary>
Public NotInheritable Class Teaser
  ReadOnly DaysInAWeek  As Integer = 7
  Readonly HoursInAWeek As Integer = 24 * DaysInAWeek

  <Fact>
  Public Sub DaysShouldEqualHours()
    Dim today = Dated.Now ' NOTE: single, hard-coded value

    Dim days  = today + Timed.FromDays(daysInAWeek)
    Dim hours = today + Timed.FromHours(hoursInAWeek)

    Assert.Equal(days, hours)
  End Sub

  <[Property]>
  Public Function UnitOfTimeShouldNotEffectAddition(anyDate As Dated) As Boolean
    ' NOTE: lots of different, random values
    Dim days  = anyDate + Timed.FromDays(daysInAWeek)
    Dim hours = anyDate + Timed.FromHours(hoursInAWeek)

    Return days = hours
  End Function
End Class
