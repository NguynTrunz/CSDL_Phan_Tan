using Microsoft.AspNetCore.Mvc;
using HRManagement.BLL.Interfaces;
using HRManagement.Models;

namespace HRManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LuongController : ControllerBase
    {
        private readonly ILuongService _service;

        public LuongController(ILuongService service)
        {
            _service = service;
        }

        // GET /api/luong
        // Admin & KeToan xem danh sách lương từ DB_Payroll
        [HttpGet]
        public async Task<IActionResult> GetDanhSach()
        {
            var role = HttpContext.Items["UserRole"]?.ToString();
            if (role != "Admin" && role != "KeToan")
                return StatusCode(403, new { message = $"Role '{role}' không có quyền xem thông tin lương." });

            try
            {
                var data = await _service.GetDanhSachLuongKeToanAsync();
                return Ok(data);
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // GET /api/luong/{maNV}
        [HttpGet("{maNV}")]
        public async Task<IActionResult> GetLuong(string maNV)
        {
            var role = HttpContext.Items["UserRole"]?.ToString();
            if (role != "Admin" && role != "KeToan")
                return StatusCode(403, new { message = $"Role '{role}' không có quyền xem thông tin lương." });

            var luong = await _service.GetLuongByIdAsync(maNV);
            if (luong == null)
                return NotFound(new { message = $"Không tìm thấy lương của nhân viên {maNV}." });
            return Ok(luong);
        }

        // PUT /api/luong/{maNV}
        // Admin & KeToan đều gọi sp_UpdateLuong (SP đã có @SoTaiKhoanMoi)
        // Admin: SoTaiKhoan = null (không đổi)
        // KeToan: SoTaiKhoan = giá trị mới
        [HttpPut("{maNV}")]
        public async Task<IActionResult> CapNhatLuong(string maNV, [FromBody] CapNhatLuongRequest request)
        {
            var role = HttpContext.Items["UserRole"]?.ToString();
            if (role != "Admin" && role != "KeToan")
                return StatusCode(403, new { message = "Chỉ Admin hoặc KeToan mới có quyền cập nhật lương." });

            // Admin không được đổi SoTaiKhoan
            if (role == "Admin")
                request.SoTaiKhoan = null;

            try
            {
                await _service.CapNhatLuongAsync(maNV, request, role);
                return Ok(new { message = $"Cập nhật lương nhân viên {maNV} thành công." });
            }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = "Lỗi: " + ex.Message }); }
        }
    }
}