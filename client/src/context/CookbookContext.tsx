import { createContext, useContext, useEffect, useState, type ReactNode } from "react";
import { useAuth } from "./AuthContext";
import { useProfiles } from "./ProfileContext";
import type { CookbookEntry } from "../types/Cookbook";

type SaveableCocktail = {
    id: string;
    name: string;
    category: string;
    imageUrl: string | null;
};

type CookbookContextValue = {
    // Just the ids, not full entries -- this is purely for "is this
    // cocktail saved" lookups from CocktailCard everywhere it appears.
    // Pages that need the full saved list (the Drink Menu page) still do
    // their own fetch for the actual display data (name/image/etc).
    savedIds: Set<string>;
    toggleSaved: (cocktail: SaveableCocktail) => void;
};

const CookbookContext = createContext<CookbookContextValue | undefined>(undefined);

export function CookbookProvider({ children }: { children: ReactNode }) {
    const { token } = useAuth();
    const { activeProfile } = useProfiles();
    const [savedIds, setSavedIds] = useState<Set<string>>(new Set());

    useEffect(() => {
        if (!token || !activeProfile) {
            setSavedIds(new Set());
            return;
        }

        fetch(`${import.meta.env.VITE_API_BASE_URL}/api/profiles/${activeProfile.id}/cookbook`, {
            headers: { 'Authorization': `Bearer ${token}` }
        })
            .then(response => response.json())
            .then((entries: CookbookEntry[]) => setSavedIds(new Set(entries.map(e => e.cocktailId))));
    }, [token, activeProfile]);

    function toggleSaved(cocktail: SaveableCocktail) {
        if (!activeProfile) return;

        const isCurrentlySaved = savedIds.has(cocktail.id);

        // Optimistic -- flip the local set immediately so every card
        // showing this cocktail updates in the same render, rather than
        // waiting on the round-trip.
        setSavedIds(prev => {
            const next = new Set(prev);
            if (isCurrentlySaved) {
                next.delete(cocktail.id);
            } else {
                next.add(cocktail.id);
            }
            return next;
        });

        if (isCurrentlySaved) {
            fetch(`${import.meta.env.VITE_API_BASE_URL}/api/profiles/${activeProfile.id}/cookbook/${cocktail.id}`, {
                method: 'DELETE',
                headers: { 'Authorization': `Bearer ${token}` }
            });
        } else {
            fetch(`${import.meta.env.VITE_API_BASE_URL}/api/profiles/${activeProfile.id}/cookbook`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`,
                },
                body: JSON.stringify({ cocktailId: cocktail.id }),
            });
        }
    }

    return (
        <CookbookContext.Provider value={{ savedIds, toggleSaved }}>
            {children}
        </CookbookContext.Provider>
    );
}

export function useCookbook() {
    const context = useContext(CookbookContext);
    if (context === undefined) {
        throw new Error('useCookbook must be used inside a CookbookProvider');
    }
    return context;
}
