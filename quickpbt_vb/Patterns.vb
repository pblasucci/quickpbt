' Domain Under Test
Imports Dated = System.DateTimeOffset
Imports Timed = System.TimeSpan
Imports Zoned = System.TimeZoneInfo

''' <summary>
''' demonstrates testing for very common properties
''' </summary>
Public NotInheritable Class Patterns
  Shared ReadOnly CentralEuroTime As String = Platform.Swap(
      win   := Function() "Central Europe Standard Time",
      mac   := Function() "Europe/Amsterdam",
      unix  := Function() As String
                 throw new PlatformNotSupportedException()
               End Function
  )

  ''' <summary>
  ''' inversion ... the property by which one action “undoes” the work of another action
  ''' </summary>
  <[Property]>
  Public Function AddingAndSubtractingDaysAreInverses(anyDate As Dated, total As PositiveInt) As Boolean
    Dim days = Timed.FromDays(CInt(total))

    Return (anyDate + days) - days = anyDate
  End Function

  ''' <summary>
  ''' interchange ... the property by which the order of two or more actions does not affect the outcome
  ''' </summary>
  <[Property]>
  Public Function AddingAndChangingZoneCanBeReordered(anyDate As Dated, total As PositiveInt) As Boolean
    Dim days = Timed.FromDays(CInt(total))
    
    Dim addThenShift = Zoned.ConvertTimeBySystemTimeZoneId(anyDate + days, CentralEuroTime)
    Dim shiftThenAdd = Zoned.ConvertTimeBySystemTimeZoneId(anyDate, CentralEuroTime) + days

    Return addThenShift = shiftThenAdd
  End Function

  ''' <summary>
  ''' invariance ... the property by which something remains constant, despite action being taken
  ''' </summary>
  <[Property]>
  Public Function AddingDoesNotChangeTheDateOffset(anyDate As Dated, months As PositiveInt) As Boolean
    Dim offset  = anyDate.Offset
    Dim shifted = anyDate.AddMonths(CInt(months))

    Return shifted.Offset = offset
  End Function

  ''' <summary>
  ''' idempotence ... the property of an action having the same effect no matter how many times it occurs
  ''' </summary>
  <[Property]>
  Public Function TakingTimeDurationIsIdempotent(anyTime As Timed) As Boolean
    Dim once  = anyTime.Duration()
    Dim twice = anyTime.Duration().Duration()

    Return once = twice
  End Function
End Class
