# eCommerce API

.NET 9 ile yazılmış, modüler monolit mimaride bir e-ticaret backend'i. Sipariş akışı RabbitMQ üzerinden event tabanlı bir saga ile yürür: stok rezervasyonu, cüzdan ile ödeme ve hata durumunda telafi (compensation) adımları birbirinden bağımsız consumer'lar tarafından işlenir.

## Teknolojiler

- .NET 9, ASP.NET Core Minimal API
- PostgreSQL 16, Entity Framework Core 9 (Npgsql)
- RabbitMQ, MassTransit 8
- JWT kimlik doğrulama, BCrypt ile parola hash'leme
- ImageSharp (ürün görseli işleme)
- Swagger / OpenAPI

## Hızlı Başlangıç

Gereksinimler: .NET 9 SDK, Docker.

```bash
# 1. PostgreSQL ve RabbitMQ'yu başlat
docker compose up -d

# 2. Ortam değişkenlerini hazırla
cd ECommerce.API
cp .env.example .env
# .env içindeki değerleri doldur

# 3. Uygulamayı çalıştır
dotnet run
```

Uygulama açılışta bekleyen migration'ları otomatik uygular ve `SuperAdmin` rolünü oluşturur. `.env` içinde `INITIAL_SUPERADMIN_EMAIL` ve `INITIAL_SUPERADMIN_PASSWORD` tanımlıysa ve sistemde henüz bir SuperAdmin yoksa bu bilgilerle ilk yönetici hesabı açılır.

| Servis | Adres |
|---|---|
| API | `http://localhost:5166` / `https://localhost:7223` |
| Swagger UI (Development) | `http://localhost:5166/swagger` |
| RabbitMQ yönetim paneli | `http://localhost:15672` (guest / guest) |
| PostgreSQL | `localhost:5432` (postgres / mysecretpassword) |

## Yapılandırma

`.env` dosyası `ECommerce.API` klasöründe okunur ve `appsettings.json` değerlerini ezer.

| Değişken | Açıklama |
|---|---|
| `INITIAL_SUPERADMIN_EMAIL` | İlk SuperAdmin hesabının e-posta adresi |
| `INITIAL_SUPERADMIN_PASSWORD` | İlk SuperAdmin hesabının parolası |
| `JwtOptions__SecretKey` | JWT imzalama anahtarı (en az 32 karakter) |
| `JwtOptions__Issuer` | JWT issuer |
| `JwtOptions__Audience` | JWT audience |

Veritabanı (`ConnectionStrings:DefaultConnection`) ve RabbitMQ (`RabbitMQ:*`) ayarları `appsettings.json` içindedir; varsayılan değerler `docker-compose.yml` ile uyumludur. Üretim ortamında `appsettings.json` içindeki JWT anahtarı ve veritabanı parolası mutlaka ortam değişkeniyle değiştirilmelidir.

CORS yalnızca `localhost:3000`, `5173`, `5174` ve `5175` adreslerine açıktır (`Program.cs`).

## Mimari

```
ECommerce.API/
├── Infrastructure/       Ortak altyapı: DbContext, endpoint kaydı, handler arayüzleri,
│                         loglama, yetki filtresi, dosya depolama
├── Modules/
│   ├── Auth/             Kayıt, giriş, profil, kullanıcı ve rol yönetimi
│   ├── Product/          Ürün, kategori, görsel, toplu işlemler, stok rezervasyonu
│   ├── Order/            Sipariş oluşturma, sorgulama, iade
│   ├── Payment/          Cüzdandan ödeme alma
│   ├── Finance/          Cüzdan, bakiye yükleme talepleri, bakiye düzeltme
│   ├── Dashboard/        Yönetim paneli metrikleri
│   ├── Audit/            Sistem logları ve istemci hata kaydı
│   └── Basket, Shipment, Reporting   (henüz boş iskelet)
└── Shared/Contracts/     Modüller arası mesaj ve event tanımları
```

Her özellik kendi klasöründe Command/Query, Handler ve Endpoint üçlüsü olarak durur (vertical slice). Endpoint'ler reflection ile otomatik kaydedilir:

- `IEndpoint` uygulayan sınıflar `/api` altına bağlanır.
- `IAdminEndpoint` uygulayan sınıflar `/api/admin` altına bağlanır ve kimlik doğrulama zorunludur.

Command ve query handler'lar loglama decorator'ı ile sarılır. Loglar bir kuyruğa yazılır ve arka plan servisi (`LogBackgroundWorker`) tarafından veritabanına aktarılır. `AuditInterceptor` entity değişikliklerini EF Core seviyesinde kayıt altına alır.

