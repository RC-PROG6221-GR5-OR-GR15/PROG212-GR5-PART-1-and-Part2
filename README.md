# Marathon RaceDay — Event Management System

[![CI](https://github.com/RC-PROG6221-GR5-OR-GR15/PROG212-GR5-PART-1-and-Part2/actions/workflows/ci.yml/badge.svg)](https://github.com/RC-PROG6221-GR5-OR-GR15/PROG212-GR5-PART-1-and-Part2/actions/workflows/ci.yml)

A full-stack system for managing marathon and endurance events, their categories,
participant enrolments and race results. Part 3 adds a **role-aware ASP.NET Core MVC
web application** that consumes the Part 2 REST API, plus cloud file storage,
Docker containerisation and a continuous-integration pipeline.

---

## Solution Structure

| Project | Description |
| --- | --- |
| `Application/Marathon/Marathon` | **RESTful Web API** (Part 2). JWT-secured, EF Core code-first, Azure Blob Storage. |
| `Application/Marathon/Maraton.mvc` | **MVC web application** (Part 3). Consumes the API over HTTP; never touches the database directly. |
| `Application/Marathon/TestMarathon` | xUnit unit-test project covering authentication, role enforcement and CRUD failure paths. |

The MVC application is a pure consumer of the API. All data access is performed by
the API through Entity Framework Core; the MVC layer holds only a JWT and forwards it
on every request.

---

## Architecture

```text
Browser  →  Maraton.mvc (cookie auth + session)  →  HttpClient (JWT bearer)  →  Marathon API  →  SQL Server
                                                                                     │
                                                                                     └→ Azure Blob Storage (banners / avatars)
```

* **Authentication** — the API issues a JWT on login. The MVC application stores it in
  the server-side session and signs the user in with a cookie, attaching the JWT as a
  `Bearer` token on every API call via a typed `RaceDayApiClient`.
* **Role-based UI** — navigation, dashboards and action buttons are rendered from the
  signed-in user's role (`Organiser`, `Participant`, `Admin`).
* **Failure handling** — the typed client converts API error responses into friendly
  validation messages (including `ProblemDetails`), so the user never sees a raw stack trace.

---

## Features

### Participant
* Browse and filter events, and view full event details.
* Enrol in a category with a payment amount (partial payments are flagged as *Pending*).
* Personal dashboard showing total, confirmed and pending enrolments.
* Upload a profile picture (stored in Azure Blob Storage).

### Organiser
* Dashboard summarising active events, total enrolments and enrolments awaiting approval.
* Full event CRUD, including status management.
* Category CRUD with age ranges, gender restrictions, capacity and entry fees.
* Review the enrolment register for an event and confirm or reject each enrolment.
* Upload event banners (stored in Azure Blob Storage).

### Administrative / cross-cutting
* Role enforcement on every write endpoint (`Organiser, Admin` for management, `Participant` for enrolment).
* Capacity checks, duplicate-enrolment prevention and clear `400` / `404` / `409` responses.
* Realistic seed data for every entity, applied automatically on first run.

---

## Technology Stack

| Layer | Technology |
| --- | --- |
| API | ASP.NET Core 8 Web API, JWT bearer authentication |
| Web app | ASP.NET Core 8 MVC, Bootstrap 5, bootstrap-icons |
| Data | Entity Framework Core 8 (code-first), SQL Server |
| Cloud storage | Azure Blob Storage (`Azure.Storage.Blobs`) |
| Testing | xUnit, EF Core In-Memory provider |
| Containerisation | Docker, Docker Compose, Azurite (storage emulator) |
| CI | GitHub Actions |

---

## Database Model

```text
USER (UserId) ──< ENROLMENT >── EVENT (EventId)
                        │
                        └──< CATEGORY >── EVENT
USER (UserId) ──< USERROLE >── ROLE
USER (UserId) ──< PARTICIPANT_EVENT >── EVENT (bibs, finish times)
PARTICIPANT_EVENT ──< RESULT (positions, pace, prize money)
USERACCOUNT (UserAccountId)  — authentication identity, linked to USER by e-mail
```

Key cardinalities: one **Event** has many **Categories** (1:M); one **Category** has many
**Enrolments** (1:M); **User** and **Event** relate many-to-many through **Enrolment**;
one **Participant_Event** produces one **Result**.

---

## API Endpoints

### Authentication
```text
POST   /api/Auth/register            Create an account (participants supply profile details)
POST   /api/Auth/login               Obtain a JWT
GET    /api/Auth/me                  Current account (requires token)
```

### Events
```text
GET    /api/Events                   List all events                     (anonymous)
GET    /api/Events/{id}              Event details                       (anonymous)
POST   /api/Events                   Create event                        (Organiser, Admin)
PUT    /api/Events/{id}              Update event                        (Organiser, Admin)
DELETE /api/Events/{id}              Delete event                        (Organiser, Admin)
```

### Categories
```text
GET    /api/Category                 List categories                     (anonymous)
GET    /api/Category/{id}            Category details                    (anonymous)
POST   /api/Category                 Create category                     (Organiser, Admin)
PUT    /api/Category/{id}            Update category                     (Organiser, Admin)
DELETE /api/Category/{id}            Delete category                     (Organiser, Admin)
```

### Enrolments
```text
POST   /api/Enrolment                Enrol in a category                 (Participant)
GET    /api/Enrolment/me             My enrolments                       (authenticated)
GET    /api/Enrolment                Full enrolment register             (Organiser, Admin)
GET    /api/Enrolment/{id}           Single enrolment                    (owner or staff)
GET    /api/Enrolment/user/{userId}  Enrolments for a user               (owner or staff)
PATCH  /api/Enrolment/{id}           Confirm / reject an enrolment       (Organiser, Admin)
```

### Uploads (Azure Blob Storage)
```text
POST   /api/Upload/event-banner      Upload an event banner              (Organiser, Admin)
POST   /api/Upload/profile-picture   Upload a profile picture            (authenticated)
```

Swagger UI is available at `https://localhost:7233/swagger` while the API runs in Development.

---

## Getting Started (local development)

### Prerequisites
* .NET 8 SDK
* SQL Server (Express is sufficient) or SQL Server in Docker
* Visual Studio 2022 (or the `dotnet` CLI)

### 1. Clone
```bash
git clone https://github.com/RC-PROG6221-GR5-OR-GR15/PROG212-GR5-PART-1-and-Part2.git
cd PROG212-GR5-PART-1-and-Part2
```

### 2. Configure the API
Edit `Application/Marathon/Marathon/appsettings.json` and point
`ConnectionStrings:DefaultConnection` at your SQL Server instance. The database is
created and seeded automatically the first time the API starts.

### 3. Run the API
```bash
cd Application/Marathon/Marathon
dotnet run
```
The API listens on `https://localhost:7233`.

### 4. Run the MVC application
```bash
cd Application/Marathon/Maraton.mvc
dotnet run
```
The web application listens on `https://localhost:7120` and reads the API base URL from
`ApiSettings:BaseUrl` in its `appsettings.json`.

### 5. Sign in
Seed data creates these demonstration accounts (password `Passw0rd!` for all):

| Role | E-mail |
| --- | --- |
| Organiser | `organiser@raceday.com` |
| Organiser | `events@capetownmarathon.co.za` |
| Participant | `participant@raceday.com` |
| Participant | `thabo.mokoena@raceday.com` |
| Participant | `lindiwe.dlamini@raceday.com` |

---

## Running with Docker

A single command starts the whole stack — SQL Server, Azurite (storage emulator), the
API and the MVC application:

```bash
cd Application/Marathon
docker compose up --build
```

| Service | URL |
| --- | --- |
| MVC web application | http://localhost:8081 |
| API + Swagger | http://localhost:8080/swagger |

The API applies its migrations and seeds the database automatically once SQL Server is
healthy.

---

## Running the Tests

```bash
cd Application/Marathon
dotnet test Marathon.sln
```

The suite (`TestMarathon`) covers account registration and login, duplicate and
incomplete-registration failures, JWT issuance, event/category CRUD failure paths,
enrolment rules (capacity, duplicates, negative payments) and role-based access on
every protected endpoint — **39 tests**.

---

## Continuous Integration

`.github/workflows/ci.yml` runs on every push and pull request to `main`. It restores,
builds and tests the solution on .NET 8, uploads the test results as an artifact, and
builds both Docker images. The status badge is shown at the top of this document.

---

## User Roles

| Role | Responsibilities |
| --- | --- |
| Administrator | Full access to events, categories, enrolments and accounts |
| Organiser | Creates and manages events, categories and enrolments |
| Participant | Browses events, enrols in categories and tracks results |

---

## Demo Video

Part 3 walkthrough: <https://youtu.be/ycXFP0hjffo>

---

## References

Microsoft (2024) *ASP.NET Core documentation*. Available at: https://learn.microsoft.com/aspnet/core (Accessed: 2026).

Microsoft (2024) *Entity Framework Core documentation*. Available at: https://learn.microsoft.com/ef/core (Accessed: 2026).

Microsoft (2024) *Azure Blob Storage client library for .NET*. Available at: https://learn.microsoft.com/azure/storage/blobs/storage-quickstart-blobs-dotnet (Accessed: 2026).

Docker Inc. (2024) *Docker Compose overview*. Available at: https://docs.docker.com/compose/ (Accessed: 2026).

GitHub (2024) *GitHub Actions documentation*. Available at: https://docs.github.com/actions (Accessed: 2026).

Bootstrap (2024) *Bootstrap 5 documentation*. Available at: https://getbootstrap.com/docs/5.3/ (Accessed: 2026).
