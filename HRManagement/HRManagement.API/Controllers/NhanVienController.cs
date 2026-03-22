using Microsoft.AspNetCore.Mvc;
using HRManagement.BLL.Interfaces;
using HRManagement.Models;

namespace HRManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NhanVienController : ControllerBase
    {
        private readonly INhanVienService _service;

        public NhanVienController(INhanVienService service)
        {
            _service = service;
        }

        // GET /api/nhanvien
        // Admin  → full profile (cả 2 DB)
        // IT     → chỉ hành chính
        // KeToan → 403 (dùng /api/luong)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var role = HttpContext.Items["UserRole"]?.ToString();
            try
            {
                return role switch
                {
                    "Admin" => Ok(await _service.GetDanhSachFullAsync()),
                    "IT" => Ok(await _service.GetDanhSachHanhChinhAsync()),
                    "KeToan" => StatusCode(403, new { message = "KeToan dùng /api/luong để xem dữ liệu lương." }),
                    _ => StatusCode(403, new { message = "Không có quyền truy cập." })
                };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // GET /api/nhanvien/{maNV}
        [HttpGet("{maNV}")]
        public async Task<IActionResult> GetById(string maNV)
        {
            var role = HttpContext.Items["UserRole"]?.ToString();
            if (role == null) return Unauthorized();

            var nv = await _service.GetNhanVienByIdAsync(maNV);
            if (nv == null) return NotFound(new { message = $"Không tìm thấy nhân viên {maNV}." });
            return Ok(nv);
        }

        // POST /api/nhanvien — chỉ Admin
        [HttpPost]
        public async Task<IActionResult> ThemNhanVien([FromBody] ThemNhanVienRequest request)
        {
            var role = HttpContext.Items["UserRole"]?.ToString();
            if (role != "Admin")
                return StatusCode(403, new { message = "Chỉ Admin mới có quyền thêm nhân viên." });

            try
            {
                await _service.ThemNhanVienAsync(request);
                return CreatedAtAction(nameof(GetById), new { maNV = request.MaNV },
                    new { message = $"Thêm nhân viên {request.MaNV} thành công vào cả 2 DB." });
            }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (Exception ex) when (ex.Message.Contains("đã tồn tại")) { return Conflict(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // DELETE /api/nhanvien/{maNV} — chỉ Admin
        [HttpDelete("{maNV}")]
        public async Task<IActionResult> XoaNhanVien(string maNV)
        {
            var role = HttpContext.Items["UserRole"]?.ToString();
            if (role != "Admin")
                return StatusCode(403, new { message = "Chỉ Admin mới có quyền xóa nhân viên." });

            try
            {
                await _service.XoaNhanVienAsync(maNV);
                return Ok(new { message = $"Xóa nhân viên {maNV} khỏi cả 2 DB thành công." });
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // PUT /api/nhanvien/{maNV} — chỉ Admin (cập nhật hành chính)
        [HttpPut("{maNV}")]
        public async Task<IActionResult> CapNhatNhanVien(string maNV, [FromBody] CapNhatNhanVienRequest request)
        {
            var role = HttpContext.Items["UserRole"]?.ToString();
            if (role != "Admin")
                return StatusCode(403, new { message = "Chỉ Admin mới có quyền cập nhật thông tin nhân viên." });

            try
            {
                await _service.CapNhatNhanVienAsync(maNV, request);
                return Ok(new { message = $"Cập nhật thông tin nhân viên {maNV} thành công." });
            }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }
    }
}