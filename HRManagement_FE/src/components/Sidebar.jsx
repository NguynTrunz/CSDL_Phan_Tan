export default function Sidebar({ role, activePage, setActivePage }) {
  const menus = {
    Admin:   [
      { id: "nhanvien", label: "Nhân Sự",  icon: "👤" },
      { id: "baocao",   label: "Báo Cáo",  icon: "📋" },
    ],
    KeToan:  [
      { id: "luong",    label: "Lương",     icon: "💲" },
    ],
    IT:      [
      { id: "nhanvien", label: "Nhân Viên", icon: "👤" },
    ],
  };

  const items = menus[role] || [];

  return (
    <div className="w-56 bg-white border-r border-gray-200 min-h-screen pt-4">
      {items.map((m) => (
        <button
          key={m.id}
          onClick={() => setActivePage(m.id)}
          className={`w-full flex items-center gap-3 px-5 py-3 text-sm font-medium transition-colors
            ${activePage === m.id
              ? "bg-blue-600 text-white rounded-lg mx-2 w-[calc(100%-16px)]"
              : "text-gray-600 hover:bg-gray-100"
            }`}
        >
          <span>{m.icon}</span>
          {m.label}
        </button>
      ))}
    </div>
  );
}
