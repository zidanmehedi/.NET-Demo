# NewsFeed — Architecture Overview

A small ASP.NET Core Web API that exposes CRUD operations over a `News` aggregate (with child `Comments`). The codebase follows a classic **Controller → Service → DbContext** layering with interface-based DI and EF Core.

---

## 1. Solution Layout

```
NewsFeed/
├── NewsFeed.slnx                  # Solution (single project)
└── NewsFeed/
    ├── NewsFeed.csproj            # Web SDK, net10.0
    ├── Program.cs                 # Composition root + pipeline
    ├── appsettings*.json          # Logging only (no ConnectionStrings)
    ├── Properties/launchSettings.json
    ├── NewsFeed.http              # Scratch HTTP file (still references default template)
    ├── Controllers/
    │   └── NewsController.cs      # Only controller in the project
    ├── Services/
    │   └── NewsFeedService.cs     # Business logic / EF access
    ├── Interfaces/
    │   └── INewsFeedService.cs    # Service contract
    ├── Data/
    │   └── NewsDbContext.cs       # EF Core DbContext + model config
    └── Entities/
        ├── News.cs
        └── Comments.cs
```

---

## 2. Tech Stack

| Concern        | Choice                                                |
| -------------- | ----------------------------------------------------- |
| Framework      | ASP.NET Core on **.NET 10** (`Microsoft.NET.Sdk.Web`) |
| Language opts  | `Nullable=enable`, `ImplicitUsings=enable`            |
| ORM            | EF Core 10.0.3                                        |
| Persistence    | **InMemory provider** (`UseInMemoryDatabase("NewsDb")`) — *non-persistent, resets every run* |
| API docs       | Swashbuckle (Swagger UI at `/swagger`)                |
| OpenAPI (new)  | `Microsoft.AspNetCore.OpenApi` referenced but `AddOpenApi`/`MapOpenApi` calls are commented out |
| Auth           | `UseAuthorization()` is wired but **no scheme is registered** (effectively open) |
| HTTPS          | `UseHttpsRedirection()` + dev cert (`https://localhost:7232`) |

> Note: the recent commit message mentions "sqlite and crud added", but the current `Program.cs` and `.csproj` use the **InMemory** provider only. There is no `Microsoft.EntityFrameworkCore.Sqlite` package or connection string. Likely an in-progress migration.

---

## 3. Layered Architecture

```
              HTTP
               │
               ▼
   ┌──────────────────────┐
   │   NewsController     │   [ApiController], route "api/[controller]"
   └──────────┬───────────┘
              │ INewsFeedService (DI)
              ▼
   ┌──────────────────────┐
   │   NewsFeedService    │   business logic + EF queries
   └──────────┬───────────┘
              │ NewsDbContext (DI, Scoped)
              ▼
   ┌──────────────────────┐
   │   NewsDbContext      │   DbSet<News>, DbSet<Comments>
   └──────────┬───────────┘
              ▼
       EF Core InMemory ("NewsDb")
```

- **Controller** is thin: parses input, delegates to the service, maps results to HTTP status codes (`Ok`, `Created`, `NoContent`, `NotFound`).
- **Service** holds all EF Core access. It is registered `Scoped`, matching the DbContext lifetime.
- **DbContext** owns the relational model configuration (one-to-many News → Comments with cascade delete).
- **Entities** are used directly as request/response models — no DTO layer.

---

## 4. Domain Model

### News (`Entities/News.cs`)
| Field        | Type               | Notes                                          |
| ------------ | ------------------ | ---------------------------------------------- |
| `Id`         | `int` (PK)         | EF convention                                  |
| `Title`      | `string`           | Non-nullable property, no `required` keyword → nullable warnings |
| `AuthorName` | `string`           |                                                |
| `Body`       | `string`           |                                                |
| `CreatedDate`| `DateTime`         | Not auto-populated by the service              |
| `Comments`   | `ICollection<Comments>` | Initialized to empty list                |

### Comments (`Entities/Comments.cs`)
| Field        | Type        | Notes                                                |
| ------------ | ----------- | ---------------------------------------------------- |
| `Id`         | `int` (PK)  |                                                      |
| `Name`       | `string`    | Commenter name                                       |
| `Content`    | `string`    |                                                      |
| `NewsId`     | `int` (FK)  | Required (changed from nullable in the latest commit)|
| `CreatedDate`| `DateTime`  | Not auto-populated                                   |
| `News`       | `News`      | Navigation property                                  |

