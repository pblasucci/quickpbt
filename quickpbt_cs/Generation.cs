using Xunit;
using Xunit.Abstractions;
using FsCheck;
using FsCheck.Xunit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace quickpbt
{
  /* domain under test */
  using Time = System.TimeSpan;
  using Zone = System.TimeZoneInfo;
  
  /// <summary>
  /// represent a time value which is always greater then zero (> 0)
  /// (note: only meant for use with FsCheck's generation functionality)
  /// </summary>
  public sealed class PositiveTime
  {
    /// <summary>
    /// extracts the TimeSpan from a PositiveTime instance
    /// </summary>
    public Time Value { get; }

    /// <summary>
    /// returns a new PositiveTime instance, throwing an exception on values less than zero
    /// </summary>
    /// <param name="value">a time with a duration of greater than zero (> 0)</param>
    public PositiveTime(Time value)
    {
      if (value <= Time.Zero)
        { throw new ArgumentOutOfRangeException(nameof(value), "value must be greater than 0"); }
          
      Value = value;
    }

    public override int GetHashCode() => 397 ^ Value.GetHashCode();

    public override bool Equals(object obj)
    { 
      if (ReferenceEquals(obj, null)) { return false; }
      if (ReferenceEquals(obj, this)) { return true;  }
      return (obj is PositiveTime that && that.Value == this.Value);
    }

    public override string ToString() => $"PositiveTime({Value})";

    public static implicit operator PositiveTime(Time value) => new PositiveTime(value);
    public static implicit operator Time(PositiveTime positive) => positive.Value;
  }
  
  /// <summary>
  /// encapsulates several IArbitrary instances
  /// </summary>
  public static class Generator
  { 
    /// <summary>
    /// generates arbitrary TimeZoneInfo instances, with no shrinking
    /// </summary>
    /// <returns>an IArbitrary capable of producing TimeZoneInfo instances</returns>
    public static Arbitrary<Zone> TimeZoneInfo()
      => Gen.Elements(from z in Zone.GetSystemTimeZones() select z).ToArbitrary();

    /// <summary>
    /// generates PositiveTime instances by leveraging FsCheck's 
    /// built-in support for generating and shrinking TimeSpan instances
    /// </summary>
    /// <returns>an IArbitrary capable of producing PositiveTime instances</returns>
    public static Arbitrary<PositiveTime> PositiveTime()
    {
      bool IsPositive(Time value) => value > Time.Zero;

      return Arb.From(
        // generator
        from t in Arb.Generate<Time>() 
        where IsPositive(t) 
        select new PositiveTime(t),
        // shrinker
        positive => 
          from t in Arb.Shrink(positive.Value) 
          where IsPositive(t) 
          select new PositiveTime(t)
      );
    }
    
    /// <summary>
    /// generates `count` random instances of the given IArbitrary,
    /// using the given `size` as a seeding value, and returns a sequence of
    /// values paired with the number of times each value occurs in the sequence
    /// </summary>
    /// <typeparam name="T">type for which a distribution will be generated</typeparam>
    /// <param name="arb">the IArbitrary responsible for producing instances of `T`</param>
    /// <param name="size">the FsCheck seed value</param>
    /// <param name="count">the number of data points in the distribution</param>
    /// <returns>a sequence of tuples pairing each unique instance of `T` with its number of occurances</returns>
    public static IEnumerable<(T Item, int Count)> Distribute<T,TKey>(this Arbitrary<T> arb, int size, int count, Func<T,TKey> groupBy)
      => arb.Generator
            .Sample (size, count)
            .GroupBy(groupBy)
            .Select (g => (Item: g.First(), Count: g.Count()))
            .OrderBy(p => p.Count);
  }

  /// <summary>
  /// shows examples of working with data generation (in and out of property tests)
  /// (NOTE: `dotnet test` requires a `--verbosity` of *at least* 'normal' to see distribution info on the command line)
  /// </summary>
  public sealed class Generation
  { 
    private readonly  ITestOutputHelper _out;
    public Generation(ITestOutputHelper output) => _out = output;
   
    /// <summary>
    /// reports the random distribution of 100 TimeZoneInfo instances
    /// </summary>
    [Fact]
    public void time_zone_info_distribution()
    { 
      _out.WriteLine("\n[Distribution of 100 TimeZoneInfo Instances]\n");
      
      var zones   = Generator.TimeZoneInfo().Distribute(0, 100, z => z.BaseUtcOffset).ToList();
      var length  = (from pair in zones select pair.Item.StandardName.Length).Max() + 4;
      foreach ((Zone zone, int count) in zones)
      { 
        var name  = zone.StandardName;
        var bar   = string.Join("", Enumerable.Repeat("=", count));
        var pad   = string.Join("", Enumerable.Repeat(" ", length - name.Length));
         
        _out.WriteLine($"{name + pad} | {count,2:##} | {bar}");
      }
    }

    /// <summary>
    /// reports the random distribution of 100 PositiveTime instances
    /// </summary>
    [Fact]
    public void positive_time_distribution()
    { 
      _out.WriteLine("\n[Distribution of 100 PositiveTime Instances]\n");

      var times = Generator.PositiveTime().Distribute(0, 100, t => t).ToList();
      foreach ((PositiveTime time, int count) in times)
      {
        var bar   = string.Join("", Enumerable.Repeat("=", count));
        var value = time.Value.ToString("dddddddd'.'hh':'mm':'ss'.'fffffff");

        _out.WriteLine($"{value} | {count,2:##} | {bar}");
      }
    }

    /// <summary>
    /// demonstrates attaching a collection of IArbitrary instances to a tests  
    /// </summary>
    [Property(Arbitrary=new []{ typeof(Generator) })]
    public bool zone_is_unchanged_through_round_trip_serialization(Zone anyZone)
      /**
      NOTE: since we've registered our generator, FsCheck will automatically
            create `zone`s for use in testing... for fun, compare this test
            to the one with the same name in the `Filtered` module
      */
      => Platform.As (
        win:  () => { var deflated = anyZone.ToSerializedString();
                      var inflated = Zone.FromSerializedString(deflated);
                      return anyZone.Equals(inflated); },
        osx:  () => true,
        unix: () => true
      );
      /**
        NOTE: there is a known issue with deseriazing TimeZoneInfo on non-Windows OSes
      */
  }
}
