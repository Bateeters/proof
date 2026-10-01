import { useState } from "react";
import { useAuth } from "../context/AuthContext";
import { useProfiles } from "../context/ProfileContext";
import type { RankedCocktail } from "../types/Preferences";

export function Recommendations() {
    const { token } = useAuth();
    const { activeProfile } = useProfiles();
    const [ranked, setRanked] = useState<RankedCocktail[]>([]);
    const [loaded, setLoaded] = useState(false);

    async function loadRecommendations() {
        if (!activeProfile) return;

        const response = await fetch(
            `${import.meta.env.VITE_API_BASE_URL}/api/profiles/${activeProfile.id}/recommendations`,
            { headers: { 'Authorization': `Bearer ${token}` } }
        );
        const data = await response.json();
        setRanked(data);
        setLoaded(true);
    }

    async function handleSaveToDrinkMenu(cocktailId: string) {
        if (!activeProfile) return;

        await fetch(`${import.meta.env.VITE_API_BASE_URL}/api/profiles/${activeProfile.id}/cookbook`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`,
            },
            body: JSON.stringify({ cocktailId }),
        });
    }

    if (!activeProfile) {
        return <p className="text-ink-600 italic">Select a profile to see recommendations.</p>;
    }

    return (
        <div>
            <h1 className="text-3xl mb-1">Recommended For You</h1>
            <p className="text-ink-600 mb-6">For {activeProfile.displayName}.</p>

            <button className="btn-secondary mb-6" onClick={loadRecommendations}>Refresh Recommendations</button>

            {loaded && ranked.length === 0 && (
                <p className="text-ink-600 italic">No cocktails to show — every match is excluded by an allergen on this profile.</p>
            )}

            {loaded && ranked.length > 0 && (
                <ul className="flex flex-col gap-3">
                    {ranked.map(cocktail => (
                        <li
                            key={cocktail.id}
                            className="flex flex-wrap items-center justify-between gap-3 bg-white border border-marble-300 rounded-lg px-4 py-3"
                        >
                            <p className="font-medium text-ink-900">
                                {cocktail.name} <span className="text-ink-600 font-normal">| {cocktail.category} | match score: {cocktail.matchScore}</span>
                            </p>
                            <button className="btn-secondary" onClick={() => handleSaveToDrinkMenu(cocktail.id)}>Save To Drink Menu</button>
                        </li>
                    ))}
                </ul>
            )}
        </div>
    );
}
