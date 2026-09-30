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
    if (cocktails.length === 0) {
        return null;
    }

    // mb-2 on the section (not a large margin) -- the rail's own pb-10
    // below already supplies most of the gap before the next section (it
    // has to, for shadow room), so stacking a large margin on top of that
    // would leave an oversized gap between rails.
    return (
        <section className="mb-2">
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
            <div className="flex gap-4 overflow-x-auto rail-scroll pt-8 -mt-8 pb-10 -mx-1 px-1">
                {cocktails.map(cocktail => (
                    <div key={cocktail.id} className="w-56 shrink-0">
                        <CocktailCard
                            id={cocktail.id}
                            name={cocktail.name}
                            category={cocktail.category}
                            imageUrl={cocktail.imageUrl}
                            subtitle={subtitleFor?.(cocktail)}
                        />
                    </div>
                ))}
            </div>
        </section>
    );
}
