import { useEffect, useState } from "react";
import { NavLink } from "react-router-dom";
import { useAuth } from "../context/AuthContext";

// Left side stays rounded, right side is flat and runs flush to the
// sidebar's own edge (no rounded-r, no right padding on the tab itself —
// the right-side inset instead lives on the text via pr-4).
const navLinkClass = ({ isActive }: { isActive: boolean }) =>
    `block rounded-l-md pl-3 pr-4 py-2 text-sm transition-colors ${isActive
        ? "bg-gold-400/15 text-gold-600 font-medium"
        : "text-ink-900 hover:bg-gold-400/10 hover:text-gold-600"
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
                bg-white/60 backdrop-blur-lg border-r border-gold-400/60
                shadow-[8px_0_30px_-4px_rgba(0,0,0,0.15)]"
        >
            <div className="shrink-0 pl-4 pr-4 pt-6 pb-4">
                <span className="font-display text-2xl text-gold-600 tracking-wide">Proof</span>
            </div>

            {/* pl-4 only (no pr-4) so each tab's own right edge — flat,
                not rounded — can run flush to the sidebar's true edge. */}
            <div className="flex-1 overflow-y-auto sidebar-scroll pl-4 pb-6 flex flex-col gap-8">
                <nav className="flex flex-col gap-1">
                    <NavLink to="/" end className={navLinkClass}>Home</NavLink>
                </nav>

                <div>
                    <p className="pl-3 mb-2 text-xs uppercase tracking-[0.15em] text-ink-600/70">Browse</p>
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
                    <p className="pl-3 mb-2 text-xs uppercase tracking-[0.15em] text-ink-600/70">You</p>
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
