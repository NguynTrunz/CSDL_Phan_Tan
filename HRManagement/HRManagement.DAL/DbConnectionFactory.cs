using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace HRManagement.DAL
{
    /// <summary>
    /// Tạo SqlConnection theo đúng SQL Login của từng role.
    /// Mỗi Login được cấu hình sẵn trong appsettings.json.
    /// SQL Server tự enforce phân quyền theo Login đó.
    /// </summary>
    public class DbConnectionFactory
    {
        private readonly IConfiguration _config;

        public DbConnectionFactory(IConfiguration config)
        {
            _config = config;
        }

        // DB_Personnel - Login_Admin (toàn quyền Personnel)
        public SqlConnection GetPersonnelAdminConnection()
            => new SqlConnection(_config.GetConnectionString("Personnel_Admin"));

        // DB_Personnel - Login_NhanVienIT (chỉ đọc View_IT_HanhChinh)
        public SqlConnection GetPersonnelITConnection()
            => new SqlConnection(_config.GetConnectionString("Personnel_IT"));

        // DB_Payroll - Login_KeToan (SELECT/UPDATE lương, không DELETE/INSERT)
        public SqlConnection GetPayrollKeToanConnection()
            => new SqlConnection(_config.GetConnectionString("Payroll_KeToan"));

        // DB_Payroll - Login_Admin (toàn quyền Payroll)
        public SqlConnection GetPayrollAdminConnection()
            => new SqlConnection(_config.GetConnectionString("Payroll_Admin"));
    }
}