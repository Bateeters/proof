import { useEffect, useState } from "react";
import { useAuth } from "../context/AuthContext";
import { useProfiles } from "../context/ProfileContext";
import { CategoryRail } from "../components/CategoryRail";
import { CocktailCard } from "../components/CocktailCard";
import type { CategoryPreview, CocktailSummary } from "../types/Cocktail";
import type { RankedCocktail } from "../types/Preferences";

export function Home() {
    const { token } = useAuth();
    const { activeProfile } = useProfiles();
    const [previews, setPreviews] = useState<CategoryPreview[]>([]);
    const [recommended, setRecommended] = useState<RankedCocktail[]>([]);
    const [search, setSearch] = useState('');
    const [searchResults, setSearchResults] = useState<CocktailSummary[]>([]);

    useEffect(() => {
        if (!token) return;

        fetch(`${import.meta.env.VITE_API_BASE_URL}/api/cocktails/browse`, {
            headers: { 'Authorization': `Bearer ${token}` }
        })
            .then(response => response.json())
            .then(setPreviews);
    }, [token]);

    useEffect(() => {
        if (!token || !activeProfile) {
            setRecommended([]);
            return;
        }

        fetch(`${import.meta.env.VITE_API_BASE_URL}/api/profiles/${activeProfile.id}/recommendations`, {
            headers: { 'Authorization': `Bearer ${token}` }
        })
            .then(response => response.json())
            .then((data: RankedCocktail[]) => setRecommended(data.slice(0, 10)));
    }, [token, activeProfile]);

    // Same live-as-you-type pattern as CategoryPage, just without a category
    // filter -- searches the whole synced catalog instead of one slice of it.
    useEffect(() => {
        if (!token || !search.trim()) {
            setSearchResults([]);
            return;
        }

        const params = new URLSearchParams({ search: search.trim() });
        fetch(`${import.meta.env.VITE_API_BASE_URL}/api/cocktails?${params}`, {
            headers: { 'Authorization': `Bearer ${token}` }
        })
            .then(response => response.json())
            .then(setSearchResults);
    }, [token, search]);

    const isSearching = search.trim().length > 0;

    return (
        <div>
            <div className="flex flex-wrap items-center justify-between gap-4 mb-10">
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
