import { Navigate, Outlet } from "react-router-dom";
import { useAuth } from "../context/AuthContext";

export function ProtectedRoute() {
    const { token, isLoading } = useAuth();

    if (isLoading) {
        return (
            <div className="min-h-svh flex items-center justify-center text-ink-600">
                Loading...
            </div>
        );
    }

    if (!token) {
        return <Navigate to="/login" replace />;
    }

    return <Outlet />;
}
