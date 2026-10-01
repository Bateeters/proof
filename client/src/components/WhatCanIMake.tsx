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
            <h1 className="text-3xl mb-1">What Can I Make?</h1>
            <p className="text-ink-600 mb-6">List what you have on hand, then see what you can make (or almost make).</p>

            {haveIngredients.length > 0 && (
                <ul className="flex flex-wrap gap-2 mb-4">
                    {haveIngredients.map(ingredient => (
                        <li
                            key={ingredient}
                            className="flex items-center gap-2 bg-white border border-marble-300 rounded-full pl-3 pr-1 py-1 text-sm"
                        >
                            {ingredient}
                            <button
                                type="button"
                                className="text-xs text-red-700 hover:bg-red-50 rounded-full px-2 py-0.5"
                                onClick={() => removeIngredient(ingredient)}
                            >
                                Remove
                            </button>
                        </li>
                    ))}
                </ul>
            )}

            <form onSubmit={handleSearch} className="flex flex-wrap items-end gap-3 mb-6">
                <div>
                    <label className="field-label" htmlFor="have-ingredient-input">Ingredient</label>
                    <input
                        id="have-ingredient-input"
                        className="field-input"
                        type="text"
                        value={ingredientInput}
                        onChange={e => setIngredientInput(e.target.value)}
                        placeholder="e.g. vodka, lime, mint"
                    />
                </div>
                <button type="button" className="btn-secondary" onClick={addIngredient}>Add Ingredient</button>
                <button type="submit" className="btn-primary">Find Cocktails</button>
            </form>

            {searched && results.length === 0 && (
                <p className="text-ink-600 italic">No cocktails match anything in that list.</p>
            )}

            {searched && results.length > 0 && (
                <ul className="flex flex-col gap-3">
                    {results.map(cocktail => (
                        <li
                            key={cocktail.id}
                            className="bg-white border border-marble-300 rounded-lg px-4 py-3"
                        >
                            <p className="font-medium text-ink-900">{cocktail.name} <span className="text-ink-600 font-normal">| {cocktail.category}</span></p>
                            <p className="text-sm text-ink-600">
                                {cocktail.missingIngredients.length === 0
                                    ? 'You have everything!'
                                    : `Missing: ${cocktail.missingIngredients.join(', ')}`}
                            </p>
                        </li>
                    ))}
                </ul>
            )}
        </div>
    );
}
