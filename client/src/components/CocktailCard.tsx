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
        <Link
            to={`/cocktails/${id}`}
            className="group block rounded-lg overflow-hidden border border-marble-300 bg-white
                transition-all hover:border-gold-400 hover:shadow-[0_8px_24px_rgba(179,135,42,0.18)] hover:-translate-y-0.5"
        >
            <div className="aspect-square bg-marble-200 overflow-hidden">
                {imageUrl ? (
                    <img
                        src={imageUrl}
                        alt={name}
                        className="w-full h-full object-cover transition-transform duration-300 group-hover:scale-105"
                    />
                ) : (
                    <div className="w-full h-full flex items-center justify-center text-ink-600 text-xs px-2 text-center">
                        {category}
                    </div>
                )}
            </div>
            <div className="p-3">
                <p className="font-medium text-ink-900 text-sm leading-snug truncate">{name}</p>
                <p className="text-xs text-ink-600 truncate">{subtitle ?? category}</p>
            </div>
        </Link>
    );
}
