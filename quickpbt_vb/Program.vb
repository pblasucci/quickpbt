Imports Xunit
Imports FsCheck
Imports FsCheck.Xunit

Namespace quickpbt

  Public Class Scratch

    <Fact>
    Public Sub UnitTest()
      Assert.True(1 = 1)
    End Sub

    <[Property]>
    Public Function PropertyTest(x As Integer) As Boolean
      Return (x = x)
    End Function

  End Class

End Namespace
