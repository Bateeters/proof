import { Outlet, useNavigate } from "react-router-dom";
import { Sidebar } from "./Sidebar";
import { useAuth } from "../context/AuthContext";
import { useProfiles } from "../context/ProfileContext";

export function Layout() {
    const { account, logout } = useAuth();
    const { activeProfile } = useProfiles();
    const navigate = useNavigate();

    return (
        <div className="flex min-h-svh">
            <Sidebar />

            <div className="flex-1 min-w-0">
                <header className="flex flex-wrap items-center justify-between gap-3 border-b border-gold-600/30 bg-gold-600 px-8 py-4 sticky top-0 z-10">
                    {/* RequireProfile guarantees activeProfile is set for every
                        route this header renders on, so this is safe */}
                    <button
                        onClick={() => navigate('/profiles')}
                        className="flex items-center gap-2 rounded-full pl-1 pr-3 py-1 border border-white/60 bg-white
                            hover:border-ink-900/40 transition-colors"
                    >
                        <span
                            className="w-7 h-7 rounded-full flex items-center justify-center text-xs text-white shrink-0"
                            style={{ backgroundColor: activeProfile?.avatarColor }}
                        >
                            {activeProfile?.displayName.charAt(0).toUpperCase()}
                        </span>
                        <span className="text-sm text-ink-700">{activeProfile?.displayName}</span>
                        <span className="text-xs text-ink-900 font-medium">Switch</span>
                    </button>

                    {/* gold-600 is notably darker than the gold-400 this was
                        tuned for — dark text here would be borderline
                        illegible (~4:1 contrast), so this is light instead. */}
                    <div className="flex items-center gap-3 text-sm text-white">
                        <span>{account?.email}</span>
                        <button
                            className="inline-flex items-center justify-center rounded-md px-4 py-2 text-sm font-medium
                                bg-white/80 border border-white text-ink-900 hover:bg-white transition-colors"
                            onClick={logout}
                        >
                            Log Out
                        </button>
                    </div>
                </header>

                <main className="px-8 py-8">
                    <Outlet />
                </main>
            </div>
        </div>
    );
}
