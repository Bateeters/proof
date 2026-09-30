import { useEffect, useState } from "react";
import type { SubmitEvent } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";
import { useProfiles } from "../context/ProfileContext";
import { CategoryRail } from "../components/CategoryRail";
import type { CategoryPreview } from "../types/Cocktail";
import type { RankedCocktail } from "../types/Preferences";

export function Home() {
    const { token } = useAuth();
    const { activeProfile } = useProfiles();
    const navigate = useNavigate();
    const [previews, setPreviews] = useState<CategoryPreview[]>([]);
    const [recommended, setRecommended] = useState<RankedCocktail[]>([]);
    const [search, setSearch] = useState('');

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

    function handleSearch(e: SubmitEvent) {
        e.preventDefault();
        if (!search.trim()) return;
        navigate(`/search?q=${encodeURIComponent(search.trim())}`);
    }

    return (
        <div>
            <div className="grid grid-cols-1 sm:grid-cols-3 items-center gap-4 mb-10">
                <div>
                    <h1 className="text-3xl mb-1">Good to see you.</h1>
                    <p className="text-ink-600">
                        {activeProfile
                            ? `Browsing as ${activeProfile.displayName}.`
                            : "Select or create a profile above to get personalized recommendations."}
                    </p>
                </div>

                <form onSubmit={handleSearch} className="flex justify-center w-full">
                    <input
                        className="field-input w-full max-w-sm"
                        type="text"
                        placeholder="Search cocktails..."
                        value={search}
                        onChange={e => setSearch(e.target.value)}
                    />
                </form>

                <div className="flex justify-start sm:justify-end">
                    <Link to="/cookbook" className="btn-primary">Your Cookbook</Link>
                </div>
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
