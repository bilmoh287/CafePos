# Offline-First Architecture & Cloud Sync Design Note

## 1. Purpose

This document provides a design blueprint for extending the current **CafePos** tablet application into an **offline-first, cloud-synchronized Point-of-Sale system**.

> **Note on Scope**: The current CafePos application implements a fully functional local SQLite database that records transactions reliably without network dependencies. This design document outlines how a future synchronization engine could connect local tablet databases to a centralized ASP.NET Core Web API cloud backend.

---

## 2. Current Architecture vs. Future Extension

### Current Local Architecture
* **Local Data Store**: SQLite database (`cafepos.db3`) managed via Entity Framework Core (`AppDbContext`).
* **Order Creation**: Completed sales orders and line item price snapshots are persisted directly to SQLite during cashier checkout.
* **Network Dependency**: Zero. The cashier can create orders, compute sales tax, and review past transaction receipts entirely offline.

### Proposed Future Offline-First Extension
```text
┌────────────────────────────────────────────────────────┐
│                   POS TABLET DEVICE                    │
│                                                        │
│   Cashier Interface ──► MAUI App ──► SQLite DB         │
│                                           │            │
│                                  Local Outbox Queue    │
│                                           │            │
└───────────────────────────────────────────┼────────────┘
                                            │
                              ConnectivityListener
                                            │
                                            ▼
                                   Background Sync Worker
                                            │
                                  (HTTPS / TLS Outbound)
                                            │
                                            ▼
                                  ASP.NET Core Web API
                                            │
                                            ▼
                                  Server Central DB
```

---

## 3. Core Architectural Principles

### Principle 1: Local Database as Primary Source of Truth
The local tablet database (`cafepos.db3`) is the **primary source of truth** for all cashier transactions. The cashier experience must never be blocked or delayed by waiting for network latency, server handshakes, or HTTP response timeouts.

### Principle 2: Eventual Consistency
Cloud synchronization is decoupled from local order persistence. Local transactions are committed immediately. Cloud availability is checked asynchronously, ensuring eventual consistency across the restaurant enterprise without degrading cashier throughput.

### Principle 3: Distinction Between Order Status and Sync Status
* **`PaymentStatus`**: Tracks transaction business state (`Completed`, `Voided`).
* **`SyncStatus`**: Conceptual queue state (`PendingSync`, `Syncing`, `Synced`, `SyncFailed`).
* *Key Rule*: A cloud sync failure never alters or invalidates a completed customer sale locally.

---

## 4. Local Outbox Pattern & Sync Flow

1. **Local Order Commitment**:
   When a cashier confirms checkout, `OrderService` persists the order and its `OrderItem` snapshots into SQLite within a local database transaction.
2. **Outbox Queue Entry**:
   Concurrently, a record is added to a local `OutboxQueue` table referencing the `ClientOrderGuid` with `SyncStatus = PendingSync`.
3. **Connectivity Detection**:
   A platform connectivity listener (`Microsoft.Maui.Networking.Connectivity`) monitors network state (`Connectivity.NetworkAccess`).
4. **Asynchronous Transmission**:
   When network access becomes `NetworkAccess.Internet`, a background worker polls the `OutboxQueue` for `PendingSync` orders and submits an HTTP `POST /api/v1/orders/sync` payload to the ASP.NET Core Web API.
5. **Acknowledgement & State Transition**:
   Upon receiving HTTP `200 OK` / `201 Created` with a server acknowledgement, the local outbox status transitions to `Synced` with a `SyncedAt` timestamp.

---

## 5. Idempotency Strategy (Duplicate Prevention)

To prevent duplicate order creation when retrying network requests after drops or timeouts:

* **Client-Generated Unique Identifier (UUID)**:
  Each order is assigned a `ClientOrderGuid` (`Guid.NewGuid()`) at local creation time on the tablet.
* **Server-Side Idempotent Endpoint**:
  The ASP.NET Core Web API indexes `ClientOrderGuid` with a unique SQL constraint.
* **Retry Behavior**:
  If the tablet sends a retry request for an order that the server already saved before the previous connection dropped, the server detects the existing `ClientOrderGuid` and returns HTTP `200 OK` with the existing server order ID instead of creating a duplicate sale.

---

## 6. Failure Handling & Retry Strategy

1. **Exponential Backoff with Jitter**:
   Temporary network drops (e.g. transient Wi-Fi dead zones in a cafe) trigger retries using exponential backoff (e.g. 2s, 4s, 8s, 16s, up to 5 minutes max delay) with random jitter to prevent server thundering-herd issues.
2. **Repeated & Persistent Sync Failures**:
   * If an order fails sync repeatedly due to schema version mismatches or 5xx server errors, `SyncStatus` is updated to `SyncFailed` with `LastErrorDetails`.
   * **Receipt & Cashier Safety**: The order remains 100% accessible to the cashier in the **Order History** screen and receipt modal. Local sales reporting continues uninterrupted.
   * **Administrative Resolution**: An administrative "Retry Failed Syncs" command allows manual triggering once network or server issues are resolved.

---

## 7. Future Backend API Interaction

### Endpoint Payload Schema (Conceptual)
```json
{
  "clientOrderGuid": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
  "orderNumber": "ORD-20260828-104",
  "orderDate": "2026-08-28T05:07:00+00:00",
  "paymentMethod": "Cash",
  "subtotal": 10.00,
  "tax": 0.85,
  "grandTotal": 10.85,
  "items": [
    {
      "productId": 1,
      "productName": "Croissant",
      "unitPrice": 5.00,
      "quantity": 2,
      "totalPrice": 10.00
    }
  ]
}
```

---

## 8. Architectural Trade-offs & Considerations

| Consideration | Benefit | Trade-off / Mitigation |
| :--- | :--- | :--- |
| **Outbox Queue Storage** | Zero risk of lost sales if app closes while offline. | Slightly increased local SQLite storage usage. (Mitigation: Prune `Synced` records older than 30 days). |
| **Idempotency GUIDs** | Eliminates duplicate sales during network reconnects. | Server must maintain unique index on `ClientOrderGuid`. |
| **Asynchronous Background Sync** | Non-blocking cashier UI; fast checkout experience. | Server inventory levels are eventually consistent rather than real-time. |
