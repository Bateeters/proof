import { Outlet, useNavigate } from "react-router-dom";
import { Sidebar } from "./Sidebar";
import { useAuth } from "../context/AuthContext";
import { useProfiles } from "../context/ProfileContext";

export function Layout() {
    const { account, logout } = useAuth();
    const { activeProfile } = useProfiles();
    const navigate = useNavigate();

    return (
        <div className="flex min-h-svh bg-marble-50">
            <Sidebar />

            <div className="flex-1 min-w-0">
                <header className="flex flex-wrap items-center justify-between gap-3 border-b border-marble-300 bg-marble-50/90 backdrop-blur px-8 py-4 sticky top-0 z-10">
                    {/* RequireProfile guarantees activeProfile is set for every
                        route this header renders on, so this is safe */}
                    <button
                        onClick={() => navigate('/profiles')}
                        className="flex items-center gap-2 rounded-full pl-1 pr-3 py-1 border border-marble-300 bg-white
                            hover:border-gold-400 transition-colors"
                    >
                        <span
                            className="w-7 h-7 rounded-full flex items-center justify-center text-xs text-white shrink-0"
                            style={{ backgroundColor: activeProfile?.avatarColor }}
                        >
                            {activeProfile?.displayName.charAt(0).toUpperCase()}
                        </span>
                        <span className="text-sm text-ink-700">{activeProfile?.displayName}</span>
                        <span className="text-xs text-gold-600">Switch</span>
                    </button>

                    <div className="flex items-center gap-3 text-sm text-ink-600">
                        <span>{account?.email}</span>
                        <button className="btn-secondary" onClick={logout}>Log Out</button>
                    </div>
                </header>

                <main className="px-8 py-8">
                    <Outlet />
                </main>
            </div>
        </div>
    );
}
