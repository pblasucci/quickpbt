Imports Dated = System.DateTimeOffset
Imports Timed = System.TimeSpan
Imports Zoned = System.TimeZoneInfo

''' <summary>
''' contains functions for approximating System.TimeZoneInfo behavior
''' </summary>
Public Module Zone
  ReadOnly cal As Calendar = CultureInfo.CurrentCulture.Calendar
  
  ''' <summary>
  ''' determines the Nth weekday for a given month of a given year (e.g. 2nd Sunday in March 2016) 
  ''' </summary>
  Private Function NthDay(range As (NumDays As Integer, WeekDay As Integer),
                          slice As (Year    As Integer, Month   As Integer)) As Date
    Dim monthDays = cal.GetDaysInMonth(slice.Year, slice.Month)
    Dim nthBounds = range.NumDays - 1

    Dim days  = From num In Enumerable.Range(1, monthDays)
                Select New Date(slice.Year, slice.Month, num)

    Dim weeks = From day In days
                Group day By key = day.DayOfWeek Into weeksDay = Group
                Where key = range.WeekDay
                Select weeksDay.Skip(nthBounds).First()

    Return weeks.Single()
  End Function
  
  ''' <summary>
  ''' determines (approximately) if a DateTimeOffset Is within Daylight Saving Time
  ''' (note: only applies post-2007 rules And only for years 2007 Or later)
  Public Function InUnitedStatesDaylightTime(value As Dated) As Boolean
    ' algorithm only applies rules in effect since 2007
    If value.Year < 2007 Then Return False
    
    Select value.Month
      ' very near boundaries
      Case 3
        Dim secondSunday = NthDay((2,DayOfWeek.Sunday), (value.Year, value.Month))
        Return value.Day >= secondSunday.Day

      Case 11
        Dim  firstSunday = NthDay((1,DayOfWeek.Sunday), (value.Year, value.Month))
        Return value.Day <= firstSunday.Day

        ' away from boundaries
      Case Else
        Return value.Month > 3 AndAlso value.Month < 11
    End Select
  End Function

End Module
