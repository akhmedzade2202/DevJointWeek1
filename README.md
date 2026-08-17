# LibraryApi — .NET 8 / ASP.NET Core Web API

## Layihə haqqında

Library Management System — CodeAcademy tapşırığının Spring Boot original versiyasının .NET stack-ə uyğunlaşdırılmış həyata keçirilməsi. Clean Architecture prinsipləri əsasında qurulub, JWT əsaslı autentifikasiya, rol əsaslı avtorizasiya, in-memory cache, fayl upload/download, asinxron email bildirişi və avtomatik scheduled job ilə tam funksional REST API.

**Stack:** .NET 8 · ASP.NET Core Web API · Entity Framework Core · SQL Server · BCrypt · JWT Bearer · IMemoryCache · Swagger/Swashbuckle

---

## Layihə strukturu

```
LibraryApi.sln
├── LibraryApi.API               → Controller, Middleware, Program.cs
├── LibraryApi.Application       → Service interfeyslər, DTO, iş məntiqi
├── LibraryApi.Infrastructure    → EF Core, Repository, Cache, Email, BackgroundJob
└── LibraryApi.Domain            → Entity, Enum (heç bir asılılıq yoxdur)
```

---

## Həftə 2 — Autentifikasiya və Avtorizasiya

- **BCrypt hash:** Şifrələr `workFactor: 12` ilə hash-lənir, plain text saxlanmır
- **Register/Login:** `POST /api/auth/register`, `POST /api/auth/login` — uğurlu girişdə JWT token qaytarır
- **JWT middleware:** `AddAuthentication().AddJwtBearer()`, `ClockSkew = TimeSpan.Zero`
- **Rol əsaslı nəzarət:** oxuma — `[Authorize]`, yazma/silmə — `[Authorize(Roles="Admin")]`
- **Xəta cavabları:** token yoxdur/etibarsız → 401, rol uyğun deyil → 403

---

## Həftə 3 — Verilənlər Bazası Əlaqələri və Qabaqcıl Sorğular

- **One-to-Many:** `Author → Book`, `Member → Loan`
- **Many-to-Many:** `Book ↔ Category`, EF Core Fluent API ilə `BookCategories` join cədvəli
- **Dinamik axtarış:** `GET /api/books/search` — title, authorId, categoryId, yearFrom/yearTo ilə şərti LINQ `Where` zənciri
- **N+1 həlli:** `.Include()` / `.ThenInclude()` ilə eager loading
- **Tranzaksiya:** Loan yaradılması + Book statusu yenilənməsi `BeginTransactionAsync` / `CommitAsync` / `RollbackAsync` ilə atomik əməliyyat

---

## Həftə 4 — Cache, Fayl, Async Email, Scheduled Job

### Caching
`IMemoryCache` üzərindən `ICacheService` / `MemoryCacheService` abstraksiyas ilə.

- `GET /api/books` və `GET /api/books/{id}` — cavablar cache-ə yazılır
- TTL dəyərləri konfiqurasiyadan oxunur (`Cache:AbsoluteExpirationMinutes`, `Cache:SlidingExpirationMinutes`)
- Cache HIT / MISS loglanır

```csharp
public Task<IEnumerable<BookDto>> GetAllAsync()
{
    return _cache.GetOrCreateAsync("books:all", async () =>
    {
        var books = _bookRepository.Query().Select(b => new BookDto { ... });
        return (IEnumerable<BookDto>)await Task.FromResult(books.ToList());
    });
}
```

### Cache Invalidation
Hər yazma əməliyyatından sonra müvafiq cache key-lər silinir.

| Əməliyyat | Silinən key-lər |
|-----------|-----------------|
| Create    | `books:all` |
| Update    | `books:all`, `books:id:{id}` |
| Delete    | `books:all`, `books:id:{id}` |

```csharp
_cache.Remove("books:all");
_cache.Remove($"books:id:{id}");
```

