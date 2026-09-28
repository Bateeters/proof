import { useState, type SubmitEvent } from "react";
import { useAuth } from "../context/AuthContext";

export function RegisterForm() {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const { register } = useAuth();

    async function handleSubmit(e: SubmitEvent) {
        e.preventDefault();
        await register(email, password)
    }

    return (
        <form onSubmit={handleSubmit}>
            <div>
                <label htmlFor="register-email">Email</label>
                <input
                    id="register-email"
                    type="email"
                    value={email}
                    onChange={e => setEmail(e.target.value)}
                />
            </div>
            <div>
                <label htmlFor="register-password">Password</label>
                <input
                    id="register-password"
                    type="password"
                    value={password}
                    onChange={e => setPassword(e.target.value)}
                />
            </div>
            <button type="submit">Register</button>
        </form>
    )
}