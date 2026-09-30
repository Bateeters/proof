import { useEffect, useState } from "react";
import { useAuth } from "../context/AuthContext";
import { useProfiles } from "../context/ProfileContext";
import { CategoryRail } from "../components/CategoryRail";
import type { CategoryPreview } from "../types/Cocktail";
import type { RankedCocktail } from "../types/Preferences";

export function Home() {
    const { token } = useAuth();
    const { activeProfile } = useProfiles();
    const [previews, setPreviews] = useState<CategoryPreview[]>([]);
    const [recommended, setRecommended] = useState<RankedCocktail[]>([]);

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

    return (
        <div>
            <div className="mb-10">
                <h1 className="text-3xl mb-1">Good to see you.</h1>
                <p className="text-ink-600">
                    {activeProfile
                        ? `Browsing as ${activeProfile.displayName}.`
                        : "Select or create a profile above to get personalized recommendations."}
                </p>
            </div>

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
        </div>
    );
}
