namespace quickpbt

open Xunit
open FsCheck
open FsCheck.Xunit

open DomainUnderTest

/// demonstrates a few different ways to filter inputs to a test
module Filtered =
  (* naive test fails (because the range of inputs is too broad) *)
  [<Property>]
  let ``daylight savings test oracle (naive)`` (anyDate :Date) =
    (**
      NOTE: this test also demonstrates the common pattern of the
            "test oracle" ... using a known-good implementation of
            something to test out an alternate, equivalent implementation
    *)
    let eastern   = Zone.FindSystemTimeZoneById("America/New_York")
    let eastDate  = Zone.ConvertTime(anyDate, eastern)
    Zone.inUnitedStatesDaylightTime eastDate = eastern.IsDaylightSavingTime(eastDate)

  (* uses a conditional property to ensure only valid inputs are used *)
  [<Property>]
  let ``daylight savings test oracle (conditional)`` (anyDate :Date) =
    let eastern   = Zone.FindSystemTimeZoneById("America/New_York")
    let eastDate  = Zone.ConvertTime(anyDate, eastern)
    ( anyDate.Year >= 2007 && eastern.IsDaylightSavingTime eastDate )
      ==> lazy Zone.inUnitedStatesDaylightTime(eastDate)

  (* instead of a conditional property, here we use a IArbitrary with a "universal quantifier" *)
  [<Property>]
  let ``zone is unchanged through round-trip serialization`` () =
    // arbitrary generators can be easily defined
    let zones =
      Zone.GetSystemTimeZones()
      |> Gen.elements
      |> Arb.fromGen
    // "for all" zones, run a test...
    Prop.forAll zones (fun z ->
      lazy (let deflated = z.ToSerializedString()
            Zone.FromSerializedString(deflated) = z))
    (**
      NOTE: this test also demonstrates another variation on the "inversion" pattern
    *)
