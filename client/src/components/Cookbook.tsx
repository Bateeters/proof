import { useEffect, useState } from "react";
import { useAuth } from "../context/AuthContext";
import { useProfiles } from "../context/ProfileContext";
import { useCookbook } from "../context/CookbookContext";
import { CocktailCard } from "./CocktailCard";
import type { CookbookEntry } from "../types/Cookbook";

export function Cookbook() {
    const { token } = useAuth();
    const { activeProfile } = useProfiles();
    const { savedIds } = useCookbook();
    const [entries, setEntries] = useState<CookbookEntry[]>([]);
    const [search, setSearch] = useState('');

    useEffect(() => {
        if (!token || !activeProfile) return;

        fetch(`${import.meta.env.VITE_API_BASE_URL}/api/profiles/${activeProfile.id}/cookbook`, {
            headers: { 'Authorization': `Bearer ${token}` }
        })
            .then(response => response.json())
            .then(setEntries);
    }, [token, activeProfile]);

    if (!activeProfile) {
        return <p className="text-ink-600 italic">Select a profile to see its drink menu.</p>;
    }

    // Removing via a card's own heart icon (CookbookContext.savedIds) should
    // drop it from this grid immediately, without waiting on a re-fetch --
    // filtering the fetched entries by the shared live set gets that for
    // free. The fetch above still supplies the actual display data
    // (name/image/etc) that savedIds alone doesn't carry.
    const normalizedSearch = search.trim().toLowerCase();
    const visibleEntries = entries
        .filter(entry => savedIds.has(entry.cocktailId))
        .filter(entry => !normalizedSearch || entry.cocktailName.toLowerCase().includes(normalizedSearch));

    return (
        <div className="w-full">
            <div className="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-4 mb-6">
                <div>
                    <h1 className="text-3xl mb-1">Drink Menu</h1>
                    <p className="text-ink-600">{activeProfile.displayName}'s saved recipes.</p>
                </div>
                <input
                    className="field-input w-full lg:w-80 lg:max-w-[50%]"
                    type="text"
                    placeholder="Search your drink menu..."
                    value={search}
                    onChange={e => setSearch(e.target.value)}
                />
            </div>

            {entries.length === 0 ? (
                <p className="text-ink-600 italic">Nothing saved yet.</p>
            ) : visibleEntries.length === 0 ? (
                <p className="text-ink-600 italic">
                    {normalizedSearch ? "No saved drinks match that search." : "Nothing saved yet."}
                </p>
            ) : (
                <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-6">
                    {visibleEntries.map(entry => (
                        <CocktailCard
                            key={entry.cocktailId}
                            id={entry.cocktailId}
                            name={entry.cocktailName}
                            category={entry.cocktailCategory}
                            imageUrl={entry.cocktailImageUrl}
                            flavorTags={entry.flavorTags}
                        />
                    ))}
                </div>
            )}
        </div>
    );
}
