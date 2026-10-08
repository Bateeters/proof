export type Visibility = 'Private' | 'Local' | 'Global';

export type CocktailSummary = {
    id: string;
    name: string;
    category: string;
    glass: string;
    imageUrl: string | null;
    flavorTags: string[];
    // 0 for every cocktail when the request was made without a profileId --
    // backend's alphabetical tiebreaker reduces that to plain A-Z order.
    matchScore: number;
}

export type CocktailDetail = CocktailSummary & {
    instructions: string;
    ingredients: CocktailIngredient[];
    flavorTagIds: string[];
    seasons: string[];
    isCustom: boolean;
    visibility: Visibility;
    isOwnedByCaller: boolean;
}

export type CocktailIngredient = {
    ingredientId: string;
    ingredientName: string;
    measure: string | null;
}

export type CategoryPreview = {
    category: string;
    cocktails: CocktailSummary[];
}