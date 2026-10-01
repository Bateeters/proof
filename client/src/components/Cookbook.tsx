import { useEffect, useState } from "react";
import { useAuth } from "../context/AuthContext";
import { useProfiles } from "../context/ProfileContext";
import { CocktailCard } from "./CocktailCard";
import type { CookbookEntry } from "../types/Cookbook";

export function Cookbook() {
    const { token } = useAuth();
    const { activeProfile } = useProfiles();
    const [entries, setEntries] = useState<CookbookEntry[]>([]);
    const [search, setSearch] = useState('');

    function loadCookbook() {
        if (!token || !activeProfile) return;

        fetch(`${import.meta.env.VITE_API_BASE_URL}/api/profiles/${activeProfile.id}/cookbook`, {
            headers: { 'Authorization': `Bearer ${token}` }
        })
            .then(response => response.json())
            .then(setEntries);
    }

    useEffect(loadCookbook, [token, activeProfile]);

    async function handleRemove(cocktailId: string) {
        if (!activeProfile) return;

        await fetch(`${import.meta.env.VITE_API_BASE_URL}/api/profiles/${activeProfile.id}/cookbook/${cocktailId}`, {
            method: 'DELETE',
            headers: { 'Authorization': `Bearer ${token}` }
        });

        setEntries(prev => prev.filter(entry => entry.cocktailId !== cocktailId));
    }

    if (!activeProfile) {
        return <p className="text-ink-600 italic">Select a profile to see its drink menu.</p>;
    }

    // Client-side filter, not a new request per keystroke -- unlike the
    // catalog-wide searches elsewhere, this is only ever filtering a
    // profile's own (typically small) saved list that's already loaded,
    // so there's nothing to gain from round-tripping to the server for it.
    const normalizedSearch = search.trim().toLowerCase();
    const filteredEntries = normalizedSearch
        ? entries.filter(entry => entry.cocktailName.toLowerCase().includes(normalizedSearch))
        : entries;

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
            ) : filteredEntries.length === 0 ? (
                <p className="text-ink-600 italic">No saved drinks match that search.</p>
            ) : (
                <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-6">
                    {filteredEntries.map(entry => (
                        <div key={entry.cocktailId} className="relative">
                            <CocktailCard
                                id={entry.cocktailId}
                                name={entry.cocktailName}
                                category={entry.cocktailCategory}
                                imageUrl={entry.cocktailImageUrl}
                            />
                            {/* Sibling of the card's own <Link>, not a child of
                                it, so clicking Remove never also triggers the
                                card's navigation -- the button simply sits on
                                top of it via absolute positioning + z-index. */}
                            <button
                                onClick={(e) => {
                                    e.preventDefault();
                                    e.stopPropagation();
                                    handleRemove(entry.cocktailId);
                                }}
                                className="absolute top-2 right-2 z-10 rounded-full bg-white/90 hover:bg-white
                                    text-red-700 text-xs font-medium px-2.5 py-1 shadow"
                            >
                                Remove
                            </button>
                        </div>
                    ))}
                </div>
            )}
        </div>
    );
}
