import { useState } from "react";
import { useAuth } from "../context/AuthContext";
import type { SubstitutionSuggestion } from "../types/Substitution";

type Props = {
    cocktailId: string;
    ingredientId: string;
    ingredientName: string;
};

// Mirrors the backend's two-question flow: ask Taste-vs-Availability first,
// only drill into Cost-vs-Supply if Availability was picked.
type Step = 'closed' | 'reason' | 'subReason' | 'result';

const miniBtn = "text-xs rounded-full border border-marble-300 px-2.5 py-1 text-ink-700 hover:border-gold-400 hover:text-gold-600 transition-colors";

export function SubstitutionSuggester({ cocktailId, ingredientId, ingredientName }: Props) {
    const { token } = useAuth();
    const [step, setStep] = useState<Step>('closed');
    const [suggestion, setSuggestion] = useState<SubstitutionSuggestion | null>(null);
    const [notFound, setNotFound] = useState(false);

    async function fetchSuggestion(reason: string, subReason?: string) {
        const response = await fetch(`${import.meta.env.VITE_API_BASE_URL}/api/substitutions/suggest`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`,
            },
            body: JSON.stringify({ cocktailId, ingredientId, reason, subReason }),
        });

        if (response.status === 404) {
            setSuggestion(null);
            setNotFound(true);
        } else {
            setSuggestion(await response.json());
            setNotFound(false);
        }
        setStep('result');
    }

    function reset() {
        setStep('closed');
        setSuggestion(null);
        setNotFound(false);
    }

    if (step === 'closed') {
        return <button className={miniBtn} onClick={() => setStep('reason')}>Suggest Substitute</button>;
    }

    if (step === 'reason') {
        return (
            <span className="inline-flex items-center gap-2 text-xs text-ink-600">
                Why swap it?
                <button className={miniBtn} onClick={() => fetchSuggestion('Taste')}>Taste</button>
                <button className={miniBtn} onClick={() => setStep('subReason')}>Availability</button>
                <button className="text-xs text-ink-500 underline" onClick={reset}>Cancel</button>
            </span>
        );
    }

    if (step === 'subReason') {
        return (
            <span className="inline-flex items-center gap-2 text-xs text-ink-600">
                Cost, or hard to find?
                <button className={miniBtn} onClick={() => fetchSuggestion('Availability', 'Cost')}>Cost</button>
                <button className={miniBtn} onClick={() => fetchSuggestion('Availability', 'Supply')}>Supply</button>
                <button className="text-xs text-ink-500 underline" onClick={reset}>Cancel</button>
            </span>
        );
    }

    return (
        <span className="inline-flex items-center gap-2 text-xs">
            {suggestion
                ? <span className="text-ink-700">Try <strong className="text-gold-600">{suggestion.replacementIngredientName}</strong> — {suggestion.notes}</span>
                : notFound && <span className="text-ink-600">No known substitution for {ingredientName} for that reason.</span>}
            <button className="text-ink-500 underline" onClick={reset}>Close</button>
        </span>
    );
}
