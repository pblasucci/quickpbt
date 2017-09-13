' Domain Under Test
Imports Dated = System.DateTimeOffset
Imports Timed = System.TimeSpan
Imports Zoned = System.TimeZoneInfo

''' <summary>
''' contains examples of gathering diagnostics abour generated data
''' (NOTE: `dotnet test` requires a `--verbosity` of *at least* 'normal' to see observations on the command line)
''' </summary>
<Properties(Arbitrary := new Type(){ GetType(Generator) })>
Public NotInheritable Class Observations
  ''' <summary>
  ''' a trival observation partions data into one of two buckets
  ''' </summary>
  <[Property]>
  Public Function TrivialDaylightSavingsSupport(anyDate As Dated, anyZone As ZOned, total As NonNegativeInt) As [Property]
    Dim days = Timed.FromDays(CInt(total))

    Dim addThenShift = Zoned.ConvertTime(anyDate + days, anyZone)
    Dim shiftThenAdd = Zoned.ConvertTime(anyDate, anyZone) + days

    Return (addThenShift = shiftThenAdd).Trivial(anyZone.SupportsDaylightSavingTime)
  End Function

  ''' <summary>
  ''' a classification partions data into one of N, labelled buckets
  ''' </summary>
  <[Property]>
  Public Function ClassifyMeridianPosition(anyDate As Dated, anyZone As ZOned, total As NonNegativeInt) As [Property]
    Dim days = Timed.FromDays(CInt(total))

    Dim addThenShift = Zoned.ConvertTime(anyDate + days, anyZone)
    Dim shiftThenAdd = Zoned.ConvertTime(anyDate, anyZone) + days

    Return (addThenShift = shiftThenAdd)                                  _
              .Classify(anyDate.Offset < Timed.Zero, "West of Greenwich") _
              .Classify(anyDate.Offset = Timed.Zero, "Within Greenwich" ) _
              .Classify(anyDate.Offset > Timed.Zero, "East of Greenwich")
  End Function

  ''' <summary>
  ''' rather than using a boolean observation, collect reports any value
  ''' </summary>
  <[Property]>
  Public Function CollectWeekdayName(anyDate As Dated, anyZone As ZOned, total As NonNegativeInt) As [Property]
    Dim days = Timed.FromDays(CInt(total))

    Dim addThenShift = Zoned.ConvertTime(anyDate + days, anyZone)
    Dim shiftThenAdd = Zoned.ConvertTime(anyDate, anyZone) + days

    Return (addThenShift = shiftThenAdd).Collect(anyDate.DayOfWeekName())
  End Function

  ''' <summary>
  ''' observations may be combined as mush as is desired
  ''' </summary>
  <[Property]>
  Public Function ManyObservationsCombined(anyDate As Dated, anyZone As ZOned, total As NonNegativeInt) As [Property]
    Dim days = Timed.FromDays(CInt(total))

    Dim addThenShift = Zoned.ConvertTime(anyDate + days, anyZone)
    Dim shiftThenAdd = Zoned.ConvertTime(anyDate, anyZone) + days

    Return (addThenShift = shiftThenAdd)                                  _
              .Trivial(anyZone.SupportsDaylightSavingTime)                _
              .Classify(anyDate.Offset < Timed.Zero, "West of Greenwich") _
              .Classify(anyDate.Offset = Timed.Zero, "Within Greenwich" ) _
              .Classify(anyDate.Offset > Timed.Zero, "East of Greenwich") _
              .Collect(anyDate.DayOfWeekName())
  End Function
End Class
