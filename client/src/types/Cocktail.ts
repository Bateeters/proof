export type CocktailSummary = {
    id: string;
    name: string;
    category: string;
    glass: string;
    imageUrl: string | null;
}

export type CocktailDetail = CocktailSummary & {
    instructions: string;
    ingredients: CocktailIngredient[];
}

export type CocktailIngredient = {
    ingredientName: string;
    measure: string | null;
}