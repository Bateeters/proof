import { useEffect, useState } from "react";
import { useAuth } from "../context/AuthContext";
import { useProfiles } from "../context/ProfileContext";
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
        <div>
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
                <ul className="flex flex-col gap-3">
                    {filteredEntries.map(entry => (
                        <li
                            key={entry.cocktailId}
                            className="flex flex-wrap items-center justify-between gap-3 bg-white border border-marble-300 rounded-lg px-4 py-3"
                        >
                            <div>
                                <p className="font-medium text-ink-900">
                                    {entry.cocktailName} <span className="text-ink-600 font-normal">| {entry.cocktailCategory}</span>
                                </p>
                                {entry.notes && <p className="text-sm text-ink-600">{entry.notes}</p>}
                            </div>
                            <button className="btn-danger" onClick={() => handleRemove(entry.cocktailId)}>Remove</button>
                        </li>
                    ))}
                </ul>
            )}
        </div>
    );
}
