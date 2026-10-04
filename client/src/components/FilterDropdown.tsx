type Props = {
    label: string;
    options: string[];
    onSelect: (value: string) => void;
};

// A plain single-select that resets back to its placeholder immediately
// after a pick, rather than staying on the chosen value -- the actual
// "what's selected" state lives in the shared tray below instead, so this
// control is always ready to add another value from the same facet.
export function FilterDropdown({ label, options, onSelect }: Props) {
    return (
        // field-input is w-full for real form fields -- overridden here so
        // the three dropdowns sit inline in one row (parent uses
        // justify-between) instead of each claiming the full row width.
        <select
            className="field-input w-[32%]"
            value=""
            onChange={e => {
                const value = e.target.value;
                if (value) {
                    onSelect(value);
                }
            }}
        >
            <option value="">{label}</option>
            {options.map(option => (
                <option key={option} value={option}>{option}</option>
            ))}
        </select>
    );
}
