import { useState, useEffect } from "react";
import { apiFetch, fmt } from "../api/apiFetch";
import ThemNhanVienModal from "../components/modals/ThemNhanVienModal";
import CapNhatLuongModal from "../components/modals/CapNhatLuongModal";

export default function AdminNhanVienPage({ role }) {
  const [data,    setData]    = useState([]);
  const [loading, setLoading] = useState(true);
  const [search,  setSearch]  = useState("");
  const [showAdd, setShowAdd] = useState(false);
  const [editNV,  setEditNV]  = useState(null);
  const [toast,   setToast]   = useState("");

  const load = async () => {
    setLoading(true);
    const res = await apiFetch("/api/NhanVien", role);
    if (res.ok) setData(res.data);
    setLoading(false);
  };

  useEffect(() => { load(); }, []);

  const showToast = (msg) => { setToast(msg); setTimeout(() => setToast(""), 3000); };

  const handleDelete = async (maNV) => {
    if (!confirm(`Xóa nhân viên ${maNV} khỏi cả 2 DB?`)) return;
    const res = await apiFetch(`/api/NhanVien/${maNV}`, role, { method: "DELETE" });
    if (res.ok) { showToast("✅ Xóa nhân viên thành công khỏi cả 2 DB!"); load(); }
    else showToast("❌ " + (res.data?.message || "Lỗi xóa nhân viên"));
  };

  const filtered = data.filter((nv) =>
    nv.ten?.toLowerCase().includes(search.toLowerCase()) ||
    nv.maNV?.toLowerCase().includes(search.toLowerCase())
  );

  return (
    <div className="p-8">
      {/* Toast */}
      {toast && (
        <div className="fixed top-4 right-4 bg-gray-800 text-white px-4 py-2 rounded-lg text-sm z-50 shadow-lg">
          {toast}
        </div>
      )}

      {/* Modals */}
      {showAdd && (
        <ThemNhanVienModal role={role} onClose={() => setShowAdd(false)}
          onSuccess={() => { load(); showToast("✅ Thêm nhân viên thành công vào cả 2 DB!"); }} />
      )}
      {editNV && (
        <CapNhatLuongModal nv={editNV} role={role} onClose={() => setEditNV(null)}
          onSuccess={() => { load(); showToast("✅ Cập nhật lương thành công!"); }} />
      )}

      <h1 className="text-2xl font-bold text-gray-800 mb-1">Quản Lý Nhân Viên</h1>
      <p className="text-gray-500 text-sm mb-6">Quản lý toàn bộ thông tin nhân viên và lương thưởng</p>

      <div className="flex justify-between items-center mb-4">
        <button onClick={() => setShowAdd(true)}
          className="bg-green-600 hover:bg-green-700 text-white px-4 py-2 rounded-lg text-sm font-medium flex items-center gap-2">
          + Thêm Nhân Viên
        </button>
        <input value={search} onChange={(e) => setSearch(e.target.value)}
          placeholder="Tìm kiếm nhân viên..."
          className="border border-gray-300 rounded-lg px-4 py-2 text-sm w-64 focus:outline-none focus:ring-2 focus:ring-blue-500" />
      </div>

      <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
        <table className="w-full text-sm">
          <thead className="bg-gray-50 border-b border-gray-200">
            <tr>
              {["Mã NV","Họ Tên","Phòng Ban","Chức Vụ","Lương Cơ Bản","Hệ Số","Phụ Cấp","Số Tài Khoản","Tổng Thu Nhập","Actions"].map((h) => (
                <th key={h} className="text-left px-4 py-3 text-gray-600 font-medium">{h}</th>
              ))}
            </tr>
          </thead>
          <tbody>
            {loading ? (
              <tr><td colSpan={10} className="text-center py-8 text-gray-400">Đang tải...</td></tr>
            ) : filtered.length === 0 ? (
              <tr><td colSpan={10} className="text-center py-8 text-gray-400">Không có dữ liệu</td></tr>
            ) : filtered.map((nv) => (
              <tr key={nv.maNV} className="border-b border-gray-100 hover:bg-gray-50">
                <td className="px-4 py-3 font-medium text-gray-800">{nv.maNV}</td>
                <td className="px-4 py-3">{nv.ten}</td>
                <td className="px-4 py-3 text-gray-600">{nv.phongBan}</td>
                <td className="px-4 py-3 text-gray-600">{nv.chucVu}</td>
                <td className="px-4 py-3">{nv.luongCoBan?.toLocaleString("vi-VN")}</td>
                <td className="px-4 py-3">{nv.heSo}</td>
                <td className="px-4 py-3">{nv.phuCap?.toLocaleString("vi-VN")}</td>
                <td className="px-4 py-3 text-gray-600">{nv.soTaiKhoan}</td>
                <td className="px-4 py-3 text-green-600 font-semibold">{fmt(nv.tongThuNhap)}</td>
                <td className="px-4 py-3">
                  <div className="flex gap-2">
                    <button onClick={() => setEditNV(nv)} className="text-blue-500 hover:text-blue-700">✏️</button>
                    <button onClick={() => handleDelete(nv.maNV)} className="text-red-500 hover:text-red-700">🗑️</button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
