import { Outlet } from "react-router-dom";
import { Sidebar } from "./Sidebar";
import { ProfileSwitcher } from "./ProfileSwitcher";
import { useAuth } from "../context/AuthContext";
import { useProfiles } from "../context/ProfileContext";

export function Layout() {
    const { account, logout } = useAuth();
    const { activeProfile } = useProfiles();

    return (
        <div className="flex min-h-svh bg-marble-50">
            <Sidebar />

            <div className="flex-1 min-w-0">
                <header className="flex flex-wrap items-center justify-between gap-3 border-b border-marble-300 bg-marble-50/90 backdrop-blur px-8 py-4 sticky top-0 z-10">
                    <ProfileSwitcher />

                    <div className="flex items-center gap-3 text-sm text-ink-600">
                        <span>{account?.email}</span>
                        {activeProfile && (
                            <span className="rounded-full bg-gold-300/20 text-gold-600 px-3 py-1 text-xs font-medium">
                                {activeProfile.displayName}
                            </span>
                        )}
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
