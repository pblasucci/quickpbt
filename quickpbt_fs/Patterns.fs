namespace QuickPBT.FS

open Xunit
open FsCheck
open FsCheck.Xunit

/// demonstrates testing for very common properties
module Patterns = 
  (* inversion ... the property by which an action and its inverse cancel out *)
  [<Property; Trait ("section","patterns")>]
  let ``adding and subtracting days are inverses`` (civil :date) (PositiveInt total) =
    let days = time.FromDays total
    
    (civil + days) - days = civil

  (* interchange ... the property by which the order of two or more actions does not affect the outcome *)
  [<Property; Trait ("section","patterns")>]
  let ``adding & changing zone can be reordered`` (civil :date) (PositiveInt total) =
    let pacStd = "Pacific Standard Time"
    let days   = time.FromDays total

    let addThenShift = zone.ConvertTimeBySystemTimeZoneId (civil + days,pacStd)
    let shiftThenAdd = zone.ConvertTimeBySystemTimeZoneId (civil,pacStd) + days
    
    addThenShift = shiftThenAdd

  (* invariance ... the property by which something remains constant, despite action being taken *)
  [<Property; Trait ("section","patterns")>]
  let ``adding does not change the date offset`` (civil :date) (PositiveInt months) =
    let offset  = civil.Offset
    let shifted = civil.AddMonths months
    
    shifted.Offset = offset

  (* idempotence ... the property of an action having the same effect no matter how many times it occurs *)
  [<Property; Trait ("section","patterns")>]
  let ``taking a time duration is idempotent`` (value :time) = 
    let once  = value.Duration()
    let twice = value.Duration().Duration()
    
    once = twice
