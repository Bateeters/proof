import { Link } from "react-router-dom";
import { useCookbook } from "../context/CookbookContext";

type Props = {
    id: string;
    name: string;
    category: string;
    imageUrl: string | null;
    subtitle?: string;
    flavorTags?: string[];
};

export function CocktailCard({ id, name, category, imageUrl, subtitle, flavorTags = [] }: Props) {
    const { savedIds, toggleSaved } = useCookbook();
    const isSaved = savedIds.has(id);

    return (
        // overflow-hidden lives on the inner wrapper below, not here — this
        // outer element carries the hover shadow/lift, which must NOT share
        // an overflow-hidden box with anything, or the shadow gets clipped
        // at its own edge. The heart button below is a sibling of that
        // wrapper for the same reason, and so it never ends up inside the
        // <Link> in a way that would make clicking it also navigate.
        <Link
            to={`/cocktails/${id}`}
            className="group relative block w-full aspect-[3/4] rounded-lg border border-marble-300
                transition-all hover:border-gold-400 hover:shadow-[0_10px_32px_rgba(179,135,42,0.22)] hover:-translate-y-0.5"
        >
            <div className="absolute inset-0 rounded-lg overflow-hidden bg-marble-200">
                {imageUrl ? (
                    <img
                        src={imageUrl}
                        alt={name}
                        className="absolute inset-0 w-full h-full object-cover transition-transform duration-300 group-hover:scale-105"
                    />
                ) : (
                    <div className="absolute inset-0 flex items-center justify-center text-ink-600 text-xs px-2 text-center">
                        {category}
                    </div>
                )}

                {/* Bottom 1/4 gradient so the overlaid text stays legible over
                    any photo, without darkening the rest of the image. */}
                <div className="absolute inset-x-0 bottom-0 h-1/4 bg-gradient-to-t from-ink-900/60 to-transparent" />

                <div className="absolute inset-x-0 bottom-0 p-3 text-left">
                    <p className="font-medium text-white text-sm leading-snug drop-shadow-sm truncate">{name}</p>
                    <p className="text-xs text-marble-200/90 truncate">{subtitle ?? category}</p>
                </div>
            </div>

            {flavorTags.length > 0 && (
                <div className="absolute top-2 left-2 flex flex-wrap gap-1 max-w-[75%]">
                    {flavorTags.slice(0, 2).map(tag => (
                        <span
                            key={tag}
                            className="rounded-full bg-white/85 text-ink-900 text-[11px] font-medium px-2 py-0.5 shadow-sm"
                        >
                            {tag}
                        </span>
                    ))}
                </div>
            )}

            <button
                onClick={(e) => {
                    e.preventDefault();
                    e.stopPropagation();
                    toggleSaved({ id, name, category, imageUrl });
                }}
                aria-label={isSaved ? "Remove from Drink Menu" : "Add to Drink Menu"}
                aria-pressed={isSaved}
                className="absolute top-2 right-2 z-10 w-8 h-8 rounded-full bg-white/80 hover:bg-white
                    flex items-center justify-center shadow-sm transition-colors"
            >
                <svg
                    viewBox="0 0 24 24"
                    className={`w-4.5 h-4.5 transition-colors ${isSaved ? "fill-red-600 stroke-red-600" : "fill-none stroke-ink-900"}`}
                    strokeWidth={2}
                >
                    <path
                        strokeLinecap="round"
                        strokeLinejoin="round"
                        d="M12 20.5s-7.5-4.6-10-9.1C.6 8.1 2.1 4.8 5.3 4.1c2-.4 3.9.5 5 2.1a.9.9 0 0 0 1.4 0c1.1-1.6 3-2.5 5-2.1 3.2.7 4.7 4 3.3 7.3-2.5 4.5-10 9.1-10 9.1Z"
                    />
                </svg>
            </button>
        </Link>
    );
}
