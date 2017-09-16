using FsCheck;
using FsCheck.Xunit;

namespace quickpbt
{
  /* domain under test */
  using Date = System.DateTimeOffset;
  using Time = System.TimeSpan;
  using Zone = System.TimeZoneInfo;

  /// <summary>
  /// contains examples of gathering diagnostics about generated data
  /// (NOTE: `dotnet test` requires a `--verbosity` of *at least* `normal` to see observations on the command line)
  /// </summary>
  [Properties(Arbitrary = new []{ typeof(Generator) })]
  public sealed class Observations
  {
    /// <summary>
    /// a trival observation partitions data into one of two buckets 
    /// </summary>
    [Property]
    public Property trivial_daylight_savings_support(Date anyDate, Zone anyZone, NonNegativeInt total)
    { 
      var days = Time.FromDays((int) total);

      var addThenShift = Zone.ConvertTime(anyDate + days, anyZone);
      var shiftThenAdd = Zone.ConvertTime(anyDate, anyZone) + days;

      return (addThenShift == shiftThenAdd).Trivial(anyZone.SupportsDaylightSavingTime);
    }

    /// <summary>
    /// a classification partitions data into one of N, labelled buckets
    /// </summary>
    [Property]
    public Property classify_meridian_position(Date anyDate, Zone anyZone, NonNegativeInt total)
    { 
      var days = Time.FromDays((int) total);

      var addThenShift = Zone.ConvertTime(anyDate + days, anyZone);
      var shiftThenAdd = Zone.ConvertTime(anyDate, anyZone) + days;

      return (addThenShift == shiftThenAdd)
                .Classify(anyDate.Offset <  Time.Zero, "West of Greenwich")
                .Classify(anyDate.Offset == Time.Zero, "Within Greenwich")
                .Classify(anyDate.Offset >  Time.Zero, "East of Greenwich");
    }

    /// <summary>
    /// rather than using a boolean observation, collect reports any value
    /// </summary>
    [Property]
    public Property collect_weekday_name(Date anyDate, Zone anyZone, NonNegativeInt total)
    { 
      var days = Time.FromDays((int) total);

      var addThenShift = Zone.ConvertTime(anyDate + days, anyZone);
      var shiftThenAdd = Zone.ConvertTime(anyDate, anyZone) + days;

      return (addThenShift == shiftThenAdd).Collect(anyDate.DayOfWeekName());
    }

    /// <summary>
    /// observations may be combined as mush as is desired
    /// </summary>
    [Property]
    public Property many_observations_combined(Date anyDate, Zone anyZone, NonNegativeInt total)
    { 
      var days = Time.FromDays((int) total);

      var addThenShift = Zone.ConvertTime(anyDate + days, anyZone);
      var shiftThenAdd = Zone.ConvertTime(anyDate, anyZone) + days;

      return (addThenShift == shiftThenAdd)
                .Trivial(anyZone.SupportsDaylightSavingTime)
                .Classify(anyDate.Offset <  Time.Zero, "West of Greenwich")
                .Classify(anyDate.Offset == Time.Zero, "Within Greenwich")
                .Classify(anyDate.Offset >  Time.Zero, "East of Greenwich")
                .Collect(anyDate.DayOfWeekName());
    }
  }
}
