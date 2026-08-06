# LibraryApi — .NET 8 / ASP.NET Core Web API

## Layihə haqqında

Library Management System — CodeAcademy tapşırığının Spring Boot original versiyasının .NET stack-ə uyğunlaşdırılmış həyata keçirilməsi. Clean Architecture prinsipləri əsasında qurulub, JWT əsaslı autentifikasiya, rol əsaslı avtorizasiya və relational data əlaqələri ilə tam funksional REST API.

**Stack:** .NET 8, ASP.NET Core Web API, Entity Framework Core, SQL Server, BCrypt, JWT Bearer, Swagger/Swashbuckle, xUnit

**Layihə strukturu:**
---

## Həftə 2 — Autentifikasiya və Avtorizasiya (JWT + ASP.NET Core Identity ekvivalenti)

- **User entity + hash-lənmə:** Şifrələr BCrypt ilə hash-lənir, plain text saxlanmır
- **Register/Login:** `POST /api/auth/register`, `POST /api/auth/login` — uğurlu girişdə JWT token qaytarır
- **JWT filter chain:** `AddAuthentication().AddJwtBearer()` ilə stateless authentication, `UseAuthentication → UseAuthorization → MapControllers` düzgün middleware sırası
- **Rol əsaslı giriş nəzarəti:** JWT-də `role` claim-i, oxuma əməliyyatları hər login olmuş user üçün (`[Authorize]`), yazma/silmə yalnız Admin üçün (`[Authorize(Roles="Admin")]`)
- **Auth xətaları:** token yoxdur/etibarsızdır → 401, rol uyğun deyil → 403
- **Token expiry:** `Jwt:ExpiryMinutes` konfiqurasiyası, `ValidateLifetime=true`, `ClockSkew=TimeSpan.Zero`

## Həftə 3 — Verilənlər Bazası Əlaqələri və Qabaqcıl Sorğular

- **One-to-Many:** `Author → Book`, `Member → Loan` (mövcud strukturda)
- **Many-to-Many:** yeni `Book ↔ Category` əlaqəsi, EF Core Fluent API ilə join cədvəli (`BookCategories`) konfiqurasiyası
- **Derived query / LINQ sorğular:** `GET /api/books/by-year-range` — il aralığı və müəllif üzrə filtrasiya
- **Dinamik axtarış endpoint-i:** `GET /api/books/search` — title, authorId, categoryId, yearFrom/yearTo parametrləri ilə şərti (dynamic) LINQ `Where` zənciri
- **Tranzaksiya idarəetməsi:** kitab checkout əməliyyatı (`Loan` yaradılması + `Book.IsAvailable` yenilənməsi) EF Core `BeginTransactionAsync`/`CommitAsync`/`RollbackAsync` ilə bir bütöv əməliyyat kimi işləyir
- **N+1 problem həlli:** `.Include()`/`.ThenInclude()` istifadə edilərək əlaqəli data (Author, Categories) tək SQL sorğusu ilə çəkilir
- **Rollback testi:** xUnit + EF Core In-Memory provider ilə tranzaksiya uğursuz olduqda heç bir dəyişikliyin (Loan yaranması, Book statusu) tətbiq olunmadığını təsdiqləyən unit test

---

## Quraşdırma və işə salma

```bash
dotnet restore
dotnet ef database update --project LibraryApi.Infrastructure --startup-project LibraryApi.API
dotnet run --project LibraryApi.API
```

Swagger UI: `http://localhost:{port}/swagger`

`appsettings.json` konfiqurasiyası:
```json
"Jwt": {
  "Key": "...",
  "Issuer": "LibraryApi",
  "Audience": "LibraryApiUsers",
  "ExpiryMinutes": 60
},
"ConnectionStrings": {
  "DefaultConnection": "..."
}
```

## Test axını

1. `POST /api/auth/register` — yeni user yarat (default rol: User)
2. `POST /api/auth/login` — token al
3. Swagger-də "Authorize" düyməsi ilə token-i əlavə et (`Bearer` prefiksi olmadan)
4. `POST /api/authors`, `POST /api/books` — Admin rolu tələb edir
5. `GET /api/books/search?title=&categoryId=&yearFrom=&yearTo=` — dinamik axtarış
6. `POST /api/loans/checkout?bookId=&memberId=` — tranzaksiyalı checkout əməliyyatı

## Testlərin işə salınması

```bash
dotnet test
```
