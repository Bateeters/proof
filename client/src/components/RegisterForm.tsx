import { useState, type SubmitEvent } from "react";
import { useAuth } from "../context/AuthContext";

export function RegisterForm() {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState('');
    const { register } = useAuth();

    async function handleSubmit(e: SubmitEvent) {
        e.preventDefault();
        setError('');
        try {
            await register(email, password);
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Something went wrong.');
        }
    }

    return (
        <form onSubmit={handleSubmit} className="flex flex-col gap-4">
            <div>
                <label className="field-label" htmlFor="register-email">Email</label>
                <input
                    id="register-email"
                    className="field-input"
                    type="email"
                    value={email}
                    onChange={e => setEmail(e.target.value)}
                />
            </div>
            <div>
                <label className="field-label" htmlFor="register-password">Password</label>
                <input
                    id="register-password"
                    className="field-input"
                    type="password"
                    value={password}
                    onChange={e => setPassword(e.target.value)}
                />
            </div>
            {error && <p className="text-sm text-red-600">{error}</p>}
            <button type="submit" className="btn-secondary">Register</button>
        </form>
    )
}
