# SmartIT Pro v1.0.1 — Foundation Update

SmartIT Pro; çalışanları, BT varlıklarını, cihaz atamalarını ve destek taleplerini tek panelden yönetmek için geliştirilmiş ASP.NET Core tabanlı bir IT operasyon sistemidir.

Bu sürüm, ilk SmartIT Pro projesinin yapısını ve temel iş akışlarını korur. Ayrı bir Swagger sitesi değildir. İlk projenin doğrudan devamı olan **v1.0 Foundation Update** sürümüdür.

## Canlı uygulama

[SmartIT Pro'yu Azure üzerinde aç](https://smartitpro-web-26-cbb6dudma8h3g2ak.westeurope-01.azurewebsites.net/)

Canlı uygulamanın yönetici bilgileri kaynak kodda tutulmaz. Erişim bilgileri yalnızca Azure App Service ortam değişkenlerinden yönetilir.

## Hızlı başlangıç

1. ZIP dosyasını tamamen klasöre çıkarın.
2. Ana klasördeki `SMARTIT_SITEYI_AC.bat` dosyasına çift tıklayın.
3. İlk çalıştırmada açılan güvenli kurulum ekranında yerel yönetici e-postanızı ve parolanızı belirleyin.
4. NuGet paketlerinin indirilmesini bekleyin.
5. Tarayıcı otomatik olarak `http://localhost:5101` adresini açar.

Yerel parola, .NET User Secrets alanında saklanır; `appsettings` dosyalarına veya Git deposuna yazılmaz.

## v1.0 ile gelenler

- İlk sürümün çalışan, varlık, atama ve ticket akışları korundu.
- Baştan tasarlanmış responsive yönetim paneli eklendi.
- Dark/light tema ve mobil yan menü eklendi.
- KPI kartları ve Chart.js dashboard grafikleri eklendi.
- Ticket detay ve durum yönetimi geliştirildi.
- Varlık detay, QR kod, atama ve iade akışları birleştirildi.
- Çalışan düzenlemede departman ve profil fotoğrafı güncellemesi eklendi.
- Lisans, bakım ve audit log sayfaları yenilendi.
- CSV, Excel ve PDF rapor merkezi eklendi.
- SignalR ile gerçek zamanlı bildirim altyapısı eklendi.
- SQLite ile sıfır kurulumlu yerel veritabanı oluşturuldu.
- Güvenli cookie, giriş kilitleme, rol kontrolü ve form korumaları güçlendirildi.
- API için JWT token endpoint'i geliştirici aracı olarak korundu.

## Teknolojiler

- .NET 8 / ASP.NET Core MVC
- Entity Framework Core + SQLite
- ASP.NET Core Identity
- Clean Architecture katmanları
- MediatR + FluentValidation
- SignalR
- ClosedXML, QuestPDF, QRCoder
- Bootstrap 5 + özel SmartIT Pro tasarım sistemi
- xUnit

## Proje yapısı

```text
SmartIT.Domain          Temel entity ve enumlar
SmartIT.Application     Use-case, DTO, doğrulama ve MediatR akışları
SmartIT.Infrastructure  EF Core, Identity, repository ve demo veri kurulumu
SmartIT.Web             Kullanıcıya gösterilen MVC paneli (port 5101)
SmartIT.API             Geliştirici API'si ve Swagger (port 5201)
SmartIT.Tests           Handler ve repository testleri
```

## Yardımcı dosyalar

- `SMARTIT_SITEYI_AC.bat`: Ana siteyi açmak için kullanacağınız dosyadır.
- `START_SMARTIT.bat`: Web panelini restore, build ve run adımlarıyla hazırlar.
- `SETUP_LOCAL_ADMIN.bat`: Yerel yönetici hesabını güvenli biçimde yapılandırır.
- `VERIFY_PROJECT.bat`: Restore, Release build ve testleri çalıştırır.
- `PUBLISH_AZURE.bat`: Release doğrulaması yapar ve mevcut Azure sitesine yüklenecek ZIP paketini hazırlar.
- `CREATE_AZURE_ZIP.ps1`: Linux tabanlı Azure App Service ile uyumlu ZIP girişleri üretir.
- `AZURE_UPDATE_GUIDE.md`: Canlı Azure Web App sürümünü güncelleme adımlarını açıklar.
- `RESET_LOCAL_DATABASE.bat`: Yerel SQLite verisini silip temiz başlangıç sağlar.
- `developer-tools/START_API.bat`: Yalnızca API/Swagger geliştirmesi için kullanılır.

Detaylı kurulum için `INSTALLATION.md`, değişiklikler için `CHANGELOG.md` dosyasına bakın.

## Güvenlik notu

- Parola, JWT anahtarı, Azure yayın profili ve veritabanı dosyaları GitHub'a eklenmez.
- `*.PublishSettings`, `*.db`, `azure-publish/` ve üretilen Azure ZIP'i `.gitignore` kapsamındadır.
- Azure dağıtımından sonra **SCM Basic Auth Publishing Credentials** ayarını kapatın.
