import {  Outlet, useNavigate } from "react-router-dom";
import Navbar from "../../../components/layout/Navbar";
import { useEffect, useState, type ReactNode } from "react";
import { useUser } from "../../../global-hooks/authHooks";
import Sidebar from "../../../components/layout/Sidebar";

type Props = {
  navItems: ReactNode;
};

export default function DashboardLayout({ navItems }: Props) {
  const navigate = useNavigate();
  const { user } = useUser();
  const [sidebarOpen, setSidebarOpen] = useState(false);

  useEffect(() => {
    if (user === undefined) navigate("/");
  }, [user, navigate]);

  useEffect(() => {
    if (!sidebarOpen) return;

    const handleClick = (e: MouseEvent) => {
      const target = e.target as HTMLElement;
      if (target.closest(".sidebar")) return;
      setSidebarOpen(false);
    };

    const handleKey = (e: KeyboardEvent) => {
      if (e.key === "Escape") setSidebarOpen(false);
    };

    document.addEventListener("click", handleClick);
    document.addEventListener("keydown", handleKey);

    return () => {
      document.removeEventListener("click", handleClick);
      document.removeEventListener("keydown", handleKey);
    };
  }, [sidebarOpen]);

  if (user === undefined) return null;

  return (
    <>
      <Navbar>
        <div>
          <h1>Welcome {user.firstName} {user.lastName}</h1>
        </div>

        <div>
          {navItems}
          <button onClick={(e) => {e.stopPropagation(); setSidebarOpen((prev) => !prev);}}>
            ☰
          </button>
        </div>
      </Navbar>

      <main className="main-content">
        {sidebarOpen && (
          <div
            className="sidebar-backdrop"
            onClick={() => setSidebarOpen(false)}
          />
        )}

        <Sidebar open={sidebarOpen} />
        <Outlet />
      </main>
    </>
  );
}