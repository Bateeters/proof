import { useState, type SubmitEvent } from "react";
import { useProfiles } from "../context/ProfileContext";

export function ProfileSwitcher() {
    const { profiles, activeProfile, setActiveProfile, createProfile } = useProfiles();
    const [displayName, setDisplayName] = useState('');

    async function handleSubmit(e: SubmitEvent) {
        e.preventDefault();
        await createProfile(displayName)
    }

    return (
        <div>
            <ul>
                {profiles.map((profile) => (
                    <li 
                        key={profile.id}
                        onClick={() => setActiveProfile(profile)}
                        className={activeProfile?.id == profile.id ? 
                            "active-profile" : "inactive-profile"}
                    >
                        {profile.displayName}
                    </li>
                ))}
            </ul>
            <form onSubmit={handleSubmit}>
                <input 
                    type="text"
                    value={displayName}
                    onChange={e => setDisplayName(e.target.value)}
                />
                <button type="submit">Create Profile</button>
            </form>
        </div>
    )
}