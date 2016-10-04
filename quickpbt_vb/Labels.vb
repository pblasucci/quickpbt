Imports Dated = System.DateTimeOffset
Imports Time  = System.TimeSpan
Imports Zone  = System.TimeZoneInfo

''' <summary>
''' demonstrates labelling properties for diagnostic purposes
''' </summary>
Public NotInheritable Class Labels
  ' compound property tests can lead to obtuse failures... which propery is at fault?
  <[Property](Arbitrary := { GetType(Generator) }), Trait("section", "labelled")>
  Public Function ZoneConversion_NotAffectedByDetours_Naive (civil As Dated, zone1 As Zone, zone2 As Zone) As Boolean
    Dim viaZone1 = zone.ConvertTime(zone.ConvertTime(civil, zone1), zone2)
    Dim directly = zone.ConvertTime(civil, zone2)

    Return  (viaZone1 = directly) _ 
            AndAlso
            (directly.Offset = zone2.BaseUtcOffset)
  End Function
    
  ' FsCheck propvides tools for labelling individual properties
  <[Property](Arbitrary := { GetType(Generator) }), Trait("section", "labelled")>
  Public Function ZoneConversion_NotAffectedByDetours_Labelled (civil As Dated, zone1 As Zone, zone2 As Zone) As [Property]
    Dim viaZone1 = zone.ConvertTime(zone.ConvertTime(civil, zone1), zone2)
    Dim directly = zone.ConvertTime(civil, zone2)

    Dim sameDate  As Func(Of Boolean) = Function () viaZone1 = directly
    Dim sameShift As Func(Of Boolean) = Function () directly.Offset = zone2.BaseUtcOffset
    
    ' if either of the following properties does not hold, its label will be printed
    return  sameDate.Label($"Same Date? {viaZone1} = {directly}")                     _
            .And(                                                                     _
            sameShift.Label($"Same Date? {zone2.BaseUtcOffset} = {directly.Offset}"))
  End Function
  
  ' based on the knowledge gained by labelling properties, we can fix the test
  <[Property](Arbitrary := { GetType(Generator) }), Trait("section", "labelled")>
  Public Function ZoneConversion_NotAffectedByDetours_Corrected (civil As Dated, zone1 As Zone, zone2 As Zone) As [Property]
    Dim viaZone1 = zone.ConvertTime(zone.ConvertTime(civil, zone1), zone2)
    Dim directly = zone.ConvertTime(civil, zone2)

    Dim sameDate  As Func(Of Boolean) = Function () viaZone1 = directly
    Dim sameShift As Func(Of Boolean) = Function () directly.Offset = zone2.GetUtcOffset(directly)
    ' note that we had the wrong check previously

    ' if either of the following properties does not hold, its label will be printed
    Return sameDate.ToProperty().And(sameShift)
  End Function
End Class
