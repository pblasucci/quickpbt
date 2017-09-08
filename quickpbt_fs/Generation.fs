namespace quickpbt

open FsCheck

module Generated =
  /// represent a time value which is always greater then zero (> 0)
  /// (note: only meant for use with FsCheck's generation functionality)
  type PositiveTime = private PosTime of Time

  /// returns a new PositiveTime instance, throwing an exception on values less than zero
  let positiveTime value =
    if value <= Time.Zero then
      invalidArg "value" "value must be greater than 0"
    PosTime value

  /// extracts the TimeSpan from a PositiveTime instance
  let (|PositiveTime|) (PosTime value) = value

open Generated

/// encapsulates several IArbitrary instances
type Generator =
  /// generates arbitrary TimeZoneInfo instances, with no shrinking
  static member TimeZoneInfo =
    Zone.GetSystemTimeZones ()
    |> Gen.elements
    |> Arb.fromGen

  /// generates PositiveTime instances by leveraging FsCheck's built-in
  /// support for generating and shrinking TimeSpan instances
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
