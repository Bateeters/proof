import { useState } from "react";
import type { SubmitEvent } from "react";
import type { CocktailDetail, CocktailSummary } from "../types/Cocktail";
import { useAuth } from "../context/AuthContext";
import { useProfiles } from "../context/ProfileContext";
import { SubstitutionSuggester } from "./SubstitutionSuggester";

export function CocktailDiscovery() {
    const { token } = useAuth();
    const { activeProfile } = useProfiles();
    const [saveMessage, setSaveMessage] = useState('');
    const [search, setSearch] = useState('');
    const [category, setCategory] = useState('');
    const [season, setSeason] = useState('');
    const [results, setResults] = useState<CocktailSummary[]>([]);
    const [hasSearched, setHasSearched] = useState(false);
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
            .then(data => {
                setResults(data);
                setHasSearched(true);
            });
    }

    async function handleSelectCocktail(id: string) {
        setSaveMessage('');
        fetch(`${import.meta.env.VITE_API_BASE_URL}/api/cocktails/${id}`, {
            headers: { 'Authorization': `Bearer ${token}` }
        })
            .then(response => response.json())
            .then(data => setSelectedCocktail(data));
    }

    async function handleSaveToCookbook() {
        if (!activeProfile || !selectedCocktail) return;

        await fetch(`${import.meta.env.VITE_API_BASE_URL}/api/profiles/${activeProfile.id}/cookbook`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`,
            },
            body: JSON.stringify({ cocktailId: selectedCocktail.id }),
        });

        setSaveMessage('Saved to cookbook!');
    }

    if (selectedCocktail) {
        return (
            <div className="cocktail-detail">
                <button className="button-secondary" onClick={() => setSelectedCocktail(null)}>Back To Results</button>
                <h3>{selectedCocktail.name}</h3>
                <p>{selectedCocktail.category} | {selectedCocktail.glass}</p>
                {selectedCocktail.imageUrl && (
                    <img src={selectedCocktail.imageUrl} alt={selectedCocktail.name} />
                )}
                <ul>
                    {selectedCocktail.ingredients.map((ingredient, index) => (
                        <li key={index}>
                            {ingredient.measure} {ingredient.ingredientName}
                            {' '}
                            <SubstitutionSuggester
                                cocktailId={selectedCocktail.id}
                                ingredientId={ingredient.ingredientId}
                                ingredientName={ingredient.ingredientName}
                            />
                        </li>
                    ))}
                </ul>
                <p>{selectedCocktail.instructions}</p>
                {activeProfile && (
                    <div>
                        <button onClick={handleSaveToCookbook}>Save To Cookbook</button>
                        {saveMessage && <span> {saveMessage}</span>}
                    </div>
                )}
            </div>
        );
    }

    return (
        <div>
            <form onSubmit={handleSearch}>
                <div>
                    <label htmlFor="cocktail-search">Drink Name</label>
                    <input
                        id="cocktail-search"
                        type="text"
                        value={search}
                        onChange={e => setSearch(e.target.value)}
                    />
                </div>
                <div>
                    <label htmlFor="cocktail-category">Drink Category</label>
                    <select
                        id="cocktail-category"
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
                </div>
                <div>
                    <label htmlFor="cocktail-season">Season</label>
                    <select
                        id="cocktail-season"
                        value={season}
                        onChange={e => setSeason(e.target.value)}
                    >
                        <option value="">Any</option>
                        <option value="Spring">Spring</option>
                        <option value="Summer">Summer</option>
                        <option value="Fall">Fall</option>
                        <option value="Winter">Winter</option>
                    </select>
                </div>
                <button type="submit">Search</button>
            </form>

            {hasSearched && results.length === 0 && (
                <p className="empty-state">No cocktails match those filters.</p>
            )}

            {results.length > 0 && (
                <ul>
                    {results.map((result) => (
                        <li
                            key={result.id}
                            className="cocktail-result"
                            onClick={() => handleSelectCocktail(result.id)}
                        >
                            <strong>{result.name}</strong> | {result.category}
                        </li>
                    ))}
                </ul>
            )}
        </div>
    );
}