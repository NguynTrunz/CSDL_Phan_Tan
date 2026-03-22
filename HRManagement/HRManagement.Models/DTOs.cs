namespace HRManagement.Models
{
    // Model JOIN đầy đủ (dùng cho Admin)
    public class NhanVienFullProfile
    {
        public string MaNV { get; set; } = string.Empty;
        public string Ten { get; set; } = string.Empty;
        public string? PhongBan { get; set; }
        public string? ChucVu { get; set; }
        public decimal? LuongCoBan { get; set; }
        public double? HeSo { get; set; }
        public decimal? PhuCap { get; set; }
        public string? SoTaiKhoan { get; set; }
        public decimal? TongThuNhap { get; set; }
    }

    // DTO: Request thêm nhân viên mới (Admin)
    public class ThemNhanVienRequest
    {
        public string MaNV { get; set; } = string.Empty;
        public string Ten { get; set; } = string.Empty;
        public string? PhongBan { get; set; }
        public string? ChucVu { get; set; }
        public decimal LuongCoBan { get; set; }
        public double HeSo { get; set; }
        public decimal PhuCap { get; set; }
        public string? SoTaiKhoan { get; set; }
    }

    // DTO: Admin cập nhật thông tin hành chính
    public class CapNhatNhanVienRequest
    {
        public string Ten { get; set; } = string.Empty;
        public string? PhongBan { get; set; }
        public string? ChucVu { get; set; }
    }

    // DTO: Cập nhật lương (Admin & KeToan dùng chung)
    public class CapNhatLuongRequest
    {
        public decimal LuongCoBan { get; set; }
        public double HeSo { get; set; }
        public decimal PhuCap { get; set; }
        public string? SoTaiKhoan { get; set; }
    }
}
