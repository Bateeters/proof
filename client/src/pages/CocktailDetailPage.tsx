import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { useAuth } from "../context/AuthContext";
import { useProfiles } from "../context/ProfileContext";
import { SubstitutionSuggester } from "../components/SubstitutionSuggester";
import type { CocktailDetail } from "../types/Cocktail";

export function CocktailDetailPage() {
    const { cocktailId } = useParams<{ cocktailId: string }>();
    const { token } = useAuth();
    const { activeProfile } = useProfiles();
    const [cocktail, setCocktail] = useState<CocktailDetail | null>(null);
    const [saveMessage, setSaveMessage] = useState('');

    useEffect(() => {
        if (!token || !cocktailId) return;

        fetch(`${import.meta.env.VITE_API_BASE_URL}/api/cocktails/${cocktailId}`, {
            headers: { 'Authorization': `Bearer ${token}` }
        })
            .then(response => response.json())
            .then(setCocktail);
    }, [token, cocktailId]);

    async function handleSaveToDrinkMenu() {
        if (!activeProfile || !cocktail) return;

        await fetch(`${import.meta.env.VITE_API_BASE_URL}/api/profiles/${activeProfile.id}/cookbook`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`,
            },
            body: JSON.stringify({ cocktailId: cocktail.id }),
        });

        setSaveMessage('Saved to your drink menu!');
    }

    if (!cocktail) {
        return <p className="text-ink-600 italic">Loading...</p>;
    }

    return (
        <div className="max-w-3xl">
            <Link to={`/category/${encodeURIComponent(cocktail.category)}`} className="text-sm text-gold-600 hover:text-gold-500">
                &larr; Back to {cocktail.category}
            </Link>

            <div className="mt-4 grid md:grid-cols-[280px_1fr] gap-8">
                {cocktail.imageUrl && (
                    <img
                        src={cocktail.imageUrl}
                        alt={cocktail.name}
                        className="w-full aspect-square object-cover rounded-lg border border-marble-300"
                    />
                )}

                <div>
                    <h1 className="text-3xl mb-1">{cocktail.name}</h1>
                    <p className="text-ink-600 mb-3">{cocktail.category} &middot; {cocktail.glass}</p>

                    {cocktail.flavorTags.length > 0 && (
                        <div className="flex flex-wrap gap-2 mb-6">
                            {cocktail.flavorTags.map(tag => (
                                <span
                                    key={tag}
                                    className="rounded-full bg-gold-400/15 text-gold-600 text-xs font-medium px-3 py-1"
                                >
                                    {tag}
                                </span>
                            ))}
                        </div>
                    )}

                    <h2 className="text-base mb-2">Ingredients</h2>
                    <ul className="flex flex-col gap-2 mb-6">
                        {cocktail.ingredients.map((ingredient, index) => (
                            <li key={index} className="flex flex-wrap items-center gap-2 text-sm border-b border-marble-200 pb-2">
                                <span className="text-ink-800">
                                    {ingredient.measure} {ingredient.ingredientName}
                                </span>
                                <SubstitutionSuggester
                                    cocktailId={cocktail.id}
                                    ingredientId={ingredient.ingredientId}
                                    ingredientName={ingredient.ingredientName}
                                />
                            </li>
                        ))}
                    </ul>

                    <h2 className="text-base mb-2">Instructions</h2>
                    <p className="text-ink-700 mb-6">{cocktail.instructions}</p>

                    {activeProfile && (
                        <div className="flex items-center gap-3">
                            <button className="btn-primary" onClick={handleSaveToDrinkMenu}>Save To Drink Menu</button>
                            {saveMessage && <span className="text-sm text-gold-600">{saveMessage}</span>}
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
}
