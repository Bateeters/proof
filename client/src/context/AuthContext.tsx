import { createContext, useContext, useState, type ReactNode } from "react";
import type { Account } from "../types/Account";

type AuthContextValue = {
    token: string | null;
    account: Account | null;
    login: (email: string, password: string) => Promise<void>;
    register: (email: string, password: string) => Promise<void>;
    logout: () => void
};

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
    const [token, setToken] = useState<string | null>(null);
    const [account, setAccount] = useState<Account | null>(null);

    async function login(email: string, password: string) {
        const response = await fetch(`${import.meta.env.VITE_API_BASE_URL}/api/auth/login`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ email, password }),
        })

        // Login returns a bare 401 (no body) on bad credentials -- parsing
        // JSON from an empty response throws, so this has to be checked
        // before reading the body, not after.
        if (!response.ok) {
            throw new Error('Invalid email or password.');
        }

        const data = await response.json();

        setAccount(data.account);
        setToken(data.token);
    }

    async function register(email: string, password: string) {
        const response = await fetch(`${import.meta.env.VITE_API_BASE_URL}/api/auth/register`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ email, password }),
        })

        if (!response.ok) {
            throw new Error('Could not register with that email and password.');
        }

        const data = await response.json();

        setAccount(data.account);
        setToken(data.token);
    }

    function logout() {
        setToken(null);
        setAccount(null);
    }

    const value: AuthContextValue = { token, account, login, register, logout };

    return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
    const context = useContext(AuthContext);
    if (context === undefined) {
        throw new Error('useAuth must be used inside an AuthProvider');
    }
    return context;
}
