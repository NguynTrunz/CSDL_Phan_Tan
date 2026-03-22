import { useState, useEffect } from "react";
import { apiFetch } from "../api/apiFetch";

export default function ITPage({ role }) {
  const [data,    setData]    = useState([]);
  const [loading, setLoading] = useState(true);
  const [search,  setSearch]  = useState("");

  useEffect(() => {
    apiFetch("/api/NhanVien", role).then((res) => {
      if (res.ok) setData(res.data);
      setLoading(false);
    });
  }, []);

  const filtered = data.filter((nv) =>
    nv.ten?.toLowerCase().includes(search.toLowerCase()) ||
    nv.maNV?.toLowerCase().includes(search.toLowerCase())
  );

  return (
    <div className="p-8">
      <div className="flex justify-between items-start mb-6">
        <div>
          <h1 className="text-2xl font-bold text-gray-800 mb-1">Danh Sách Nhân Viên</h1>
          <p className="text-gray-500 text-sm">Xem thông tin cơ bản của nhân viên</p>
        </div>
        <span className="flex items-center gap-1 text-gray-500 text-sm bg-gray-100 px-3 py-1 rounded-full">
          🔒 Chỉ xem
        </span>
      </div>

      <div className="flex justify-end mb-4">
        <input value={search} onChange={(e) => setSearch(e.target.value)}
          placeholder="Tìm kiếm nhân viên..."
          className="border border-gray-300 rounded-lg px-4 py-2 text-sm w-64 focus:outline-none focus:ring-2 focus:ring-blue-500" />
      </div>

      <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
        <table className="w-full text-sm">
          <thead className="bg-gray-50 border-b border-gray-200">
            <tr>
              {["Mã NV","Họ Tên","Phòng Ban","Chức Vụ","🔒 Dữ liệu lương"].map((h) => (
                <th key={h} className="text-left px-4 py-3 text-gray-600 font-medium">{h}</th>
              ))}
            </tr>
          </thead>
          <tbody>
            {loading ? (
              <tr><td colSpan={5} className="text-center py-8 text-gray-400">Đang tải...</td></tr>
            ) : filtered.map((nv) => (
              <tr key={nv.maNV} className="border-b border-gray-100 hover:bg-gray-50">
                <td className="px-4 py-3 font-medium">{nv.maNV}</td>
                <td className="px-4 py-3">{nv.ten}</td>
                <td className="px-4 py-3 text-gray-600">{nv.phongBan}</td>
                <td className="px-4 py-3 text-gray-600">{nv.chucVu}</td>
                <td className="px-4 py-3 text-gray-300 tracking-widest">🔒 ● ● ● ● ●</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <div className="mt-4 bg-blue-50 border border-blue-200 rounded-lg px-4 py-3 text-sm text-blue-700">
        ℹ️ <strong>Thông báo:</strong> Bạn chỉ có quyền xem thông tin cơ bản của nhân viên. Dữ liệu về lương và tài khoản ngân hàng đã được ẩn để bảo mật.
      </div>
    </div>
  );
}
