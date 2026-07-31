# Değişiklik Geçmişi

## [v1.0.1] - 2026-07-31

### Eklendi

- Azure App Service canlı dağıtımı
- GitHub Actions ile otomatik restore, build ve test süreci
- Responsive yönetim paneli
- Dark/light tema ve mobil menü
- KPI kartları ve Chart.js grafikleri
- SignalR gerçek zamanlı bildirim altyapısı
- CSV, Excel ve PDF rapor merkezi
- Varlıklar için QR kod desteği
- Güvenli yerel yönetici kurulum sistemi

### Geliştirildi

- Çalışan, varlık, atama ve destek talebi iş akışları
- Ticket detay ve durum yönetimi
- Varlık atama ve iade işlemleri
- Çalışan profil ve departman düzenleme işlemleri
- Lisans, bakım ve audit log sayfaları
- Cookie güvenliği, giriş kilitleme ve rol kontrolleri

### Düzeltildi

- GitHub Actions test projesi NuGet restore sorunu
- Azure ZIP dağıtım ve Linux uyumluluk sorunları
- Yönetici hesabının güvenli biçimde oluşturulması
- Yerel ve Azure ortam yapılandırma farklılıkları



# Changelog

## v1.0.1 — Azure ve GitHub Güncellemesi

### Dağıtım
- Azure App Service için kalıcı SQLite ve log yolları eklendi.
- Linux Kudu ile uyumlu, ileri eğik çizgili ZIP paketleme aracı eklendi.
- `WEBSITE_RUN_FROM_PACKAGE` ve build kapatma ayarları dağıtım rehberine eklendi.

### Güvenlik
- Sabit geliştirme parolaları ve JWT anahtarı kaynak koddan kaldırıldı.
- İlk yerel çalıştırma için .NET User Secrets tabanlı güvenli yönetici kurulumu eklendi.
- Yayın profili, veritabanı, log ve üretilen dağıtım dosyaları `.gitignore` kapsamına alındı.

### Dokümantasyon
- Canlı Azure uygulaması README'ye eklendi.
- Yerel kurulum ve Azure güncelleme adımları yenilendi.

## v1.0.0 — Foundation Update

### Kullanıcı arayüzü
- İlk SmartIT Pro akışı korunarak tüm MVC paneli yeniden tasarlandı.
- Kurumsal sidebar, topbar, responsive mobil navigasyon ve dark mode eklendi.
- Dashboard KPI kartları, varlık dağılımı ve ticket durum grafikleri eklendi.
- Liste, form, detay, durum ve boş ekran tasarımları ortak tasarım sistemine taşındı.

### Help Desk
- Requester bilgisi liste ve detay ekranlarına eklendi.
- Admin ticket durum güncelleme akışı eklendi.
- Ticket değişiklikleri audit log ve SignalR bildirimleriyle ilişkilendirildi.

### Varlık yönetimi
- Düzenleme ekranındaki eksik ID problemi giderildi.
- Varlık detay sayfası, aktif atama, iade geçmişi ve QR erişimi eklendi.
- Atama ve iade işlemleri audit log'a bağlandı.

### Çalışan yönetimi
- Departman seçimi ve profil fotoğrafı değişimi geliştirildi.
- Dosya boyutu, MIME türü ve uzantı doğrulaması eklendi.
- Aktif cihaz ataması bulunan çalışanların yanlışlıkla silinmesi engellendi.

### Operasyon ve raporlar
- Lisans kullanım, bakım takvimi ve audit sayfaları yenilendi.
- CSV, biçimlendirilmiş Excel ve PDF varlık raporları eklendi.
- QR kodlar doğrudan varlık detay sayfasına bağlandı.

### Yerel çalışma
- SQL Server/LocalDB gereksinimi kaldırıldı; SQLite eklendi.
- İlk çalıştırmada veritabanı, roller, yönetici ve demo verileri otomatik oluşturulur.
- `START_SMARTIT.bat`, `VERIFY_PROJECT.bat` ve veritabanı sıfırlama aracı eklendi.

### Güvenlik ve teknik iyileştirmeler
- Güvenli dönüş URL kontrolü, giriş kilitleme ve rol bazlı yetkilendirme güçlendirildi.
- Cookie, güvenlik başlıkları, antiforgery ve üretim hata akışı düzenlendi.
- AutoMapper kaldırılarak açık ve izlenebilir manuel eşlemeye geçildi.
- API için JWT token üretim endpoint'i eklendi.
