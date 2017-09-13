''' <summary>
''' contains helpers for simplifying cross-platform execution
''' </summary>
Module Platform
  ''' <summary>
  ''' true on Windows platforms
  ''' </summary>
  Public ReadOnly IsWin As Boolean = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)

  ''' <summary>
  ''' true on Apple platforms
  ''' </summary>
  Public Readonly IsMac As Boolean = RuntimeInformation.IsOSPlatform(OSPlatform.OSX)

  ''' <summary>
  ''' true on *nix platforms
  ''' </summary>
  Public ReadOnly IsUnix As Boolean = RuntimeInformation.IsOSPlatform(OSPlatform.Linux)

  ''' <summary>
  ''' executes the appropriate function based on OS platform
  ''' </summary>
  Public Function Swap(Of TResult)(win   As Func(Of TResult),
                                   mac   As Func(Of TResult),
                                   unix  As Func(Of TResult)) As TResult
    If IsWin Then Return win()
    If IsMac Then Return mac()
      
    Return unix() ' Unix or Linux
  End Function
End Module


''' <summary>
''' contains miscellaneous functions for working 
''' with DateTime, DateTimeOffset, and TimeSpan instances
''' </summary>
module DateAndTimeUtils
  ''' <summary>
  ''' gets the textual value of the day of the week for a given date
  ''' </summary>
  ''' <param name="value">the date from which to extract the weekday name</param>
  ''' <returns>textual identifier for a weekday</returns>
  <Extension>
  Public Function DayOfWeekName(value As DateTimeOffset) As String
    Return [Enum].GetName(GetType(DayOfWeek), value.DayOfWeek)
  End Function
End Module
