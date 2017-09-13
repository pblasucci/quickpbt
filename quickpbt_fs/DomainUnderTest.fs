namespace quickpbt

open System
open System.Globalization

module DomainUnderTest =
  type Date = System.DateTimeOffset
  type Time = System.TimeSpan
  type Zone = System.TimeZoneInfo

  /// contains functions for approximating System.TimeZoneInfo behavior
  module Zone =
    let private cal = CultureInfo.CurrentCulture.Calendar

    /// determines the Nth weekday for a given month of a given year (e.g. 2nd Sunday in March 2016)
    let private nthDay (numDays,weekDay) (year,month) =
      let monthDays = cal.GetDaysInMonth (year,month)
      let nthBounds = numDays - 1
      seq { for day in 1 .. monthDays -> DateTime(year, month, day) }
      |> Seq.groupBy  (fun theDate    -> theDate.DayOfWeek)
      |> Seq.filter   (fun (key,_   ) -> key = weekDay)
      |> Seq.map      (fun (_  ,days) -> days |> Seq.item nthBounds)
      |> Seq.head

    let private  firstSunday = nthDay (1,DayOfWeek.Sunday)
    let private secondSunday = nthDay (2,DayOfWeek.Sunday)

    /// determines (approximately) if a DateTimeOffset is within Daylight Saving Time
    /// (note: only applies post-2007 rules and only for years 2007 or later)
    let inUnitedStatesDaylightTime (value :Date) =
      match value.Year,value.Month with
      // algorithm only applies rules in effect since 2007
      | year,_ when year < 2007 -> false
      // very near boundaries
      | year,( 3 as mar) -> let bounds = secondSunday (year,mar)
                            value.Day >= bounds.Day
      | year,(11 as nov) -> let bounds = firstSunday (year,nov)
                            value.Day <= bounds.Day
      // away from boundaries
      | _,month when month < 3 ||month > 11 -> false
      | _                                   -> true
