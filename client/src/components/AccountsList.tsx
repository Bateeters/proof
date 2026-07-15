import { useEffect, useState } from "react";
import type { Account } from "../types/Account";
import { useAuth } from "../context/AuthContext";

export function AccountsList() {
    const [accounts, setAccounts] = useState<Account[]>([]);
    const { token } = useAuth();

    useEffect(() => {
        fetch(`${import.meta.env.VITE_API_BASE_URL}/api/accounts`, {
            headers: { 'Authorization': `Bearer ${token}` }
        })
            .then(response => response.json())
            .then(data => setAccounts(data));
    }, [token]);

    return (
        <ul>
            {accounts.map((account) => (
                <li key={account.id}>
                    {account.email}
                </li>
            ))}
        </ul>
    )
}