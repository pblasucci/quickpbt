namespace quickpbt

open System
open System.Runtime.InteropServices

[<AutoOpen>]
module internal Library =
  /// contains helpers for simplifying cross-platform execution
  [<RequireQualifiedAccess>]
  type Platform =
    /// true on Windows platforms
    static member IsWin = RuntimeInformation.IsOSPlatform OSPlatform.Windows
    /// true on Apple platforms
    static member IsMac = RuntimeInformation.IsOSPlatform OSPlatform.OSX
    /// true on *nix platforms
    static member IsUnix = RuntimeInformation.IsOSPlatform OSPlatform.Linux

    /// executes the appropriate function based on OS platform
    static member inline As(win,osx,unix) =
      if    Platform.IsWin then win  ()
      elif  Platform.IsMac then osx  ()
      else  (* Unix or Linux *) unix ()

  /// Gets the textual value of the day of the week for a given date
  let weekdayName (value :DateTimeOffset) = Enum.GetName(typeof<DayOfWeek>, value.DayOfWeek)

  /// extension members to facilitate using TimeSpan with Int32
  type TimeSpan with
    /// Returns a TimeSpan that represents a specified number of days, where the specification is accurate to the nearest second
    static member inline FromDays(value :int) = TimeSpan.FromDays(float value)
    /// Returns a TimeSpan that represents a specified number of hours, where the specification is accurate to the nearest second
    static member inline FromHours(value :int) = TimeSpan.FromHours(float value)
