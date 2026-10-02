import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { useAuth } from "../context/AuthContext";
import { useCocktailFilters } from "../hooks/useCocktailFilters";
import { CocktailCard } from "../components/CocktailCard";
import { FilterChipRow } from "../components/FilterChipRow";
import type { CocktailSummary } from "../types/Cocktail";

export function CategoryPage() {
    const { categoryName } = useParams<{ categoryName: string }>();
    const { token } = useAuth();
    const [cocktails, setCocktails] = useState<CocktailSummary[]>([]);
    const [search, setSearch] = useState('');
    const {
        seasonOptions, flavorTagOptions,
        selectedSeasons, selectedFlavorTags,
        toggleSeason, toggleFlavorTag,
    } = useCocktailFilters();

    useEffect(() => {
        if (!token || !categoryName) return;

        const params = new URLSearchParams({ category: categoryName });
        if (search) params.append('search', search);
        if (selectedSeasons.length > 0) params.append('seasons', selectedSeasons.join(','));
        if (selectedFlavorTags.length > 0) params.append('flavorTags', selectedFlavorTags.join(','));

        fetch(`${import.meta.env.VITE_API_BASE_URL}/api/cocktails?${params}`, {
            headers: { 'Authorization': `Bearer ${token}` }
        })
            .then(response => response.json())
            .then(setCocktails);
    }, [token, categoryName, search, selectedSeasons, selectedFlavorTags]);

    return (
        <div>
            {/* Stacked (title, then full-width search) below the lg
                breakpoint -- a half-width search bar next to a category
                name gets cramped on tablet/mobile. At lg and up, they share
                one row, search capped at 50% of it via max-w-[50%] so a
                long category name always has room. */}
            <div className="mb-4 flex flex-col lg:flex-row lg:items-center lg:justify-between gap-4">
                <h1 className="text-3xl">{categoryName}</h1>
                <input
                    className="field-input w-full lg:w-80 lg:max-w-[50%]"
                    type="text"
                    placeholder={`Search ${categoryName}...`}
                    value={search}
                    onChange={e => setSearch(e.target.value)}
                />
            </div>

            <div className="flex flex-col gap-2 mb-6">
                <FilterChipRow
                    label="Season"
                    options={seasonOptions}
                    selected={selectedSeasons}
                    onToggle={toggleSeason}
                />
                <FilterChipRow
                    label="Flavor"
                    options={flavorTagOptions}
                    selected={selectedFlavorTags}
                    onToggle={toggleFlavorTag}
                />
            </div>

            {cocktails.length === 0 ? (
                <p className="text-ink-600 italic">No cocktails found.</p>
            ) : (
                <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-6">
                    {cocktails.map(cocktail => (
                        <CocktailCard
                            key={cocktail.id}
                            id={cocktail.id}
                            name={cocktail.name}
                            category={cocktail.category}
                            imageUrl={cocktail.imageUrl}
                            flavorTags={cocktail.flavorTags}
                        />
                    ))}
                </div>
            )}
        </div>
    );
}
