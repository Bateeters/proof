import { useState } from "react";
import type { SubmitEvent } from "react";
import { useAuth } from "../context/AuthContext";
import type { WhatCanIMakeResult } from "../types/WhatCanIMake";

export function WhatCanIMake() {
    const { token } = useAuth();
    const [ingredientInput, setIngredientInput] = useState('');
    const [haveIngredients, setHaveIngredients] = useState<string[]>([]);
    const [results, setResults] = useState<WhatCanIMakeResult[]>([]);
    const [searched, setSearched] = useState(false);

    function addIngredient() {
        const trimmed = ingredientInput.trim();
        if (trimmed && !haveIngredients.includes(trimmed)) {
            setHaveIngredients(prev => [...prev, trimmed]);
        }
        setIngredientInput('');
    }

    function removeIngredient(name: string) {
        setHaveIngredients(prev => prev.filter(i => i !== name));
    }

    async function handleSearch(e: SubmitEvent) {
        e.preventDefault();
        if (haveIngredients.length === 0) return;

        const params = new URLSearchParams({ ingredients: haveIngredients.join(',') });
        const response = await fetch(
            `${import.meta.env.VITE_API_BASE_URL}/api/cocktails/what-can-i-make?${params}`,
            { headers: { 'Authorization': `Bearer ${token}` } }
        );
        setResults(await response.json());
        setSearched(true);
    }

    return (
        <div>
            <h2>What Can I Make?</h2>
            <p>List what you have on hand, then see what you can make (or almost make).</p>

            <ul>
                {haveIngredients.map(ingredient => (
                    <li key={ingredient}>
                        {ingredient}{' '}
                        <button type="button" onClick={() => removeIngredient(ingredient)}>Remove</button>
                    </li>
                ))}
            </ul>

            <form onSubmit={handleSearch}>
                <input
                    type="text"
                    value={ingredientInput}
                    onChange={e => setIngredientInput(e.target.value)}
                    placeholder="e.g. vodka, lime, mint"
                />
                <button type="button" onClick={addIngredient}>Add Ingredient</button>
                <button type="submit">Find Cocktails</button>
            </form>

            {searched && results.length === 0 && (
                <p>No cocktails match anything in that list.</p>
            )}

            {searched && results.length > 0 && (
                <ul>
                    {results.map(cocktail => (
                        <li key={cocktail.id}>
                            <strong>{cocktail.name}</strong> | {cocktail.category}
                            {' — '}
                            {cocktail.missingIngredients.length === 0
                                ? 'You have everything!'
                                : `Missing: ${cocktail.missingIngredients.join(', ')}`}
                        </li>
                    ))}
                </ul>
            )}
        </div>
    );
}
