import { useEffect, useState } from "react";
import type { Account } from "../types/Account";

export function AccountsList() {
    const [accounts, setAccounts] = useState<Account[]>([]);

    useEffect(() => {
        fetch(`${import.meta.env.VITE_API_BASE_URL}/api/accounts`)
            .then(response => response.json())
            .then(data => setAccounts(data));
    }, []);

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