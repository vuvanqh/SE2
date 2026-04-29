
import { useState } from "react";
import { useUser } from "../../../global-hooks/authHooks";
import { useAllEventRequests, useMyEventRequests } from "../../../features/eventRequests/hooks/eventRequestHooks";
import { EventRequestPreview } from "../../../features/eventRequests/components/EventRequestPreview";
import AdminRequestPreview from "../../../features/eventRequests/components/AdminRequestPreview";

export default function EventRequestPage() {
  const { user } = useUser();
  const isAdmin = user?.userRole === "Admin";

  const { eventRequests: allRequests, isPending: isAllPending } = useAllEventRequests();
  const { eventRequests: myRequests, isPending: isMyPending } = useMyEventRequests();

  const eventRequests = isAdmin ? allRequests : myRequests;
  const isPending = isAdmin ? isAllPending : isMyPending;

  const [search, setSearch] = useState("");
  const [statusFilter, setStatusFilter] = useState<"All" | "Pending" | "Approved" | "Rejected">("All");

  const filteredRequests = eventRequests.filter((request) => {
    const matchesSearch = request.eventDetails.title.toLowerCase().includes(search.toLowerCase());
    const matchesStatus = statusFilter === "All" || request.status === statusFilter;

    return matchesSearch && matchesStatus;
  });

  if (isPending) {
    return (
      <div className="user-management-page">
        <section className="users-card">
          <div className="users-header">
            <h2>Event Requests</h2>
          </div>
          <p>Loading...</p>
        </section>
      </div>
    );
  }

  return (
    <div className="user-management-page">
      <section className="users-card">
        <div className="users-header">
          <h2>Event Requests</h2>
        </div>
        <ul className="users-list">
          {filteredRequests.length > 0 ? (
            isAdmin ? (
              filteredRequests.map((request) => (
                <li key={request.id}>
                  <AdminRequestPreview eventRequest={request} />
                </li>
              ))
            ) : (
              filteredRequests.map((request) => (
                <li key={request.id}>
                  <EventRequestPreview eventRequest={request} />
                </li>
              ))
            )
          ) : (
            <p>No event requests found matching your criteria.</p>
          )}
        </ul>
      </section>

      <aside className="user-panel">
        <input
          className="search-input"
          placeholder="Search by title..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />

        <div className="filter-group">
          <p className="filter-title">Status</p>
          {(["All", "Pending", "Approved", "Rejected"] as const).map((status) => (
            <label key={status} className="filter-option">
              <input
                type="radio"
                name="status"
                value={status}
                checked={statusFilter === status}
                onChange={(e) => setStatusFilter(e.target.value as "All" | "Pending" | "Approved" | "Rejected")}
              />
              {status}
            </label>
          ))}
        </div>

        <button className="ghost-btn" onClick={() => {
          setSearch("");
          setStatusFilter("All");
        }}>
          Clear filters
        </button>
      </aside>
    </div>
  );
}