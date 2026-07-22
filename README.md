# 📚 Kitabxana API

**Repozitri:** `akhmedzade2202/DevJointWeek1`

## Ümumi Baxış

Sadə, **RESTful** API, kitabxananın kitab, müəllif və istifadəçi kolleksiyasını idarə etməyə imkan verir. API CRUD (Create, Read, Update, Delete) əməliyyatları, axtarış və kitabların götürülməsi/qaytarılması üçün son nöqtələr təqdim edir. Təmiz arxitektura prinsipləri ilə hazırlanıb, genişləndirilə və saxlanıla bilər.

## Xüsusiyyətlər

- 📖 **Kitablar** – Kitabların əlavə edilməsi, siyahıya alınması, yenilənməsi, silinməsi və axtarılması.
- 👩‍🏫 **Müəlliflər** – Müəllif məlumatlarının idarə edilməsi və kitabların müəlliflərlə əlaqələndirilməsi.
- 🙋‍♂️ **İstifadəçilər** – Kitabxana üzvlərinin qeydiyyatı və borc tarixi izlənməsi.
- 🔄 **Götür/Qaytar** – Kitabların götürülməsi və qaytarılması iş axını, mövcudluq avtomatik idarə olunur.
- 🛡️ **Doğrulama və Xəta İdarəetməsi** – Konsistent HTTP status kodları və təsvirli xəta mesajları.
- 🧪 **Testlər** – Unit və inteqrasiya testləri daxildir.

## Texnologiya Yığını

| Təbəqə | Texnologiya |
|-------|------------|
| **Dil** | C# (.NET 6) |
| **Veb Çərçivəsi** | ASP.NET Core Web API |
| **Verilənlər Bazası** | Entity Framework Core ilə SQLite (default) |
| **Asılılıq İnjektsiyası** | .NET‑in daxili DI |
| **Testlər** | xUnit + Moq |
| **Sənədləşdirmə** | Swagger / OpenAPI |

## Başlamaq

### Tələb olunanlar

- [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) və ya daha yuxarı versiya
- İstəyə bağlı, Visual Studio və ya VS Code kimi bir IDE

### Quraşdırma

```bash
# Repozitri klonlayın
git clone https://github.com/akhmedzade2202/DevJointWeek1.git
cd LibraryApi

# Asılılıqları bərpa edin
dotnet restore

# Migrasiyaları tətbiq edin və verilənlər bazasını (SQLite) doldurun
dotnet ef database update
```

### API‑ni İşə Salmaq

```bash
# İnkişaf rejimində işlədin
dotnet run --project src/LibraryApi
```

API `http://localhost:5000` ünvanında əlçatan olacaq. Swagger UI‑ni `http://localhost:5000/swagger` ünvanından interaktiv test üçün istifadə edə bilərsiniz.

## API Nöqtələri

| Metod | Marşrut | Təsvir |
|--------|-------|-------------|
| `GET` | `/api/books` | Bütün kitabların səhifələnmiş siyahısı |
| `GET` | `/api/books/{id}` | Tək kitabın detalları |
| `POST` | `/api/books` | Yeni kitab əlavə edin |
| `PUT` | `/api/books/{id}` | Mövcud kitabı yeniləyin |
| `DELETE` | `/api/books/{id}` | Kitabı silin |
| `POST` | `/api/books/{id}/borrow` | Kitabı bir istifadəçi üçün götürün |
| `POST` | `/api/books/{id}/return` | Götürülmüş kitabı qaytarın |
| `GET` | `/api/authors` | Müəlliflərin siyahısı |
| `POST` | `/api/authors` | Müəllif əlavə edin |
| `GET` | `/api/patrons` | Kitabxana istifadəçilərinin siyahısı |
| `POST` | `/api/patrons` | Yeni istifadəçi qeydiyyatı |

> **Qeyd:** Bütün sorğu/cevab gövdələri `Models` qovluğunda təyin olunub və Swagger‑də sənədləşdirilib.

## Testlər

```bash
# Bütün unit və inteqrasiya testlərini işlədin
dotnet test
```

## Töhfə Vermək

Töhfə vermək üçün:

1. Repozitorini forklayın.
2. Yeni xüsusiyyət budağı yaradın (`git checkout -b feature/awesome-feature`).
3. Təmiz, testləşdirilmiş kod yazın.
4. Bütün testlərin keçdiyinə əmin olun (`dotnet test`).
5. Dəyişikliklərinizi təsvir edən Pull Request göndərin.

## Lisenziya

Bu layihə **MIT Lisenziyası** ilə lisenziyalanıb – ətraflı məlumat üçün [LICENSE](LICENSE) faylını oxuyun.

---

*Kodlaşdırmanın zövqü!*