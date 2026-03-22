using HRManagement.BLL.Interfaces;
using HRManagement.Models;

namespace HRManagement.DAL.Interfaces
{
    public interface ILuongRepository
    {
        Task<NhanVienKeToan?> GetLuongByIdAsync(string maNV);
        Task CapNhatLuongAsync(string maNV, CapNhatLuongRequest request);           // Admin
        Task<IEnumerable<NhanVienKeToan>> GetDanhSachLuongKeToanAsync();             // KeToan xem danh sách
    }
}
