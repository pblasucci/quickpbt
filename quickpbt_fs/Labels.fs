namespace QuickPBT.FS

open Xunit
open FsCheck
open FsCheck.Xunit

/// demonstrates labelling properties for diagnostic purposes
module Labels =
  (* compound property tests can lead to obtuse failures... which propery is at fault? *)
  [<Property (Arbitrary=[| typeof<Generator> |]); Trait ("section","labelled")>]
  let ``zone conversion is not affected by detours (naive)`` (civil :date) (zone1 :zone) (zone2 :zone) =
    let viaZone1 = zone.ConvertTime (zone.ConvertTime (civil,zone1),zone2)
    let directly = zone.ConvertTime (civil,zone2)
    (viaZone1 = directly) 
    && 
    (directly.Offset = zone2.BaseUtcOffset)
    
  (* FsCheck propvides tools for labelling individual properties... see the (|@) operator *)  
  [<Property (Arbitrary=[| typeof<Generator> |]); Trait ("section","labelled")>]
  let ``zone conversion is not affected by detours (labelled)`` (civil :date) (zone1 :zone) (zone2 :zone) =
    let viaZone1  = zone.ConvertTime (zone.ConvertTime (civil,zone1),zone2)
    let directly  = zone.ConvertTime (civil,zone2)
    let sameDate  = (viaZone1 = directly)
    let sameShift = (directly.Offset = zone2.BaseUtcOffset)
    // if either of the following properties does not hold, its label will be printed
    sameDate  |@ sprintf "Same Date?  (%A = %A)" viaZone1 directly
    .&. // <<< note the use a special operator to logically "and" 2 properties together
    sameShift |@ sprintf "Same Shift? (%A = %A)" zone2.BaseUtcOffset directly.Offset 

  (* based on the knowledge gained by labelling properties, we can fix the test *)
  [<Property (Arbitrary=[| typeof<Generator> |]); Trait ("section","labelled")>]
  let ``zone conversion is not affected by detours (corrected)`` (civil :date) (zone1 :zone) (zone2 :zone) =
    let viaZone1  = zone.ConvertTime (zone.ConvertTime (civil,zone1),zone2)
    let directly  = zone.ConvertTime (civil,zone2)
    (viaZone1 = directly)
    .&.
    (directly.Offset = zone2.GetUtcOffset directly) // note that we had the wrong check previously
    