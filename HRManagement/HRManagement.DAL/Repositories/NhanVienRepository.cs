using HRManagement.BLL.Interfaces;
using HRManagement.DAL.Interfaces;
using HRManagement.Models;
using Microsoft.Data.SqlClient;

namespace HRManagement.DAL.Repositories
{
    public class NhanVienRepository : INhanVienRepository
    {
        private readonly DbConnectionFactory _factory;

        public NhanVienRepository(DbConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task<IEnumerable<NhanVienHanhChinh>> GetAllAsync()
        {
            var result = new List<NhanVienHanhChinh>();
            const string sql = "SELECT MaNV, Ten, PhongBan, ChucVu FROM View_IT_HanhChinh";
            await using var conn = _factory.GetPersonnelITConnection();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(new NhanVienHanhChinh
                {
                    MaNV = reader.GetString(0),
                    Ten = reader.GetString(1),
                    PhongBan = reader.IsDBNull(2) ? null : reader.GetString(2),
                    ChucVu = reader.IsDBNull(3) ? null : reader.GetString(3)
                });
            }
            return result;
        }

        public async Task<NhanVienHanhChinh?> GetByIdAsync(string maNV)
        {
            const string sql = "SELECT MaNV, Ten, PhongBan, ChucVu FROM NhanVien_HanhChinh WHERE MaNV = @MaNV";
            await using var conn = _factory.GetPersonnelAdminConnection();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@MaNV", maNV);
            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new NhanVienHanhChinh
                {
                    MaNV = reader.GetString(0),
                    Ten = reader.GetString(1),
                    PhongBan = reader.IsDBNull(2) ? null : reader.GetString(2),
                    ChucVu = reader.IsDBNull(3) ? null : reader.GetString(3)
                };
            }
            return null;
        }

        public async Task<IEnumerable<NhanVienFullProfile>> GetFullProfilesAsync()
        {
            var result = new List<NhanVienFullProfile>();
            const string sql = @"SELECT MaNV, Ten, PhongBan, ChucVu,
                                         LuongCoBan, HeSo, PhuCap, SoTaiKhoan, TongThuNhap
                                  FROM View_Admin_Full_Profile";
            await using var conn = _factory.GetPersonnelAdminConnection();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(new NhanVienFullProfile
                {
                    MaNV = reader.GetString(0),
                    Ten = reader.GetString(1),
                    PhongBan = reader.IsDBNull(2) ? null : reader.GetString(2),
                    ChucVu = reader.IsDBNull(3) ? null : reader.GetString(3),
                    LuongCoBan = reader.IsDBNull(4) ? null : (decimal?)Convert.ToDecimal(reader.GetValue(4)),
                    HeSo = reader.IsDBNull(5) ? null : (double?)Convert.ToDouble(reader.GetValue(5)),
                    PhuCap = reader.IsDBNull(6) ? null : (decimal?)Convert.ToDecimal(reader.GetValue(6)),
                    SoTaiKhoan = reader.IsDBNull(7) ? null : reader.GetString(7),
                    TongThuNhap = reader.IsDBNull(8) ? null : (decimal?)Convert.ToDecimal(reader.GetValue(8))
                });
            }
            return result;
        }

        public async Task<IEnumerable<NhanVienKeToan>> GetPayrollKeToanConnection()
        {
            var result = new List<NhanVienKeToan>();
            const string sql = @"SELECT MaNV, Ten, PhongBan, LuongCoBan, HeSo, PhuCap, SoTaiKhoan,TongThuNhap
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
                    LuongCoBan = reader.IsDBNull(4) ? null : (decimal?)Convert.ToDecimal(reader.GetValue(4)),
                    HeSo = reader.IsDBNull(4) ? null : (double?)Convert.ToDouble(reader.GetValue(4)),
                    PhuCap = reader.IsDBNull(5) ? null : (decimal?)Convert.ToDecimal(reader.GetValue(5)),
                    SoTaiKhoan = reader.IsDBNull(7) ? null : reader.GetString(7),
                    TongThuNhap = reader.IsDBNull(6) ? null : (decimal?)Convert.ToDecimal(reader.GetValue(6))
                });
            }
            return result;
        }

        public async Task ThemNhanVienAsync(ThemNhanVienRequest req)
        {
            await using var conn = _factory.GetPersonnelAdminConnection();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand("sp_InsertEmployeeFull", conn)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@MaNV", req.MaNV);
            cmd.Parameters.AddWithValue("@Ten", req.Ten);
            cmd.Parameters.AddWithValue("@PB", req.PhongBan ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@CV", req.ChucVu ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Luong", req.LuongCoBan);
            cmd.Parameters.AddWithValue("@HeSo", req.HeSo);
            cmd.Parameters.AddWithValue("@PC", req.PhuCap);
            cmd.Parameters.AddWithValue("@STK", req.SoTaiKhoan ?? (object)DBNull.Value);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task XoaNhanVienAsync(string maNV)
        {
            await using var conn = _factory.GetPersonnelAdminConnection();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand("sp_DeleteEmployeeFull", conn)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@MaNV", maNV);
            await cmd.ExecuteNonQueryAsync();
        }
        public async Task CapNhatNhanVienAsync(string maNV, CapNhatNhanVienRequest req)
        {
            await using var conn = _factory.GetPersonnelAdminConnection();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand("sp_UpdateNhanVienHanhChinh", conn)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@MaNV", maNV);
            cmd.Parameters.AddWithValue("@Ten", req.Ten);
            cmd.Parameters.AddWithValue("@PB", req.PhongBan ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@CV", req.ChucVu ?? (object)DBNull.Value);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}