### Relationship (`Data/NewsDbContext.cs`)
```csharp
News (1) ──< Comments (many)
   on delete: Cascade
```
Configured fluently in `OnModelCreating`.

---

## 5. HTTP Surface

All routes mounted under `api/News`.

| Verb   | Route               | Handler                  | Returns           |
| ------ | ------------------- | ------------------------ | ----------------- |
| GET    | `/api/News`         | `GetAllNewsAsync`        | 200 list / 404    |
| GET    | `/api/News/{id}`    | `GetNewsByIdAsync`       | 200 item / 404    |
| POST   | `/api/News`         | `CreateNewsAsync`        | 201 Created       |
| PUT    | `/api/News/{id}`    | `UpdateNewsAsync`        | 204 / 404         |
| DELETE | `/api/News/{id}`    | `Delete`                 | 204 / 404         |

### Service-only operations (not exposed yet)
`INewsFeedService` declares two endpoints that are **not surfaced by any controller**:
- `Task<Comments> AddCommentAsync(int newsId, Comments comment)`
- `Task<IEnumerable<News>> GetAllCommentsByNewsId(int newsId)`

A `CommentsController` (or extra routes on `NewsController`) is the obvious next step.

---

## 6. Composition Root (`Program.cs`)

```csharp
builder.Services.AddControllers();
builder.Services.AddDbContext<NewsDbContext>(o => o.UseInMemoryDatabase("NewsDb"));
builder.Services.AddScoped<INewsFeedService, NewsFeedService>();
builder.Services.AddSwaggerGen();
```

Pipeline (Development):
```
UseSwagger → UseSwaggerUI("/swagger") → UseHttpsRedirection → UseAuthorization → MapControllers
```

Launch URLs (`launchSettings.json`):
- HTTP:  `http://localhost:5037`
- HTTPS: `https://localhost:7232`
- Both open `/swagger` on launch.

---

## 7. Observations & Likely Change Targets

These are spots where a future change is likely to land — useful to know before editing.

1. **Persistence is in-memory.** Nothing survives a restart. Switching to SQLite (which the recent commit hinted at) requires:
   - Add `Microsoft.EntityFrameworkCore.Sqlite` package.
   - Add a connection string to `appsettings.json`.
   - Replace `UseInMemoryDatabase(...)` with `UseSqlite(...)`.
   - Add an initial migration.

2. **No DTOs.** Entities are passed straight in/out. POST/PUT accept the full graph, including client-supplied `Id` and `CreatedDate`. Worth introducing request/response models if the API grows.

3. **`CreatedDate` is never set server-side.** Both create and update paths trust whatever the client sends (or default `DateTime.MinValue`).

4. **`AddCommentAsync` doesn't bind `NewsId`.** It checks the parent exists but does not assign `comment.NewsId = newsId` before saving — comments will save with whatever id the caller sent (or 0).

5. **`GetAllCommentsByNewsId` returns `IEnumerable<News>`**, not comments. Probably should be `IEnumerable<Comments>` (or a single `News` with its `Comments` included).

6. **`UpdateNewsAsync` merges comments oddly:** it loops over the incoming comments, updates matched ones, and *adds unmatched ones without setting `NewsId`*. It also never deletes removed comments. Likely needs a clearer semantic (full replace vs. patch).

7. **`UseAuthorization()` with no auth scheme.** Either remove it or add an authentication scheme — currently it's dead middleware.

8. **OpenAPI duplication.** Both `Microsoft.AspNetCore.OpenApi` (commented out) and `Swashbuckle` are referenced. Pick one to avoid confusion.

9. **Nullable warnings.** Entity string properties are non-nullable but have no initializer / `required` modifier — compiler will warn under `Nullable=enable`.

10. **`NewsFeed.http` still has the template `weatherforecast` request** — safe to delete or replace with real `api/News` calls.

---

## 8. Quick Mental Map

> **One Web API project. One controller. One service. One DbContext. Two entities (News + Comments, 1-to-many, cascade delete). InMemory EF. Swagger UI. No auth, no DTOs, no migrations yet.**

That's the whole system at a glance — anything new will slot into one of these four layers.
