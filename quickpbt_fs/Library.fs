namespace QuickPBT.FS

open System
open System.Globalization

/// represent a time value which is always greater then zero (> 0)
/// (note: only meant for use with FsCheck's generation functionality)
type PositiveTime = private PositiveTime of value:TimeSpan

[<AutoOpen>]
module internal Libary =
  (* domain under test *)
  type date = System.DateTimeOffset
  type time = System.TimeSpan
  type zone = System.TimeZoneInfo

  /// extension members to facilitate using TimeSpan with Int32
  type System.TimeSpan with
    static member inline FromDays  days   = days  |> float |> time.FromDays
    static member inline FromHours hours  = hours |> float |> time.FromHours

  /// extract the TimeSpan from a PositiveTime instance
  let (|PositiveTime|) (PositiveTime value) = value

  /// constructs a new PositiveTime instance
  let positiveTime value =
    if value <= time.Zero then 
      invalidArg "value" "value must be greater than 0"
    PositiveTime value

  /// cheap helper to get prettier diagnostic output
  type Divisibility = Even | Odd with
    override self.ToString () = match self with Even -> "Even" | Odd -> "Odd"

  /// contains functions for approximating System.TimeZoneInfo behavior
  module Zone = 
    let private cal = CultureInfo.CurrentCulture.Calendar
    
    /// determines the Nth weekday for a given month of a given year (e.g. 2nd Sunday in March 2016)
    let nthDay (numDays,weekDay) (year,month) =
      let monthDays = cal.GetDaysInMonth (year,month)
      let nthBounds = numDays - 1
      seq { for day in 1 .. monthDays -> DateTime (year,month,day) }
      |> Seq.groupBy  (fun theDate    -> theDate.DayOfWeek  )
      |> Seq.filter   (fun (key,_   ) -> key = weekDay      )
      |> Seq.map      (fun (_  ,days) -> days |> Seq.nth nthBounds)
      |> Seq.head

    let private  firstSunday = nthDay (2,DayOfWeek.Sunday)
    let private secondSunday = nthDay (1,DayOfWeek.Sunday)

    /// determines (approximately) if a DateTimeOffset is within Daylight Saving Time
    /// (note: only applies post-2007 rules and only for years 2007 or later)
    let inUnitedStatesDaylightTime (civil :date) =
      match civil.Year,civil.Month with
      // algorithm only applies rules in effect since 2007
      | year,_ when year < 2007 -> false 
      // very near boundaries
      | year,( 3 as mar) -> let bounds = secondSunday (year,mar)
                            civil.Day >= bounds.Day
      | year,(11 as nov) -> let bounds = firstSunday (year,nov)
                            civil.Day <= bounds.Day
      // away from boundaries
      | _,month when month < 3 ||month > 11 -> false
      | _                                   -> true 
