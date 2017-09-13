namespace quickpbt

open FsCheck
open FsCheck.Xunit

open DomainUnderTest

/// demonstrates labelling properties for diagnostic purposes
[<Properties(Arbitrary=[| typeof<Generator> |])>]
module Labelled =
  /// compound property tests can lead to obtuse failures... which propery is at fault?
  [<Property>]
  let ``zone conversion is not affected by detours (naive)`` (anyDate :Date) (zone1 :Zone) (zone2 :Zone) =
    let viaZone1 = Zone.ConvertTime(Zone.ConvertTime(anyDate, zone1), zone2)
    let directly = Zone.ConvertTime(anyDate, zone2)

    (viaZone1 = directly) // same date
    &&
    (directly.Offset = zone2.BaseUtcOffset) // same shift

  /// FsCheck provides tools for labelling individual properties... see the (|@) operator
  [<Property>]
  let ``zone conversion is not affected by detours (labelled)`` (anyDate :Date) (zone1 :Zone) (zone2 :Zone) =
    let viaZone1  = Zone.ConvertTime(Zone.ConvertTime(anyDate, zone1), zone2)
    let directly  = Zone.ConvertTime(anyDate, zone2)
    let sameDate  = (viaZone1 = directly)
    let sameShift = (directly.Offset = zone2.BaseUtcOffset)

    // if either of the following properties does not hold, its label will be printed
    sameDate  |@ sprintf "Same Date?  (%A = %A)" viaZone1 directly
    .&. // <<< note the use a special operator to logically "and" 2 properties together
    sameShift |@ sprintf "Same Shift? (%A = %A)" zone2.BaseUtcOffset directly.Offset

  /// based on the knowledge gained by labelling properties, we can fix the test
  [<Property>]
  let ``zone conversion is not affected by detours (corrected)`` (anyDate :Date) (zone1 :Zone) (zone2 :Zone) =
    let viaZone1  = Zone.ConvertTime(Zone.ConvertTime(anyDate, zone1), zone2)
    let directly  = Zone.ConvertTime(anyDate, zone2)
    let sameDate  = (viaZone1 = directly)
    let sameShift = (directly.Offset = zone2.GetUtcOffset(directly)) // note that we had the wrong check previously

    sameDate  |@ sprintf "Same Date?  (%A = %A)" viaZone1 directly
    .&.
    sameShift |@ sprintf "Same Shift? (%A = %A)" zone2.BaseUtcOffset directly.Offset
