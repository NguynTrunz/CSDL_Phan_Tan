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
        public async Task CapNhatLuongAsync(string maNV, CapNhatLuongRequest req)
        {
            // Nếu Admin không truyền SoTaiKhoan → lấy giá trị cũ để không bị ghi đè thành NULL
            string? soTaiKhoan = req.SoTaiKhoan;
            if (soTaiKhoan == null)
            {
                // Lấy SoTaiKhoan hiện tại
                const string sqlGet = "SELECT SoTaiKhoan FROM NhanVien_Luong WHERE MaNV = @MaNV";
                await using var connGet = _factory.GetPayrollAdminConnection();
                await connGet.OpenAsync();
                await using var cmdGet = new SqlCommand(sqlGet, connGet);
                cmdGet.Parameters.AddWithValue("@MaNV", maNV);
                var val = await cmdGet.ExecuteScalarAsync();
                soTaiKhoan = val == DBNull.Value ? null : val?.ToString();
            }

            await using var conn = _factory.GetPayrollAdminConnection();
            await conn.OpenAsync();
            await using var cmd = new SqlCommand("sp_UpdateLuong", conn)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };

            // Đúng thứ tự theo SP: @MaNV, @LuongMoi, @HeSoMoi, @PhuCapMoi, @SoTaiKhoanMoi
            cmd.Parameters.AddWithValue("@MaNV", maNV);
            cmd.Parameters.AddWithValue("@LuongMoi", req.LuongCoBan);
            cmd.Parameters.AddWithValue("@HeSoMoi", req.HeSo);
            cmd.Parameters.AddWithValue("@PhuCapMoi", req.PhuCap);
            cmd.Parameters.AddWithValue("@SoTaiKhoanMoi", soTaiKhoan ?? (object)DBNull.Value);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}