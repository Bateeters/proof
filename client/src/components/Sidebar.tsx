import { useEffect, useState } from "react";
import { NavLink } from "react-router-dom";
import { useAuth } from "../context/AuthContext";

const navLinkClass = ({ isActive }: { isActive: boolean }) =>
    `block rounded-md px-3 py-2 text-sm transition-colors ${isActive
        ? "bg-gold-400/15 text-gold-600 font-medium"
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
        <aside className="w-60 shrink-0 bg-ink-900 text-marble-200 min-h-svh px-4 py-6 flex flex-col gap-8">
            <div className="px-3">
                <span className="font-display text-2xl text-white tracking-wide">Proof</span>
            </div>

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
        </aside>
    );
}
