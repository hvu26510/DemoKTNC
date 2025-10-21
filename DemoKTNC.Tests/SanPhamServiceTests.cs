using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoKTNC.Tests
{
    [TestFixture]
    internal class SanPhamServiceTests
    {
        private SanPhamService _svc = null!;

        [SetUp] public void Setup() => _svc = new SanPhamService();

        [Test]
        public void KiemTraMaTonTai_ReturnsTrue_WhenMaTonTai()
            => Assert.IsTrue(_svc.KiemTraMaTonTai("SP01"));

        [Test]
        public void KiemTraMaTonTai_ReturnsFalse_WhenMaKhongTonTai()
            => Assert.IsFalse(_svc.KiemTraMaTonTai("SP99"));

        [Test]
        public void Sua_ThanhCong_WhenMaTonTai_AndDataHopLe()
        {
            var sp = new SanPham { Ma = "SP01", Ten = "Laptop Gaming", NamBaoHanh = 2025, Gia = 2000, SoLuong = 5, DanhMuc = "CongNghe" };
            var ok = _svc.Sua(sp);
            Assert.IsTrue(ok);
            var after = _svc.GetByMa("SP01")!;
            Assert.That(after.Ten, Is.EqualTo("Laptop Gaming"));
            Assert.That(after.Gia, Is.EqualTo(2000).Within(0.001f));
            Assert.That(after.SoLuong, Is.EqualTo(5));
        }

        [Test]
        public void Sua_False_WhenMaKhongTonTai()
        {
            var sp = new SanPham { Ma = "SP99", Ten = "X", NamBaoHanh = 2025, Gia = 1, SoLuong = 1, DanhMuc = "DM" };
            Assert.IsFalse(_svc.Sua(sp));
        }

        [Test]
        public void Sua_False_WhenGiaAm_PhanVungKhongHopLe()
        {
            var sp = new SanPham { Ma = "SP01", Ten = "X", NamBaoHanh = 2025, Gia = -1, SoLuong = 1, DanhMuc = "DM" };
            Assert.IsFalse(_svc.Sua(sp));
        }

        [Test]
        public void Sua_False_WhenSoLuongAm_BienThap()
        {
            var sp = new SanPham { Ma = "SP01", Ten = "X", NamBaoHanh = 2025, Gia = 1, SoLuong = -1, DanhMuc = "DM" };
            Assert.IsFalse(_svc.Sua(sp));
        }

        [Test]
        public void Sua_False_WhenInputNullOrMaRong()
        {
            Assert.IsFalse(_svc.Sua(null!));
            Assert.IsFalse(_svc.Sua(new SanPham { Ma = "  " }));
        }
    }
}
