import { useEffect, useState } from "react";
import { useSearchParams } from "react-router-dom";
import { useAuth } from "../context/AuthContext";
import { CocktailCard } from "../components/CocktailCard";
import type { CocktailSummary } from "../types/Cocktail";

export function SearchResultsPage() {
    const [searchParams] = useSearchParams();
    const query = searchParams.get('q') ?? '';
    const { token } = useAuth();
    const [cocktails, setCocktails] = useState<CocktailSummary[]>([]);

    useEffect(() => {
        if (!token || !query) {
            setCocktails([]);
            return;
        }

        fetch(`${import.meta.env.VITE_API_BASE_URL}/api/cocktails?search=${encodeURIComponent(query)}`, {
            headers: { 'Authorization': `Bearer ${token}` }
        })
            .then(response => response.json())
            .then(setCocktails);
    }, [token, query]);

    return (
        <div>
            <h1 className="text-3xl mb-6">Results for &ldquo;{query}&rdquo;</h1>

            {cocktails.length === 0 ? (
                <p className="text-ink-600 italic">No cocktails found.</p>
            ) : (
                <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-6">
                    {cocktails.map(cocktail => (
                        <CocktailCard
                            key={cocktail.id}
                            id={cocktail.id}
                            name={cocktail.name}
                            category={cocktail.category}
                            imageUrl={cocktail.imageUrl}
                        />
                    ))}
                </div>
            )}
        </div>
    );
}
