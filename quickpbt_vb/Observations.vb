Imports Dated = System.DateTimeOffset
Imports Time  = System.TimeSpan
Imports Zone  = System.TimeZoneInfo

''' <summary>
''' contains examples of gathering diagnostics abour generated data
''' </summary>
Public NotInheritable Class Observations
  ' a trival observation partions data into one of two buckets
  <[Property] (Arbitrary := { GetType(Generator) }), Trait("group", "observations")>
  Public Function SupportsDaylightSavings_Trivial (civil As Dated, target As Zone, total As NonNegativeInt) As [Property]
    Dim days = Time.FromDays(total.Item)

    Dim addThenShift = Zone.ConvertTime(civil + days, target)
    Dim shiftThenAdd = Zone.ConvertTime(civil, target) + days
    
    Return (addThenShift = shiftThenAdd).Trivial(target.SupportsDaylightSavingTime)
  End Function

  ' a classification partions data into one of N, labelled buckets
  <[Property] (Arbitrary := { GetType(Generator) }), Trait("group", "observations")>
  Public Function RelativeMeridianPosition_Classify (civil As Dated, target As Zone, total As NonNegativeInt) As [Property]
    Dim days = Time.FromDays(total.Item)

    Dim addThenShift = Zone.ConvertTime(civil + days, target)
    Dim shiftThenAdd = Zone.ConvertTime(civil, target) + days
    
    Return (addThenShift = shiftThenAdd).Classify(civil.Offset < Time.Zero, "West of Greenwich") _
                                        .Classify(civil.Offset = Time.Zero, "Within Greenwich" ) _
                                        .Classify(civil.Offset > Time.Zero, "East of Greenwich")
  End Function

  ' rather than using a boolean observation, collect reports any value
  <[Property] (Arbitrary := { GetType(Generator) }), Trait("group", "observations")>
  Public Function DivisibilityOfAddedDays_Collect (civil As Dated, target As Zone, total As NonNegativeInt) As [Property]
    Dim days = Time.FromDays(total.Item)

    Dim addThenShift = Zone.ConvertTime(civil + days, target)
    Dim shiftThenAdd = Zone.ConvertTime(civil, target) + days
    
    Return (addThenShift = shiftThenAdd).Collect(WeekdayName(civil.DayOfWeek))
  End Function

  ' observations may be combined as mush as is desired
  <[Property] (Arbitrary := { GetType(Generator) }), Trait("group", "observations")>
  Public Function ManyObservationsCombined (civil As Dated, target As Zone, total As NonNegativeInt) As [Property]
    Dim days = Time.FromDays(total.Item)

    Dim addThenShift = Zone.ConvertTime(civil + days, target)
    Dim shiftThenAdd = Zone.ConvertTime(civil, target) + days
    
    Return (addThenShift = shiftThenAdd).Trivial(target.SupportsDaylightSavingTime)               _
                                        .Classify(civil.Offset < Time.Zero, "West of Greenwich")  _
                                        .Classify(civil.Offset = Time.Zero, "Within Greenwich" )  _
                                        .Classify(civil.Offset > Time.Zero, "East of Greenwich")  _
                                        .Collect(WeekdayName(civil.DayOfWeek))
  End Function
End Class
