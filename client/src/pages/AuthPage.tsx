import { Navigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";
import { LoginForm } from "../components/LoginForm";
import { RegisterForm } from "../components/RegisterForm";

export function AuthPage() {
    const { token, isLoading } = useAuth();

    if (isLoading) {
        return (
            <div className="min-h-svh flex items-center justify-center text-ink-600">
                Loading...
            </div>
        );
    }

    if (token) {
        return <Navigate to="/" replace />;
    }

    return (
        <div className="min-h-svh flex items-center justify-center px-4">
            <div className="w-full max-w-md">
                <h1 className="text-center text-4xl mb-1 tracking-wide">Proof</h1>
                <p className="text-center text-ink-600 mb-10 text-sm tracking-[0.2em] uppercase">
                    Craft, Curated
                </p>

                <div className="bg-white border border-gold-300/40 rounded-lg shadow-[0_4px_30px_rgba(179,135,42,0.08)] p-8 mb-6">
                    <h2 className="text-xl mb-5">Log In</h2>
                    <LoginForm />
                </div>

                <div className="bg-white border border-marble-300 rounded-lg p-8">
                    <h2 className="text-xl mb-5">Register</h2>
                    <RegisterForm />
                </div>
            </div>
        </div>
    );
}
