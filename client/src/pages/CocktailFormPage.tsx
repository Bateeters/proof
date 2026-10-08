import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { useAuth } from "../context/AuthContext";
import { useProfiles } from "../context/ProfileContext";
import { IngredientRows, emptyIngredientRow, type IngredientRow } from "../components/IngredientRows";
import type { LookupItem } from "../types/Lookup";
import type { CocktailDetail, Visibility } from "../types/Cocktail";
import type { SaveCustomCocktailRequest, SuggestTagsResponse } from "../types/CustomCocktail";

const SEASON_OPTIONS = ["Spring", "Summer", "Fall", "Winter"];

const VISIBILITY_HELP: Record<Visibility, string> = {
    Private: "Only you can see this.",
    Local: "Every profile on your account can see this.",
    Global: "Everyone using Proof can see this.",
};

export function CocktailFormPage() {
    const { cocktailId } = useParams<{ cocktailId: string }>();
    const isEditing = Boolean(cocktailId);
    const navigate = useNavigate();
    const { token } = useAuth();
    const { activeProfile } = useProfiles();

    const [loading, setLoading] = useState(isEditing);
    const [name, setName] = useState('');
    const [category, setCategory] = useState('');
    const [glass, setGlass] = useState('');
    const [instructions, setInstructions] = useState('');
    const [imageUrl, setImageUrl] = useState('');
    const [visibility, setVisibility] = useState<Visibility>('Private');
    const [ingredientRows, setIngredientRows] = useState<IngredientRow[]>([emptyIngredientRow()]);

    const [flavorTagOptions, setFlavorTagOptions] = useState<LookupItem[]>([]);
    const [categoryOptions, setCategoryOptions] = useState<string[]>([]);
    const [selectedFlavorTagIds, setSelectedFlavorTagIds] = useState<string[]>([]);
    const [selectedSeasons, setSelectedSeasons] = useState<string[]>([]);

    // While false, a tag/season section auto-fills from suggest-tags as
    // ingredients change. The moment the user personally toggles a
    // checkbox in that section, it flips true and stops auto-applying --
    // the suggestion call still fires, it just no longer overwrites their
    // picks. Editing an existing cocktail starts both true immediately,
    // since it already has tags someone chose.
    const [tagsTouched, setTagsTouched] = useState(isEditing);
    const [seasonsTouched, setSeasonsTouched] = useState(isEditing);

    const [error, setError] = useState('');
    const [saving, setSaving] = useState(false);

    useEffect(() => {
        if (!token) return;

        fetch(`${import.meta.env.VITE_API_BASE_URL}/api/lookup/flavor-tags`, {
            headers: { 'Authorization': `Bearer ${token}` }
        })
            .then(response => response.json())
            .then(setFlavorTagOptions);

        fetch(`${import.meta.env.VITE_API_BASE_URL}/api/cocktails/categories`, {
            headers: { 'Authorization': `Bearer ${token}` }
        })
            .then(response => response.json())
            .then(setCategoryOptions);
    }, [token]);

    useEffect(() => {
        if (!token || !isEditing || !cocktailId || !activeProfile) return;

        fetch(`${import.meta.env.VITE_API_BASE_URL}/api/cocktails/${cocktailId}?profileId=${activeProfile.id}`, {
            headers: { 'Authorization': `Bearer ${token}` }
        })
            .then(response => response.json())
            .then((data: CocktailDetail) => {
                setName(data.name);
                setCategory(data.category);
                setGlass(data.glass);
                setInstructions(data.instructions);
                setImageUrl(data.imageUrl ?? '');
                setVisibility(data.visibility);
                setIngredientRows(data.ingredients.map(ingredient => ({
                    key: crypto.randomUUID(),
                    ingredientId: ingredient.ingredientId,
                    measure: ingredient.measure,
                    displayName: ingredient.ingredientName,
                })));
                setSelectedFlavorTagIds(data.flavorTagIds);
                setSelectedSeasons(data.seasons);
                setLoading(false);
            });
    }, [token, isEditing, cocktailId, activeProfile]);

    // Live tag/season suggestions as the ingredient list changes.
    useEffect(() => {
        if (!token) return;

        const resolvable = ingredientRows
            .filter(row => row.ingredientId || row.newIngredientName)
            .map(row => ({ ingredientId: row.ingredientId, newIngredientName: row.newIngredientName }));

        if (resolvable.length === 0) return;

        const handle = setTimeout(() => {
            fetch(`${import.meta.env.VITE_API_BASE_URL}/api/cocktails/suggest-tags`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json', 'Authorization': `Bearer ${token}` },
                body: JSON.stringify({ ingredients: resolvable }),
            })
                .then(response => response.json())
                .then((data: SuggestTagsResponse) => {
                    if (!tagsTouched) setSelectedFlavorTagIds(data.flavorTagIds);
                    if (!seasonsTouched) setSelectedSeasons(data.seasons);
                });
        }, 400);

        return () => clearTimeout(handle);
        // eslint-disable-next-line react-hooks/exhaustive-deps -- tagsTouched/seasonsTouched are read, not meant to retrigger this effect
    }, [token, ingredientRows]);

    function toggleFlavorTag(id: string) {
        setTagsTouched(true);
        setSelectedFlavorTagIds(prev => prev.includes(id) ? prev.filter(x => x !== id) : [...prev, id]);
    }

    function toggleSeason(season: string) {
        setSeasonsTouched(true);
        setSelectedSeasons(prev => prev.includes(season) ? prev.filter(x => x !== season) : [...prev, season]);
    }

    async function handleSubmit(e: React.FormEvent) {
        e.preventDefault();
        if (!activeProfile) return;

        setSaving(true);
        setError('');

        const request: SaveCustomCocktailRequest = {
            name,
            category,
            glass,
            instructions,
            imageUrl: imageUrl.trim() || null,
            visibility,
            ingredients: ingredientRows
                .filter(row => row.ingredientId || row.newIngredientName)
                .map(row => ({
                    ingredientId: row.ingredientId,
                    newIngredientName: row.newIngredientName,
                    spiritId: row.spiritId,
                    measure: row.measure,
                })),
            flavorTagIds: selectedFlavorTagIds,
            seasons: selectedSeasons,
        };

        const url = isEditing
            ? `${import.meta.env.VITE_API_BASE_URL}/api/profiles/${activeProfile.id}/cocktails/${cocktailId}`
            : `${import.meta.env.VITE_API_BASE_URL}/api/profiles/${activeProfile.id}/cocktails`;

        const response = await fetch(url, {
            method: isEditing ? 'PUT' : 'POST',
            headers: { 'Content-Type': 'application/json', 'Authorization': `Bearer ${token}` },
            body: JSON.stringify(request),
        });

        setSaving(false);

        if (!response.ok) {
            const body = await response.json().catch(() => null);
            setError(body?.message ?? 'Something went wrong saving this cocktail.');
            return;
        }

        if (isEditing) {
            navigate(`/cocktails/${cocktailId}`);
        } else {
            const body = await response.json();
            navigate(`/cocktails/${body.id}`);
        }
    }

    if (!activeProfile) {
        return <p className="text-ink-600 italic">Select a profile to add a drink.</p>;
    }

    if (loading) {
        return <p className="text-ink-600 italic">Loading...</p>;
    }

    return (
        <div className="max-w-3xl">
            <h1 className="text-3xl mb-6">{isEditing ? 'Edit Drink' : 'Add a Drink'}</h1>

            <form onSubmit={handleSubmit} className="flex flex-col gap-6">
                <div className="grid sm:grid-cols-2 gap-4">
                    <div>
                        <label className="field-label" htmlFor="name">Name</label>
                        <input id="name" className="field-input" type="text" value={name} onChange={e => setName(e.target.value)} required />
                    </div>

                    <div>
                        <label className="field-label" htmlFor="category">Category</label>
                        <input
                            id="category"
                            className="field-input"
                            type="text"
                            list="category-options"
                            value={category}
                            onChange={e => setCategory(e.target.value)}
                            required
                        />
                        <datalist id="category-options">
                            {categoryOptions.map(option => <option key={option} value={option} />)}
                        </datalist>
                    </div>

                    <div>
                        <label className="field-label" htmlFor="glass">Glass</label>
                        <input id="glass" className="field-input" type="text" value={glass} onChange={e => setGlass(e.target.value)} required />
                    </div>

                    <div>
                        <label className="field-label" htmlFor="imageUrl">Image URL</label>
                        <input id="imageUrl" className="field-input" type="text" value={imageUrl} onChange={e => setImageUrl(e.target.value)} placeholder="Optional" />
                    </div>
                </div>

                <div>
                    <label className="field-label" htmlFor="instructions">Instructions</label>
                    <textarea
                        id="instructions"
                        className="field-input"
                        rows={4}
                        value={instructions}
                        onChange={e => setInstructions(e.target.value)}
                        required
                    />
                </div>

                <div>
                    <h2 className="text-base mb-2">Ingredients</h2>
                    <IngredientRows rows={ingredientRows} onChange={setIngredientRows} />
                </div>

                <div>
                    <h2 className="text-base mb-2">Flavor Tags</h2>
                    <div className="flex flex-wrap gap-2">
                        {flavorTagOptions.map(tag => (
                            <label
                                key={tag.id}
                                className={`inline-flex items-center gap-1.5 rounded-full border px-3 py-1 text-sm cursor-pointer transition-colors ${selectedFlavorTagIds.includes(tag.id)
                                    ? "bg-gold-600 border-gold-600 text-white"
                                    : "bg-white border-marble-300 text-ink-700"
                                    }`}
                            >
                                <input
                                    type="checkbox"
                                    className="sr-only"
                                    checked={selectedFlavorTagIds.includes(tag.id)}
                                    onChange={() => toggleFlavorTag(tag.id)}
                                />
                                {tag.name}
                            </label>
                        ))}
                    </div>
                </div>

                <div>
                    <h2 className="text-base mb-2">Seasons</h2>
                    <div className="flex flex-wrap gap-2">
                        {SEASON_OPTIONS.map(season => (
                            <label
                                key={season}
                                className={`inline-flex items-center gap-1.5 rounded-full border px-3 py-1 text-sm cursor-pointer transition-colors ${selectedSeasons.includes(season)
                                    ? "bg-gold-600 border-gold-600 text-white"
                                    : "bg-white border-marble-300 text-ink-700"
                                    }`}
                            >
                                <input
                                    type="checkbox"
                                    className="sr-only"
                                    checked={selectedSeasons.includes(season)}
                                    onChange={() => toggleSeason(season)}
                                />
                                {season}
                            </label>
                        ))}
                    </div>
                </div>

                <div>
                    <h2 className="text-base mb-2">Who can see this?</h2>
                    <div className="flex flex-col gap-2">
                        {(['Private', 'Local', 'Global'] as Visibility[]).map(option => (
                            <label key={option} className="flex items-center gap-2 text-sm text-ink-700">
                                <input
                                    type="radio"
                                    name="visibility"
                                    checked={visibility === option}
                                    onChange={() => setVisibility(option)}
                                />
                                <span className="font-medium text-ink-900">{option}</span>
                                <span className="text-ink-600">{VISIBILITY_HELP[option]}</span>
                            </label>
                        ))}
                    </div>
                </div>

                {error && <p className="text-sm text-red-600">{error}</p>}

                <div className="flex items-center gap-3">
                    <button type="submit" className="btn-primary" disabled={saving}>
                        {saving ? 'Saving...' : isEditing ? 'Save Changes' : 'Create Drink'}
                    </button>
                </div>
            </form>
        </div>
    );
}
