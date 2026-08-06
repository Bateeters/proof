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

        // TODO: build a URLSearchParams form search/category/season (only
        // .append() the ones that acutally have a value), then fetch
        // GET /api/cocktails?<params> with the Authorization header, and
        // store the result with setResults.
    }

    async function handleSelectCocktail(id: string) {
        // TODO: fetch GET /api/cocktails/{id} with the Authorization header,
        // and store the result with setSelectedCocktail.
    }

    if (selectedCocktail) {
        return (
            <div>
                <button onClick={() => setSelectedCocktail(null)}>Back To Results</button>
                {/* TODO: render selectedCocktail's name, category, glass, image,
                instructions, and ingredient list (name + measure each) */}
            </div>
        );
    }

    return (
        <div>
            <form onSubmit={handleSearch}>
                {/* TODO: controlled text input for search */}
                {/* TODO: controlled text input for category */}
                {/* TODO: controlled <select> for season - options: "" (any), Spring, Summer, Fall, Winter */}
                <button type="submit">Search</button>
            </form>

            <ul>
                {/* TODO: one item per result in 'results', showing name + category,
                with an onClick calling handleSelectCocktail(result.id) */}
            </ul>
        </div>
    );
}