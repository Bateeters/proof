import { useEffect, useState } from "react";
import { useAuth } from "../context/AuthContext";
import type { LookupItem } from "../types/Lookup";

// Richer than the save-DTO shape (CocktailIngredientInput) -- key is a
// stable React list key independent of ingredientId/newIngredientName
// (which change as the user picks/types), and displayName is what the
// search box shows, kept separate from the save payload.
export type IngredientRow = {
    key: string;
    ingredientId?: string;
    newIngredientName?: string;
    spiritId?: string;
    measure: string | null;
    displayName: string;
}

export function emptyIngredientRow(): IngredientRow {
    return { key: crypto.randomUUID(), measure: null, displayName: '' };
}

type Props = {
    rows: IngredientRow[];
    onChange: (rows: IngredientRow[]) => void;
}

export function IngredientRows({ rows, onChange }: Props) {
    function updateRow(key: string, patch: Partial<IngredientRow>) {
        onChange(rows.map(row => row.key === key ? { ...row, ...patch } : row));
    }

    function removeRow(key: string) {
        const next = rows.filter(row => row.key !== key);
        // Always at least one row -- removing the last one just clears it
        // instead of leaving an empty list.
        onChange(next.length === 0 ? [emptyIngredientRow()] : next);
    }

    // Swaps the row at index with its neighbor -- SortOrder is derived
    // from array position server-side at save time, so reordering here is
    // all that's needed for it to stick.
    function moveRow(index: number, direction: -1 | 1) {
        const target = index + direction;
        if (target < 0 || target >= rows.length) return;

        const next = [...rows];
        [next[index], next[target]] = [next[target], next[index]];
        onChange(next);
    }

    return (
        <div className="flex flex-col gap-3">
            {rows.map((row, index) => (
                <IngredientRowEditor
                    key={row.key}
                    row={row}
                    onUpdate={patch => updateRow(row.key, patch)}
                    onRemove={() => removeRow(row.key)}
                    onMoveUp={() => moveRow(index, -1)}
                    onMoveDown={() => moveRow(index, 1)}
                    isFirst={index === 0}
                    isLast={index === rows.length - 1}
                />
            ))}
            <button type="button" className="btn-secondary self-start" onClick={() => onChange([...rows, emptyIngredientRow()])}>
                + Add Ingredient
            </button>
        </div>
    );
}

type RowProps = {
    row: IngredientRow;
    onUpdate: (patch: Partial<IngredientRow>) => void;
    onRemove: () => void;
    onMoveUp: () => void;
    onMoveDown: () => void;
    isFirst: boolean;
    isLast: boolean;
}

function IngredientRowEditor({ row, onUpdate, onRemove, onMoveUp, onMoveDown, isFirst, isLast }: RowProps) {
    const { token } = useAuth();
    const [searchText, setSearchText] = useState(row.displayName);
    const [results, setResults] = useState<LookupItem[]>([]);
    const [spirits, setSpirits] = useState<LookupItem[]>([]);
    const [isOpen, setIsOpen] = useState(false);

    // Debounced autocomplete against the ingredient catalog.
    useEffect(() => {
        if (!token || searchText.trim().length < 2) {
            setResults([]);
            return;
        }

        const handle = setTimeout(() => {
            fetch(`${import.meta.env.VITE_API_BASE_URL}/api/lookup/ingredients?search=${encodeURIComponent(searchText.trim())}`, {
                headers: { 'Authorization': `Bearer ${token}` }
            })
                .then(response => response.json())
                .then(setResults);
        }, 300);

        return () => clearTimeout(handle);
    }, [token, searchText]);

    // Lazy-loaded once a free-text new ingredient is actually in play --
    // most rows resolve to an existing ingredient and never need this.
    useEffect(() => {
        if (!token || !row.newIngredientName || spirits.length > 0) return;

        fetch(`${import.meta.env.VITE_API_BASE_URL}/api/lookup/spirits`, {
            headers: { 'Authorization': `Bearer ${token}` }
        })
            .then(response => response.json())
            .then(setSpirits);
    }, [token, row.newIngredientName, spirits.length]);

    function selectExisting(item: LookupItem) {
        setSearchText(item.name);
        setIsOpen(false);
        onUpdate({ ingredientId: item.id, newIngredientName: undefined, spiritId: undefined, displayName: item.name });
    }

    function selectNew(name: string) {
        setSearchText(name);
        setIsOpen(false);
        onUpdate({ ingredientId: undefined, newIngredientName: name, displayName: name });
    }

    const trimmedSearch = searchText.trim();
    const hasExactMatch = results.some(r => r.name.toLowerCase() === trimmedSearch.toLowerCase());

    return (
        <div className="flex flex-wrap items-start gap-2 bg-white border border-marble-300 rounded-lg p-3">
            <div className="flex flex-col -my-1">
                <button
                    type="button"
                    className="px-1.5 text-ink-600 hover:text-gold-600 disabled:opacity-25 disabled:hover:text-ink-600"
                    onClick={onMoveUp}
                    disabled={isFirst}
                    aria-label="Move ingredient up"
                >
                    ▲
                </button>
                <button
                    type="button"
                    className="px-1.5 text-ink-600 hover:text-gold-600 disabled:opacity-25 disabled:hover:text-ink-600"
                    onClick={onMoveDown}
                    disabled={isLast}
                    aria-label="Move ingredient down"
                >
                    ▼
                </button>
            </div>

            <div className="relative w-64">
                <input
                    className="field-input"
                    type="text"
                    placeholder="Search ingredients..."
                    value={searchText}
                    onChange={e => {
                        setSearchText(e.target.value);
                        setIsOpen(true);
                    }}
                    onFocus={() => setIsOpen(true)}
                    // Delayed so a dropdown option's onMouseDown fires before
                    // this closes the list -- blur happens before click.
                    onBlur={() => setTimeout(() => setIsOpen(false), 150)}
                />
                {isOpen && trimmedSearch.length >= 2 && (
                    <div className="absolute z-10 mt-1 w-full max-h-56 overflow-y-auto bg-white border border-marble-300 rounded-md shadow-lg">
                        {results.map(item => (
                            <button
                                key={item.id}
                                type="button"
                                className="block w-full text-left px-3 py-2 text-sm hover:bg-gold-400/10"
                                onMouseDown={() => selectExisting(item)}
                            >
                                {item.name}
                            </button>
                        ))}
                        {!hasExactMatch && (
                            <button
                                type="button"
                                className="block w-full text-left px-3 py-2 text-sm text-gold-600 hover:bg-gold-400/10 border-t border-marble-200"
                                onMouseDown={() => selectNew(trimmedSearch)}
                            >
                                + Add new ingredient: "{trimmedSearch}"
                            </button>
                        )}
                    </div>
                )}
            </div>

            <input
                className="field-input w-32"
                type="text"
                placeholder="Measure"
                value={row.measure ?? ''}
                onChange={e => onUpdate({ measure: e.target.value || null })}
            />

            {row.newIngredientName && (
                <select
                    className="field-input w-44"
                    value={row.spiritId ?? ''}
                    onChange={e => onUpdate({ spiritId: e.target.value || undefined })}
                >
                    <option value="">Base spirit (optional)</option>
                    {spirits.map(spirit => (
                        <option key={spirit.id} value={spirit.id}>{spirit.name}</option>
                    ))}
                </select>
            )}

            <button type="button" className="btn-secondary" onClick={onRemove}>Remove</button>
        </div>
    );
}
