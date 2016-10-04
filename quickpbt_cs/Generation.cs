using System;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using FsCheck;
using FsCheck.Xunit;
using System.Collections.Generic;

using date = System.DateTimeOffset;
using time = System.TimeSpan;
using zone = System.TimeZoneInfo;

namespace QuickPBT.CS
{
  /// <summary>
  /// encapsulates several IArbitrary instances
  /// </summary>
  public static class Generator
  {
    /// <summary>
    /// generates arbitrary TimeZoneInfo instances, with no shrinking
    /// </summary>
    public static Arbitrary<zone> Zone () => 
      Gen.Elements(from z in zone.GetSystemTimeZones() select z)
         .ToArbitrary();

    /// <summary>
    /// generates PositiveTime instances by leveraging FsCheck's built-in
    /// support for generating and shrinking TimeSpan instances
    /// </summary>
    public static Arbitrary<PositiveTime> PosTimes () =>
      Arb.From (
        // generator
        from  t in Arb.Generate<time>()
        where t > time.Zero
        select new PositiveTime(t)
      , // shrinker
        posTime =>  from  t in Arb.Shrink(posTime.Value)
                    where t > time.Zero
                    select new PositiveTime(t));
    
    /// <summary>
    /// generates `count` random instances of the given IArbitrary, 
    /// using the given `size` as a seeding value, and returns a sequence of 
    /// values paired with the number of times each value occurs in the sequence
    /// </summary>
    public static IEnumerable<Tuple<T,Int32>> Distribute<T>(this Arbitrary<T> arb, Int32 size, Int32 count) =>
      from  item in arb.Generator.Sample(size,count)
      group item by item into g
      orderby g.Count()
      select Tuple.Create(g.Key, g.Count());
  }

  /// examples of working with IArbitrary instances
  public class Generation
  {
    /**
    NOTE: here we're using a primary-constructor class, rather than a module,
          because we need to hook into xUnit's I/O mechanisms.
    */
    private readonly ITestOutputHelper output;

    public Generation (ITestOutputHelper output) { this.output = output; }

    /// <summary>
    /// reports the random distribution of 100 PositiveTime instances
    /// </summary>
    [Fact, Trait("section", "generation")]
    public void PositiveTimeDistribution ()
    {
      this.output.WriteLine("\n[Distribution of 100 PositiveTime Instances]\n");
      foreach (var pair in Generator.PosTimes().Distribute(5,100))
      {
        var posTime = pair.Item1;
        var count   = pair.Item2;
        var bar     = String.Join("",Enumerable.Repeat("=", count));

        this.output.WriteLine($"{posTime.Value:dddddddd'.'hh':'mm':'ss'.'fffffff} | {count,2:##} | {bar}"); 
      } 
      this.output.WriteLine("");
    }

    /// <summary>
    /// reports the random distribution of 100 TimeZoneInfo instances
    /// </summary>
    [Fact, Trait("section", "generation")]
    public void TimeZoneInfoDistribution ()
    {
      this.output.WriteLine("\n[Distribution of 100 TimeZoneInfo Instances]\n");
      var zones   = Generator.Zone()
                             .Distribute(1,100)
                             .ToList();
      var length  = zones.Select(pair =>
                          {
                            var zone = pair.Item1;
                            return zone.StandardName.Length;
                          })
                         .Max() + 4; 
                    
      foreach (var pair in zones)
      {
        var zone  = pair.Item1;
        var count = pair.Item2;
        
        var bar   = String.Join("", Enumerable.Repeat("=", count));
        var pads  = String.Join("", Enumerable.Repeat(" ", length - zone.StandardName.Length));
        var show  = zone.StandardName + pads;

        this.output.WriteLine($"{show} | {count,2:##} | {bar}");
      }
      this.output.WriteLine("");
    } 
 
    /* deonstrates attaching a collection of IArbitrary instances to a test */
    [Property(Arbitrary = new []{ typeof(Generator) }), Trait("section", "generation")]
    public Boolean TimeZoneInfo_IsUnchanged_RoundTripSerialization (zone target)
    {
      /**
        NOTE: since we've registered our generator, FsCheck will automatically 
              create `zone`s for use in testing... for fun, compare this test 
              to the one with the same name in the `Filtered` module
      */
      var deflated = target.ToSerializedString ();
      var inflated = zone.FromSerializedString(deflated);
      return target.Equals(inflated);
    }
  }
}
