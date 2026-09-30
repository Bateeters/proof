import { useState, type SubmitEvent } from "react";
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
        <form onSubmit={handleSubmit} className="flex flex-col gap-4">
            <div>
                <label className="field-label" htmlFor="login-email">Email</label>
                <input
                    id="login-email"
                    className="field-input"
                    type="email"
                    value={email}
                    onChange={e => setEmail(e.target.value)}
                />
            </div>
            <div>
                <label className="field-label" htmlFor="login-password">Password</label>
                <input
                    id="login-password"
                    className="field-input"
                    type="password"
                    value={password}
                    onChange={e => setPassword(e.target.value)}
                />
            </div>
            <button type="submit" className="btn-primary">Log In</button>
        </form>
    )
}
