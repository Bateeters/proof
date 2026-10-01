export type Sentiment = 'Positive' | 'Negative';

export type SpiritPreference = {
    spiritId: string;
    spiritName: string;
    sentiment: Sentiment;
}

export type FlavorPreference = {
    flavorTagId: string;
    flavorTagName: string;
    sentiment: Sentiment;
}

export type ProfilePreferences = {
    spiritPreferences: SpiritPreference[];
    flavorPreferences: FlavorPreference[];
    allergens: string[];
}

export type RankedCocktail = {
    id: string;
    name: string;
    category: string;
    glass: string;
    imageUrl: string | null;
    flavorTags: string[];
    matchScore: number;
}
