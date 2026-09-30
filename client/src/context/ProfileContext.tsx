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
    const { token, account, justLoggedIn, clearJustLoggedIn } = useAuth();
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

                if (data.length === 1) {
                    // Only one profile ever exists to pick — never worth
                    // making someone click through a picker for a choice
                    // that isn't actually a choice.
                    setActiveProfileState(data[0]);
                    localStorage.setItem(storageKey(account.id), data[0].id);
                } else if (data.length > 1 && !justLoggedIn) {
                    // Multiple profiles, but this is a silent session
                    // restore (page refresh), not a fresh login — respect
                    // whatever was last selected instead of re-prompting.
                    const rememberedId = localStorage.getItem(storageKey(account.id));
                    const remembered = data.find(p => p.id === rememberedId);
                    if (remembered) {
                        setActiveProfileState(remembered);
                    }
                }
                // Otherwise (multiple profiles + just logged in, or zero
                // profiles): leave activeProfile null so RequireProfile
                // sends them to the "Who's Drinking?" picker.

                setProfilesLoaded(true);
            });
    }, [token, account, justLoggedIn]);

    function setActiveProfile(profile: Profile) {
        setActiveProfileState(profile);
        if (account) {
            localStorage.setItem(storageKey(account.id), profile.id);
        }
        // A profile has now been resolved for this login — later refreshes
        // in the same session should go back to silently restoring it.
        clearJustLoggedIn();
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
