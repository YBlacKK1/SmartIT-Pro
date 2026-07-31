# SmartIT Pro v1.0.1 — Foundation Update

[![Build and test](https://github.com/yusufgurgen/SmartIT-Pro/actions/workflows/ci.yml/badge.svg)](https://github.com/yusufgurgen/SmartIT-Pro/actions/workflows/ci.yml)

SmartIT Pro; çalışanları, BT varlıklarını, cihaz atamalarını ve destek taleplerini tek panel üzerinden yönetmek amacıyla geliştirilmiş ASP.NET Core tabanlı bir IT operasyon sistemidir.

Bu sürüm, ilk SmartIT Pro projesinin yapısını ve temel iş akışlarını koruyan doğrudan devam sürümüdür. Ayrı bir Swagger projesi değildir. Kullanıcıların kullanacağı ana uygulama `SmartIT.Web`, geliştirici API’si ise `SmartIT.API` projesidir.

## Canlı uygulama

[SmartIT Pro’yu Azure üzerinde aç](https://smartitpro-web-26-cbb6dudma8h3g2ak.westeurope-01.azurewebsites.net/)

Canlı uygulamanın yönetici bilgileri kaynak kod içerisinde tutulmaz. Erişim bilgileri yalnızca Azure App Service ortam değişkenleri üzerinden yönetilir.

## Hızlı başlangıç

1. ZIP dosyasını tamamen bir klasöre çıkarın.
2. Ana klasördeki `SMARTIT_SITEYI_AC.bat` dosyasına çift tıklayın.
3. İlk çalıştırmada açılan güvenli kurulum ekranında yerel yönetici e-postanızı ve parolanızı belirleyin.
4. NuGet paketlerinin indirilmesini ve projenin hazırlanmasını bekleyin.
5. Tarayıcı otomatik olarak `http://localhost:5101` adresini açacaktır.

Yerel yönetici parolası .NET User Secrets alanında saklanır; `appsettings` dosyalarına veya Git deposuna yazılmaz.

## v1.0.1 ile gelen yenilikler

* İlk sürümün çalışan, varlık, atama ve destek talebi akışları korundu.
* Responsive yönetim paneli yenilendi.
* Dark/light tema ve mobil yan menü eklendi.
* KPI kartları ve Chart.js dashboard grafikleri eklendi.
* Ticket detay ve durum yönetimi geliştirildi.
* Varlık detay, QR kod, atama ve iade işlemleri birleştirildi.
* Çalışan düzenleme ekranına departman ve profil fotoğrafı güncellemesi eklendi.
* Lisans, bakım ve audit log sayfaları yenilendi.
* CSV, Excel ve PDF rapor merkezi eklendi.
* SignalR ile gerçek zamanlı bildirim altyapısı eklendi.
* SQLite ile sıfır kurulumlu yerel veritabanı desteği sağlandı.
* Güvenli cookie, giriş kilitleme, rol kontrolü ve form korumaları güçlendirildi.
* API için JWT token endpoint’i geliştirici aracı olarak korundu.
* Azure App Service dağıtım desteği hazırlandı.
* GitHub Actions üzerinden otomatik restore, build ve test kontrolü eklendi.
* Test projesinin NuGet restore süreci düzeltilerek CI doğrulaması başarıyla tamamlandı.

## Kullanılan teknolojiler

* .NET 8
* ASP.NET Core MVC
* Entity Framework Core
* SQLite
* SQL Server
* ASP.NET Core Identity
* Clean Architecture
* MediatR
* FluentValidation
* SignalR
* Chart.js
* ClosedXML
* QuestPDF
* QRCoder
* Bootstrap 5
* xUnit
* Azure App Service
* GitHub Actions

## Proje yapısı

```text
SmartIT.Domain          Temel entity ve enumlar
SmartIT.Application     Use-case, DTO, doğrulama ve MediatR akışları
SmartIT.Infrastructure  EF Core, Identity, repository ve veri kurulumu
SmartIT.Web             Ana MVC yönetim paneli (port 5101)
SmartIT.API             Geliştirici API’si ve Swagger (port 5201)
SmartIT.Tests           Handler ve repository testleri
```

## Yardımcı dosyalar

* `SMARTIT_SITEYI_AC.bat`: Ana siteyi yerel ortamda başlatır.
* `START_SMARTIT.bat`: Web panelini restore, build ve run adımlarıyla hazırlar.
* `SETUP_LOCAL_ADMIN.bat`: Yerel yönetici hesabını güvenli biçimde yapılandırır.
* `VERIFY_PROJECT.bat`: Restore, Release build ve test işlemlerini çalıştırır.
* `PUBLISH_AZURE.bat`: Release doğrulaması yaparak Azure ZIP paketini hazırlar.
* `CREATE_AZURE_ZIP.ps1`: Linux tabanlı Azure App Service ile uyumlu ZIP paketi oluşturur.
* `AZURE_UPDATE_GUIDE.md`: Mevcut Azure Web App sürümünü güncelleme adımlarını açıklar.
* `RESET_LOCAL_DATABASE.bat`: Yerel SQLite verisini silerek temiz başlangıç sağlar.
* `developer-tools/START_API.bat`: API ve Swagger geliştirme ortamını başlatır.

Detaylı kurulum için `INSTALLATION.md`, sürüm değişiklikleri için `CHANGELOG.md` dosyasına bakabilirsiniz.

## Güvenlik

* Parolalar, JWT anahtarları, Azure yayın profilleri ve veritabanı dosyaları GitHub’a yüklenmez.
* `*.PublishSettings`, `*.db`, `azure-publish/` ve oluşturulan Azure ZIP paketleri `.gitignore` kapsamındadır.
* Yönetici bilgileri Azure App Service ortam değişkenleri üzerinden yönetilir.
* Azure dağıtımı tamamlandıktan sonra `SCM Basic Auth Publishing Credentials` ayarının kapatılması önerilir.

## CI/CD durumu

Her `main` dalı güncellemesinde GitHub Actions otomatik olarak:

1. NuGet paketlerini geri yükler.
2. Projeyi Release modunda derler.
3. xUnit testlerini çalıştırır.

Son `v1.0.1` build ve test kontrolleri başarıyla tamamlanmıştır.
