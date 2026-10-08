import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { useAuth } from "../context/AuthContext";
import { useProfiles } from "../context/ProfileContext";
import { CocktailCard } from "../components/CocktailCard";
import type { MyCocktailSummary } from "../types/CustomCocktail";

export function MyCreationsPage() {
    const { token } = useAuth();
    const { activeProfile } = useProfiles();
    const [cocktails, setCocktails] = useState<MyCocktailSummary[]>([]);
    const [loaded, setLoaded] = useState(false);

    useEffect(() => {
        if (!token || !activeProfile) return;

        fetch(`${import.meta.env.VITE_API_BASE_URL}/api/profiles/${activeProfile.id}/cocktails`, {
            headers: { 'Authorization': `Bearer ${token}` }
        })
            .then(response => response.json())
            .then((data: MyCocktailSummary[]) => {
                setCocktails(data);
                setLoaded(true);
            });
    }, [token, activeProfile]);

    if (!activeProfile) {
        return <p className="text-ink-600 italic">Select a profile to see your creations.</p>;
    }

    return (
        <div>
            <div className="flex flex-wrap items-center justify-between gap-4 mb-6">
                <div>
                    <h1 className="text-3xl mb-1">My Creations</h1>
                    <p className="text-ink-600">Drinks {activeProfile.displayName} has added.</p>
                </div>
                <Link to="/cocktails/new" className="btn-primary">+ Add a Drink</Link>
            </div>

            {loaded && cocktails.length === 0 ? (
                <p className="text-ink-600 italic">No creations yet -- add your first drink.</p>
            ) : (
                <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-6">
                    {cocktails.map(cocktail => (
                        <CocktailCard
                            key={cocktail.id}
                            id={cocktail.id}
                            name={cocktail.name}
                            category={cocktail.category}
                            imageUrl={cocktail.imageUrl}
                            flavorTags={cocktail.flavorTags}
                            subtitle={cocktail.visibility}
                        />
                    ))}
                </div>
            )}
        </div>
    );
}
