type Props = {
    label: string;
    options: string[];
    selected: string[];
    onToggle: (option: string) => void;
};

// Toggle-able pill buttons for a single filter facet (e.g. all Seasons, or
// all Flavor Tags) -- multiple can be active within one row at once (OR
// semantics within the row; the caller combines multiple rows with AND by
// just including both in the same fetch).
export function FilterChipRow({ label, options, selected, onToggle }: Props) {
    if (options.length === 0) {
        return null;
    }

    return (
        <div className="flex flex-wrap items-center gap-2">
            <span className="text-xs uppercase tracking-wide text-ink-600 mr-1 shrink-0">{label}</span>
            {options.map(option => {
                const isSelected = selected.includes(option);
                return (
                    <button
                        key={option}
                        type="button"
                        onClick={() => onToggle(option)}
                        aria-pressed={isSelected}
                        className={`rounded-full px-3 py-1 text-sm transition-colors ${isSelected
                            ? "bg-gold-600 text-white"
                            : "bg-white border border-marble-300 text-ink-700 hover:border-gold-400"
                            }`}
                    >
                        {option}
                    </button>
                );
            })}
        </div>
    );
}
