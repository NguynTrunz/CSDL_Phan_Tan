using HRManagement.BLL.Interfaces;
using HRManagement.Models;

namespace HRManagement.DAL.Interfaces
{
    public interface INhanVienRepository
    {
        Task<IEnumerable<NhanVienHanhChinh>> GetAllAsync();
        Task<NhanVienHanhChinh?> GetByIdAsync(string maNV);
        Task<IEnumerable<NhanVienFullProfile>> GetFullProfilesAsync();       // Admin
        Task<IEnumerable<NhanVienKeToan>> GetPayrollKeToanConnection();   // KeToan
        Task ThemNhanVienAsync(ThemNhanVienRequest request);                  // Gọi SP Insert (cả 2 DB)
        Task XoaNhanVienAsync(string maNV);                                   // Gọi SP Delete (cả 2 DB)
        Task CapNhatNhanVienAsync(string maNV, CapNhatNhanVienRequest request);
    }
}
