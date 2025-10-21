using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoKTNC.Tests
{
    [TestFixture]
    internal class TinhToanTests
    {

        private TinhToan _tt = null!;

        [SetUp] public void Setup() => _tt = new TinhToan();

        [Test] public void Chan_Duong() => Assert.IsTrue(_tt.LaSoChan(2));
        [Test] public void Le_Duong() => Assert.IsFalse(_tt.LaSoChan(3));
        [Test] public void Chan_Am() => Assert.IsTrue(_tt.LaSoChan(-4));
        [Test] public void Le_Am() => Assert.IsFalse(_tt.LaSoChan(-9));
        [Test] public void So0_Chan() => Assert.IsTrue(_tt.LaSoChan(0));
        // Có thể thêm biên lớn:
        [Test] public void MaxInt_LeHayChan() => Assert.IsFalse(_tt.LaSoChan(int.MaxValue)); // 2,147,483,647 lẻ
    }
}
