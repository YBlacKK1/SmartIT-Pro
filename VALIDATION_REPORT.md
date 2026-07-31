# Teslim Doğrulama Raporu

Paket hazırlanırken aşağıdaki statik kontroller uygulanmıştır:

- JSON dosyalarının parse edilmesi
- `.csproj`, `.props` ve NuGet XML yapılarının kontrolü
- Proje referanslarının ve çözüm yollarının varlık kontrolü
- Değiştirilen repository arayüzlerinin implementasyon/fake karşılıklarının kontrolü
- AutoMapper, SQL Server ve eski migration çağrılarının kaldırıldığının taranması
- Razor view, controller, statik dosya ve başlatma dosyalarının paket içinde bulunduğunun kontrolü
- ZIP bütünlük kontrolü ve SHA-256 oluşturulması

Bu çalışma ortamında .NET SDK çalıştırılamadığı için teslim sırasında gerçek `dotnet build` yürütülememiştir. Paket içindeki `VERIFY_PROJECT.bat`, kullanıcının .NET 8 kurulu Windows bilgisayarında restore, Release build ve testleri tek adımda çalıştırmak için eklenmiştir.
