import { useEffect, useState } from "react";
import { NavLink } from "react-router-dom";
import { useAuth } from "../context/AuthContext";

// Left side stays rounded, right side is flat and runs flush to the
// sidebar's own edge. Hover/selected now use the solid color (not a tint),
// so the text has to flip to white to stay legible against it.
const navLinkClass = ({ isActive }: { isActive: boolean }) =>
    `block rounded-l-md pl-3 pr-4 py-2 text-sm transition-colors ${isActive
        ? "bg-gold-600 text-white font-medium"
        : "text-ink-900 hover:bg-gold-600 hover:text-white"
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
                bg-white/60 backdrop-blur-lg border-r border-gold-600/60
                shadow-[8px_0_30px_-4px_rgba(0,0,0,0.15)]"
        >
            <div className="shrink-0 pl-4 pr-4 pt-6 pb-4">
                <span className="font-display text-2xl text-gold-600 tracking-wide">Proof</span>
            </div>

            {/* direction:rtl moves the scrollbar to the left edge instead of
                the right (the standard CSS trick for this — browsers put the
                scrollbar on the "start" side, which RTL makes the left).
                Without the inner direction:ltr wrapper, the nav text/reading
                order would also flip, which isn't wanted — only the
                scrollbar's side should change. Physical padding utilities
                are unaffected by direction either way, so the tabs' flush
                right edge still works the same as before. */}
            <div className="flex-1 overflow-y-auto sidebar-scroll pb-6 [direction:rtl]">
                <div className="[direction:ltr] pl-4 flex flex-col gap-8">
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
            </div>
        </aside>
    );
}
