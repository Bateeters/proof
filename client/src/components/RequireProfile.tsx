import { Navigate, Outlet } from "react-router-dom";
import { useProfiles } from "../context/ProfileContext";

export function RequireProfile() {
    const { activeProfile, profilesLoaded } = useProfiles();

    if (!profilesLoaded) {
        return (
            <div className="min-h-svh flex items-center justify-center text-ink-600">
                Loading...
            </div>
        );
    }

    if (!activeProfile) {
        return <Navigate to="/profiles" replace />;
    }

    return <Outlet />;
}
