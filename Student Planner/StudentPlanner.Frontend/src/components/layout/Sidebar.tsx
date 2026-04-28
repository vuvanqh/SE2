import { useAuth, useUser } from "../../global-hooks/authHooks";
import { useNotificationPreferences } from "../../features/settings/hooks/notificationPreferencesHooks";

export default function Sidebar({open}: {open: boolean}){
    const {user} = useUser();
    const {logout} = useAuth();
    const {preferences, isLoading, isPending, updatePreferences} = useNotificationPreferences(!!user);

    async function handleNotificationToggle() {
        if (!preferences) return;

        await updatePreferences({
            notificationsEnabled: !preferences.notificationsEnabled
        });
    }

    return <aside className={`sidebar ${open ? "open" : ""}`} onClick={(e) => e.stopPropagation()}>
        <div className="user-details">
            <img/>
            <div>
                <p>{user?.firstName} {user?.lastName}</p>
                <p>{user?.userRole} {user?.facultyCode?` - ${user.facultyCode}`:""}</p>
            </div>
        </div>

        <section className="sidebar-section">
            <button
                className="sidebar-toggle"
                onClick={handleNotificationToggle}
                disabled={!preferences || isLoading || isPending}
                type="button"
            >
                <span className="sidebar-section-title">Notifications</span>
                <span>{preferences?.notificationsEnabled ? "On" : "Off"}</span>
            </button>
        </section>

        <div className="sidebar-actions">
            <button>Change Passwd</button>
            <button className="danger">Delete Account</button>
            <button className="danger" onClick={async ()=>{await logout()}}>Log out</button>
        </div>
    </aside>
}

