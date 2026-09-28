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
        return <p>Select a profile to see its cookbook.</p>;
    }

    return (
        <div>
            <h2>{activeProfile.displayName}'s Cookbook</h2>
            {entries.length === 0 ? (
                <p>Nothing saved yet.</p>
            ) : (
                <ul>
                    {entries.map(entry => (
                        <li key={entry.cocktailId}>
                            <strong>{entry.cocktailName}</strong> | {entry.cocktailCategory}
                            {entry.notes && <span> — {entry.notes}</span>}
                            {' '}
                            <button onClick={() => handleRemove(entry.cocktailId)}>Remove</button>
                        </li>
                    ))}
                </ul>
            )}
        </div>
    );
}
