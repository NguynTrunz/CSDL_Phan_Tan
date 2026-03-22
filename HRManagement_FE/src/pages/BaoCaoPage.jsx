import { useState, useEffect } from "react";
import { apiFetch, fmt } from "../api/apiFetch";

export default function BaoCaoPage({ role }) {
  const [data, setData] = useState([]);

  useEffect(() => {
    apiFetch("/api/NhanVien", role).then((res) => { if (res.ok) setData(res.data); });
  }, []);

  const tongQuy  = data.reduce((s, nv) => s + (nv.tongThuNhap  || 0), 0);
  const avgLuong = data.length
    ? data.reduce((s, nv) => s + (nv.luongCoBan || 0), 0) / data.length
    : 0;

  return (
    <div className="p-8">
      <h1 className="text-2xl font-bold text-gray-800 mb-1">Báo Cáo</h1>
      <p className="text-gray-500 text-sm mb-6">Thống kê tổng quan hệ thống nhân sự</p>

      {/* Stats cards */}
      <div className="grid grid-cols-3 gap-4 mb-8">
        <div className="bg-blue-50 border border-blue-200 rounded-xl p-5">
          <p className="text-blue-600 text-sm font-medium">Tổng Nhân Viên</p>
          <p className="text-blue-800 text-2xl font-bold mt-1">{data.length} người</p>
        </div>
        <div className="bg-green-50 border border-green-200 rounded-xl p-5">
          <p className="text-green-600 text-sm font-medium">Tổng Quỹ Lương</p>
          <p className="text-green-800 text-2xl font-bold mt-1">{fmt(tongQuy)}</p>
        </div>
        <div className="bg-yellow-50 border border-yellow-200 rounded-xl p-5">
          <p className="text-yellow-600 text-sm font-medium">Lương TB/NV</p>
          <p className="text-yellow-800 text-2xl font-bold mt-1">{fmt(avgLuong)}</p>
        </div>
      </div>

      {/* Detail table */}
      <div className="bg-white rounded-xl border border-gray-200 p-5">
        <h2 className="font-semibold text-gray-700 mb-4">Chi tiết lương theo nhân viên</h2>
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b border-gray-200">
              {["Mã NV","Họ Tên","Phòng Ban","Lương CB","Hệ Số","Phụ Cấp","Tổng Thu Nhập"].map((h) => (
                <th key={h} className="text-left pb-2 text-gray-500 font-medium">{h}</th>
              ))}
            </tr>
          </thead>
          <tbody>
            {data.map((nv) => (
              <tr key={nv.maNV} className="border-b border-gray-50">
                <td className="py-2 font-medium">{nv.maNV}</td>
                <td className="py-2">{nv.ten}</td>
                <td className="py-2 text-gray-500">{nv.phongBan}</td>
                <td className="py-2">{nv.luongCoBan?.toLocaleString("vi-VN")}</td>
                <td className="py-2">{nv.heSo}</td>
                <td className="py-2">{nv.phuCap?.toLocaleString("vi-VN")}</td>
                <td className="py-2 text-green-600 font-semibold">{fmt(nv.tongThuNhap)}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
