import { useState } from "react";
import { apiFetch, fmt } from "../../api/apiFetch";

export default function CapNhatNhanVienModal({ nv, role, onClose, onSuccess }) {
  const [form, setForm] = useState({
    ten: nv.ten || "",
    phongBan: nv.phongBan || "",
    chucVu: nv.chucVu || "",
    soTaiKhoan: nv.soTaiKhoan || "",
    luongCoBan: nv.luongCoBan || "",
    heSo: nv.heSo || "",
    phuCap: nv.phuCap || "",
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const set = (key) => (e) => setForm((f) => ({ ...f, [key]: e.target.value }));

  const tongThuNhap =
    parseFloat(form.luongCoBan || 0) * parseFloat(form.heSo || 0) +
    parseFloat(form.phuCap || 0);

  const handleSubmit = async () => {
    if (!form.ten) { setError("Họ tên không được để trống."); return; }
    setLoading(true);
    setError("");

    try {
      // Bước 1: Cập nhật thông tin hành chính (DB_Personnel)
      const resHC = await apiFetch(`/api/NhanVien/${nv.maNV}`, role, {
        method: "PUT",
        body: JSON.stringify({
          ten: form.ten,
          phongBan: form.phongBan || null,
          chucVu: form.chucVu || null,
        }),
      });

      if (!resHC.ok) {
        setError(resHC.data?.message || "Lỗi cập nhật thông tin hành chính.");
        setLoading(false);
        return;
      }

      // Bước 2: Cập nhật lương (DB_Payroll) — Admin null SoTaiKhoan, backend giữ nguyên
      const resLuong = await apiFetch(`/api/Luong/${nv.maNV}`, role, {
        method: "PUT",
        body: JSON.stringify({
          luongCoBan: parseFloat(form.luongCoBan) || 0,
          heSo: parseFloat(form.heSo) || 0,
          phuCap: parseFloat(form.phuCap) || 0,
          soTaiKhoan: form.soTaiKhoan || null,
        }),
      });

      if (!resLuong.ok) {
        setError(resLuong.data?.message || "Lỗi cập nhật thông tin lương.");
        setLoading(false);
        return;
      }

      onSuccess();
      onClose();
    } catch (err) {
      setError("Lỗi kết nối: " + err.message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50 p-4">
      <div className="bg-white rounded-2xl shadow-2xl w-full max-w-3xl p-8">
        <div className="flex justify-between items-center mb-6">
          <div>
            <h2 className="text-xl font-bold text-gray-800">Cập Nhật Thông Tin Nhân Viên</h2>
            <p className="text-sm text-gray-500 mt-1">Mã NV: <strong>{nv.maNV}</strong></p>
          </div>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-600 text-3xl leading-none">&times;</button>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
          {/* Cột 1: Thông tin hành chính → PUT /api/NhanVien */}
          <div>
            <h3 className="text-sm font-semibold text-blue-600 mb-3 uppercase tracking-wider border-b pb-2">
              Thông Tin Hành Chính
            </h3>

            <div className="mb-3">
              <label className="block text-sm text-gray-600 mb-1">Mã NV (Không thể đổi)</label>
              <input value={nv.maNV} readOnly
                className="w-full bg-gray-100 border border-gray-200 rounded-lg px-3 py-2 text-sm text-gray-500 cursor-not-allowed" />
            </div>

            {[
              { label: "Họ Tên *", key: "ten" },
              { label: "Phòng Ban", key: "phongBan" },
              { label: "Chức Vụ", key: "chucVu" },
            ].map(({ label, key }) => (
              <div key={key} className="mb-3">
                <label className="block text-sm text-gray-600 mb-1">{label}</label>
                <input value={form[key]} onChange={set(key)}
                  className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
              </div>
            ))}
          </div>

          {/* Cột 2: Thông tin lương → PUT /api/Luong */}
          <div>
            <h3 className="text-sm font-semibold text-green-600 mb-3 uppercase tracking-wider border-b pb-2">
              Thông Tin Lương
            </h3>

            {[
              { label: "Lương Cơ Bản *", key: "luongCoBan", type: "number" },
              { label: "Hệ Số *", key: "heSo", type: "number", step: "0.1" },
              { label: "Phụ Cấp", key: "phuCap", type: "number" },
              { label: "Số Tài Khoản", key: "soTaiKhoan", type: "text" },
            ].map(({ label, key, type, step }) => (
              <div key={key} className="mb-3">
                <label className="block text-sm text-gray-600 mb-1">{label}</label>
                <input type={type} step={step} value={form[key]} onChange={set(key)}
                  className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
              </div>
            ))}

            <div className="bg-green-50 border border-green-200 rounded-lg px-4 py-3 mt-2">
              <span className="block text-xs text-green-600 mb-1">Tổng Thu Nhập Dự Kiến</span>
              <span className="text-lg text-green-700 font-bold">{fmt(tongThuNhap)}</span>
            </div>
          </div>
        </div>

        {error && <p className="text-red-500 text-sm mt-4 text-center">{error}</p>}

        <div className="flex justify-end gap-3 mt-8 pt-4 border-t border-gray-100">
          <button onClick={onClose}
            className="px-5 py-2 border border-gray-300 rounded-lg text-sm text-gray-600 hover:bg-gray-50">
            Hủy
          </button>
          <button onClick={handleSubmit} disabled={loading}
            className="px-6 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg text-sm font-medium disabled:opacity-50 transition-colors">
            {loading ? "Đang lưu..." : "Lưu Thay Đổi"}
          </button>
        </div>
      </div>
    </div>
  );
}