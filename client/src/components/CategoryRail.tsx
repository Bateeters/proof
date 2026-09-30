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
                allow one axis scrollable and the other fully open) -- pt-3
                paired with -mt-3 keeps the rail's visual position under the
                heading unchanged while giving hovered cards' shadow/lift
                room to render before hitting that clipped edge. */}
            <div className="flex gap-4 overflow-x-auto rail-scroll pt-3 -mt-3 pb-4 -mx-1 px-1">
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
