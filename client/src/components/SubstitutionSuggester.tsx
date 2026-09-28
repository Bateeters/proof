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
        return <button onClick={() => setStep('reason')}>Suggest Substitute</button>;
    }

    if (step === 'reason') {
        return (
            <span>
                {' '}Why swap it?{' '}
                <button onClick={() => fetchSuggestion('Taste')}>Taste</button>{' '}
                <button onClick={() => setStep('subReason')}>Availability</button>{' '}
                <button onClick={reset}>Cancel</button>
            </span>
        );
    }

    if (step === 'subReason') {
        return (
            <span>
                {' '}Cost, or hard to find (Supply)?{' '}
                <button onClick={() => fetchSuggestion('Availability', 'Cost')}>Cost</button>{' '}
                <button onClick={() => fetchSuggestion('Availability', 'Supply')}>Supply</button>{' '}
                <button onClick={reset}>Cancel</button>
            </span>
        );
    }

    return (
        <span>
            {' '}
            {suggestion
                ? <>Try <strong>{suggestion.replacementIngredientName}</strong> instead — {suggestion.notes}</>
                : notFound && <>No known substitution for {ingredientName} for that reason.</>}
            {' '}
            <button onClick={reset}>Close</button>
        </span>
    );
}
