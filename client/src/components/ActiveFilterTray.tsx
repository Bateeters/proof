import type { ActiveFilter, FilterFacet } from "../hooks/useCocktailFilters";

type Props = {
    filters: ActiveFilter[];
    onRemove: (facet: FilterFacet, value: string) => void;
};

// One shared tray for every active filter across all facets -- the actual
// filter names (seasons, flavor tags, spirits) don't overlap with each
// other, so a plain "[value ×]" pill reads fine without needing to also
// label which facet it came from.
export function ActiveFilterTray({ filters, onRemove }: Props) {
    if (filters.length === 0) {
        return null;
    }

    return (
        <div className="flex flex-wrap gap-2">
            {filters.map(filter => (
                <button
                    key={`${filter.facet}-${filter.value}`}
                    type="button"
                    onClick={() => onRemove(filter.facet, filter.value)}
                    className="flex items-center gap-1.5 rounded-full bg-gold-600 text-white text-sm pl-3 pr-2 py-1 hover:bg-gold-700 transition-colors"
                >
                    {filter.value}
                    <span aria-hidden className="text-xs">&times;</span>
                </button>
            ))}
        </div>
    );
}
