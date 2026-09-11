# Changelog

Bu proje [Keep a Changelog](https://keepachangelog.com/tr/1.0.0/) ve
[SemVer](https://semver.org/spec/v2.0.0.html) biçimine uyar.

## [0.8.1] - 2026-09-11

### Değişti

- Ayarlar iletişim kutusu genişletildi ve içeriği kaydırılabilir yapıldı:
  düşük çözünürlüklerde veya dar pencerelerde menü artık ekrana sığıyor
  (içerik gerekirse kaydırılıyor).

## [0.8.0] - 2026-09-11

### Eklendi

- Global kısayol kombinasyonu artık kullanıcı tarafından atanabiliyor:
  Ayarlar'daki kısayol kutusuna tıklayıp istenen kombinasyona basılınca
  (ör. `Ctrl+Alt+P`) kaydedilir ve pencere göster/gizle için kullanılır.
  Değiştirici tuş kontrolü yapılır; kombinasyon başka bir uygulamada
  kullanılıyorsa uyarı gösterilir.

### Düzeltildi

- Uygulama global kısayol veya sistem tepsisi ile çağrıldığında pencere
  artık diğer pencerelerin üzerinde öne getiriliyor.

## [0.7.0] - 2026-09-10

### Eklendi

- Koleksiyona ait pinlerin kartlarında koleksiyon adı ve rengi gösterilir:
  kartın altında renkli nokta + koleksiyon adı (koleksiyonu olmayan pinlerde gizli).

### Değişti

- İkinci örnek çalıştırma uyarısındaki "Açık pencere öne getirildi." ifadesi
  kaldırıldı; artık yalnızca "Uygulama zaten çalışıyor." gösteriliyor.

### Düzeltildi

- Sürükle-bırak ile pin ekleme geri kazanıldı ve daha güvenilir hale getirildi:
  bırakma işleminde async veri okuma tamamlanana kadar işlem erteleniyor.

## [0.6.0] - 2026-09-10

### Eklendi

- Koleksiyonlara renk seçimi: koleksiyon iletişim kutusunda hazır renk kutusu
  ve renk seçici bulunur; seçilen renk koleksiyon listesinde nokta olarak gösterilir.

### Düzeltildi

- Bir koleksiyon seçiliyken "Tüm pinler" filtresine tıklanınca liste koleksiyonda
  takılı kalıyordu; artık herhangi bir filtre seçiminde (filtre zaten seçili olsa bile)
  koleksiyon filtresi temizleniyor.
- Ctrl+Click ile çoklu pin seçimi, kart tıklamaları sırasında tek seçime daralıyordu;
  toplu "koleksiyona ekle" ve toplu "sil" işlemleri artık güvenilir şekilde çalışıyor.
- Uygulama zaten açıkken yeniden başlatıldığında artık "zaten açık" uyarısı gösteriliyor,
  açık pencere öne getiriliyor ve ikinci örnek kapanıyor.

## [0.5.0] - 2026-09-10

### Eklendi

- Sistem tepsisi desteği: pencere kapatıldığında/minimize edilirken tepsiye küçültme,
  tepsi menüsünden gösterme/çıkış ve uygulama ikonu
- Global kısayol: ayardan etkinleştirilince `Ctrl+Space` ile pencere gösterilir/gizlenir
- "Son Kullanılanlar" filtresi (son açılış zamanına göre sıralı)
- "Bozuk Pinler" filtresi ve kartlarda erişilemeyen hedefler için uyarı rozeti
- Kart boyutu seçimi (Küçük / Orta / Büyük): başlık çubuğundaki simge butondan
  anında değiştirilir
- Veri yedekleme/geri yükleme (ZIP): `%LOCALAPPDATA%\PinQuick\Backup\`
  ve menüden "Yedek Al / Yedekten Geri Yükle"
- Sağ tık menüsüne "Koleksiyona Ekle" alt menüsü ve seçili koleksiyonu
  yeniden adlandırma butonu (sesli)
- İlk açılışta otomatik başlayan, menüden veya ayarlardan "Eğitimi Göster" ile
  yeniden çağrılabilen adım adım arayüz eğitimi (teaching tips)
- Detay panelinde pinin son kullanım zamanı
- Başlık çubuğunda tek tıkla tema değiştirici (Koyu / Açık / Sistem)
- Çoklu pin seçimi (Ctrl+Click) ve seçilenlere toplu "Koleksiyona Ekle" /
  toplu silme işlemleri
- Pin kartlarını sürükleyip bırakarak doğrudan koleksiyona ekleme
- Sidebar'da son kullanılan pinlerin hızlı erişim listesi
- Arama geçmişi: arama kutusu odaklanınca son aramalar önerisi ve
  geçmişi temizleme
- Dışa aktarma öncesi "Tüm pinler / Mevcut filtre" seçimi
- Kart açıklamalarında Markdown düz metin görünümü
- Otomatik veri yedekleme (Yok / Günlük / Haftalık) ayarı
- Eğitim turunda ve metin kutusunda Escape ile kapatma
- "Hakkında" iletişim kutusunda web sitesi ve e-posta adresleri tıklanabilir
  bağlantı olarak açılır
- Çoklu pin seçiminde gridin üstünde beliren işlem çubuğu: seçilen pin sayısı,
  toplu "Koleksiyona ekle", toplu "Sil" ve "Seçimi temizle" butonları

### Değişti

- "Açma Konumu" ve "Yönetici Olarak Çalıştır" butonları yalnızca pin türüne
  uygun olduğunda etkinleştirilir
- Koleksiyon iletişim kutusu artık adlandırma/açıklama düzenleme modlarını
  destekler

### Düzeltildi

- Klasör ve ağ (UNC) pinleri artık çift tıklandığında doğru hedefi açıyor.
  Daha önce `explorer.exe` üzerinden başlatma bazı durumlarda yanlışlıkla
  Belgeler klasörünü açabiliyordu; artık hedef doğrudan Windows kabuğuyla
  açılıyor.
- Program ilk kez çalışırken pinlerin yüklenmesi devam ederken yapılan
  sürükle-bırak eklemelerinin listede görünmemesi sorunu giderildi.
- Sürükle-bırak ile gelen `.ps1` dosyaları artık "Dosya" yerine
  "PowerShell" pin türüyle ekleniyor.
- Komut, PowerShell ve batch pinleri için komut satırı oluşturma daha
  güvenilir hale getirildi (yol ve boşluk içeren komutlarda hatalı çift
  tırnak kullanımı giderildi).
- Pencere boyutu ve büyütülmüş (maximize) durumu artık hatırlanıyor;
  uygulama açıldığında son ayar geri yükleniyor.
- Uygulama artık tek örnek (tek instance) olarak çalışıyor: zaten açıkken
  tekrar başlatılırsa mevcut pencere öne getirilir ve ikinci örnek kapanır.
- Pin kartlarının sürükle-bırak ile yeniden sıralanması geri kazandırıldı;
  kartlar koleksiyonlara da sürüklenebiliyor.
- Arayüz eğitimi sıfırdan yeniden tasarlandı: karartılmış arka plan, hedefi
  vurgulayan çerçeve ve alt bilgi kartı (adım sayacı, Sonraki/Atla) ile çalışır.
- Sistem tepsisi ikonu artık her zaman görünür; kurulum sürümünde uygulama
  ikonunun publish çıktısına kopyalanmaması sorunu giderildi.

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