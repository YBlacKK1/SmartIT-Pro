# SmartIT Pro v1.0 Yerel Kurulum

## Gereksinim

- Windows 10 veya Windows 11
- .NET 8 SDK (`dotnet --version` çıktısı `8.0.x` olmalı)
- İnternet bağlantısı (ilk NuGet restore ve CDN dosyaları için)

## Önerilen yöntem

1. ZIP'i sağ tıklayıp **Tümünü Ayıkla** ile çıkarın.
2. Çıkan ana klasörü açın.
3. `SMARTIT_SITEYI_AC.bat` dosyasına çift tıklayın.
4. İlk çalıştırmada yerel yönetici e-postanızı ve parolanızı belirleyin.
5. Siyah terminal penceresini kapatmayın.
6. Tarayıcı açılınca belirlediğiniz bilgilerle giriş yapın.

```text
Adres:   http://localhost:5101
```

Yönetici parolası `.NET User Secrets` alanında saklanır ve proje dosyalarına yazılmaz. Hesabı yeniden yapılandırmak için `SETUP_LOCAL_ADMIN.bat` dosyasını çalıştırın.

İlk açılışta `SmartIT.Web/smartit-v1.db` otomatik oluşur ve örnek veriler eklenir.

## VS Code ile çalıştırma

Ana proje klasöründe terminal açın:

```powershell
dotnet restore SmartIT.sln
SETUP_LOCAL_ADMIN.bat
dotnet run --project .\SmartIT.Web\SmartIT.Web.csproj --urls http://localhost:5101
```

## Visual Studio ile çalıştırma

1. `SmartIT.sln` dosyasını açın.
2. `SmartIT.Web` projesine sağ tıklayın.
3. **Set as Startup Project** seçin.
4. Üstten HTTP profilini seçip Start'a basın.

## Veritabanını sıfırlama

Önce çalışan terminali `Ctrl+C` ile durdurun. Ardından `RESET_LOCAL_DATABASE.bat` dosyasını çalıştırın. Sonraki açılışta veritabanı ve demo verileri yeniden üretilir.

## Doğrulama

`VERIFY_PROJECT.bat` şu işlemleri yapar:

1. `dotnet restore`
2. Release build
3. xUnit testleri

Bir hata oluşursa terminalde görünen ilk kırmızı hata, asıl çözülmesi gereken hatadır.
