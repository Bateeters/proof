import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { useAuth } from "../context/AuthContext";
import { CocktailCard } from "../components/CocktailCard";
import type { CocktailSummary } from "../types/Cocktail";

export function CategoryPage() {
    const { categoryName } = useParams<{ categoryName: string }>();
    const { token } = useAuth();
    const [cocktails, setCocktails] = useState<CocktailSummary[]>([]);
    const [search, setSearch] = useState('');

    useEffect(() => {
        if (!token || !categoryName) return;

        const params = new URLSearchParams({ category: categoryName });
        if (search) params.append('search', search);

        fetch(`${import.meta.env.VITE_API_BASE_URL}/api/cocktails?${params}`, {
            headers: { 'Authorization': `Bearer ${token}` }
        })
            .then(response => response.json())
            .then(setCocktails);
    }, [token, categoryName, search]);

    return (
        <div>
            <div className="mb-6 flex flex-wrap items-end justify-between gap-4">
                <h1 className="text-3xl">{categoryName}</h1>
                <input
                    className="field-input w-64"
                    type="text"
                    placeholder={`Search ${categoryName}...`}
                    value={search}
                    onChange={e => setSearch(e.target.value)}
                />
            </div>

            {cocktails.length === 0 ? (
                <p className="text-ink-600 italic">No cocktails found.</p>
            ) : (
                <div className="grid gap-5 [grid-template-columns:repeat(auto-fill,minmax(11rem,1fr))]">
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
