import { useEffect, useState } from "react";
import type { SubmitEvent } from "react";
import { useAuth } from "../context/AuthContext";
import { useProfiles } from "../context/ProfileContext";
import type { LookupItem } from "../types/Lookup";
import type { ProfilePreferences, Sentiment } from "../types/Preferences";

// '' means "no opinion set" — distinct from Positive/Negative, and simplest
// as a plain falsy sentinel rather than a third enum-ish value.
type SentimentChoice = Sentiment | '';

const radioLabelClass = "inline-flex items-center gap-1.5 text-sm text-ink-700";

export function PreferencesEditor() {
    const { token } = useAuth();
    const { activeProfile } = useProfiles();

    const [spirits, setSpirits] = useState<LookupItem[]>([]);
    const [flavorTags, setFlavorTags] = useState<LookupItem[]>([]);
    const [spiritSentiments, setSpiritSentiments] = useState<Record<string, SentimentChoice>>({});
    const [flavorSentiments, setFlavorSentiments] = useState<Record<string, SentimentChoice>>({});
    const [allergens, setAllergens] = useState<string[]>([]);
    const [allergenInput, setAllergenInput] = useState('');
    const [saved, setSaved] = useState(false);

    useEffect(() => {
        if (!token) return;

        fetch(`${import.meta.env.VITE_API_BASE_URL}/api/lookup/spirits`, {
            headers: { 'Authorization': `Bearer ${token}` }
        })
            .then(response => response.json())
            .then(setSpirits);

        fetch(`${import.meta.env.VITE_API_BASE_URL}/api/lookup/flavor-tags`, {
            headers: { 'Authorization': `Bearer ${token}` }
        })
            .then(response => response.json())
            .then(setFlavorTags);
    }, [token]);

    useEffect(() => {
        if (!token || !activeProfile) return;

        fetch(`${import.meta.env.VITE_API_BASE_URL}/api/profiles/${activeProfile.id}/preferences`, {
            headers: { 'Authorization': `Bearer ${token}` }
        })
            .then(response => response.json())
            .then((data: ProfilePreferences) => {
                const spiritMap: Record<string, SentimentChoice> = {};
                data.spiritPreferences.forEach(sp => { spiritMap[sp.spiritId] = sp.sentiment; });
                setSpiritSentiments(spiritMap);

                const flavorMap: Record<string, SentimentChoice> = {};
                data.flavorPreferences.forEach(fp => { flavorMap[fp.flavorTagId] = fp.sentiment; });
                setFlavorSentiments(flavorMap);

                setAllergens(data.allergens);
            });
    }, [token, activeProfile]);

    function addAllergen() {
        const trimmed = allergenInput.trim();
        if (trimmed && !allergens.includes(trimmed)) {
            setAllergens(prev => [...prev, trimmed]);
        }
        setAllergenInput('');
    }

    function removeAllergen(name: string) {
        setAllergens(prev => prev.filter(a => a !== name));
    }

    async function handleSave(e: SubmitEvent) {
        e.preventDefault();
        if (!activeProfile) return;

        const spiritPreferences = Object.entries(spiritSentiments)
            .filter((entry): entry is [string, Sentiment] => entry[1] !== '')
            .map(([spiritId, sentiment]) => ({ spiritId, sentiment }));

        const flavorPreferences = Object.entries(flavorSentiments)
            .filter((entry): entry is [string, Sentiment] => entry[1] !== '')
            .map(([flavorTagId, sentiment]) => ({ flavorTagId, sentiment }));

        await fetch(`${import.meta.env.VITE_API_BASE_URL}/api/profiles/${activeProfile.id}/preferences`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`,
            },
            body: JSON.stringify({ spiritPreferences, flavorPreferences, allergens }),
        });

        setSaved(true);
        setTimeout(() => setSaved(false), 2000);
    }

    if (!activeProfile) {
        return <p className="text-ink-600 italic">Select a profile to set taste preferences.</p>;
    }

    return (
        <div>
            <h1 className="text-3xl mb-1">Taste Preferences</h1>
            <p className="text-ink-600 mb-6">For {activeProfile.displayName}.</p>

            <form onSubmit={handleSave}>
                <h2 className="text-xl mb-3">Spirits</h2>
                <div className="bg-white border border-marble-300 rounded-lg divide-y divide-marble-200 mb-8">
                    {spirits.map(spirit => (
                        <div key={spirit.id} className="flex flex-wrap items-center gap-x-6 gap-y-2 px-4 py-3">
                            <span className="min-w-28 font-medium text-ink-900">{spirit.name}</span>
                            <label className={radioLabelClass}>
                                <input
                                    type="radio"
                                    name={`spirit-${spirit.id}`}
                                    checked={spiritSentiments[spirit.id] === 'Positive'}
                                    onChange={() => setSpiritSentiments(prev => ({ ...prev, [spirit.id]: 'Positive' }))}
                                /> Likes
                            </label>
                            <label className={radioLabelClass}>
                                <input
                                    type="radio"
                                    name={`spirit-${spirit.id}`}
                                    checked={spiritSentiments[spirit.id] === 'Negative'}
                                    onChange={() => setSpiritSentiments(prev => ({ ...prev, [spirit.id]: 'Negative' }))}
                                /> Dislikes
                            </label>
                            <label className={radioLabelClass}>
                                <input
                                    type="radio"
                                    name={`spirit-${spirit.id}`}
                                    checked={!spiritSentiments[spirit.id]}
                                    onChange={() => setSpiritSentiments(prev => ({ ...prev, [spirit.id]: '' }))}
                                /> No opinion
                            </label>
                        </div>
                    ))}
                </div>

                <h2 className="text-xl mb-3">Flavors</h2>
                <div className="bg-white border border-marble-300 rounded-lg divide-y divide-marble-200 mb-8">
                    {flavorTags.map(flavorTag => (
                        <div key={flavorTag.id} className="flex flex-wrap items-center gap-x-6 gap-y-2 px-4 py-3">
                            <span className="min-w-28 font-medium text-ink-900">{flavorTag.name}</span>
                            <label className={radioLabelClass}>
                                <input
                                    type="radio"
                                    name={`flavor-${flavorTag.id}`}
                                    checked={flavorSentiments[flavorTag.id] === 'Positive'}
                                    onChange={() => setFlavorSentiments(prev => ({ ...prev, [flavorTag.id]: 'Positive' }))}
                                /> Prefers
                            </label>
                            <label className={radioLabelClass}>
                                <input
                                    type="radio"
                                    name={`flavor-${flavorTag.id}`}
                                    checked={flavorSentiments[flavorTag.id] === 'Negative'}
                                    onChange={() => setFlavorSentiments(prev => ({ ...prev, [flavorTag.id]: 'Negative' }))}
                                /> Avoids
                            </label>
                            <label className={radioLabelClass}>
                                <input
                                    type="radio"
                                    name={`flavor-${flavorTag.id}`}
                                    checked={!flavorSentiments[flavorTag.id]}
                                    onChange={() => setFlavorSentiments(prev => ({ ...prev, [flavorTag.id]: '' }))}
                                /> No opinion
                            </label>
                        </div>
                    ))}
                </div>

                <h2 className="text-xl mb-3">Allergens</h2>
                <p className="text-sm text-ink-600 mb-3">
                    Matched against ingredient names on a best-effort basis — not a guarantee.
                    Always double-check ingredients yourself for severe allergies.
                </p>
                {allergens.length === 0 ? (
                    <p className="text-ink-600 italic mb-3">No allergens set.</p>
                ) : (
                    <ul className="flex flex-wrap gap-2 mb-3">
                        {allergens.map(allergen => (
                            <li
                                key={allergen}
                                className="flex items-center gap-2 bg-white border border-marble-300 rounded-full pl-3 pr-1 py-1 text-sm"
                            >
                                {allergen}
                                <button
                                    type="button"
                                    className="text-xs text-red-700 hover:bg-red-50 rounded-full px-2 py-0.5"
                                    onClick={() => removeAllergen(allergen)}
                                >
                                    Remove
                                </button>
                            </li>
                        ))}
                    </ul>
                )}
                <div className="flex flex-wrap items-end gap-3 mb-8">
                    <div>
                        <label className="field-label" htmlFor="allergen-input">Add an allergen</label>
                        <input
                            id="allergen-input"
                            className="field-input"
                            type="text"
                            value={allergenInput}
                            onChange={e => setAllergenInput(e.target.value)}
                            placeholder="e.g. dairy, tree nuts"
                        />
                    </div>
                    <button type="button" className="btn-secondary" onClick={addAllergen}>Add Allergen</button>
                </div>

                <div className="flex items-center gap-3">
                    <button type="submit" className="btn-primary">Save Preferences</button>
                    {saved && <span className="text-sm text-gold-600">Saved!</span>}
                </div>
            </form>
        </div>
    );
}
