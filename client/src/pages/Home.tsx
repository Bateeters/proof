import { useEffect, useState } from "react";
import { useAuth } from "../context/AuthContext";
import { useProfiles } from "../context/ProfileContext";
import { useCocktailFilters } from "../hooks/useCocktailFilters";
import { CategoryRail } from "../components/CategoryRail";
import { CocktailCard } from "../components/CocktailCard";
import { FilterDropdown } from "../components/FilterDropdown";
import { ActiveFilterTray } from "../components/ActiveFilterTray";
import type { CategoryPreview, CocktailSummary } from "../types/Cocktail";

export function Home() {
    const { token } = useAuth();
    const { activeProfile } = useProfiles();
    const [previews, setPreviews] = useState<CategoryPreview[]>([]);
    const [recommended, setRecommended] = useState<CocktailSummary[]>([]);
    const [search, setSearch] = useState('');
    const [searchResults, setSearchResults] = useState<CocktailSummary[]>([]);
    const {
        seasonOptions, flavorTagOptions, spiritOptions,
        selectedSeasons, selectedFlavorTags, selectedSpirits,
        addSeason, addFlavorTag, addSpirit,
        activeFilters, removeFilter,
    } = useCocktailFilters();

    useEffect(() => {
        if (!token) return;

        // profileId lets the backend score and sort each category's rail by
        // match score instead of leaving it alphabetical -- omitted when
        // there's no active profile yet, which falls back to alphabetical.
        const params = new URLSearchParams();
        if (activeProfile) params.append('profileId', activeProfile.id);

        fetch(`${import.meta.env.VITE_API_BASE_URL}/api/cocktails/browse?${params}`, {
            headers: { 'Authorization': `Bearer ${token}` }
        })
            .then(response => response.json())
            .then(setPreviews);
    }, [token, activeProfile]);

    useEffect(() => {
        if (!token || !activeProfile) {
            setRecommended([]);
            return;
        }

        fetch(`${import.meta.env.VITE_API_BASE_URL}/api/profiles/${activeProfile.id}/recommendations`, {
            headers: { 'Authorization': `Bearer ${token}` }
        })
            .then(response => response.json())
            .then((data: CocktailSummary[]) => setRecommended(data.slice(0, 10)));
    }, [token, activeProfile]);

    // isSearching now covers both typed text AND an active filter -- picking
    // "Summer" with an empty search box should still swap the rails for a
    // filtered grid, not require typing something first.
    const isSearching = search.trim().length > 0 || activeFilters.length > 0;

    // Same live-as-you-type pattern as CategoryPage, just without a category
    // filter -- searches the whole synced catalog instead of one slice of it.
    useEffect(() => {
        if (!token || !isSearching) {
            setSearchResults([]);
            return;
        }

        const params = new URLSearchParams();
        if (search.trim()) params.append('search', search.trim());
        if (selectedSeasons.length > 0) params.append('seasons', selectedSeasons.join(','));
        if (selectedFlavorTags.length > 0) params.append('flavorTags', selectedFlavorTags.join(','));
        if (selectedSpirits.length > 0) params.append('spirits', selectedSpirits.join(','));
        if (activeProfile) params.append('profileId', activeProfile.id);

        fetch(`${import.meta.env.VITE_API_BASE_URL}/api/cocktails?${params}`, {
            headers: { 'Authorization': `Bearer ${token}` }
        })
            .then(response => response.json())
            .then(setSearchResults);
    }, [token, search, isSearching, selectedSeasons, selectedFlavorTags, selectedSpirits, activeProfile]);

    return (
        <div>
            <div className="flex flex-wrap items-center justify-between gap-4 mb-4">
                <div>
                    <h1 className="text-3xl mb-1">Good to see you.</h1>
                    <p className="text-ink-600">
                        {activeProfile
                            ? `Browsing as ${activeProfile.displayName}.`
                            : "Select or create a profile above to get personalized recommendations."}
                    </p>
                </div>

                <input
                    className="field-input w-full max-w-sm"
                    type="text"
                    placeholder="Search drinks..."
                    value={search}
                    onChange={e => setSearch(e.target.value)}
                />
            </div>

            <div className="flex flex-col gap-3 mb-10">
                <div className="flex flex-wrap gap-3">
                    <FilterDropdown label="Season" options={seasonOptions} onSelect={addSeason} />
                    <FilterDropdown label="Flavor" options={flavorTagOptions} onSelect={addFlavorTag} />
                    <FilterDropdown label="Liquor Base" options={spiritOptions} onSelect={addSpirit} />
                </div>
                <ActiveFilterTray filters={activeFilters} onRemove={removeFilter} />
            </div>

            {isSearching ? (
                searchResults.length === 0 ? (
                    <p className="text-ink-600 italic">No cocktails found.</p>
                ) : (
                    <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-6">
                        {searchResults.map(cocktail => (
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
                )
            ) : (
                <>
                    {recommended.length > 0 && (
                        <CategoryRail
                            title="Recommended For You"
                            seeAllTo="/recommendations"
                            cocktails={recommended}
                            subtitleFor={c => `Match score: ${c.matchScore}`}
                        />
                    )}

                    {previews.map(preview => (
                        <CategoryRail
                            key={preview.category}
                            title={preview.category}
                            seeAllTo={`/category/${encodeURIComponent(preview.category)}`}
                            cocktails={preview.cocktails}
                        />
                    ))}
                </>
            )}
        </div>
    );
}
