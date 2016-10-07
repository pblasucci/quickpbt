namespace QuickPBT.FS

open Xunit
open Xunit.Abstractions
open FsCheck
open FsCheck.Xunit

/// encapsulates several IArbitrary instances
type Generator =
  /// generates arbitrary TimeZoneInfo instances, with no shrinking
  static member TimeZoneInfo =
    zone.GetSystemTimeZones ()
    |> Gen.elements
    |> Arb.fromGen

  /// generates PositiveTime instances by leveraging FsCheck's built-in
  /// support for generating and shrinking TimeSpan instances
  static member PositiveTime =
    let inline isPositive t = t > time.Zero
    Arb.fromGenShrink 
      ( // generator
        Arb.generate<time>
        |> Gen.where isPositive
        |> Gen.map positiveTime
      , // shrinker
        (fun (PositiveTime t) -> 
          Arb.shrink t 
          |> Seq.where isPositive
          |> Seq.map positiveTime) )

/// examples of working with IArbitrary instances
type Generation (out :ITestOutputHelper) =
  (**
    NOTE: here we're using a primary-constructor class, rather than a module,
          because we need to hook into xUnit's I/O mechanisms.
  *)

  /// generates `count` random instances of the given IArbitrary, 
  /// using the given `size` as a seeding value, and returns a sequence of 
  /// values paired with the number of times each value occurs in the sequence
  let distribute size count (arb:Arbitrary<_>) =
    arb.Generator
    |> Gen.sample   size count
    |> Seq.groupBy  id 
    |> Seq.map      (fun (key,value) -> key,Seq.length value)
    |> Seq.sortBy   (fun (_ ,count) -> count)

  /// reports the random distribution of 100 PositiveTime instances
  [<Fact; Trait ("group","generation")>]
  member __.``positive time distribution`` () = 
    out.WriteLine "\n[Distribution of 100 PositiveTime Instances]\n"
    Generator.PositiveTime 
    |> distribute 5 100
    |> Seq.iter (fun (PositiveTime posTime,count) ->
        let bar = String.replicate count "="
        let fts = posTime.ToString "dddddddd'.'hh':'mm':'ss'.'fffffff"
        out.WriteLine <| sprintf "%25s | %2i | %s" fts count bar) 
    out.WriteLine ""

  /// reports the random distribution of 100 TimeZoneInfo instances
  [<Fact; Trait ("group","generation")>]
  member __.``time zone info distribution`` () = 
    out.WriteLine "\n[Distribution of 100 TimeZoneInfo Instances]\n"
    let zones   = Generator.TimeZoneInfo
                  |> distribute 1 100
                  |> Seq.toList
    let length  = ( zones 
                    |> Seq.map (fun (z,_) -> z.StandardName.Length)
                    |> Seq.max )
                  + 4
    zones 
    |> Seq.iter (fun (zone,count) ->
        let bar   = String.replicate count "="
        let name  = zone.StandardName
        out.WriteLine <| sprintf "%-*s | %2i | %s" length name count bar)
    out.WriteLine ""

  (* deonstrates attaching a collection of IArbitrary instances to a test *)
  [<Property (Arbitrary=[| typeof<Generator> |]); Trait ("group","generation")>]
  member __.``zone is unchanged through round-trip serialization`` (target :zone) =
    (**
      NOTE: since we've registered our generator, FsCheck will automatically 
            create `zone`s for use in testing... for fun, compare this test 
            to the one with the same name in the `Filtered` module
    *)
    let deflated = target.ToSerializedString ()
    zone.FromSerializedString deflated = target
