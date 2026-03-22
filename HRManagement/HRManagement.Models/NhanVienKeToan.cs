namespace HRManagement.BLL.Interfaces
{
    public class NhanVienKeToan
    {
        public string MaNV { get; set; } = string.Empty;
        public decimal? LuongCoBan { get; set; }
        public double? HeSo { get; set; }
        public decimal? PhuCap { get; set; }
        public string? SoTaiKhoan { get; set; }
        public decimal? TongThuNhap { get; set; }
        public DateTime? NgayCapNhat { get; set; }
    }
}
