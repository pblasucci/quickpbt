Imports Dated = System.DateTimeOffset
Imports Time  = System.TimeSpan
Imports Zone  = System.TimeZoneInfo
  
''' <summary>
''' cheap helper to get prettier diagnostic output
''' </summary>
Public Enum Divisibility 
  Even  = 0
  Odd   = 1
End Enum

''' <summary>
''' represent a time value which is always greater then zero (> 0)
''' (note: only meant for use with FsCheck's generation functionality)
''' </summary>
Public NotInheritable Class PositiveTime
  Public ReadOnly Property Value As Time

  Public Sub New(value As Time)
    ' PositiveTime encapsulates the following domain rules
    If value <= Time.Zero Then
        Throw New ArgumentException("value must be greater than 0", NameOf(value))
    End If

    Me.Value = value
  End Sub

  Public Overrides Function Equals (obj As Object) As Boolean
    Dim that As PositiveTime = obj
    If  that Is Nothing Then Return False

    Return Me.Value.Equals(that.Value)
  End Function
  
  Public Overrides Function GetHashCode () As Integer 
    Return -1640531527 Xor Me.Value.GetHashCode()
  End Function

  Public Overrides Function ToString () As String
    Return $"PositiveTime({Value})"
  End Function

End Class

''' <summary>
''' contains functions for approximating System.TimeZoneInfo behavior
''' </summary>
Friend Module ZoneUtils
  Private ReadOnly cal = CultureInfo.CurrentCulture.Calendar

  ''' <summary>
  ''' determines the Nth weekday for a given month of a given year (e.g. 2nd Sunday in March 2016)
  ''' </summary>
  Public Function NthDay(numDays As Integer, weekDay As DayOfWeek, year As Integer, month As Integer) As Date
    Dim monthDays = cal.GetDaysInMonth(year, month)
    Dim nthBounds = numDays - 1

    Dim days  = From dayNum in Enumerable.Range(1, monthDays)
                Select New DateTime(year, month, dayNum)
    Dim weeks = From  day In days
                Group day By Key = day.DayOfWeek Into weeksDay = Group
                Where  Key = weekDay
                Select weeksDay.Skip(nthBounds).First()

    Return weeks.Single()
  End Function

  ''' <summary>
  ''' determines (approximately) if a DateTimeOffset is within Daylight Saving Time
  ''' (note: only applies post-2007 rules and only for years 2007 or later)
  ''' </summary>
  Public Function InUnitedStatesDaylightTime(civil As Dated) As Boolean
    If civil.Year < 2007 Then Return False
    ' algorithm only applies rules in effect since 2007
    Select civil.Month
      ' very near boundaries
      Case 3
        Dim secondSunday = NthDay(2,DayOfWeek.Sunday,civil.Year,3)
        Return civil.Day >= secondSunday.Day
      Case 11
        Dim firstSunday = NthDay(1,DayOfWeek.Sunday,civil.Year,11)
        Return civil.Day <= firstSunday.Day
      ' away from boundaries
      Case Else
        If civil.Month < 3 OrElse civil.Month > 11 Then Return False
        Return True
    End Select 
  End Function
End Module
