using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoKTNC
{
    public class SanPhamService
    {
        private readonly List<SanPham> _ds = new();

        public SanPhamService()
        {
            // seed 1 sản phẩm để có dữ liệu test
            _ds.Add(new SanPham { Ma = "SP01", Ten = "Laptop", NamBaoHanh = 2024, Gia = 1500, SoLuong = 10, DanhMuc = "DienTu" });
        }

        public bool KiemTraMaTonTai(string ma) => _ds.Any(x => x.Ma == ma);

        // “Sửa” theo đề: cập nhật theo Ma; trả về true nếu sửa được, false nếu không có mã
        public bool Sua(SanPham sp)
        {
            if (sp == null || string.IsNullOrWhiteSpace(sp.Ma)) return false;
            var ex = _ds.FirstOrDefault(x => x.Ma == sp.Ma);
            if (ex == null) return false;

            // Validate tối thiểu: giá & số lượng biên >= 0 (phân vùng tương đương)
            if (sp.Gia < 0 || sp.SoLuong < 0) return false;

            ex.Ten = sp.Ten;
            ex.NamBaoHanh = sp.NamBaoHanh;
            ex.Gia = sp.Gia;
            ex.SoLuong = sp.SoLuong;
            ex.DanhMuc = sp.DanhMuc;
            return true;
        }

        // tiện cho kiểm thử quan sát dữ liệu
        public SanPham? GetByMa(string ma) => _ds.FirstOrDefault(x => x.Ma == ma);
    }
}
