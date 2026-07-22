import { createContext, useContext, useEffect, useState, type ReactNode } from "react";
import type { Profile } from "../types/Profile";
import { useAuth } from "./AuthContext";

type ProfileContextValue = {
    profiles: Profile[];
    activeProfile: Profile | null;
    setActiveProfile: (profile: Profile) => void;
    createProfile: (displayName: string, avatarColor?: string) => Promise<void>;
};

const ProfileContext = createContext<ProfileContextValue | undefined>(undefined);

export function ProfileProvider({ children }: { children: ReactNode }) {
    const { token } = useAuth();
    const [profiles, setProfiles] = useState<Profile[]>([]);
    const [activeProfile, setActiveProfile] = useState<Profile | null>(null);

    useEffect(() => {
        if (!token) {
            setProfiles([]);
            setActiveProfile(null);
            return;
        }

        fetch(`${import.meta.env.VITE_API_BASE_URL}/api/profiles`, {
            headers: { 'Authorization': `Bearer ${token}` }
        })
            .then(response => response.json())
            .then(data => setProfiles(data));
    }, [token]);

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

        setProfiles(prev => [...prev, newProfile])
    }

    const value: ProfileContextValue = { profiles, activeProfile, setActiveProfile, createProfile };

    return <ProfileContext.Provider value={value}>{children}</ProfileContext.Provider>
}

export function useProfiles() {
    const context = useContext(ProfileContext);
    if (context === undefined) {
        throw new Error('useProfiles must be used inside a ProfileProvider');
    }
    return context;
}