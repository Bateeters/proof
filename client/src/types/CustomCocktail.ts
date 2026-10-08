import type { Visibility } from "./Cocktail";

// Exactly one of ingredientId/newIngredientName set -- mirrors the backend's
// CocktailIngredientInputDto. spiritId is only meaningful alongside
// newIngredientName (manual override when the heuristic doesn't recognize
// the typed name).
export type CocktailIngredientInput = {
    ingredientId?: string;
    newIngredientName?: string;
    spiritId?: string;
    measure: string | null;
}

export type SaveCustomCocktailRequest = {
    name: string;
    category: string;
    glass: string;
    instructions: string;
    imageUrl: string | null;
    visibility: Visibility;
    ingredients: CocktailIngredientInput[];
    flavorTagIds: string[];
    seasons: string[];
}

export type MyCocktailSummary = {
    id: string;
    name: string;
    category: string;
    glass: string;
    imageUrl: string | null;
    flavorTags: string[];
    visibility: Visibility;
}

export type SuggestTagsRequest = {
    ingredients: { ingredientId?: string; newIngredientName?: string }[];
}

export type SuggestTagsResponse = {
    flavorTagIds: string[];
    seasons: string[];
}
