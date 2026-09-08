# Changelog

Bu proje [Keep a Changelog](https://keepachangelog.com/tr/1.0.0/) ve
[SemVer](https://semver.org/spec/v2.0.0.html) biçimine uyar.

## [0.4.0] - 2026-09-08

### Değişti

- Uygulama adı "Pin Anything" iken **PinQuick** olarak yeniden adlandırıldı
  (proje, çözüm ve assembly adları dahil; eski isim koddan tamamen kaldırıldı)
- Kullanıcı verileri artık `%LOCALAPPDATA%\PinQuick\` altında tutuluyor
  (eski `%LOCALAPPDATA%\PinAnything\` dizinindeki veriler bu sürümde taşınmaz)

## [0.3.0] - 2026-09-08

### Eklendi

- Ayarlar iletişim kutusu (başlık çubuğundaki dişli simgesi): tema seçimi,
  dil seçimi ve "Windows ile başlat" seçeneği
- Kalıcı uygulama ayarları (`%LOCALAPPDATA%\PinQuick\settings.json`):
  tema ve dil tercihleri kaydedilir
- "Windows ile başlat" desteği (geçerli kullanıcının Run kayıt defteri anahtarı)
- Temel yerelleştirme (Türkçe/İngilizce) altyapısı: arayüz metinleri, filtre ve
  pin türü adları, durum mesajları, iletişim kutusu metinleri ve "Hakkında"
- Dil değiştirilince uygulamanın otomatik yeniden başlatılması

### Değişti

- Tema seçimi başlık çubuğundaki açılan kutudan "Ayarlar" dişli butonuna taşındı
- "Hakkında" ve durum mesajları artık seçili dile göre görüntülenir

## [0.2.0] - 2026-09-08

### Eklendi

- Pin hedefi için "Gözat" seçici (dereceli dosya/klasör türüne göre)
- Kartlarda çift tıklama ile açma
- Kartlarda favori (yıldız) işareti ve "Favoriler" filtresi
- Dışarıdan dosya/klasör sürükle-bırak ile hızlı pin ekleme
- Otomatik sistem ikonları (uygulama/dosya/klasör thumbnail'leri) ve ikon cache
- İkon kütüphanesi (dahili simge seçimi) ve özel ikon yükleme (.ico/.png/.exe)
- Ana sayfada yenileme (Ctrl+R): isim ve ikonları yeniden tara
- Kartları sürükle-bırak ile elle sıralama (`SortOrder` kalıcı)
- Kartlarda sağ tık menüsü: Aç, Yönetici olarak çalıştır, Konumu aç,
  Favorilere ekle/çıkar, Düzenle, Sil
- "Hakkında" iletişim kutusu ve sürüm bilgisi (assembly tabanlı)

### Değişti

- Sıralama artık kullanıcı kontrolünde; favori pinler otomatik üstte gruplanmaz
- Deterministik sıralama için yeni pinler `SortOrder` ile sona eklenir

## [0.1.0] - 2026-09-08

### Eklendi

- Çözüm yapısı: `PinQuick.Core`, `PinQuick.Storage`, `PinQuick.Windows`,
  `PinQuick.App` (WinUI 3 / Windows App SDK, MVVM), `PinQuick.Tests` (xUnit)
- Pin modeli ve tüm pin türleri (Application, File, Folder, Website, Url,
  Command, PowerShell, Batch, WindowsSetting, SystemTool, NetworkPath, Custom)
- SQLite veritabanı; migration/schema yönetimi, pin ve koleksiyon repository'leri
- Pin CRUD, duplicate detection, koleksiyon yönetimi
- Hedef validasyonu: URL, Windows URI, UNC yol, ortam değişkeni çözümleme
- `ProcessLauncher`: ShellExecute ile güvenli başlatma, `runas` ile UAC
  elevation, CMD/PowerShell/URI/explorer desteği
- JSON tabanlı import/export (sürüm 1)
- WinUI 3 ana arayüz: sidebar kategorileri, koleksiyonlar, pin grid kartları,
  detay paneli, arama, favoriler, tema seçimi (Sistem/Koyu/Açık)
- Klavye kısayolları (Ctrl+K, Ctrl+N, F2, Delete, Ctrl+E, Ctrl+I)
- MIT lisansı, README, .gitignore, CONTRIBUTING, SECURITY, CODE_OF_CONDUCT

### Güvenlik

- UAC atlatma girişimi yok; yönetici gerektiren işlemler standart `runas`
  mekanizmasıyla çalışır
- Komut/powerShell pinleri iddia edilen hedef üzerinden process argüman listesiyle
  başlatılır
- Telemetri yoktur; pin, yol, komut ve kullanım verisi cihaz dışına gönderilmez

### Bilinen Sınırlama

- Arayüz metinleri henüz localization (resw) sistemine taşınmadı; MVP tek dil
  (Türkçe) içerir