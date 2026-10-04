import { useEffect, useState } from "react";
import { Link, Outlet, useNavigate } from "react-router-dom";
import { Sidebar } from "./Sidebar";
import { useAuth } from "../context/AuthContext";
import { useProfiles } from "../context/ProfileContext";

// Two full variants per button (not one base + a swapped color), and the
// two groups swap visual weight between states: on gold (scrolled),
// Drink Menu/Log Out are the light/white pair and Account/profile are the
// outlined pair. Transparent (atop the marble background) flips that --
// Drink Menu/Log Out become solid dark charcoal (fading to black on
// hover), Account/profile become solid white with a gray outline (filling
// to dark gold with white text on hover) -- per Brian's spec, 2026-10-04.
function lightButtonClass(scrolled: boolean) {
    return "inline-flex items-center justify-center rounded-md px-4 py-2 text-sm font-medium transition-colors " +
        (scrolled
            ? "bg-white/80 border border-white text-ink-900 hover:bg-white"
            : "bg-ink-700 border border-ink-700 text-white hover:bg-black hover:border-black");
}

function outlineButtonClass(scrolled: boolean) {
    return "inline-flex items-center justify-center rounded-md px-4 py-2 text-sm font-medium transition-colors " +
        (scrolled
            ? "bg-transparent border border-white text-white hover:border-gold-300 hover:text-gold-300"
            : "bg-white border border-marble-300 text-ink-900 hover:bg-gold-600 hover:border-gold-600 hover:text-white");
}

export function Layout() {
    const { logout } = useAuth();
    const { activeProfile } = useProfiles();
    const navigate = useNavigate();
    const [scrolled, setScrolled] = useState(false);

    useEffect(() => {
        // Small threshold, not >0 -- avoids flickering the transition at
        // the very top from sub-pixel scroll jitter (trackpads especially).
        function handleScroll() {
            setScrolled(window.scrollY > 8);
        }

        handleScroll();
        window.addEventListener('scroll', handleScroll, { passive: true });
        return () => window.removeEventListener('scroll', handleScroll);
    }, []);

    return (
        <div className="flex min-h-svh">
            <Sidebar />

            <div className="flex-1 min-w-0">
                {/* z-20, not z-10 -- the card heart buttons also use z-10
                    for their own local stacking, and since neither this
                    header nor those cards' ancestors establish an isolated
                    stacking context, equal z-index ties resolve by DOM
                    order. main (the cards) comes after header in the DOM,
                    so without a higher z-index here, scrolled-up cards were
                    painting on top of the sticky header instead of under it.

                    Transparent at the top of the page (the marble
                    background shows through), transitioning to the solid
                    gold bar once scrolled -- transition-colors on bg/border
                    animates that swap instead of snapping. */}
                <header
                    className={
                        "flex flex-wrap items-center justify-between gap-3 border-b px-8 py-4 sticky top-0 z-20 " +
                        "transition-colors duration-300 " +
                        (scrolled ? "bg-gold-600 border-gold-600/30" : "bg-transparent border-transparent")
                    }
                >
                    <div className="flex items-center gap-3">
                        <Link to="/cookbook" className={lightButtonClass(scrolled)}>Your Drink Menu</Link>

                        {/* RequireProfile guarantees activeProfile is set for
                            every route this header renders on, so this is safe.
                            group/group-hover lets the avatar circle keep its
                            own profile color while the text still turns
                            gold-300/gold-600 together with the button's own
                            hover state. */}
                        <button
                            onClick={() => navigate('/profiles')}
                            className={
                                "group flex items-center gap-2 rounded-full pl-1 pr-3 py-1 border transition-colors " +
                                (scrolled
                                    ? "bg-transparent border-white hover:border-gold-300"
                                    : "bg-white border-marble-300 hover:bg-gold-600 hover:border-gold-600")
                            }
                        >
                            <span
                                className="w-7 h-7 rounded-full flex items-center justify-center text-xs text-white shrink-0"
                                style={{ backgroundColor: activeProfile?.avatarColor }}
                            >
                                {activeProfile?.displayName.charAt(0).toUpperCase()}
                            </span>
                            <span className={
                                "text-sm transition-colors " +
                                (scrolled ? "text-white group-hover:text-gold-300" : "text-ink-900 group-hover:text-white")
                            }>
                                {activeProfile?.displayName}
                            </span>
                            <span className={
                                "text-xs font-medium transition-colors " +
                                (scrolled ? "text-white group-hover:text-gold-300" : "text-ink-900 group-hover:text-white")
                            }>
                                Switch
                            </span>
                        </button>
                    </div>

                    <div className="flex items-center gap-3">
                        <Link to="/account" className={outlineButtonClass(scrolled)}>Account</Link>
                        <button className={lightButtonClass(scrolled)} onClick={logout}>Log Out</button>
                    </div>
                </header>

                <main className="px-8 py-8">
                    <Outlet />
                </main>
            </div>
        </div>
    );
}
