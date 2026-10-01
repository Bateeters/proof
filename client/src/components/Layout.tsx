import { Link, Outlet, useNavigate } from "react-router-dom";
import { Sidebar } from "./Sidebar";
import { useAuth } from "../context/AuthContext";
import { useProfiles } from "../context/ProfileContext";

// Drink Menu + Log Out: same off-white-turns-white treatment.
const lightButtonClass =
    "inline-flex items-center justify-center rounded-md px-4 py-2 text-sm font-medium " +
    "bg-white/80 border border-white text-ink-900 hover:bg-white transition-colors";

// Account (+ the profile button, built separately below since it has an
// avatar circle inside it): white outline, white text, gold-300 on hover.
const outlineButtonClass =
    "inline-flex items-center justify-center rounded-md px-4 py-2 text-sm font-medium " +
    "bg-transparent border border-white text-white hover:border-gold-300 hover:text-gold-300 transition-colors";

export function Layout() {
    const { logout } = useAuth();
    const { activeProfile } = useProfiles();
    const navigate = useNavigate();

    return (
        <div className="flex min-h-svh">
            <Sidebar />

            <div className="flex-1 min-w-0">
                <header className="flex flex-wrap items-center justify-between gap-3 border-b border-gold-600/30 bg-gold-600 px-8 py-4 sticky top-0 z-10">
                    <div className="flex items-center gap-3">
                        <Link to="/cookbook" className={lightButtonClass}>Your Drink Menu</Link>

                        {/* RequireProfile guarantees activeProfile is set for
                            every route this header renders on, so this is safe.
                            group/group-hover lets the avatar circle keep its
                            own profile color while the text still turns
                            gold-300 together with the button's own hover state. */}
                        <button
                            onClick={() => navigate('/profiles')}
                            className="group flex items-center gap-2 rounded-full pl-1 pr-3 py-1
                                bg-transparent border border-white hover:border-gold-300 transition-colors"
                        >
                            <span
                                className="w-7 h-7 rounded-full flex items-center justify-center text-xs text-white shrink-0"
                                style={{ backgroundColor: activeProfile?.avatarColor }}
                            >
                                {activeProfile?.displayName.charAt(0).toUpperCase()}
                            </span>
                            <span className="text-sm text-white group-hover:text-gold-300 transition-colors">
                                {activeProfile?.displayName}
                            </span>
                            <span className="text-xs text-white group-hover:text-gold-300 font-medium transition-colors">
                                Switch
                            </span>
                        </button>
                    </div>

                    <div className="flex items-center gap-3">
                        <Link to="/account" className={outlineButtonClass}>Account</Link>
                        <button className={lightButtonClass} onClick={logout}>Log Out</button>
                    </div>
                </header>

                <main className="px-8 py-8">
                    <Outlet />
                </main>
            </div>
        </div>
    );
}
