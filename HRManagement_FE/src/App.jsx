import { useState } from "react";
import Navbar            from "./components/Navbar";
import Sidebar           from "./components/Sidebar";
import LoginPage         from "./pages/LoginPage";
import AdminNhanVienPage from "./pages/AdminNhanVienPage";
import KeToanPage        from "./pages/KeToanPage";
import ITPage            from "./pages/ITPage";
import BaoCaoPage        from "./pages/BaoCaoPage";

export default function App() {
  const [role,       setRole]       = useState(null);
  const [activePage, setActivePage] = useState("nhanvien");

  const handleLogin = (r) => {
    setRole(r);
    // Set trang mặc định theo role
    if (r === "KeToan") setActivePage("luong");
    else setActivePage("nhanvien");
  };

  if (!role) return <LoginPage onLogin={handleLogin} />;

  const renderPage = () => {
    if (role === "IT")     return <ITPage role={role} />;
    if (role === "KeToan") return <KeToanPage role={role} />;

    // Admin
    if (activePage === "baocao") return <BaoCaoPage role={role} />;
    return <AdminNhanVienPage role={role} />;
  };

  return (
    <div className="min-h-screen bg-gray-50 flex flex-col">
      <Navbar role={role} onLogout={() => setRole(null)} />
      <div className="flex flex-1">
        <Sidebar role={role} activePage={activePage} setActivePage={setActivePage} />
        <main className="flex-1 overflow-auto">{renderPage()}</main>
      </div>
    </div>
  );
}
