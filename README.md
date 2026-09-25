## 🔐 İlk Kurulum: SuperAdmin Hesabının Tanımlanması (Bootstrapping)

Sistem sıfır güven (Zero-Trust) mimarisiyle tasarlanmıştır; veritabanında veya kaynak kodda varsayılan/sabit bir şifre bulunmaz. Veritabanı ilk kez ayağa kalktığında sistem otomatik olarak tek bir **SuperAdmin** hesabı oluşturur.

Uygulamanın bu hesabı oluşturabilmesi için `INITIAL_ADMIN_EMAIL` ve `INITIAL_ADMIN_PASSWORD` değerlerinin çalışma ortamına tanımlanması gerekir.

---

### Seçenek 1: Yerel Geliştirme Ortamı (.NET User Secrets)

Geliştirme yaparken şifrelerin kaynak koda ve Git reposuna kazayla sızmasını engellemek için .NET SDK'nın sağladığı **User Secrets** mekanizması kullanılır. Bu yöntem Windows, macOS ve Linux üzerinde çalışır ve verileri proje dizini dışındaki yerel kullanıcı profilinde saklar.

**1. Proje dizinine gidin (`ECommerce.API`):**
```bash
cd ECommerce.API
```

**2. User Secrets'ı başlatın ve bilgileri tanımlayın:**
```bash
dotnet user-secrets init
dotnet user-secrets set "INITIAL_ADMIN_EMAIL" "superadmin@ecommerce.com"
dotnet user-secrets set "INITIAL_ADMIN_PASSWORD" "SuperSecretPassword123!"
```

**3. Uygulamayı çalıştırın:**
```bash
dotnet run
```

---

### Seçenek 2: Sunucu ve Konteyner Ortamları (Environment Variables)

Canlı sunucularda (Production/Staging), CI/CD süreçlerinde veya Docker üzerinde `dotnet user-secrets` kullanılmaz; değerler doğrudan işletim sistemi ortam değişkenleri (Environment Variables) üzerinden enjekte edilir.

#### A. Docker / Docker Compose
`docker-compose.yml` içerisindeki API servis tanımının `environment` bloğuna ekleyin:
```yaml
services:
  api:
    image: ecommerce-api:latest
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - INITIAL_ADMIN_EMAIL=superadmin@ecommerce.com
      - INITIAL_ADMIN_PASSWORD=SuperSecretPassword123!
```

#### B. Linux Sunucu (Bash / Systemd)
```bash
export INITIAL_ADMIN_EMAIL="superadmin@ecommerce.com"
export INITIAL_ADMIN_PASSWORD="SuperSecretPassword123!"

dotnet ECommerce.API.dll
```

#### C. Windows Sunucu (PowerShell)
```powershell
$env:INITIAL_ADMIN_EMAIL="superadmin@ecommerce.com"
$env:INITIAL_ADMIN_PASSWORD="SuperSecretPassword123!"

dotnet .\ECommerce.API.dll
```

---

### ⚙️ Çalışma Kuralları ve Güvenlik
- **Idempotency (Tek Seferlik Çalışma):** `DbInitializer`, veritabanında `SuperAdmin` rolüne sahip bir kayıt gördüğü anda bu adımı atlar. Mevcut kullanıcının şifresini ezmez veya değiştirmez.
- **Kök Yetki (Wildcard):** İlk oluşturulan SuperAdmin kullanıcısı doğuştan `*` yetkisine sahiptir; sistemdeki tüm operasyonel kısıtlamaları bypass eder.
