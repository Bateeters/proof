import { useRef } from "react";
import { Link } from "react-router-dom";
import { CocktailCard } from "./CocktailCard";
import type { CocktailSummary } from "../types/Cocktail";

type Props<T extends CocktailSummary> = {
    title: string;
    seeAllTo?: string;
    cocktails: T[];
    subtitleFor?: (cocktail: T) => string | undefined;
};

export function CategoryRail<T extends CocktailSummary>({ title, seeAllTo, cocktails, subtitleFor }: Props<T>) {
    // A browser's native horizontal scrollbar always paints along its own
    // box's bottom edge, which put it awkwardly far below a short row (the
    // -scale-y-100 flip trick tried moving it to the top instead, but that
    // didn't look right). This renders a second, purely decorative strip
    // below the cards that carries the only VISIBLE scrollbar, pulled up
    // close to them with a negative margin -- safe to do because, unlike
    // the real row, this strip has no overflow-x-auto clipping of its own
    // to protect, so it can sit inside the real row's shadow-clearance
    // buffer without being cut off itself. The real card row's own native
    // scrollbar is hidden (.rail-scroll-hidden) and its scroll position is
    // kept in sync with the strip's in both directions. The strip's own
    // scrollable width is made to match the real row's automatically -- it
    // contains one invisible, zero-height placeholder per cocktail at the
    // same width/gap as a real card, rather than measuring the real row's
    // scrollWidth in JS.
    const scrollbarRef = useRef<HTMLDivElement>(null);
    const contentRef = useRef<HTMLDivElement>(null);
    const syncSource = useRef<'scrollbar' | 'content' | null>(null);

    if (cocktails.length === 0) {
        return null;
    }

    function syncFromScrollbar() {
        if (syncSource.current === 'content') {
            syncSource.current = null;
            return;
        }
        if (!scrollbarRef.current || !contentRef.current) return;
        syncSource.current = 'scrollbar';
        contentRef.current.scrollLeft = scrollbarRef.current.scrollLeft;
    }

    function syncFromContent() {
        if (syncSource.current === 'scrollbar') {
            syncSource.current = null;
            return;
        }
        if (!scrollbarRef.current || !contentRef.current) return;
        syncSource.current = 'content';
        scrollbarRef.current.scrollLeft = contentRef.current.scrollLeft;
    }

    return (
        <section className="mb-10">
            <div className="flex items-baseline justify-between mb-3">
                <h2 className="text-lg">{title}</h2>
                {seeAllTo && (
                    <Link to={seeAllTo} className="text-sm text-gold-600 hover:text-gold-500">
                        See All &rarr;
                    </Link>
                )}
            </div>

            {/* overflow-x-auto forces overflow-y to clip too (CSS doesn't
                allow one axis scrollable and the other fully open). The
                card's hover shadow has a 32px blur + 10px downward offset,
                so it needs real room (not just a token amount) before
                hitting that clipped edge -- pt-8/pb-10 paired with -mt-8
                keeps the rail's visual position under the heading
                unchanged while giving the shadow space on both sides. */}
            <div
                ref={contentRef}
                onScroll={syncFromContent}
                className="flex gap-4 overflow-x-auto rail-scroll-hidden pt-8 -mt-8 pb-10 -mx-1 px-1"
            >
                {cocktails.map(cocktail => (
                    <div key={cocktail.id} className="w-56 shrink-0">
                        <CocktailCard
                            id={cocktail.id}
                            name={cocktail.name}
                            category={cocktail.category}
                            imageUrl={cocktail.imageUrl}
                            subtitle={subtitleFor?.(cocktail)}
                            flavorTags={cocktail.flavorTags}
                        />
                    </div>
                ))}
            </div>

            {/* -mt-8 pulls this strip up into the card row's own pb-10
                buffer (its bottom 32px of 40px) instead of starting fresh
                below it -- tucks the scrollbar close to the cards while
                leaving an 8px clearance before it, rather than overlapping
                the row's shadow-clearance zone entirely. */}
            <div
                ref={scrollbarRef}
                onScroll={syncFromScrollbar}
                className="overflow-x-auto rail-scroll h-3 -mt-8 -mx-1 px-1"
            >
                <div className="flex gap-4" aria-hidden="true">
                    {cocktails.map(cocktail => (
                        <div key={cocktail.id} className="w-56 h-px shrink-0" />
                    ))}
                </div>
            </div>
        </section>
    );
}
