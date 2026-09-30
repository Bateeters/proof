import { useState, type SubmitEvent } from "react";
import { useNavigate } from "react-router-dom";
import { useProfiles } from "../context/ProfileContext";
import type { Profile } from "../types/Profile";

export function WhoIsDrinkingPage() {
    const { profiles, setActiveProfile, createProfile } = useProfiles();
    const navigate = useNavigate();
    const [adding, setAdding] = useState(false);
    const [displayName, setDisplayName] = useState('');

    function handleSelect(profile: Profile) {
        setActiveProfile(profile);
        navigate('/');
    }

    async function handleCreate(e: SubmitEvent) {
        e.preventDefault();
        const trimmed = displayName.trim();
        if (!trimmed) return;
        await createProfile(trimmed);
        navigate('/');
    }

    return (
        <div className="min-h-svh bg-marble-50 flex flex-col items-center justify-center px-4">
            <h1 className="text-3xl mb-10">Who's Drinking?</h1>

            <div className="flex flex-wrap justify-center gap-8 max-w-3xl">
                {profiles.map(profile => (
                    <button
                        key={profile.id}
                        onClick={() => handleSelect(profile)}
                        className="group flex flex-col items-center gap-3"
                    >
                        <span
                            className="w-28 h-28 rounded-xl flex items-center justify-center text-3xl font-display text-white
                                border-2 border-transparent group-hover:border-gold-400 group-hover:shadow-[0_8px_24px_rgba(179,135,42,0.25)]
                                transition-all"
                            style={{ backgroundColor: profile.avatarColor }}
                        >
                            {profile.displayName.charAt(0).toUpperCase()}
                        </span>
                        <span className="text-ink-700 group-hover:text-gold-600 transition-colors">
                            {profile.displayName}
                        </span>
                    </button>
                ))}

                {adding ? (
                    <form onSubmit={handleCreate} className="flex flex-col items-center gap-3">
                        <input
                            autoFocus
                            className="field-input w-28 text-center"
                            placeholder="Name"
                            value={displayName}
                            onChange={e => setDisplayName(e.target.value)}
                        />
                        <button type="submit" className="btn-secondary text-sm py-1">Create</button>
                    </form>
                ) : (
                    <button
                        onClick={() => setAdding(true)}
                        className="group flex flex-col items-center gap-3"
                    >
                        <span className="w-28 h-28 rounded-xl flex items-center justify-center text-4xl text-marble-300
                            border-2 border-dashed border-marble-300 group-hover:border-gold-400 group-hover:text-gold-400
                            transition-colors">
                            +
                        </span>
                        <span className="text-ink-600 group-hover:text-gold-600 transition-colors">Add Profile</span>
                    </button>
                )}
            </div>
        </div>
    );
}
