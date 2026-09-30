import { useAuth } from "../context/AuthContext";

// Minimal placeholder -- there's no backend support yet for actually
// changing account settings (email, password, etc.), so this just surfaces
// what's already known rather than faking functionality that isn't there.
export function AccountSettingsPage() {
    const { account } = useAuth();

    return (
        <div className="max-w-xl">
            <h1 className="text-3xl mb-6">Account</h1>
            <div className="bg-white border border-marble-300 rounded-lg p-6">
                <p className="field-label">Email</p>
                <p className="text-ink-800 mb-4">{account?.email}</p>
                <p className="text-ink-600 text-sm italic">
                    More account settings (changing your email, password, etc.) are coming soon.
                </p>
            </div>
        </div>
    );
}