### Fayl Upload / Download
- `POST /api/books/{id}/upload` — multipart/form-data, növ (.pdf .jpg .jpeg .png) və ölçü (5 MB) validasiyası
- `GET /api/books/{id}/download/{fileName}` — düzgün `Content-Type` ilə axın
- Fayl GUID adla saxlanılır, path traversal qorunması mövcuddur

```csharp
var uniqueName = $"{Guid.NewGuid()}{extension}";
var fullPath = Path.Combine(_uploadPath, uniqueName);
await using var stream = new FileStream(fullPath, FileMode.Create);
await file.CopyToAsync(stream);
```

### Asinxron Email Bildirişi
HTTP cavabı bloklanmadan arxa planda email göndərilir.

```csharp
_ = _emailService.SendLoanConfirmationAsync(
    member.Email, memberName, bookTitle, loanDate);

return MapToDto(withDetails!);
```

### Scheduled Cleanup Job
`BackgroundService` ilə hər 24 saatda bir gecikmiş borclar tapılır, email xatırlatması göndərilir.

```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken); 
    while (!stoppingToken.IsCancellationRequested)
    {
        await RunCleanupAsync(stoppingToken);
        await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
    }
}
```

### Konfiqurasiya Profilləri

| Parametr | Development | Production |
|----------|-------------|------------|
| Cache TTL | 5 dəq | 15 dəq |
| Maks fayl ölçüsü | 10 MB | 5 MB |
| İzin verilən növlər | .pdf .jpg .jpeg .png .txt | .pdf .jpg .jpeg .png |
| JWT müddəti | 120 dəq | 30 dəq |
| Log səviyyəsi | Debug | Warning |

---

## Quraşdırma və işə salma

```bash
dotnet restore
dotnet ef database update --project LibraryApi.Infrastructure --startup-project LibraryApi.API
dotnet run --project LibraryApi.API
```

Swagger UI: `http://localhost:{port}/swagger`

---

## Test axını

1. `POST /api/auth/register` — yeni user yarat (default rol: User)
2. `POST /api/auth/login` — JWT token al
3. Swagger-də **Authorize** düyməsinə basıb token-i daxil et (`Bearer` prefiksi olmadan)
4. `GET /api/books` — cache-dən cavab alır (ikinci sorğudan etibarən)
5. `POST /api/books` — Admin rolu tələb edir, cache-i invalidasiya edir
6. `POST /api/books/{id}/upload` — fayl yüklə (Admin)
7. `GET /api/books/{id}/download/{fileName}` — faylı geri yüklə
8. `POST /api/loans` — borc yarat, arxa planda email göndərilir

---

## Endpoint-lər

| Method | URL | Auth | Təsvir |
|--------|-----|------|--------|
| POST | `/api/auth/register` | — | Qeydiyyat |
| POST | `/api/auth/login` | — | Giriş, JWT token |
| GET | `/api/auth/me` | User | Cari istifadəçi |
| GET | `/api/books` | User | Kitab siyahısı (cached) |
| GET | `/api/books/{id}` | User | Kitab detalı (cached) |
| POST | `/api/books` | Admin | Kitab yarat |
| PUT | `/api/books/{id}` | Admin | Kitab yenilə |
| DELETE | `/api/books/{id}` | Admin | Kitab sil |
| POST | `/api/books/{id}/upload` | Admin | Fayl yüklə |
| GET | `/api/books/{id}/download/{fileName}` | User | Fayl yüklə |
| GET | `/api/authors` | User | Müəllif siyahısı |
| POST | `/api/authors` | Admin | Müəllif yarat |
| GET | `/api/members` | User | Üzv siyahısı |
| POST | `/api/members` | Admin | Üzv yarat |
| GET | `/api/loans` | User | Borc siyahısı |
| POST | `/api/loans` | Admin | Borc yarat (async email) |
| PUT | `/api/loans/{id}/return` | Admin | Geri qaytar |
