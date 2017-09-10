namespace quickpbt

open Xunit
open Xunit.Abstractions
open FsCheck
open FsCheck.Xunit

open DomainUnderTest

/// contains examples of gathering diagnostics abour generated data
///
/// (NOTE: `dotnet test` requires a `--verbosity` of *at least* 'normal' to see observations on the command line)
[<Properties(Arbitrary=[| typeof<Generator> |])>]
module Observations =
  (* a trival observation partions data into one of two buckets *)
  [<Property>]
  let ``supports daylight savings (trivial)`` (anyDate :Date) (anyZone :Zone) (NonNegativeInt total) =
    let days = Time.FromDays(total)

    let addThenShift = Zone.ConvertTime(anyDate + days, anyZone)
    let shiftThenAdd = Zone.ConvertTime(anyDate, anyZone) + days

    addThenShift = shiftThenAdd
      |> Prop.trivial anyZone.SupportsDaylightSavingTime

  (* a classification partions data into one of N, labelled buckets *)
  [<Property>]
  let ``relative meridian position (classify)`` (anyDate :Date) (anyZone :Zone) (NonNegativeInt total) =
    let days = Time.FromDays(total)

    let addThenShift = Zone.ConvertTime(anyDate + days, anyZone)
    let shiftThenAdd = Zone.ConvertTime(anyDate, anyZone) + days

    addThenShift = shiftThenAdd
      |> Prop.classify (anyDate.Offset < Time.Zero) "West of Greenwich"
      |> Prop.classify (anyDate.Offset = Time.Zero) "Within Greenwich"
      |> Prop.classify (anyDate.Offset > Time.Zero) "East of Greenwich"

  (* rather than using a boolean observation, collect reports any value *)
  [<Property>]
  let ``divisibility of added days (collect)`` (anyDate :Date) (anyZone :Zone) (NonNegativeInt total) =
    let days = Time.FromDays(total)

    let addThenShift = Zone.ConvertTime(anyDate + days, anyZone)
    let shiftThenAdd = Zone.ConvertTime(anyDate, anyZone) + days

    addThenShift = shiftThenAdd
      |> Prop.collect (weekdayName anyDate)

  (* observations may be combined as mush as is desired *)
  [<Property>]
  let ``many observations combined`` (anyDate :Date) (anyZone :Zone) (NonNegativeInt total) =
    let days = Time.FromDays(total)

    let addThenShift = Zone.ConvertTime(anyDate + days, anyZone)
    let shiftThenAdd = Zone.ConvertTime(anyDate, anyZone) + days

    addThenShift = shiftThenAdd
      |> Prop.trivial   anyZone.SupportsDaylightSavingTime
      |> Prop.classify  (anyDate.Offset < Time.Zero) "West of Greenwich"
      |> Prop.classify  (anyDate.Offset = Time.Zero) "Within Greenwich"
      |> Prop.classify  (anyDate.Offset > Time.Zero) "East of Greenwich"
      |> Prop.collect   (weekdayName anyDate)
