using HRManagement.BLL.Interfaces;
using HRManagement.DAL.Interfaces;
using HRManagement.Models;

namespace HRManagement.BLL.Services
{
    public class LuongService : ILuongService
    {
        private readonly ILuongRepository _repo;

        public LuongService(ILuongRepository repo)
        {
            _repo = repo;
        }

        public Task<IEnumerable<NhanVienKeToan>> GetDanhSachLuongKeToanAsync()
            => _repo.GetDanhSachLuongKeToanAsync();

        public Task<NhanVienKeToan?> GetLuongByIdAsync(string maNV)
            => _repo.GetLuongByIdAsync(maNV);

        public async Task CapNhatLuongAsync(string maNV, CapNhatLuongRequest request, string? role = null)
        {
            if (request.LuongCoBan <= 0)
                throw new ArgumentException("Lương cơ bản phải lớn hơn 0.");
            if (request.HeSo <= 0)
                throw new ArgumentException("Hệ số lương phải lớn hơn 0.");

            await _repo.CapNhatLuongAsync(maNV, request, role);
        }
    }
}