## Sipariş Akışı (Saga)

```
POST /api/orders
   │
   ▼
CreateOrderMessage ──► Order: siparişi oluşturur
   │
   ▼
OrderCreatedEvent ──► Product: stoğu atomik SQL update ile rezerve eder
   │                        (yetersizse StockReservationFailedEvent → sipariş iptal)
   ▼
StockReservedEvent ──► Payment: cüzdan bakiyesinden tahsil eder
   │
   ├── PaymentSucceededEvent ──► Order: onaylanır, Product: rezerve stok kesinleşir
   └── PaymentFailedEvent    ──► Order: iptal, Product: rezerve stok serbest bırakılır

POST /api/orders/{id}/refund
   └── OrderRefundedEvent ──► Finance: tutar cüzdana iade, Product: stok geri eklenir
```

- Stok rezervasyonu `Stock - ReservedStock >= Quantity` koşullu tek bir `UPDATE` ile yapılır; eşzamanlı siparişlerde fazla satış oluşmaz.
- Consumer'lar `ProcessedMessages` tablosu üzerinden idempotent çalışır; aynı mesaj iki kez işlenmez.
- Sipariş durumu `GET /api/orders/{id}/status` ile takip edilir.

## Yetkilendirme

Rol tabanlı ve izin bazlıdır. Her rol bir izin listesi taşır; kullanıcılara süreli geçici izin de atanabilir. `SuperAdmin` rolü `*` (wildcard) izniyle tüm işlemlere erişir. İzinler `PermissionEndpointFilter` ile endpoint seviyesinde kontrol edilir ve kullanıcı başına bellekte önbelleklenir.

| Grup | İzinler |
|---|---|
| Ürün | `product.create`, `product.update`, `product.delete`, `product.read_deleted` |
| Sipariş | `order.view_all`, `order.update`, `order.cancel` |
| Kullanıcı | `users.view`, `users.manage`, `users.delete` |
| Rol | `roles.view`, `roles.manage` |
| Log | `logs.view` |
| Finans | `finance.view`, `finance.manage` |

## Endpoint'ler

Tam liste ve istek şemaları için Swagger'a bakın. Korumalı endpoint'ler `Authorization: Bearer <token>` başlığı ister.

**Genel (`/api`)**

| Metot | Yol | Açıklama |
|---|---|---|
| POST | `/auth/register` | Kayıt |
| POST | `/auth/login` | Giriş, JWT döner |
| GET | `/products`, `/products/{id}` | Ürün listeleme (sayfalı) ve detay |
| POST | `/orders` | Sipariş oluşturma |
| GET | `/orders`, `/orders/{id}`, `/orders/{id}/status` | Sipariş sorgulama |
| POST | `/orders/{id}/refund` | Sipariş iadesi |
| GET | `/payments/order/{orderId}` | Siparişin ödeme bilgisi |
| POST | `/finance/deposit-requests` | Bakiye yükleme talebi |
| POST | `/logs/client` | İstemci tarafı hata kaydı |

**Yönetim (`/api/admin`)**

| Alan | Yollar |
|---|---|
| Profil | `GET/PUT /profile`, `POST /profile/change-password` |
| Ürün | `POST /products`, `PUT/DELETE /products/{id}` |
| Toplu işlem | `POST /products/bulk-delete`, `/products/bulk-update-price`, `/products/bulk-update-stock` |
| Görsel | `POST /products/{id}/images`, `PUT /products/{productId}/images/{imageId}/main`, `DELETE /products/{productId}/images/{imageId}` |
| Kategori | `GET/POST /categories`, `PUT/DELETE /categories/{id}` |
| Kullanıcı | `GET/POST /users`, `GET/DELETE /users/{id}`, `POST /users/{id}/restore`, `PUT /users/{id}/role`, `GET /users/{id}/customer-360` |
| Geçici izin | `POST /users/{userId}/permissions/temporary` |
| Rol | `GET/POST /roles`, `GET/PUT/DELETE /roles/{id}`, `GET /permissions` |
| Finans | `GET /finance/deposit-requests`, `POST /finance/deposit-requests/{id}/approve`, `/reject`, `POST /users/{userId}/wallet-adjustment` |
| Panel | `GET /dashboard/metrics` |
| Log | `GET /logs` |

Ürün görselleri `wwwroot` altına kaydedilir ve statik dosya olarak sunulur.

## Migration

Migration'lar `ECommerce.API/Infrastructure/Database/Migrations` altındadır ve açılışta otomatik uygulanır. Yeni migration eklemek için:

```bash
cd ECommerce.API
dotnet ef migrations add <Ad> -o Infrastructure/Database/Migrations
```
