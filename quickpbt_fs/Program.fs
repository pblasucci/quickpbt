namespace quickpbt

open Xunit
open FsCheck
open FsCheck.Xunit

type Scratch() =

  [<Fact>]
  member __.UnitTest() =
    Assert.True(1 = 1)

  [<Property>]
  member __.PropertyTest(x:int) =
    (x = x)
