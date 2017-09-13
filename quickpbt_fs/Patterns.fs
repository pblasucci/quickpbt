namespace quickpbt

open FsCheck
open FsCheck.Xunit

open DomainUnderTest

/// demonstrates testing for very common properties
module Patterns =

  /// inversion ... the property by which one action “undoes” the work of another action
  [<Property>]
  let ``adding and subtracting days are inverses`` (anyDate :Date) (PositiveInt total) =
    let days = Time.FromDays(total)

    (anyDate + days) - days = anyDate

  /// interchange ... the property by which the order of two or more actions does not affect the outcome
  [<Property>]
  let ``adding & changing zone can be reordered`` (anyDate :Date) (PositiveInt total) =
    let days    = Time.FromDays(total)
    let cetStd  = Platform.As
                    (win  = fun () -> "Central Europe Standard Time"
                    ,osx  = fun () -> "Europe/Amsterdam"
                    ,unix = fun () ->  failwith "platform not supported")

    let addThenShift = Zone.ConvertTimeBySystemTimeZoneId(anyDate + days, cetStd)
    let shiftThenAdd = Zone.ConvertTimeBySystemTimeZoneId(anyDate, cetStd) + days

    addThenShift = shiftThenAdd

  /// invariance ... the property by which something remains constant, despite action being taken
  [<Property>]
  let ``adding does not change the date offset`` (anyDate :Date) (PositiveInt months) =
    let offset  = anyDate.Offset
    let shifted = anyDate.AddMonths(months)

    shifted.Offset = offset

  /// idempotence ... the property of an action having the same effect no matter how many times it occurs
  [<Property>]
  let ``taking a time duration is idempotent`` (anyTime :Time) =
    let once  = anyTime.Duration()
    let twice = anyTime.Duration().Duration()

    once = twice
