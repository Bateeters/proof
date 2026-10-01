export type CookbookEntry = {
    cocktailId: string;
    cocktailName: string;
    cocktailCategory: string;
    cocktailImageUrl: string | null;
    flavorTags: string[];
    savedAt: string;
    notes: string | null;
}
