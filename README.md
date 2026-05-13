# 📦 Logistics Management API

A production-grade **RESTful API** for managing logistics operations — built with **Clean Architecture**, **Domain-Driven Design**, and the **Outbox Pattern** for reliable event processing.

> Developed by **Ahmed Mostafa**

---

## 📌 Overview

Logistics Management API handles the full lifecycle of shipments — from order creation and payment, through carrier assignment and transit tracking, to final delivery. It was built with a strong focus on **reliability**, **observability**, and **domain integrity**.

---

## ✨ Features

- 📋 **Order Management** — Create orders with pickup/delivery addresses, items, and declared value. Full lifecycle: Draft → Confirmed → InTransit → Delivered/Cancelled
- 📦 **Shipment Management** — Create shipments from orders, assign carriers, mark pickup, add transit notes, and confirm delivery
- 🔍 **Shipment Tracking** — Public tracking endpoint using a customer-facing tracking code. Full event history per shipment
- 🔔 **Notifications** — In-app notifications driven by the Outbox Pattern (order created, carrier assigned, status changed)
- 💳 **Payment** — Payment intent creation linked to orders
- 🔐 **Authentication** — JWT access tokens with refresh token rotation and session management
- 🔁 **Outbox Pattern** — Guaranteed event delivery with automatic retries, dead-letter handling, and multi-instance safe locking
- 🛠️ **Ops Admin API** — Inspect, retry, and manage failed outbox messages
- 🆔 **Correlation IDs** — Every request gets a unique `X-Correlation-Id` traced across the full request lifecycle
- 🏥 **Health Check** — Dependency health endpoint for infrastructure monitoring

---

## 🏗️ Architecture

The project follows **Clean Architecture** with strict layer separation:

```
LogisticsAPI/
├── Logis.Domain/          # Entities, Value Objects, domain rules and state machines
├── Logis.Application/     # Service interfaces, contracts, outbox event definitions
├── Logis.Infrastructure/  # EF Core, JWT, outbox processor, notification service
└── Logis.Api/             # Controllers, middleware, DI configuration, Program.cs
```

### Dependency Flow
```
Api → Application → Domain
Infrastructure → Application → Domain
```

The **Domain** layer has zero external dependencies and enforces all business rules through entity methods.

---

## 🛠️ Tech Stack

| Category | Technology |
|---|---|
| **Framework** | ASP.NET Core (.NET) |
| **Architecture** | Clean Architecture + DDD |
| **Database** | SQL Server + Entity Framework Core |
| **Authentication** | ASP.NET Identity + JWT Bearer |
| **Refresh Tokens** | Rotation with session tracking |
| **Reliability** | Outbox Pattern with background processor |
| **Logging** | Structured logging with correlation IDs |
| **API Docs** | Scalar UI + OpenAPI |
| **Containerization** | Docker + Docker Compose |

---

## 🧠 Key Design Decisions

### Domain-Driven Design
Entities enforce their own rules through methods — state transitions are protected by domain logic rather than application-level checks:

```
Order:    Draft → Confirmed (requires payment) → Cancelled (blocked after delivery/transit)
Shipment: Created → Assigned → PickedUp → InTransit → Delivered / Cancelled
```

Every state transition is validated inside the entity. Invalid transitions throw domain exceptions caught by the exception middleware.

### Outbox Pattern
Instead of calling external services directly, events are written to an `OutBoxMessages` table in the same database transaction as the business data. A background worker then processes them asynchronously:

- **Guaranteed delivery** — events survive application crashes
- **Automatic retries** — failed messages are retried up to `MaxAttempts`
- **Dead-letter handling** — messages that exhaust retries are marked as dead
- **Multi-instance safe** — optimistic locking (`LockedBy` / `LockedUntilUtc`) prevents two worker instances from processing the same message

### Exception Middleware
Exceptions are mapped to HTTP status codes at a single point — no try/catch in controllers:

| Exception | HTTP Status |
|---|---|
| `ArgumentException` | 400 Bad Request |
| `KeyNotFoundException` | 404 Not Found |
| `InvalidOperationException` | 409 Conflict |
| `UnauthorizedAccessException` | 401 Unauthorized |
| `DbUpdateConcurrencyException` | 409 Conflict |
| Unhandled | 500 Internal Server Error |

In development, full exception details are returned. In production, a safe generic message is shown.

### Correlation IDs
Every request receives a `X-Correlation-Id` header (generated if not provided). It's stored in `HttpContext.Items` for the full request lifecycle and echoed back in the response — making log tracing across distributed systems straightforward.

---

## 📖 API Endpoints

### Auth — `/api/Auth`

| Method | Endpoint | Description |
|---|---|---|
| POST | `/register` | Create a new account |
| POST | `/login` | Login and receive JWT + refresh token |
| POST | `/refresh` | Rotate refresh token |
| POST | `/change-password` | Change account password |

### Orders — `/api/Orders`

| Method | Endpoint | Description |
|---|---|---|
| POST | `/` | Create a new order |
| GET | `/{id}` | Get order by ID |
| POST | `/{id}/confirm` | Confirm a paid order |
| POST | `/{id}/cancel` | Cancel an order |

### Shipments — `/api/Shipments`

| Method | Endpoint | Description |
|---|---|---|
| POST | `/{orderId}/shipment` | Create shipment for an order |
| GET | `/{id}` | Get shipment with tracking events |
| POST | `/{id}/assign` | Assign a carrier |
| POST | `/{id}/pickup` | Mark as picked up |
| POST | `/{id}/transit` | Add transit note |
| POST | `/{id}/deliver` | Mark as delivered |
| POST | `/{id}/cancel` | Cancel shipment |

### Tracking — `/api/Tracking`

| Method | Endpoint | Description |
|---|---|---|
| GET | `/{trackingCode}` | Public tracking by customer tracking code |

### Notifications — `/api/Notifications`

| Method | Endpoint | Description |
|---|---|---|
| GET | `/` | Get user notifications |
| POST | `/{id}/read` | Mark notification as read |

### Outbox Admin — `/api/OutboxAdmin` *(ops key required)*

| Method | Endpoint | Description |
|---|---|---|
| GET | `/dead` | List dead messages |
| GET | `/failed` | List failed messages |
| POST | `/retry` | Retry a batch of failed messages |

---

## 🚀 Getting Started

### Prerequisites
- [.NET SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

### Run with Docker Compose

```bash
git clone https://github.com/A-hmedMustafa/Logistics-Management-API.git
cd Logistics-Management-API
docker-compose up --build
```

### Run locally

```bash
cd Logis.Api
dotnet run
```

### Apply migrations

```bash
dotnet ef database update --project Logis.Infrastructure --startup-project Logis.Api
```

---

## 📖 API Documentation

When running in development:

- **Scalar UI** → `https://localhost:{7048}/scalar`

---

## 👤 Author

**Ahmed Mostafa**
- GitHub: [@A-hmedMustafa](https://github.com/A-hmedMustafa)
- LinkedIn: [Ahmed Mostafa](https://www.linkedin.com/in/ahmed-mostafa-mohammed/)
