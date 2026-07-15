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