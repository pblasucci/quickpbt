namespace QuickPBT.FS

open Xunit
open FsCheck
open FsCheck.Xunit
open System

/// demonstrates a few different ways to filter inputs to a test
module Filtered =
  (* naive test fails (because the range of inputs is too broad) *)
  [<Property; Trait ("section","filtered")>]
  let ``daylight savings test oracle (naive)`` (civil :date) =
    (**
      NOTE: this test also demonstrates the common pattern of the 
            "test oracle" ... using a known-good implementation of 
            something to test out an alternate, equivalent implementation
    *)
    let eastern   = zone.FindSystemTimeZoneById "Eastern Standard Time"
    let eastDate  = zone.ConvertTime (civil,eastern)
    Zone.inUnitedStatesDaylightTime eastDate = eastern.IsDaylightSavingTime eastDate

  (* uses a conditional property to ensure only valid inputs are used *)
  [<Property; Trait ("section","filtered")>]
  let ``daylight savings test oracle (conditional)`` (civil :date) =
    let eastern   = zone.FindSystemTimeZoneById "Eastern Standard Time"
    let eastDate  = zone.ConvertTime (civil,eastern)
    ( civil.Year >= 2007 
      && eastern.IsDaylightSavingTime eastDate ) 
      ==> lazy Zone.inUnitedStatesDaylightTime eastDate

  (* instead of a conditional property, here we use a IArbitrary with a "universal quantifier" *)
  [<Property; Trait ("section","filtered")>]
  let ``zone is unchanged through round-trip serialization`` () =
    // arbitrary generators can be easily defined
    let zones =
      zone.GetSystemTimeZones ()
      |> Gen.elements
      |> Arb.fromGen
    // "for all" zones, run a test...
    Prop.forAll zones (fun z -> 
      lazy (let deflated = z.ToSerializedString ()
            zone.FromSerializedString deflated = z))
    (**
      NOTE: this test also demonstrates another variation on the "inversion" pattern
    *)
