# SmartIT Pro v1.0 — Mevcut Azure Sitesini Güncelleme

Bu işlem yeni bir site açmaz. Hazırlanan paket, mevcut SmartIT Pro Azure Web App uygulamasının üzerine yayımlanır ve canlı bağlantı değişmez.

## 1. Yayın paketini hazırla

Ana klasördeki `PUBLISH_AZURE.bat` dosyasına çift tıkla. İşlem tamamlandığında aynı klasörde `SmartIT-Pro-v1.0-Azure.zip` oluşur.

## 2. Azure ayarlarını kontrol et

Azure Portal'da mevcut SmartIT Pro Web App uygulamasını aç. **Ayarlar > Ortam değişkenleri** bölümünde aşağıdaki değerlerin bulunduğundan emin ol:

```text
Seed__AdminEmail          admin@smartit.local
Seed__AdminPassword       Güçlü ve yalnızca senin bildiğin bir parola
Seed__AdminDisplayName    Yusuf Gürgen
WEBSITE_RUN_FROM_PACKAGE  1
SCM_DO_BUILD_DURING_DEPLOYMENT false
```

`Seed__AdminPassword` değerini kaynak koda veya GitHub'a yazma. Bu değer yalnızca Azure ortam değişkenlerinde tutulmalıdır.

Bu sürüm Azure App Service üzerinde çalıştığını otomatik algılar. SQLite veritabanını kalıcı olan `%HOME%/data/smartit-v1.db` konumunda, logları ise `%HOME%/LogFiles/SmartIT` altında saklar.

## 3. Yayın kimlik doğrulamasını geçici olarak aç

ZIP dağıtımı için gerekiyorsa **Yapılandırma > Genel ayarlar** bölümünde **SCM Basic Auth Publishing Credentials** ayarını geçici olarak aç ve Web App'i yeniden başlat. `.PublishSettings` dosyasını GitHub'a ekleme veya başkasıyla paylaşma.

## 4. Mevcut siteye yükle

Azure Portal'da mevcut Web App'i seçtikten sonra **Geliştirme Araçları > Gelişmiş Araçlar > Git** seçeneğiyle Kudu panelini aç. Kudu ana adresinin sonuna `/ZipDeployUI` ekleyerek ZIP yükleme ekranına gir ve `SmartIT-Pro-v1.0-Azure.zip` dosyasını yükle.

Yeni bir Web App seçme veya oluşturma. Mevcut SmartIT Pro uygulamasına yükleme yap.

## 5. Yeniden başlat ve kontrol et

Web App'i bir kez yeniden başlat. Ardından mevcut canlı bağlantıyı aç ve şu kontrolleri yap:

1. Giriş sayfası açılıyor.
2. Yönetici hesabıyla giriş yapılabiliyor.
3. Dashboard, çalışanlar, varlıklar ve talepler sayfaları açılıyor.
4. Yeni bir test kaydı oluşturulabiliyor.

İlk açılış, yeni dosyalar ve veritabanı hazırlanırken normalden biraz uzun sürebilir.

## 6. Dağıtımdan sonra kapat

Dağıtım tamamlanınca **SCM Basic Auth Publishing Credentials** ayarını tekrar kapat. İndirdiğin `.PublishSettings` dosyasını bilgisayarından sil.
