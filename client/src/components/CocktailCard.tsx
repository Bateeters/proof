import { Link } from "react-router-dom";

type Props = {
    id: string;
    name: string;
    category: string;
    imageUrl: string | null;
    subtitle?: string;
};

export function CocktailCard({ id, name, category, imageUrl, subtitle }: Props) {
    return (
        // overflow-hidden lives on the inner wrapper below, not here — this
        // outer element carries the hover shadow/lift, which must NOT share
        // an overflow-hidden box with anything, or the shadow gets clipped
        // at its own edge.
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
        </Link>
    );
}
