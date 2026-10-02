import { useEffect, useState } from "react";
import { useAuth } from "../context/AuthContext";
import type { LookupItem } from "../types/Lookup";

// Fixed enum on the backend, not worth a lookup endpoint for 4 values.
const SEASON_OPTIONS = ["Spring", "Summer", "Fall", "Winter"];

export type FilterFacet = 'season' | 'flavor' | 'spirit';

export type ActiveFilter = {
    facet: FilterFacet;
    value: string;
};

// Shared by every page that offers the filter dropdowns (CategoryPage,
// Home's search mode): fetches the flavor-tag/spirit name lists once, and
// tracks which values are selected per facet. Exposes a single combined
// activeFilters list (for the one shared removable-chip tray) alongside
// each facet's own add/remove, since the dropdowns still need to add to
// one specific facet while removal can come from any of them.
export function useCocktailFilters() {
    const { token } = useAuth();
    const [flavorTagOptions, setFlavorTagOptions] = useState<string[]>([]);
    const [spiritOptions, setSpiritOptions] = useState<string[]>([]);
    const [selectedSeasons, setSelectedSeasons] = useState<string[]>([]);
    const [selectedFlavorTags, setSelectedFlavorTags] = useState<string[]>([]);
    const [selectedSpirits, setSelectedSpirits] = useState<string[]>([]);

    useEffect(() => {
        if (!token) return;

        fetch(`${import.meta.env.VITE_API_BASE_URL}/api/lookup/flavor-tags`, {
            headers: { 'Authorization': `Bearer ${token}` }
        })
            .then(response => response.json())
            .then((tags: LookupItem[]) => setFlavorTagOptions(tags.map(t => t.name)));

        fetch(`${import.meta.env.VITE_API_BASE_URL}/api/lookup/spirits`, {
            headers: { 'Authorization': `Bearer ${token}` }
        })
            .then(response => response.json())
            .then((spirits: LookupItem[]) => setSpiritOptions(spirits.map(s => s.name)));
    }, [token]);

    function addSeason(value: string) {
        setSelectedSeasons(prev => prev.includes(value) ? prev : [...prev, value]);
    }
    function addFlavorTag(value: string) {
        setSelectedFlavorTags(prev => prev.includes(value) ? prev : [...prev, value]);
    }
    function addSpirit(value: string) {
        setSelectedSpirits(prev => prev.includes(value) ? prev : [...prev, value]);
    }

    function removeFilter(facet: FilterFacet, value: string) {
        if (facet === 'season') setSelectedSeasons(prev => prev.filter(v => v !== value));
        else if (facet === 'flavor') setSelectedFlavorTags(prev => prev.filter(v => v !== value));
        else setSelectedSpirits(prev => prev.filter(v => v !== value));
    }

    const activeFilters: ActiveFilter[] = [
        ...selectedSeasons.map(value => ({ facet: 'season' as const, value })),
        ...selectedFlavorTags.map(value => ({ facet: 'flavor' as const, value })),
        ...selectedSpirits.map(value => ({ facet: 'spirit' as const, value })),
    ];

    return {
        // Already-selected values are left out of each dropdown's options --
        // no point offering "Citrus" again once it's already active.
        seasonOptions: SEASON_OPTIONS.filter(v => !selectedSeasons.includes(v)),
        flavorTagOptions: flavorTagOptions.filter(v => !selectedFlavorTags.includes(v)),
        spiritOptions: spiritOptions.filter(v => !selectedSpirits.includes(v)),
        selectedSeasons,
        selectedFlavorTags,
        selectedSpirits,
        addSeason,
        addFlavorTag,
        addSpirit,
        activeFilters,
        removeFilter,
    };
}
