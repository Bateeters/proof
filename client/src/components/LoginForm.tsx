import { useState } from "react";
import type { SubmitEvent } from "react";
import { useAuth } from "../context/AuthContext";

export function LoginForm() {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const { login } = useAuth();

    async function handleSubmit(e: SubmitEvent) {
        e.preventDefault();
        await login(email, password)
    }

    return (
        <form onSubmit={handleSubmit}>
            <input
                type="email"
                value={email}
                onChange={e => setEmail(e.target.value)}
            />
            <input
                type="password"
                value={password}
                onChange={e => setPassword(e.target.value)}
            />
            <button type="submit">Log In</button>
        </form>
    )
}