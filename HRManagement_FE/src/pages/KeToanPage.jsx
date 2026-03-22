import { useState, useEffect } from "react";
import { apiFetch, fmt } from "../api/apiFetch";

function CapNhatLuongModal({ nv, role, onClose, onSuccess }) {
  const [form, setForm] = useState({
    luongCoBan: nv.luongCoBan || "",
    heSo:       nv.heSo       || "",
    phuCap:     nv.phuCap     || "",
    soTaiKhoan: nv.soTaiKhoan || "",
  });
  const [loading, setLoading] = useState(false);
  const [error,   setError]   = useState("");

  const tongThuNhap =
    parseFloat(form.luongCoBan || 0) * parseFloat(form.heSo || 0) +
    parseFloat(form.phuCap     || 0);

  const handleSubmit = async () => {
    if (!form.luongCoBan || !form.heSo) {
      setError("Vui lòng điền đầy đủ các trường bắt buộc.");
      return;
    }
    setLoading(true);
    setError("");
    const res = await apiFetch(`/api/Luong/${nv.maNV}`, role, {
      method: "PUT",
      body: JSON.stringify({
        luongCoBan: parseFloat(form.luongCoBan),
        heSo:       parseFloat(form.heSo),
        phuCap:     parseFloat(form.phuCap) || 0,
        soTaiKhoan: form.soTaiKhoan || null,
      }),
    });
    setLoading(false);
    if (res.ok) { onSuccess(); onClose(); }
    else setError(res.data?.message || "Có lỗi xảy ra");
  };

  return (
    <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
      <div className="bg-white rounded-2xl shadow-2xl w-full max-w-lg p-8">
        <div className="flex justify-between items-center mb-2">
          <h2 className="text-xl font-bold text-gray-800">Cập Nhật Lương</h2>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-600 text-2xl">×</button>
        </div>
        <p className="text-gray-500 text-sm mb-6">Mã NV: <strong>{nv.maNV}</strong></p>

        {[
          { label: "Lương Cơ Bản *", key: "luongCoBan", type: "number" },
          { label: "Hệ Số *",        key: "heSo",       type: "number" },
          { label: "Phụ Cấp",        key: "phuCap",     type: "number" },
          { label: "Số Tài Khoản",   key: "soTaiKhoan", type: "text"   },
        ].map(({ label, key, type }) => (
          <div key={key} className="mb-4">
            <label className="block text-sm text-gray-600 mb-1">{label}</label>
            <input
              type={type}
              value={form[key]}
              onChange={(e) => setForm((f) => ({ ...f, [key]: e.target.value }))}
              className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-yellow-400"
            />
          </div>
        ))}

        <div className="bg-green-50 border border-green-200 rounded-lg px-4 py-3 mb-4">
          <span className="text-sm text-green-700 font-medium">
            Tổng Thu Nhập dự kiến: {fmt(tongThuNhap)}
          </span>
        </div>

        {error && <p className="text-red-500 text-sm mb-3">{error}</p>}

        <div className="flex justify-end gap-3">
          <button onClick={onClose}
            className="px-5 py-2 border border-gray-300 rounded-lg text-sm text-gray-600 hover:bg-gray-50">
            Hủy
          </button>
          <button onClick={handleSubmit} disabled={loading}
            className="px-5 py-2 bg-yellow-500 hover:bg-yellow-600 text-white rounded-lg text-sm font-medium disabled:opacity-50">
            {loading ? "Đang xử lý..." : "Cập Nhật"}
          </button>
        </div>
      </div>
    </div>
  );
}

export default function KeToanPage({ role }) {
  const [data,    setData]   = useState([]);
  const [loading, setLoading] = useState(true);
  const [search,  setSearch]  = useState("");
  const [editNV,  setEditNV]  = useState(null);
  const [toast,   setToast]   = useState("");

  const load = async () => {
    setLoading(true);
    const res = await apiFetch("/api/Luong", role);
    if (res.ok) setData(res.data);
    setLoading(false);
  };

  useEffect(() => { load(); }, []);

  const showToast = (msg) => { setToast(msg); setTimeout(() => setToast(""), 3000); };

  const filtered = data.filter((nv) =>
    nv.maNV?.toLowerCase().includes(search.toLowerCase())
  );

  return (
    <div className="p-8">
      {toast && (
        <div className="fixed top-4 right-4 bg-gray-800 text-white px-4 py-2 rounded-lg text-sm z-50 shadow-lg">{toast}</div>
      )}
      {editNV && (
        <CapNhatLuongModal nv={editNV} role={role} onClose={() => setEditNV(null)}
          onSuccess={() => { load(); showToast("✅ Cập nhật lương thành công!"); }} />
      )}

      <h1 className="text-2xl font-bold text-gray-800 mb-1">Quản Lý Lương</h1>
      <p className="text-gray-500 text-sm mb-6">Xem và cập nhật thông tin lương của nhân viên</p>

      <div className="flex justify-end mb-4">
        <input value={search} onChange={(e) => setSearch(e.target.value)}
          placeholder="Tìm kiếm mã NV..."
          className="border border-gray-300 rounded-lg px-4 py-2 text-sm w-64 focus:outline-none focus:ring-2 focus:ring-yellow-400" />
      </div>

      <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
        <table className="w-full text-sm">
          <thead className="bg-gray-50 border-b border-gray-200">
            <tr>
              {["Mã NV", "Lương Cơ Bản", "Hệ Số", "Phụ Cấp", "Số Tài Khoản", "Tổng Thu Nhập", "Ngày CN", "Actions"].map((h) => (
                <th key={h} className="text-left px-4 py-3 text-gray-600 font-medium">{h}</th>
              ))}
            </tr>
          </thead>
          <tbody>
            {loading ? (
              <tr><td colSpan={8} className="text-center py-8 text-gray-400">Đang tải...</td></tr>
            ) : filtered.length === 0 ? (
              <tr><td colSpan={8} className="text-center py-8 text-gray-400">Không có dữ liệu</td></tr>
            ) : filtered.map((nv) => (
              <tr key={nv.maNV} className="border-b border-gray-100 hover:bg-gray-50">
                <td className="px-4 py-3 font-medium">{nv.maNV}</td>
                <td className="px-4 py-3">{nv.luongCoBan?.toLocaleString("vi-VN")}</td>
                <td className="px-4 py-3">{nv.heSo}</td>
                <td className="px-4 py-3">{nv.phuCap?.toLocaleString("vi-VN")}</td>
                <td className="px-4 py-3 text-gray-600">{nv.soTaiKhoan || "—"}</td>
                <td className="px-4 py-3 text-green-600 font-semibold">{fmt(nv.tongThuNhap)}</td>
                <td className="px-4 py-3 text-gray-400 text-xs">
                  {nv.ngayCapNhat ? new Date(nv.ngayCapNhat).toLocaleDateString("vi-VN") : "—"}
                </td>
                <td className="px-4 py-3">
                  <button onClick={() => setEditNV(nv)} className="text-yellow-500 hover:text-yellow-700">✏️</button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <div className="mt-4 bg-yellow-50 border border-yellow-200 rounded-lg px-4 py-3 text-sm text-yellow-700">
        💼 Bạn có quyền xem và cập nhật toàn bộ thông tin lương bao gồm số tài khoản. Không có quyền xóa hoặc thêm nhân viên.
      </div>
    </div>
  );
}
