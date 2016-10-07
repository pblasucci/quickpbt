Imports Dated = System.DateTimeOffset
Imports Time  = System.TimeSpan
Imports Zone  = System.TimeZoneInfo

''' encapsulates several IArbitrary instances
Friend Module Generator
  ''' generates arbitrary TimeZoneInfo instances, with no shrinking
  Public Function TimeZoneInfo () As Arbitrary(Of Zone)
    Return Gen.Elements(From z In Zone.GetSystemTimeZones () Select z) _
              .ToArbitrary()
  End Function

  ''' generates PositiveTime instances by leveraging FsCheck's built-in
  ''' support for generating and shrinking TimeSpan instances
  Public Function PositiveTime () As Arbitrary(Of PositiveTime)
    Dim generator = 
      From    t In Arb.Generate(Of Time)()
      Where   t > Time.Zero
      Select  New PositiveTime(t)
    Dim shrinker As Func(Of PositiveTime,IEnumerable(Of PositiveTime)) = 
      Function (posTime)
          Return From   t In Arb.Shrink(posTime.Value)
                 Where  t > Time.Zero
                 Select New PositiveTime(t)
      End Function

    Return Arb.From(generator,shrinker) 
  End Function

  ''' <summary>
  ''' generates `count` random instances of the given IArbitrary, 
  ''' using the given `size` as a seeding value, and returns a sequence of 
  ''' values paired with the number of times each value occurs in the sequence
  ''' </summary>
  <Extension>
  Public Function Distribute(Of T)(arb As Arbitrary(Of T), size As Integer, count As Integer) As IEnumerable(Of Tuple(Of T,Int32))
    Return  From  item in arb.Generator.Sample(size,count)
            Group item by Key = item Into grp = Group
            Order By grp.Count()
            Select Tuple.Create(Key, grp.Count())
  End Function
End Module

Public NotInheritable Class Generation
  Private Readonly output As ITestOutputHelper

  Public Sub New (output As ITestOutputHelper)
    Me.output = output
  End Sub

  ''' <summary>
  ''' reports the random distribution of 100 PositiveTime instances
  ''' </summary>
  <Fact, Trait("group", "generation")>
  Public Sub PositiveTimeDistribution ()
    Me.output.WriteLine(vbNewLine & "[Distribution of 100 PositiveTime Instances]" & vbNewLine)
    For Each pair In Generator.PositiveTime().Distribute(5,100)
      Dim posTime = pair.Item1
      Dim count   = pair.Item2
      Dim bar     = String.Join("", Enumerable.Repeat("=", count))

      Me.output.WriteLine($"{posTime.Value:dddddddd'.'hh':'mm':'ss'.'fffffff} | {count,2:##} | {bar}")
    Next
    Me.output.WriteLine("")
  End Sub

  ''' <summary>
  ''' reports the random distribution of 100 TimeZoneInfo instances
  ''' </summary>
  <Fact, Trait("group", "generation")>
  Public Sub TimeZoneInfoDistribution ()
    Me.output.WriteLine(vbNewLine & "[Distribution of 100 TimeZoneInfo Instances]" & vbNewLine)
    Dim zones   = Generator.TimeZoneInfo()    _
                           .Distribute(1,100) _
                           .ToList()
    Dim length  = zones.Select(
      Function (pair)
        Dim zone = pair.Item1
        return zone.StandardName.Length
      End Function).Max() + 4
                    
    For Each pair In zones
      Dim zone  = pair.Item1
      Dim count = pair.Item2
        
      Dim bar   = String.Join("", Enumerable.Repeat("=", count))
      Dim pads  = String.Join("", Enumerable.Repeat(" ", length - zone.StandardName.Length))
      Dim show  = zone.StandardName + pads

      Me.output.WriteLine($"{show} | {count,2:##} | {bar}")
    Next
    Me.output.WriteLine("")
  End Sub
 
  ' deonstrates attaching a collection of IArbitrary instances to a test
  <[Property](Arbitrary := { GetType(Generator) }), Trait("group", "generation")>
  Public Function TimeZoneInfo_IsUnchanged_RoundTripSerialization (target As Zone) As Boolean
    ' NOTE: since we've registered our generator, FsCheck will automatically 
    '       create `zone`s for use in testing... for fun, compare this test 
    '       to the one with the same name in the `Filtered` Class

    Dim deflated = target.ToSerializedString ()
    Dim inflated = zone.FromSerializedString(deflated)
    Return target.Equals(inflated)
  End Function
End Class
