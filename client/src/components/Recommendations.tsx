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

    if (!activeProfile) {
        return <p>Select a profile to see recommendations.</p>;
    }

    return (
        <div>
            <h2>Recommended For {activeProfile.displayName}</h2>
            <button onClick={loadRecommendations}>Refresh Recommendations</button>

            {loaded && ranked.length === 0 && (
                <p>No cocktails to show — every match is excluded by an allergen on this profile.</p>
            )}

            {loaded && ranked.length > 0 && (
                <ul>
                    {ranked.map(cocktail => (
                        <li key={cocktail.id}>
                            <strong>{cocktail.name}</strong> | {cocktail.category} | match score: {cocktail.matchScore}
                        </li>
                    ))}
                </ul>
            )}
        </div>
    );
}
