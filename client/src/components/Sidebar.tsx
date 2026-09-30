import { useEffect, useState } from "react";
import { NavLink } from "react-router-dom";
import { useAuth } from "../context/AuthContext";

const navLinkClass = ({ isActive }: { isActive: boolean }) =>
    `block rounded-md px-3 py-2 text-sm transition-colors ${isActive
        ? "bg-gold-400/20 text-gold-300 font-medium"
        : "text-marble-200 hover:bg-white/5 hover:text-white"
    }`;

export function Sidebar() {
    const { token } = useAuth();
    const [categories, setCategories] = useState<string[]>([]);

    useEffect(() => {
        if (!token) return;

        fetch(`${import.meta.env.VITE_API_BASE_URL}/api/cocktails/categories`, {
            headers: { 'Authorization': `Bearer ${token}` }
        })
            .then(response => response.json())
            .then(setCategories);
    }, [token]);

    return (
        // Sticky + fixed viewport height = the column itself stays pinned
        // in view as the main content scrolls past it. The inner nav block
        // below is the part that scrolls on its own if the category list
        // ever outgrows the available height — the sidebar as a whole never
        // moves, only its interior does.
        <aside
            className="sticky top-0 h-svh w-64 shrink-0 flex flex-col
                bg-ink-900/60 backdrop-blur-lg text-marble-200
                shadow-[8px_0_30px_-4px_rgba(0,0,0,0.35)]"
        >
            <div className="shrink-0 px-4 pt-6 pb-4">
                <span className="font-display text-2xl text-white tracking-wide">Proof</span>
            </div>

            <div className="flex-1 overflow-y-auto sidebar-scroll px-4 pb-6 flex flex-col gap-8">
                <nav className="flex flex-col gap-1">
                    <NavLink to="/" end className={navLinkClass}>Home</NavLink>
                </nav>

                <div>
                    <p className="px-3 mb-2 text-xs uppercase tracking-[0.15em] text-marble-300/70">Browse</p>
                    <nav className="flex flex-col gap-1">
                        {categories.map(category => (
                            <NavLink
                                key={category}
                                to={`/category/${encodeURIComponent(category)}`}
                                className={navLinkClass}
                            >
                                {category}
                            </NavLink>
                        ))}
                    </nav>
                </div>

                <div>
                    <p className="px-3 mb-2 text-xs uppercase tracking-[0.15em] text-marble-300/70">You</p>
                    <nav className="flex flex-col gap-1">
                        <NavLink to="/recommendations" className={navLinkClass}>Recommended For You</NavLink>
                        <NavLink to="/cookbook" className={navLinkClass}>Your Cookbook</NavLink>
                        <NavLink to="/what-can-i-make" className={navLinkClass}>What Can I Make?</NavLink>
                        <NavLink to="/preferences" className={navLinkClass}>Taste Preferences</NavLink>
                    </nav>
                </div>
            </div>
        </aside>
    );
}
