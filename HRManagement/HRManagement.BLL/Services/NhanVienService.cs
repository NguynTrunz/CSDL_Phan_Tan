using HRManagement.BLL.Interfaces;
using HRManagement.DAL.Interfaces;
using HRManagement.Models;

namespace HRManagement.BLL.Services
{
    public class NhanVienService : INhanVienService
    {
        private readonly INhanVienRepository _repo;

        public NhanVienService(INhanVienRepository repo)
        {
            _repo = repo;
        }

        // IT: chỉ xem hành chính
        public Task<IEnumerable<NhanVienHanhChinh>> GetDanhSachHanhChinhAsync()
            => _repo.GetAllAsync();

        // Admin: xem full cả 2 DB
        public Task<IEnumerable<NhanVienFullProfile>> GetDanhSachFullAsync()
            => _repo.GetFullProfilesAsync();

        public Task<NhanVienHanhChinh?> GetNhanVienByIdAsync(string maNV)
            => _repo.GetByIdAsync(maNV);

        public async Task ThemNhanVienAsync(ThemNhanVienRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.MaNV))
                throw new ArgumentException("Mã nhân viên không được để trống.");
            if (string.IsNullOrWhiteSpace(request.Ten))
                throw new ArgumentException("Tên nhân viên không được để trống.");
            if (request.LuongCoBan <= 0)
                throw new ArgumentException("Lương cơ bản phải lớn hơn 0.");
            if (request.HeSo <= 0)
                throw new ArgumentException("Hệ số lương phải lớn hơn 0.");

            await _repo.ThemNhanVienAsync(request);
        }

        public async Task XoaNhanVienAsync(string maNV)
        {
            if (string.IsNullOrWhiteSpace(maNV))
                throw new ArgumentException("Mã nhân viên không được để trống.");

            var nv = await _repo.GetByIdAsync(maNV);
            if (nv == null)
                throw new KeyNotFoundException($"Không tìm thấy nhân viên có mã {maNV}.");

            await _repo.XoaNhanVienAsync(maNV);
        }

        public async Task CapNhatNhanVienAsync(string maNV, CapNhatNhanVienRequest request)
        {
            if (string.IsNullOrWhiteSpace(maNV))
                throw new ArgumentException("Mã nhân viên không được để trống.");
            if (string.IsNullOrWhiteSpace(request.Ten))
                throw new ArgumentException("Tên nhân viên không được để trống.");

            var nv = await _repo.GetByIdAsync(maNV);
            if (nv == null)
                throw new KeyNotFoundException($"Không tìm thấy nhân viên có mã {maNV}.");

            await _repo.CapNhatNhanVienAsync(maNV, request);
        }
    }
}
