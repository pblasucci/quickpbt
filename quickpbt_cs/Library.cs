using System;
using System.Runtime.InteropServices;

namespace quickpbt
{
  /// <summary>
  /// contains helpers for simplifying cross-platform execution
  /// </summary>
  public static class Platform
  {
    /// <summary>
    /// true on Windows platforms
    /// </summary>
    public static readonly bool IsWin = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
    
    /// <summary>
    /// true on Apple platforms
    /// </summary>
    public static readonly bool IsMac = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
    
    /// <summary>
    /// true on *nix platforms
    /// </summary>
    public static readonly bool IsUnix = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

    /// <summary>
    /// executes the appropriate function based on OS platform
    /// </summary>
    public static TResult As<TResult>(Func<TResult> win, Func<TResult> osx, Func<TResult> unix)
    { 
      if (IsWin) { return win(); }
      if (IsMac) { return osx(); }
      
      return unix(); // Unix or Linux
    }
  }

  /// <summary>
  /// contains miscellaneous functions for working 
  /// with DateTime, DateTimeOffset, and TimeSpan instances
  /// </summary>
  public static class DateAndTimeExtensions
  {
    /// <summary>
    /// gets the textual value of the day of the week for a given date
    /// </summary>
    /// <param name="value">the date from which to extract the weekday name</param>
    /// <returns>textual identifier for a weekday</returns>
    public static string DayOfWeekName(this DateTimeOffset value) => Enum.GetName(typeof(DayOfWeek), value.DayOfWeek);
  }
}