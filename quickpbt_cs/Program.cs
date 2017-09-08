using Xunit;
using FsCheck;
using FsCheck.Xunit;

namespace quickpbt
{
  public class Scratch
  {
    [Fact]
    public void UnitTest()
    {
      Assert.True(1 == 1);
    }

    [Property]
    public bool PropertyTest(int x)
    {
      return (x == x);
    }
  }
}
