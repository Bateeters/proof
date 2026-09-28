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
            {profiles.length === 0 ? (
                <p className="empty-state">No profiles yet — create one below.</p>
            ) : (
                <ul id="profile-list">
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
            )}
            <form onSubmit={handleSubmit}>
                <div>
                    <label htmlFor="new-profile-name">New profile name</label>
                    <input
                        id="new-profile-name"
                        type="text"
                        value={displayName}
                        onChange={e => setDisplayName(e.target.value)}
                    />
                </div>
                <button type="submit">Create Profile</button>
            </form>
        </div>
    )
}