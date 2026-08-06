import { useState } from "react";
import type { SubmitEvent } from "react";
import type { CocktailDetail, CocktailSummary } from "../types/Cocktail";
import { useAuth } from "../context/AuthContext";

export function CocktailDiscovery() {
    const { token } = useAuth();
    const [search, setSearch] = useState('');
    const [category, setCategory] = useState('');
    const [season, setSeason] = useState('');
    const [results, setResults] = useState<CocktailSummary[]>([]);
    const [selectedCocktail, setSelectedCocktail] = useState<CocktailDetail | null>(null);

    async function handleSearch(e: SubmitEvent) {
        e.preventDefault();

        const params = new URLSearchParams();
        if (search) params.append('search', search);
        if (category) params.append('category', category);
        if (season) params.append('season', season);

        fetch(`${import.meta.env.VITE_API_BASE_URL}/api/cocktails?${params}`, {
            headers: { 'Authorization': `Bearer ${token}` }
        })
            .then(response => response.json())
            .then(data => setResults(data));
    }

    async function handleSelectCocktail(id: string) {
        fetch(`${import.meta.env.VITE_API_BASE_URL}/api/cocktails/${id}`, {
            headers: { 'Authorization': `Bearer ${token}` }
        })
            .then(response => response.json())
            .then(data => setSelectedCocktail(data));
    }

    if (selectedCocktail) {
        return (
            <div>
                <button onClick={() => setSelectedCocktail(null)}>Back To Results</button>
                <h3>{selectedCocktail.name}</h3>
                <p>{selectedCocktail.category} | {selectedCocktail.glass}</p>
                <img src={selectedCocktail.imageUrl ?? undefined} alt={selectedCocktail.name} />
                <ul>
                    {selectedCocktail.ingredients.map((ingredient, index) => (
                        <li key={index}>
                            {ingredient.measure} {ingredient.ingredientName}
                        </li>
                    ))}
                </ul>
                <p>{selectedCocktail.instructions}</p>
            </div>
        );
    }

    return (
        <div>
            <form onSubmit={handleSearch}>
                <label>Drink Name:</label>
                <input
                    type="text"
                    value={search}
                    onChange={e => setSearch(e.target.value)}
                />
                <label>Drink Category:</label>
                <select
                    value={category}
                    onChange={e => setCategory(e.target.value)}
                >
                    <option value="">Any</option>
                    <option value="Beer">Beer</option>
                    <option value="Cocktail">Cocktail</option>
                    <option value="Cocoa">Cocoa</option>
                    <option value="Coffee / Tea">Coffee / Tea</option>
                    <option value="Homemade Liqueur">Homemade Liqueur</option>
                    <option value="Ordinary Drink">Ordinary Drink</option>
                    <option value="Other / Unknown">Other / Unknown</option>
                    <option value="Punch / Party Drink">Punch / Party Drink</option>
                    <option value="Shake">Shake</option>
                    <option value="Shot">Shot</option>
                    <option value="Soft Drink">Soft Drink</option>
                </select>
                <label>Season:</label>
                <select
                    value={season}
                    onChange={e => setSeason(e.target.value)}
                >
                    <option value="">Any</option>
                    <option value="Spring">Spring</option>
                    <option value="Summer">Summer</option>
                    <option value="Fall">Fall</option>
                    <option value="Winter">Winter</option>
                </select>
                <button type="submit">Search</button>
            </form>

            <ul>
                {results.map((result) => (
                    <li
                        key={result.id}
                        onClick={() => handleSelectCocktail(result.id)}
                    >
                        <strong>{result.name}</strong> | {result.category}
                    </li>
                ))}
            </ul>
        </div>
    );
}