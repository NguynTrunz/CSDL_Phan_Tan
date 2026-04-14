using Microsoft.Data.SqlClient;
using HRManagement.BLL.Interfaces;
using HRManagement.DAL.Interfaces;
using HRManagement.Models;

namespace HRManagement.DAL.Repositories
{
    public class LuongRepository : ILuongRepository
    {
        private readonly DbConnectionFactory _factory;

        public LuongRepository(DbConnectionFactory factory)
        {
            _factory = factory;
        }

        // KeToan & Admin xem danh sách lương
        // View_KeToan_Luong: MaNV[0], LuongCoBan[1], HeSo[2], PhuCap[3],
        //                    SoTaiKhoan[4], TongThuNhap[5], NgayCapNhat[6]
        public async Task<IEnumerable<NhanVienKeToan>> GetDanhSachLuongKeToanAsync()
        {
            var result = new List<NhanVienKeToan>();
            const string sql = @"
                SELECT MaNV, LuongCoBan, HeSo, PhuCap, SoTaiKhoan, TongThuNhap, NgayCapNhat
                FROM View_KeToan_Luong";

            await using var conn = _factory.GetPayrollKeToanConnection();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                result.Add(new NhanVienKeToan
                {
                    MaNV = reader.GetString(0),
                    LuongCoBan = reader.IsDBNull(1) ? null : (decimal?)Convert.ToDecimal(reader.GetValue(1)),
                    HeSo = reader.IsDBNull(2) ? null : (double?)Convert.ToDouble(reader.GetValue(2)),
                    PhuCap = reader.IsDBNull(3) ? null : (decimal?)Convert.ToDecimal(reader.GetValue(3)),
                    SoTaiKhoan = reader.IsDBNull(4) ? null : reader.GetString(4),
                    TongThuNhap = reader.IsDBNull(5) ? null : (decimal?)Convert.ToDecimal(reader.GetValue(5)),
                    NgayCapNhat = reader.IsDBNull(6) ? null : reader.GetDateTime(6),
                });
            }
            return result;
        }

        // Xem lương 1 NV
        public async Task<NhanVienKeToan?> GetLuongByIdAsync(string maNV)
        {
            const string sql = @"
                SELECT MaNV, LuongCoBan, HeSo, PhuCap, SoTaiKhoan, TongThuNhap, NgayCapNhat
                FROM View_KeToan_Luong
                WHERE MaNV = @MaNV";

            await using var conn = _factory.GetPayrollKeToanConnection();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@MaNV", maNV);
            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new NhanVienKeToan
                {
                    MaNV = reader.GetString(0),
                    LuongCoBan = reader.IsDBNull(1) ? null : (decimal?)Convert.ToDecimal(reader.GetValue(1)),
                    HeSo = reader.IsDBNull(2) ? null : (double?)Convert.ToDouble(reader.GetValue(2)),
                    PhuCap = reader.IsDBNull(3) ? null : (decimal?)Convert.ToDecimal(reader.GetValue(3)),
                    SoTaiKhoan = reader.IsDBNull(4) ? null : reader.GetString(4),
                    TongThuNhap = reader.IsDBNull(5) ? null : (decimal?)Convert.ToDecimal(reader.GetValue(5)),
                    NgayCapNhat = reader.IsDBNull(6) ? null : reader.GetDateTime(6),
                };
            }
            return null;
        }

        // Cập nhật lương - gọi sp_UpdateLuong trên DB_Payroll
        // SP định nghĩa: @MaNV, @LuongMoi, @HeSoMoi, @PhuCapMoi, @SoTaiKhoanMoi
        // Admin truyền SoTaiKhoan = null → SP giữ nguyên giá trị cũ KHÔNG được, phải xử lý
        // → Dùng ISNULL trong SP hoặc truyền giá trị cũ. Cách đơn giản: nếu null thì lấy giá trị hiện tại
        public async Task CapNhatLuongAsync(string maNV, CapNhatLuongRequest req, string? role = null)
        {
            if (string.IsNullOrWhiteSpace(role))
                throw new UnauthorizedAccessException("Thiếu role để cập nhật lương.");

            if (role != "Admin" && role != "KeToan")
                throw new UnauthorizedAccessException($"Role '{role}' không có quyền cập nhật lương.");

            await using var conn = role == "Admin"
                ? _factory.GetPayrollAdminConnection()
                : _factory.GetPayrollKeToanConnection();

            await conn.OpenAsync();

            var procedureName = role == "Admin"
                ? "sp_Admin_UpdateLuong"
                : "sp_KeToan_UpdateLuong";

            await using var cmd = new SqlCommand(procedureName, conn)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@MaNV", maNV);
            cmd.Parameters.AddWithValue("@LuongMoi", req.LuongCoBan);
            cmd.Parameters.AddWithValue("@HeSoMoi", req.HeSo);
            cmd.Parameters.AddWithValue("@PhuCapMoi", req.PhuCap);

            if (role == "KeToan")
            {
                cmd.Parameters.AddWithValue("@SoTaiKhoanMoi", req.SoTaiKhoan ?? (object)DBNull.Value);
            }

            await cmd.ExecuteNonQueryAsync();
        }
    }
}