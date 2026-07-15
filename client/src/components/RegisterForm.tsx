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
            <button type="submit">Register</button>
        </form>
    )
}