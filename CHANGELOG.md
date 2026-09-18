# Changelog

Bu proje [Keep a Changelog](https://keepachangelog.com/tr/1.0.0/) ve
[SemVer](https://semver.org/spec/v2.0.0.html) biçimine uyar.

## [1.4.0] - 2026-09-18

### Geliştirmeler

- Ayarlar bölümüne "Güncellemeleri Kontrol Et" butonu eklendi; GitHub Releases üzerinden
  en son sürüm kontrol edilerek sonuç (güncel / yeni sürüm + indirme bağlantısı) anında gösteriliyor.
- Alt bilgi çubuğunda sürüm numarasının yanına tıklanabilir "Güncelleme Kontrol Et" bağlantısı
  eklendi; yeni sürüm varsa sürüm notları ve indirme bağlantısı bir iletişim kutusunda sunuluyor.

## [1.3.0] - 2026-09-18

### Düzeltildi

- Kurulum sürümünde uygulama Varsayılan WinUI simgesiyle gözüküyordu; artık exe'ye
  PinQuick'in özel ikonu gömülüyor, görev çubuğu ve kurulan kısayollar da bu ikonu kullanıyor.
- İlk açılışta kart ikonları görünmüyor, "Yenile" basılınca geliyordu. Windows ikon
  önbelleği soğukken başarısız olan çıkarma işlemleri artık kısa bir gecikmeyle otomatik
  bir kez daha deneniyor; ikonlar "Yenile" gerekmeden kendiliğinden geliyor.
- Bozuk `settings.json` dosyası uygulamanın açılmasını engelliyordu; artık dosya
  yedeklenerek varsayılan ayarlarla açılıyor. Ayar kaydı atomik yapıldı (yarım yazma riski yok).
- Koleksiyon silinince veritabanında öksüz pin-koleksiyon kayıtları kalıyordu; SQLite
  yabancı anahtar zorlaması etkinleştirildi.
- Async-void ve tekrarlayan hatalar (ör. tek bir ContentDialog kuralı) uygulamayı
  çökertiyordu; artık günlüğe yazılıp çökme engelleniyor.
- Ayarlar dişli butonuna hızla çift tıklandığında ikinci bir iletişim kutusu açılmaya
  çalışılıp çökme olabiliyordu; koruma eklendi.
- Dışa/içe aktarmada bazı null kayıtlar ve kapalı pinlerin içe aktarımda sessizce
  yeniden etkinleşmesi sorunları giderildi; `IsEnabled` artık korunuyor.
- Filtre/sıralama değişimindeki olası yarış durumu giderildi (geç işlem sonucu
  listenin üzerine yazmıyor).

### Değişti

- Pin düzenleme penceresindeki koleksiyon kutusuna, seçimin pin'i o koleksiyona
  "eklediği" ve mevcut koleksiyonların korunduğu açıklaması eklendi.

## [1.2.0] - 2026-09-16

### Geliştirmeler

- Koleksiyonlar artık sürükle-bırak ile yeniden sıralanabiliyor; yeni sıra kalıcı olarak
  kaydediliyor.
- Arama kutusu canlı arama yapıyor; her harf yazıldığında sonuçlar anında güncelleniyor.
- Sıra durumu/hatalar için eski alt bilgi satırı yerine sıralama araç çubuğu eklendi;
  (varsayılan, ada göre A→Z, ada göre Z→A, son kullanılanlara göre, türe göre) sıralama
  ve pin sayısı gösteriliyor. Bilgi/hatalar ise kısa süreliğine görünen şeffaf bir
  InfoBar'da gösteriliyor.
- Aydınlık temada tema değiştirme ikonu daha uygun bir güneş ikonuyla değiştirildi.

### Düzeltildi

- Kart ikonları bazen olması gerekenden küçük görünüyordu; ikon alanı sabit boyuta
  alındı ve görsel her kutuyu dolduracak şekilde ölçekleniyor.
- Sıralama menüsü (RadioMenuFlyoutItem) ile çalışmayan pin sıralaması düzeltildi.
- Koleksiyon sıralaması WinUI yerleşik CanReorderItems mekanizması ile yeniden
  yazıldı; artık güvenilir şekilde çalışıyor.

## [1.1.0] - 2026-09-16

### Geliştirmeler

- Pin oluşturma/düzenleme penceresinde "Hedef" alanı Gözat ile seçilebiliyor;
  dosya seçici tüm dosyaları gösteriyor ve PowerShell (.ps1) / Sistem Aracı (.msc,
  .exe) türleri için de gözat butonu görünüyor.
- Çoklu seçim araç çubuğu opak bir zemin üzerine yeniden tasarlandı: öne çıkan
  (vurgulu) "Koleksiyona ekle", kırmızı "Sil" ve sade "Seçimi temizle" butonlarıyla
  artık kartların üzerinde net ve tıklanabilir görünüyor.
- Pin oluşturma/düzenleme penceresindeki "Tür" listesi artık seçili dilde görünüyor
  (Türkçe: Uygulama, Klasör...; İngilizce: Application, Folder...).

### Düzeltildi

- Steam tarafından oluşturulan .url kısayollarının içindeki steam://rungameid/...
  hedefleri artık uygulama kategorisinde ekleniyor, bozuk sayılmıyor ve tıklayınca
  Steam üzerinden başlatılabiliyor.
- Epic Games (com.epicgames.launcher://), Battle.net (battlenet://), Xbox (xboxlauncher://),
  Origin (origin://) ve Uplay (uplay://) gibi masaüstü oyun başlatıcılarının .url
  kısayolları da artık uygulama kategorisinde ekleniyor; hedef bir web sitesi sayılmıyor.
- Steam masaüstü .url kısayollarından eklenen pinlerin kart ikonu artık gösteriliyor
  (kısayoldaki IconFile bilgisi pin'e aktarılıyor).
- Yan panelde Koleksiyonlar'ın hemen üzerinde beliren "Son Kullanılanlar" bölümü
  kaldırıldı.
- .url hedefleri file:/// gibi URI ise gerçek yerel dosya/klasör yoluna çevriliyor;
  uygulama hedefleri kısayolun gerçek kaynağına bağlanıyor.
- Masaüstü veya başka bir konumdan bırakılan kısayollar (.lnk / .url) artık kısayol
  dosyasının kendisi yerine işaret ettiği gerçek hedefle ekleniyor (uygulama, klasör
  veya URL). Böylece masaüstündeki kısayol silinse bile pin bozulmuyor; kısayolda
  saklı komut satırı argümanları da korunuyor.

## [1.0.0] - 2026-09-16

### Eklendi

- Pencere altına footer çubuğu eklendi: web sitesi, e-posta adresi ve bağış
  (GitHub Sponsors) bağlantıları tek satırda tıklanabilir olarak sunuluyor.

### Değişti

- Footer düzeni yenilendi: "Hakkında" menü öğesi sidebar'dan footer'a taşındı
  ve web sitesi/e-posta/bağış bağlantılarıyla aynı satırda; sürüm bilgisi
  barın en sağında gösteriliyor.
- "Bağış Yap" bağlantısı dikkat çekici bir butona dönüştürüldü: ☕ simgesiyle
  (GitHub Sponsors hedefi korunuyor).

### Düzeltildi

- Masaüstü veya başka bir konumdan eklenen Windows kısayollarının (.lnk / .url)
  simgeleri artık gösteriliyor: kısayol dosyası yerine kısayolun işaret ettiği
  gerçek hedef dosya/klasörün (varsa özel ikonunun) simgesi kullanılıyor.

## [0.9.1] - 2026-09-12

### Eklendi

- "Koleksiyona Ekle" menüsünde üyelik artık anahtarlanabilir: Zaten üye olunan
  koleksiyonlar ✓ ile gösterilir; üzerine tıklamak pini o koleksiyondan çıkarır,
  üye olunmayan koleksiyona tıklamak ekler.

### Değişti

- "Yeni Koleksiyon Oluştur" menü öğesinin önündeki "+" işareti kaldırıldı.
- Yenile butonunun ipucu düzeltildi: ham anahtar ("HeadRefreshTooltip") yerine
  artık "Yenile (Ctrl + R): adları ve simgeleri yeniden tara" gösteriliyor.

## [0.9.0] - 2026-09-12

### Eklendi

- Kullanıcı atanabilir global kısayol: Ayarlar'da Ctrl/Shift/Alt değiştirici tuşları
  tek tek açılıp kapatılabilir ve ana tuş klavyeden basılarak seçilir
  (ör. CTRL+SHIFT+S, SHIFT+F9, ALT+X). Salt modifier kombinasyonlar geçersizdir;
  atama kullanıcıya açıkça gösterilir ve sonradan değiştirilebilir.
- Sağ tık menüsüne "Koleksiyona Ekle" eklendi: alt menünün en üstünde
  "＋ Yeni Koleksiyon Oluştur", altında mevcut koleksiyonlar listelenir.
  Pin zaten üye olduğu koleksiyonda ✓ işaretiyle gösterilir ve tekrar eklenmez;
  koleksiyon yoksa yalnızca "＋ Yeni Koleksiyon Oluştur" görünür. Çoklu seçim
  menüsünde de aynı işlev çalışır.
- Veri modeli çoklu koleksiyon üyeliğine taşındı: bir pin birden fazla
  koleksiyonda bulunabilir. Önceki tek `CollectionId` ilişkileri açılışta
  otomatik olarak yeni kullanıma geçirilir (şema v2 migration).
- Pin kartlarında koleksiyon rozetleri: pinin dahil olduğu koleksiyonlar
  kart üzerinde küçük etiket (chip) olarak gösterilir. En fazla iki isim
  görünür; fazlaysa `+N` olarak özetlenir. Rozete tıklayınca ilgili
  koleksiyona geçilir.
- Sol menüde koleksiyonlara sağ tıklayınca yönetim menüsü:
  "Yeniden Adlandır" adı satır üzerinde inline düzenlenebilir input ile
  değiştirir (Enter kaydeder, Esc vazgeçer), "Düzenle" mevcut koleksiyon
  iletişimini açar, "Sil" onay alarak koleksiyonu siler (içindeki pinler korunur).
- Koleksiyonlara sağ tıktan "Renk Seç": 10 hazır renk renk karesiyle listelenir,
  tıklanan renk kaydedilir ve kart/sol menü rozetlerinde anında güncellenir.
- Koleksiyon başlığındaki Düzenle / Sil butonları kaldırıldı; tüm yönetim
  işlemleri sağ tık menüsünden yürütülür (Yeni Koleksiyon butonu korunur).

### Değişti

- Pin seçildiğinde sağda açılan detay/işlem paneli kaldırıldı. Pinle ilgili işlemler
  (aç, yönetici olarak çalıştır, konumu aç, düzenle, sil, favori, koleksiyona ekle)
  artık yalnızca sağ tık menüsünden yapılıyor; seçim yalnızca görsel olarak vurgulanıyor.
  Çoklu seçim araç çubuğu pin alanı üzerinde kalıyor.
- Uygulama açıldığında önceki oturumun son aramaları otomatik olarak gösterilmiyor;
  arama kutusu başlangıçta odaklansa da "Son Aramalar" yalnızca kullanıcı arama
  kutusuna tıkladığında veya Ctrl+K ile açtığında görülüyor.
- Yeni pin ekleme/düzenleme ekranında dikey kaydırma çubuğu için içerikten ayrılmış
  bir alan ayrıldı; kaydırma çubuğu butonların ve giriş alanlarının üzerine binmiyor,
  yatay kaydırma engellendi.
- Aydınlık modda Ayarlar ve diğer iletişim pencereleri (pin, koleksiyon) artık
  uygulama temasıyla uyumlu görünüyor.
- Ayarlar penceresi küçük ekran/pencere boyutlarında içerik taşarsa dikey kaydırılabilir
  oldu; pencere boyutu tüm ayarlara erişimi engellemiyor.
- Tema seçimi yalnızca Aydınlık ve Karanlık olarak yapılıyor: üst bardaki tema butonu
  ve Ayarlar'daki tema kutusu "Sistem" seçeneği kaldırıldı.

### Düzeltildi

- Global kısayolla (Ctrl+Space) veya tepsi simgesiyle çağrıldığında pencere artık
  diğer programların üzerinde görünüyor; Windows foreground kilitlenmesi aşılıyor.
- Global kısayol başka bir programda kullanımda veya Windows'a ayrılmışsa kaydedilemiyor;
  kullanıcıya anlaşılır bir uyarı gösteriliyor ve kısayol kapalı bırakılıyor.

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