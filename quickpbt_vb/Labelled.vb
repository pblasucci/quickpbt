' Domain Under Test
Imports Dated = System.DateTimeOffset
Imports Zoned = System.TimeZoneInfo

''' <summary>
''' demonstrates labelling properties for diagnostic purposes
''' </summary>
<Properties(Arbitrary := new Type(){ GetType(Generator) })>
Public NotInheritable Class Labelled
  ''' <summary>
  ''' compound property tests can lead to obtuse failures... which propery is at fault?
  ''' </summary>
  <[Property]>
  Public Function ZoneConversionIsNotAffectedByDetoursNaive(anyDate As Dated, zone1 As Zoned, zone2 As Zoned) As Boolean 
    Dim viaZone1 = Zoned.ConvertTime(Zoned.ConvertTime(anyDate, zone1), zone2)
    Dim directly = Zoned.ConvertTime(anyDate, zone2)

    return  (viaZone1 = directly) AndAlso           ' same date
            (directly.Offset = zone2.BaseUtcOffset) ' same shift
  End Function

  ''' <summary>
  ''' FsCheck provides tools for labelling individual properties... see the .Label method
  ''' </summary>
  <[Property]>
  Public Function ZoneConversionIsNotAffectedByDetoursLabelled(anyDate  As Dated,
                                                               zone1    As Zoned,
                                                               zone2    As Zoned) As [Property]
    Dim viaZone1 = Zoned.ConvertTime(Zoned.ConvertTime(anyDate, zone1), zone2)
    Dim directly = Zoned.ConvertTime(anyDate, zone2)

    Dim sameDate  = Function() viaZone1 = directly
    Dim sameShift = Function() directly.Offset = zone2.BaseUtcOffset
      
    Return sameDate ().Label($"Same Date?  ({viaZone1} = {directly})") _
      .And(sameShift().Label($"Same Shift? ({zone2.BaseUtcOffset} = {directly.Offset})"))
    '**
    ' if either of these properties does not hold, its label will be printed
    '**
  End Function

  ''' <summary>
  ''' FsCheck provides tools for labelling individual properties... see the .Label method
  ''' </summary>
  <[Property]>
  Public Function ZoneConversionIsNotAffectedByDetoursCorrected(anyDate As Dated,
                                                                zone1   As Zoned,
                                                                zone2   As Zoned) As [Property] 
    Dim viaZone1 = Zoned.ConvertTime(Zoned.ConvertTime(anyDate, zone1), zone2)
    Dim directly = Zoned.ConvertTime(anyDate, zone2)

    Dim sameDate  = Function() viaZone1 = directly
    Dim sameShift = Function() directly.Offset = zone2.GetUtcOffset(directly) ' note that we had the wrong check previously
      
    Return sameDate ().Label($"Same Date?  ({viaZone1} = {directly})") _
      .And(sameShift().Label($"Same Shift? ({zone2.BaseUtcOffset} = {directly.Offset})"))
  End Function
End Class
