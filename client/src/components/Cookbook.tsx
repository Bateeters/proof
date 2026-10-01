import { useEffect, useState } from "react";
import { useAuth } from "../context/AuthContext";
import { useProfiles } from "../context/ProfileContext";
import type { CookbookEntry } from "../types/Cookbook";

export function Cookbook() {
    const { token } = useAuth();
    const { activeProfile } = useProfiles();
    const [entries, setEntries] = useState<CookbookEntry[]>([]);

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

    return (
        <div>
            <h1 className="text-3xl mb-1">Drink Menu</h1>
            <p className="text-ink-600 mb-6">{activeProfile.displayName}'s saved recipes.</p>

            {entries.length === 0 ? (
                <p className="text-ink-600 italic">Nothing saved yet.</p>
            ) : (
                <ul className="flex flex-col gap-3">
                    {entries.map(entry => (
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
