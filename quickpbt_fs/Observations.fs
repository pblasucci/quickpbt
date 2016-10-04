namespace QuickPBT.FS

open Xunit
open FsCheck
open FsCheck.Xunit

/// contains examples of gathering diagnostics abour generated data
module Observations =
  (* a trival observation partions data into one of two buckets *)
  [<Property (Arbitrary=[| typeof<Generator> |]); Trait ("section","observations")>]
  let ``supports daylight savings (trivial)`` (civil :date) (target :zone) (NonNegativeInt total) =
    let days = time.FromDays total

    let addThenShift = zone.ConvertTime (civil + days,target)
    let shiftThenAdd = zone.ConvertTime (civil,target) + days
    
    addThenShift = shiftThenAdd
      |> Prop.trivial target.SupportsDaylightSavingTime

  (* a classification partions data into one of N, labelled buckets *)
  [<Property (Arbitrary=[| typeof<Generator> |]); Trait ("section","observations")>]
  let ``relative meridian position (classify)`` (civil :date) (target :zone) (NonNegativeInt total) =
    let days = time.FromDays total

    let addThenShift = zone.ConvertTime (civil + days,target)
    let shiftThenAdd = zone.ConvertTime (civil,target) + days
    
    addThenShift = shiftThenAdd
      |> Prop.classify (civil.Offset < time.Zero) "West of Greenwich"
      |> Prop.classify (civil.Offset = time.Zero) "Within Greenwich"
      |> Prop.classify (civil.Offset > time.Zero) "East of Greenwich"

  (* rather than using a boolean observation, collect reports any value *)
  [<Property (Arbitrary=[| typeof<Generator> |]); Trait ("section","observations")>]
  let ``divisibility of added days (collect)`` (civil :date) (target :zone) (NonNegativeInt total) =
    let days = time.FromDays total

    let addThenShift = zone.ConvertTime (civil + days,target)
    let shiftThenAdd = zone.ConvertTime (civil,target) + days
    
    addThenShift = shiftThenAdd
      |> Prop.collect (if total % 2 = 0 then Even else Odd)

  (* observations may be combined as mush as is desired *)
  [<Property (Arbitrary=[| typeof<Generator> |]); Trait ("section","observations")>]
  let ``many observations combined`` (civil :date) (target :zone) (NonNegativeInt total) =
    let days = time.FromDays total

    let addThenShift = zone.ConvertTime (civil + days,target)
    let shiftThenAdd = zone.ConvertTime (civil,target) + days
    
    addThenShift = shiftThenAdd
      |> Prop.trivial   target.SupportsDaylightSavingTime
      |> Prop.classify  (civil.Offset < time.Zero) "West of Greenwich"
      |> Prop.classify  (civil.Offset = time.Zero) "Within Greenwich"
      |> Prop.classify  (civil.Offset > time.Zero) "East of Greenwich"
      |> Prop.collect   (if total % 2 = 0 then Even else Odd)
