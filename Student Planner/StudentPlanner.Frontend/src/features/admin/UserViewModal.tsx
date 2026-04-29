import Modal from "../../components/modals/Modal"
import type { userResponse } from "../../types/admin.types"

export default function UserViewModal({user, onClose, deleteUser}: {user: userResponse, onClose: () => void, deleteUser: (userId: string) => void}){
    return <Modal open onClose={onClose}>
        <h2 className="modal-title">
        {user.firstName} {user.lastName}
        </h2>

        <div className="view-section">
        <span className="view-label">Role</span>
        <div className="view-content">
            <span className="role-badge">{user.userRole}</span>
        </div>
        </div>

        <div className="view-section">
        <span className="view-label">Faculty</span>
        <p className="view-text">
            {user.faculty || "—"}
        </p>
        </div>

        <div className="view-section">
        <span className="view-label">Email</span>
        <p className="view-text">
            {user.email}
        </p>
        </div>

        <div className="modal-actions">
        <button className="btn-secondary" onClick={onClose}>
            Close
        </button>

        <button className="danger-btn" onClick={async () => {
            await deleteUser(user.id);
            console.log(user.id);
            onClose();
        }}>
            Delete User
        </button>
        </div>
    </Modal>
}