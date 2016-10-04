namespace QuickPBT.FS

open Xunit
open FsCheck
open FsCheck.Xunit

/// Contrasts a unit test with a property-based test
module Teaser =
  let DaysInAWeek   = 7
  let HoursInAWeek  = DaysInAWeek * 24 // hours in a day

  [<Fact; Trait ("section","teaser")>]
  let DaysShouldEqualHours () =
    let days  = time.FromDays  DaysInAWeek
    let hours = time.FromHours HoursInAWeek
    let civil = date.Now //NOTE: single, hard-coded date value
        
    Assert.Equal (civil + days,civil + hours)

  [<Property; Trait ("section","teaser")>]
  let ``the unit of time should not effect addition`` (civil :date) =
    let days  = time.FromDays  DaysInAWeek
    let hours = time.FromHours HoursInAWeek
    //NOTE: lots of different, random date values
    civil + days = civil + hours
