namespace quickpbt

open Xunit
open Xunit.Abstractions
open FsCheck
open FsCheck.Xunit

open DomainUnderTest

/// contains helper types and functions to assist with data generation
[<AutoOpen>]
module Generated =
  /// represents a time value which is always greater then zero (> 0)
  /// (note: only meant for use with FsCheck's generation facilities)
  type PositiveTime = private PosTime of Time

  /// returns a new PositiveTime instance, throwing an exception on values less than zero
  let positiveTime value =
    if value <= Time.Zero then
      invalidArg "value" "value must be greater than 0"
    PosTime value

  /// extracts the TimeSpan from a PositiveTime instance
  let (|PositiveTime|) (PosTime value) = value

/// encapsulates several IArbitrary instances
type Generator =
  /// generates arbitrary TimeZoneInfo instances, with no shrinking
  static member TimeZoneInfo =
    Zone.GetSystemTimeZones()
    |> Gen.elements
    |> Arb.fromGen

  /// generates PositiveTime instances by leveraging FsCheck's
  /// built-in support for generating and shrinking TimeSpan instances
  static member PositiveTime =
    let inline isPositive t = t > Time.Zero
    Arb.fromGenShrink
      ( // generator
        Arb.generate<Time>
        |> Gen.where isPositive
        |> Gen.map positiveTime
      , // shrinker
        (fun (PositiveTime t) ->
          Arb.shrink t
          |> Seq.where isPositive
          |> Seq.map positiveTime) )

/// shows examples of working with data generation (in and out of property tests)
/// (NOTE: `dotnet test` requires a `--verbosity` of *at least* 'normal' to see distribution info on the command line)
type Generation(out :ITestOutputHelper) =
  (**
    NOTE: here we're using a primary-constructor class, rather than a module,
          because we want to hook into xUnit's I/O mechanisms.
  *)

  /// generates `count` random instances of the given IArbitrary,
  /// using the given `size` as a seeding value, and returns a sequence of
  /// values paired with the number of times each value occurs in the sequence
  let distribute size count groupBy (arb:Arbitrary<_>) =
    arb.Generator
    |> Gen.sample   size count
    |> Seq.groupBy  groupBy
    |> Seq.map      (fun (_,items) -> (Seq.head items,Seq.length items))
    |> Seq.sortBy   (fun (_,count) -> count)

  /// reports the random distribution of 100 PositiveTime instances
  [<Fact>]
  member __.``positive time distribution``() =
    out.WriteLine("\n[Distribution of 100 PositiveTime Instances]\n")

    let times = Generator.PositiveTime
                |> distribute 0 100 id
                |> Seq.toList
    for (PositiveTime posTime,count) in times do
      let bar = String.replicate count "="
      let fts = posTime.ToString "dddddddd'.'hh':'mm':'ss'.'fffffff"
      out.WriteLine(sprintf "%25s | %2i | %s" fts count bar)

  /// reports the random distribution of 100 TimeZoneInfo instances
  /// (Note: grouping is done by time zone's offset from UTC)
  [<Fact>]
  member __.``time zone info distribution``() =
    out.WriteLine("\n[Distribution of 100 TimeZoneInfo Instances]\n")

    let zones   = Generator.TimeZoneInfo
                  |> distribute 0 100 (fun z -> z.BaseUtcOffset)
                  |> Seq.toList
    let length  = ( zones
                    |> Seq.map (fun (z,_) -> z.StandardName.Length)
                    |> Seq.max )
                  + 4

    for (zone,count) in zones do
      let bar   =  "=" |> String.replicate count
      let name  = zone.StandardName
      out.WriteLine(sprintf "%-*s | %2i | %s" length name count bar)

  /// demonstrates attaching a collection of IArbitrary instances to a test
  [<Property(Arbitrary=[| typeof<Generator> |])>]
  member __.``zone is unchanged through round-trip serialization``(anyZone :Zone) =
    (**
      NOTE: since we've registered our generator, FsCheck will automatically
            create `zone`s for use in testing... for fun, compare this test
            to the one with the same name in the `Filtered` module
    *)
    Platform.As
      (win = fun () ->  let deflated = anyZone.ToSerializedString()
                        Zone.FromSerializedString(deflated) = anyZone
      ,osx  = fun () -> true
      ,unix = fun () -> true)
      (**
        NOTE: there is a known issue with deserializing TimeZoneInfo on non-Windows OSes
      *)
