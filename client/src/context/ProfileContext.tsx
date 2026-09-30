import { createContext, useContext, useEffect, useState, type ReactNode } from "react";
import type { Profile } from "../types/Profile";
import { useAuth } from "./AuthContext";

type ProfileContextValue = {
    profiles: Profile[];
    activeProfile: Profile | null;
    // Distinguishes "haven't fetched profiles yet" from "fetched, and there
    // really is no active profile" — RequireProfile needs this so it
    // doesn't redirect to the profile picker before the fetch even runs.
    profilesLoaded: boolean;
    setActiveProfile: (profile: Profile) => void;
    createProfile: (displayName: string, avatarColor?: string) => Promise<void>;
};

const ProfileContext = createContext<ProfileContextValue | undefined>(undefined);

// Scoped per account -- if the same browser logs into a different account,
// the remembered profile shouldn't carry over to it.
function storageKey(accountId: string) {
    return `proof:activeProfileId:${accountId}`;
}

export function ProfileProvider({ children }: { children: ReactNode }) {
    const { token, account } = useAuth();
    const [profiles, setProfiles] = useState<Profile[]>([]);
    const [activeProfile, setActiveProfileState] = useState<Profile | null>(null);
    const [profilesLoaded, setProfilesLoaded] = useState(false);

    useEffect(() => {
        if (!token || !account) {
            setProfiles([]);
            setActiveProfileState(null);
            setProfilesLoaded(false);
            return;
        }

        fetch(`${import.meta.env.VITE_API_BASE_URL}/api/profiles`, {
            headers: { 'Authorization': `Bearer ${token}` }
        })
            .then(response => response.json())
            .then((data: Profile[]) => {
                setProfiles(data);

                const rememberedId = localStorage.getItem(storageKey(account.id));
                const remembered = data.find(p => p.id === rememberedId);
                if (remembered) {
                    setActiveProfileState(remembered);
                }

                setProfilesLoaded(true);
            });
    }, [token, account]);

    function setActiveProfile(profile: Profile) {
        setActiveProfileState(profile);
        if (account) {
            localStorage.setItem(storageKey(account.id), profile.id);
        }
    }

    async function createProfile(displayName: string, avatarColor?: string) {
        const response = await fetch(`${import.meta.env.VITE_API_BASE_URL}/api/profiles`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`,
            },
            body: JSON.stringify({ displayName, avatarColor })
        })

        const newProfile = await response.json();

        setProfiles(prev => [...prev, newProfile]);
        // A freshly created profile becomes active immediately -- no reason
        // to make someone select the profile they just made.
        setActiveProfile(newProfile);
    }

    const value: ProfileContextValue = { profiles, activeProfile, profilesLoaded, setActiveProfile, createProfile };

    return <ProfileContext.Provider value={value}>{children}</ProfileContext.Provider>
}

export function useProfiles() {
    const context = useContext(ProfileContext);
    if (context === undefined) {
        throw new Error('useProfiles must be used inside a ProfileProvider');
    }
    return context;
}
