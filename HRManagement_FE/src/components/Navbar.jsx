import { roleLabel, roleBadgeColor } from "../api/apiFetch";

export default function Navbar({ role, onLogout }) {
  return (
    <div className="h-14 bg-white border-b border-gray-200 flex items-center justify-between px-6">
      <div className="flex items-center gap-2">
        <div className="w-7 h-7 rounded bg-blue-100 flex items-center justify-center">
          <svg className="w-4 h-4 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2}
              d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
          </svg>
        </div>
        <span className="font-semibold text-gray-800 text-sm">HR Management System</span>
      </div>
      <div className="flex items-center gap-3">
        <div className="flex items-center gap-2">
          <div className={`w-2 h-2 rounded-full ${roleBadgeColor[role]}`} />
          <span className={`px-3 py-1 rounded-full text-white text-xs font-semibold ${roleBadgeColor[role]}`}>
            {roleLabel[role]}
          </span>
        </div>
        <button
          onClick={onLogout}
          className="flex items-center gap-1 text-gray-500 hover:text-gray-700 text-sm"
        >
          → Đăng xuất
        </button>
      </div>
    </div>
  );
}
