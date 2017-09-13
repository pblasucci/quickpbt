' Domain Under Test
Imports Timed = System.TimeSpan
Imports Zoned = System.TimeZoneInfo

''' <summary>
''' represent a time value which is always greater then zero (> 0)
''' (note: only meant for use with FsCheck's generation functionality)
''' </summary>
Public NotInheritable Class PositiveTime
  ''' <summary>
  ''' extracts the TimeSpan from a PositiveTime instance
  ''' </summary>
  Public ReadOnly Property Value As Timed

  ''' <summary>
  ''' returns a new PositiveTime instance, throwing an exception on values less than zero
  ''' </summary>
  ''' <param name="value">a time with a duration of greater than zero (> 0)</param>
  Public Sub New(value As Timed)
    If value <= Timed.Zero Then
      Throw New ArgumentOutOfRangeException(nameof(value), "value must be greater than 0")
    End If
          
    Me.Value = value
  End Sub

  Public Overrides Function GetHashCode() As Integer
    Return -1640531527 Xor Me.Value.GetHashCode()
  End Function

  Public Overrides Function Equals(obj As Object) As Boolean
    Dim you As PositiveTime = obj
    If  you Is Nothing Then Return False

    Return Me.Value = you.Value
  End Function 

  Public Overrides Function ToString() As String
    Return $"PositiveTime({Me.Value})"
  End Function

  Public Shared Widening Operator CType(value As Timed) As PositiveTime
    Return New PositiveTime(value)
  End Operator

  Public Shared Widening Operator CType(value As PositiveTime) As Timed
    Return value.Value
  End Operator
End Class

Module Generator
  ''' <summary>
  ''' generates arbitrary TimeZoneInfo instances, with no shrinking
  ''' </summary>
  ''' <returns>an IArbitrary capable of producing TimeZoneInfo instances</returns>
  Public Function TimeZoneInfo() As Arbitrary(Of Zoned)
    Return Gen.Elements(From z in Zoned.GetSystemTimeZones() Select z).ToArbitrary()
  End Function

  ''' <summary>
  ''' generates PositiveTime instances by leveraging FsCheck's 
  ''' built-in support for generating and shrinking TimeSpan instances
  ''' </summary>
  ''' <returns>an IArbitrary capable of producing PositiveTime instances</returns>
  Public Function PositiveTime() As Arbitrary(Of PositiveTime)
    Dim generator = From  t in Arb.Generate(Of Timed)()
                    Where t > Timed.Zero
                    Select New PositiveTime(t)

    Dim shrinker = Function(positive As PositiveTime)
                     Return From  t in Arb.Shrink(positive.Value)
                            Where t > Timed.Zero
                            Select new PositiveTime(t)
                   End Function

    Return Arb.From(generator, shrinker)
  End Function

  ''' <summary>
  ''' generates `count` random instances of the given IArbitrary,
  ''' using the given `size` as a seeding value, and returns a sequence of
  ''' values paired with the number of times each value occurs in the sequence
  ''' </summary>
  ''' <typeparam name="T">type for which a distribution will be generated</typeparam>
  ''' <param name="arb">the IArbitrary responsible for producing instances of `T`</param>
  ''' <param name="size">the FsCheck seed value</param>
  ''' <param name="count">the number of data points in the distribution</param>
  ''' <returns>a sequence of tuples pairing each unique instance of `T` with its number of occurances</returns>
  <Extension>
  Public Function Distribute(Of T,TKey)(arb     As Arbitrary(Of T),
                                        size    As Integer,
                                        count   As Integer,
                                        groupBy As Func(Of T,TKey)) As IEnumerable(Of (Item As T, Count As Integer))
    Return  From  item In Arb.Generator.Sample(size, count)
            Group item By key = groupBy(item) Into [group] = Group
            Order By [group].Count()
            Select ( [group].First(), [group].Count() )
End Function

End Module

''' <summary>
''' shows examples of working with data generation (in and out of property tests)
''' (NOTE: `dotnet test` requires a `--verbosity` of *at least* 'normal' to see distribution info on the command line)
''' </summary>
Public NotInheritable Class Generation
  Private Readonly _out As ITestOutputHelper

  Public Sub New(output As ITestOutputHelper)
    _out = output
  End Sub

  ''' <summary>
  ''' reports the random distribution of 100 TimeZoneInfo instances
  ''' </summary>
  <Fact>
  Public Sub TimeZoneInfoDistribution()
    _out.WriteLine($"{vbCrLf}[Distribution of 100 TimeZoneInfo Instances]{vbCrLf}")

    Dim zones   = Generator.TimeZoneInfo().Distribute(0, 100, Function(z) z.BaseUtcOffset).ToList()
    Dim longest = (from pair in zones select pair.Item.StandardName.Length).Max() + 4
    For Each pair In zones
      Dim name  = pair.Item.StandardName
      Dim bar   = string.Join("", Enumerable.Repeat("=", pair.Count))
      Dim pad   = string.Join("", Enumerable.Repeat(" ", longest - name.Length))

      _out.WriteLine($"{name + pad} | {pair.Count,2:##} | {bar}")
    Next
  End Sub
  
  ''' <summary>
  ''' reports the random distribution of 100 PositiveTime instances
  ''' </summary>
  <Fact>
  Public Sub PositiveTimeDistribution()
    _out.WriteLine($"{vbCrLf}[Distribution of 100 PositiveTime Instances]{vbCrLf}")

    Dim times = Generator.PositiveTime().Distribute(0, 100, Function(t) t).ToList()
    For Each pair In times
      Dim bar   = string.Join("", Enumerable.Repeat("=", pair.Count))
      Dim value = pair.Item.Value.ToString("dddddddd'.'hh':'mm':'ss'.'fffffff")

      _out.WriteLine($"{value} | {pair.Count,2:##} | {bar}")
    Next
  End Sub

  ''' <summary>
  ''' demonstrates attaching a collection of IArbitrary instances to a tests  
  ''' </summary>
  <[Property](Arbitrary := New Type(){ GetType(Generator) })>
  Public Function ZoneIsUnchangedThroughRoundTripSerialization(anyZone As Zoned) As Boolean
    '**
    'NOTE: since we've registered our generator, FsCheck will automatically
    '      create `zone`s for use in testing... for fun, compare this test
    '     to the one with the same name in the `Filtered` module
    '**
    Return Platform.Swap(
      win   :=  Function()
                  Dim deflated = anyZone.ToSerializedString()
                  Dim inflated = Zoned.FromSerializedString(deflated)
                  Return anyZone.Equals(inflated)
                End Function,
      mac   :=  Function() true,
      unix  :=  Function() true
    )
    '**
    '  NOTE: there is a known issue with deseriazing TimeZoneInfo on non-Windows OSes
    '**
  End Function
End Class
