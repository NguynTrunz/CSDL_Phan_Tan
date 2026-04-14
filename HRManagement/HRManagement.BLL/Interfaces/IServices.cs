using HRManagement.BLL.Interfaces;
using HRManagement.Models;

namespace HRManagement.BLL.Interfaces
{
    public interface INhanVienService
    {
        // IT: chỉ xem hành chính từ DB_Personnel
        Task<IEnumerable<NhanVienHanhChinh>> GetDanhSachHanhChinhAsync();

        // Admin: xem full cả 2 DB
        Task<IEnumerable<NhanVienFullProfile>> GetDanhSachFullAsync();

        Task<NhanVienHanhChinh?> GetNhanVienByIdAsync(string maNV);

        // Admin only
        Task ThemNhanVienAsync(ThemNhanVienRequest request);
        Task XoaNhanVienAsync(string maNV);
        Task CapNhatNhanVienAsync(string maNV, CapNhatNhanVienRequest request);
    }

    public interface ILuongService
    {
        // KeToan: xem danh sách lương từ DB_Payroll
        Task<IEnumerable<NhanVienKeToan>> GetDanhSachLuongKeToanAsync();

        Task<NhanVienKeToan?> GetLuongByIdAsync(string maNV);

        // Task CapNhatLuongAsync(string maNV, CapNhatLuongRequest request);
        Task CapNhatLuongAsync(string maNV, CapNhatLuongRequest req, string? role = null);
    }
}
