import { useState, type SubmitEvent } from "react";
import { useProfiles } from "../context/ProfileContext";

export function ProfileSwitcher() {
    const { profiles, activeProfile, setActiveProfile, createProfile } = useProfiles();
    const [displayName, setDisplayName] = useState('');
    const [adding, setAdding] = useState(false);

    async function handleSubmit(e: SubmitEvent) {
        e.preventDefault();
        await createProfile(displayName);
        setDisplayName('');
        setAdding(false);
    }

    return (
        <div className="flex items-center gap-2 flex-wrap">
            {profiles.length === 0 && !adding && (
                <span className="text-sm text-ink-600">No profiles yet.</span>
            )}

            {profiles.map((profile) => (
                <button
                    key={profile.id}
                    onClick={() => setActiveProfile(profile)}
                    className={`rounded-full px-4 py-1.5 text-sm transition-colors ${activeProfile?.id === profile.id
                        ? "bg-ink-900 text-marble-50"
                        : "bg-white border border-marble-300 text-ink-700 hover:border-gold-400"
                        }`}
                >
                    {profile.displayName}
                </button>
            ))}

            {adding ? (
                <form onSubmit={handleSubmit} className="flex items-center gap-2">
                    <input
                        autoFocus
                        className="field-input py-1.5 text-sm w-40"
                        placeholder="Profile name"
                        value={displayName}
                        onChange={e => setDisplayName(e.target.value)}
                    />
                    <button type="submit" className="btn-secondary py-1.5">Add</button>
                    <button type="button" className="text-sm text-ink-600" onClick={() => setAdding(false)}>Cancel</button>
                </form>
            ) : (
                <button
                    onClick={() => setAdding(true)}
                    className="rounded-full px-4 py-1.5 text-sm border border-dashed border-marble-300 text-ink-600 hover:border-gold-400 hover:text-gold-600"
                >
                    + New Profile
                </button>
            )}
        </div>
    )
}
