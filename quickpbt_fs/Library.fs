namespace quickpbt

open System

[<AutoOpen>]
module internal Library =
  /// Gets the textual value of the day of the week for a given date
  let weekdayName (value :DateTimeOffset) =
    // value.Date.DayOfWeek |> int |> DateAndTime.WeekdayName
    //TODO: figure out how to load VB modules
    "???"

  /// extension members to facilitate using TimeSpan with Int32
  type TimeSpan with
    /// Returns a TimeSpan that represents a specified number of days, where the specification is accurate to the nearest second
    static member inline FromDays(value :int) = TimeSpan.FromDays(float value)
    /// Returns a TimeSpan that represents a specified number of hours, where the specification is accurate to the nearest second
    static member inline FromHours(value :int) = TimeSpan.FromHours(float value)
