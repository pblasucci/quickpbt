Imports Dated = System.DateTimeOffset
Imports Time  = System.TimeSpan
Imports Zone  = System.TimeZoneInfo

''' demonstrates testing for very common properties
Public NotInheritable Class Patterns
  ' inversion ... the property by which an action and its inverse cancel out
  <[Property], Trait("group", "patterns")>
  Public Function Adding_SubtractingDays_AreInverses (civil As Dated, total As PositiveInt) As Boolean
    Dim days = Time.FromDays(total)
    Return (civil + days) - days = civil
  End Function

  ' interchange ... the property by which the order of two or more actions does Not affect the outcome
  <[Property], Trait("group", "patterns")>
  Public Function Adding_ChangingZone_CanBeReordered (civil As Dated, total As PositiveInt) As Boolean
    Dim pacStd = "Pacific Standard Time"
    Dim days   = Time.FromDays(total)

    Dim addThenShift = Zone.ConvertTimeBySystemTimeZoneId(civil + days, pacStd)
    Dim shiftThenAdd = Zone.ConvertTimeBySystemTimeZoneId(civil, pacStd) + days
    
    Return addThenShift = shiftThenAdd
  End Function

  ' invariance ... the property by which something remains constant, despite action being taken
  <[Property], Trait("group", "patterns")>
  Public Function Adding_DoesNotChange_DatesOffset (civil As Dated, months As PositiveInt) As Boolean
    Dim offset  = civil.Offset
    Dim shifted = civil.AddMonths(months)
    
    Return shifted.Offset = offset
  End Function

  ' idempotence ... the property of an action having the same effect no matter how many times it occurs
  <[Property], Trait ("group","patterns")>
  Public Function Taking_TimeDuration_IsIdempotent (value As Time) As Boolean
    Dim once  = value.Duration()
    Dim twice = value.Duration().Duration()
    
    Return once = twice
  End Function
End Class
