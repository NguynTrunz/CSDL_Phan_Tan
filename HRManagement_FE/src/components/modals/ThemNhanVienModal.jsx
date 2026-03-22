import { useState } from "react";
import { apiFetch } from "../../api/apiFetch";

function Field({ label, placeholder, value, onChange, required }) {
  return (
    <div>
      <label className="block text-sm text-gray-600 mb-1">
        {label}{required && " *"}
      </label>
      <input
        value={value}
        onChange={(e) => onChange(e.target.value)}
        placeholder={placeholder}
        className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
      />
    </div>
  );
}

export default function ThemNhanVienModal({ role, onClose, onSuccess }) {
  const [form, setForm] = useState({
    maNV: "", ten: "", phongBan: "", chucVu: "",
    luongCoBan: "", heSo: "", phuCap: "", soTaiKhoan: "",
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const set = (key) => (val) => setForm((f) => ({ ...f, [key]: val }));

  const handleSubmit = async () => {
    if (!form.maNV || !form.ten || !form.luongCoBan || !form.heSo) {
      setError("Vui lòng điền đầy đủ các trường bắt buộc (*)");
      return;
    }
    setLoading(true);
    setError("");
    const res = await apiFetch("/api/NhanVien", role, {
      method: "POST",
      body: JSON.stringify({
        maNV:       form.maNV,
        ten:        form.ten,
        phongBan:   form.phongBan   || null,
        chucVu:     form.chucVu     || null,
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
      <div className="bg-white rounded-2xl shadow-2xl w-full max-w-2xl p-8">
        <div className="flex justify-between items-center mb-6">
          <h2 className="text-xl font-bold text-gray-800">Thêm Nhân Viên Mới</h2>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-600 text-2xl">×</button>
        </div>

        <div className="grid grid-cols-2 gap-4">
          <Field label="Mã NV"        placeholder="VD: NV001"       value={form.maNV}       onChange={set("maNV")}       required />
          <Field label="Lương Cơ Bản" placeholder="VD: 10000000"    value={form.luongCoBan} onChange={set("luongCoBan")} required />
          <Field label="Họ Tên"       placeholder="VD: Nguyễn Văn A" value={form.ten}        onChange={set("ten")}        required />
          <Field label="Hệ Số"        placeholder="VD: 2.5"          value={form.heSo}       onChange={set("heSo")}       required />
          <Field label="Phòng Ban"    placeholder="VD: Công Nghệ"    value={form.phongBan}   onChange={set("phongBan")} />
          <Field label="Phụ Cấp"      placeholder="VD: 2000000"      value={form.phuCap}     onChange={set("phuCap")} />
          <Field label="Chức Vụ"      placeholder="VD: Chuyên viên"  value={form.chucVu}     onChange={set("chucVu")} />
          <Field label="Số Tài Khoản" placeholder="VD: 1234567890"   value={form.soTaiKhoan} onChange={set("soTaiKhoan")} />
        </div>

        {error && <p className="text-red-500 text-sm mt-3">{error}</p>}

        <div className="mt-4 bg-blue-50 border border-blue-200 rounded-lg px-4 py-2 text-sm text-blue-700">
          💡 Lưu ý: Dữ liệu sẽ được lưu đồng thời vào DB Hành Chính và DB Lương
        </div>

        <div className="flex justify-end gap-3 mt-6">
          <button onClick={onClose}
            className="px-5 py-2 border border-gray-300 rounded-lg text-sm text-gray-600 hover:bg-gray-50">
            Hủy
          </button>
          <button onClick={handleSubmit} disabled={loading}
            className="px-5 py-2 bg-green-600 hover:bg-green-700 text-white rounded-lg text-sm font-medium disabled:opacity-50">
            {loading ? "Đang xử lý..." : "Thêm Nhân Viên"}
          </button>
        </div>
      </div>
    </div>
  );
}
