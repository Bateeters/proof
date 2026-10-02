import { useEffect, useState } from "react";
import { useAuth } from "../context/AuthContext";
import type { LookupItem } from "../types/Lookup";

// Fixed enum on the backend, not worth a lookup endpoint for 4 values.
const SEASON_OPTIONS = ["Spring", "Summer", "Fall", "Winter"];

// Shared by every page that offers the season/flavor-tag filter rows
// (CategoryPage, Home's search mode) so the fetch-once-for-flavor-tag-names
// + toggle-state logic isn't duplicated between them.
export function useCocktailFilters() {
    const { token } = useAuth();
    const [flavorTagOptions, setFlavorTagOptions] = useState<string[]>([]);
    const [selectedSeasons, setSelectedSeasons] = useState<string[]>([]);
    const [selectedFlavorTags, setSelectedFlavorTags] = useState<string[]>([]);

    useEffect(() => {
        if (!token) return;

        fetch(`${import.meta.env.VITE_API_BASE_URL}/api/lookup/flavor-tags`, {
            headers: { 'Authorization': `Bearer ${token}` }
        })
            .then(response => response.json())
            .then((tags: LookupItem[]) => setFlavorTagOptions(tags.map(t => t.name)));
    }, [token]);

    function toggleSeason(season: string) {
        setSelectedSeasons(prev =>
            prev.includes(season) ? prev.filter(s => s !== season) : [...prev, season]
        );
    }

    function toggleFlavorTag(tag: string) {
        setSelectedFlavorTags(prev =>
            prev.includes(tag) ? prev.filter(t => t !== tag) : [...prev, tag]
        );
    }

    return {
        seasonOptions: SEASON_OPTIONS,
        flavorTagOptions,
        selectedSeasons,
        selectedFlavorTags,
        toggleSeason,
        toggleFlavorTag,
    };
}
