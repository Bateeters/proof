import { useEffect, useState } from "react";
import type { SubmitEvent } from "react";
import { useAuth } from "../context/AuthContext";
import { useProfiles } from "../context/ProfileContext";
import type { LookupItem } from "../types/Lookup";
import type { ProfilePreferences, Sentiment } from "../types/Preferences";

// '' means "no opinion set" — distinct from Positive/Negative, and simplest
// as a plain falsy sentinel rather than a third enum-ish value.
type SentimentChoice = Sentiment | '';

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
        return <p>Select a profile to set taste preferences.</p>;
    }

    return (
        <div>
            <h2>Taste Preferences for {activeProfile.displayName}</h2>
            <form onSubmit={handleSave}>
                <h3>Spirits</h3>
                {spirits.map(spirit => (
                    <div key={spirit.id}>
                        <span>{spirit.name}</span>{' '}
                        <label>
                            <input
                                type="radio"
                                name={`spirit-${spirit.id}`}
                                checked={spiritSentiments[spirit.id] === 'Positive'}
                                onChange={() => setSpiritSentiments(prev => ({ ...prev, [spirit.id]: 'Positive' }))}
                            /> Likes
                        </label>{' '}
                        <label>
                            <input
                                type="radio"
                                name={`spirit-${spirit.id}`}
                                checked={spiritSentiments[spirit.id] === 'Negative'}
                                onChange={() => setSpiritSentiments(prev => ({ ...prev, [spirit.id]: 'Negative' }))}
                            /> Dislikes
                        </label>{' '}
                        <label>
                            <input
                                type="radio"
                                name={`spirit-${spirit.id}`}
                                checked={!spiritSentiments[spirit.id]}
                                onChange={() => setSpiritSentiments(prev => ({ ...prev, [spirit.id]: '' }))}
                            /> No opinion
                        </label>
                    </div>
                ))}

                <h3>Flavors</h3>
                {flavorTags.map(flavorTag => (
                    <div key={flavorTag.id}>
                        <span>{flavorTag.name}</span>{' '}
                        <label>
                            <input
                                type="radio"
                                name={`flavor-${flavorTag.id}`}
                                checked={flavorSentiments[flavorTag.id] === 'Positive'}
                                onChange={() => setFlavorSentiments(prev => ({ ...prev, [flavorTag.id]: 'Positive' }))}
                            /> Prefers
                        </label>{' '}
                        <label>
                            <input
                                type="radio"
                                name={`flavor-${flavorTag.id}`}
                                checked={flavorSentiments[flavorTag.id] === 'Negative'}
                                onChange={() => setFlavorSentiments(prev => ({ ...prev, [flavorTag.id]: 'Negative' }))}
                            /> Avoids
                        </label>{' '}
                        <label>
                            <input
                                type="radio"
                                name={`flavor-${flavorTag.id}`}
                                checked={!flavorSentiments[flavorTag.id]}
                                onChange={() => setFlavorSentiments(prev => ({ ...prev, [flavorTag.id]: '' }))}
                            /> No opinion
                        </label>
                    </div>
                ))}

                <h3>Allergens</h3>
                <p>
                    <em>
                        Matched against ingredient names on a best-effort basis — not a guarantee.
                        Always double-check ingredients yourself for severe allergies.
                    </em>
                </p>
                <ul>
                    {allergens.map(allergen => (
                        <li key={allergen}>
                            {allergen}{' '}
                            <button type="button" onClick={() => removeAllergen(allergen)}>Remove</button>
                        </li>
                    ))}
                </ul>
                <input
                    type="text"
                    value={allergenInput}
                    onChange={e => setAllergenInput(e.target.value)}
                    placeholder="e.g. dairy, tree nuts"
                />
                <button type="button" onClick={addAllergen}>Add Allergen</button>

                <div>
                    <button type="submit">Save Preferences</button>
                    {saved && <span> Saved!</span>}
                </div>
            </form>
        </div>
    );
}
