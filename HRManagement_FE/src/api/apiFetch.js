const API_BASE = "http://localhost:5075";

export async function apiFetch(path, role, options = {}) {
  const res = await fetch(`${API_BASE}${path}`, {
    ...options,
    headers: {
      "Content-Type": "application/json",
      "X-Role": role,
      ...(options.headers || {}),
    },
  });

  const text = await res.text();
  try {
    return { ok: res.ok, status: res.status, data: JSON.parse(text) };
  } catch {
    return { ok: res.ok, status: res.status, data: text };
  }
}

export const fmt = (n) =>
  n != null ? Number(n).toLocaleString("vi-VN") + " VNĐ" : "—";

export const roleLabel = {
  Admin: "Admin",
  KeToan: "Kế Toán",
  IT: "Nhân Viên IT",
};

export const roleBadgeColor = {
  Admin: "bg-green-500",
  KeToan: "bg-yellow-500",
  IT: "bg-blue-500",
};