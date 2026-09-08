# Pin Anything — KTYSoft
## Ürün Gereksinimleri ve OpenAI/Codex Geliştirme Talimatları

> **Proje:** Pin Anything  
> **Üretici:** KTYSoft  
> **Platform:** Windows 10/11  
> **Lisans:** MIT  
> **Amaç:** Windows üzerinde kullanıcının sık eriştiği her şeyi tek, hızlı ve modern bir merkezden yönetmesini sağlamak.
>
> Bu doküman, AI destekli geliştirme ajanının projeyi planlaması, geliştirmesi, test etmesi ve güvenli biçimde tamamlaması için ana teknik ve ürün spesifikasyonudur.

---

# 1. ÜRÜN VİZYONU

Pin Anything, Windows'un herhangi bir öğesini kullanıcının kişisel hızlı erişim sistemine dönüştüren hafif bir masaüstü uygulamasıdır.

Kullanıcı aşağıdaki öğeleri pinleyebilmelidir:

- Uygulamalar
- EXE dosyaları
- Klasörler
- Dosyalar
- Web siteleri
- URL'ler
- Windows ayarları
- Sistem araçları
- CMD komutları
- PowerShell komutları
- BAT/CMD/PS1 scriptleri
- Özel komutlar
- Ağ adresleri
- UNC yolları
- Belgeler
- Çalışma klasörleri
- Kullanıcının tanımladığı özel öğeler

Temel fikir:

> **Windows'ta ulaşmak istediğim herhangi bir şeyi bir kez pinleyeyim ve daha sonra tek hareketle ulaşayım.**

Uygulama bir dosya yöneticisi, launcher veya task manager kopyası değildir.

Bunların üzerinde çalışan bir **kişisel erişim ve organizasyon katmanıdır.**

---

# 2. TEMEL TASARIM FELSEFESİ

## 2.1 Öncelik sırası

Her özellik şu sırayla değerlendirilmelidir:

1. Hız
2. Basitlik
3. Güvenilirlik
4. Güvenlik
5. Özelleştirilebilirlik
6. Görsel kalite

## 2.2 Ana prensip

Kullanıcı mümkün olduğunca:

- az tıklamalı,
- az yazmalı,
- mümkünse klavye kullanmalı,
- yaptığı işlemin sonucunu hemen görmelidir.

## 2.3 Uygulama hissi

Uygulama:

- modern,
- hızlı,
- Windows 11 ile uyumlu,
- sade,
- profesyonel,
- teknik kullanıcıyı da memnun edecek kadar güçlü

olmalıdır.

Aşırı animasyon kullanılmamalıdır.

---

# 3. HEDEF KULLANICILAR

## Birincil kullanıcı

Windows üzerinde çok sayıda dosya, klasör, uygulama, script ve sistem aracı kullanan kişiler.

Özellikle:

- yazılımcılar
- sistem yöneticileri
- IT çalışanları
- power user'lar
- teknik destek personeli
- öğrenciler
- içerik üreticileri

## İkincil kullanıcı

Windows'ta sürekli kullandığı dosya ve programlara daha hızlı ulaşmak isteyen normal kullanıcılar.

---

# 4. ANA KULLANICI SENARYOSU

Örnek:

Kullanıcı her gün:

- Visual Studio Code
- PowerShell
- Downloads
- Projeler klasörü
- 192.168.1.1
- Windows Terminal
- Device Manager
- belirli bir PowerShell scripti

kullanıyor.

Bunları Pin Anything'e ekler.

Daha sonra uygulamayı açtığında:

```text
Tüm Pinler

Visual Studio Code
Projeler
192.168.1.1
PowerShell
Aygıt Yöneticisi
Downloads
Windows Terminal
Update-Drivers.ps1
```

şeklinde tek ekranda görür.

Öğe seçilir.

Enter'a basılır.

Öğe çalışır.

---

# 5. UYGULAMA MİMARİSİ

Uygulama modüler tasarlanmalıdır.

Önerilen katmanlar:

```text
PinAnything
│
├── UI
│   ├── MainWindow
│   ├── Sidebar
│   ├── PinGrid
│   ├── PinList
│   ├── DetailsPanel
│   ├── SearchBar
│   ├── ContextMenu
│   ├── Settings
│   └── Dialogs
│
├── Core
│   ├── PinManager
│   ├── CollectionManager
│   ├── SearchEngine
│   ├── LaunchManager
│   ├── ImportManager
│   ├── ExportManager
│   └── RecentManager
│
├── Windows
│   ├── ShellIntegration
│   ├── ShortcutResolver
│   ├── ProcessLauncher
│   ├── RegistryIntegration
│   ├── SystemUriResolver
│   └── FileSystemWatcher
│
├── Storage
│   ├── Database
│   ├── Repositories
│   └── Migrations
│
├── Security
│   ├── CommandValidation
│   ├── PathValidation
│   └── PermissionHandling
│
└── Infrastructure
    ├── Logging
    ├── Settings
    ├── Update
    └── Diagnostics
```

AI geliştirici bu yapıyı teknoloji seçimine göre uyarlayabilir ancak sorumluluklar birbirine karıştırılmamalıdır.

---

# 6. TEKNOLOJİ TERCİHİ

Öncelikli tercih:

- C#
- .NET 8 veya daha güncel LTS sürüm
- Windows App SDK / WinUI 3

Alternatif ancak gerekçeli olarak:

- WPF

Öncelik modern Windows görünümü olduğu için WinUI 3 tercih edilmelidir.

UI ile Windows native işlemleri birbirinden ayrılmalıdır.

---

# 7. PIN MODELİ

Her pin aşağıdaki temel alanlara sahip olmalıdır:

```text
Id
Title
Description
Type
Target
Arguments
WorkingDirectory
Icon
CollectionId
IsFavorite
IsEnabled
CreatedAt
UpdatedAt
LastUsedAt
UseCount
SortOrder
Tags
RunAsAdministrator
OpenWith
CustomColor
CustomShortcut
```

## PinType

En az:

```text
Application
File
Folder
Website
Url
Command
PowerShell
Batch
WindowsSetting
SystemTool
NetworkPath
Custom
```

---

# 8. PIN OLUŞTURMA

Kullanıcı aşağıdaki yöntemlerle pin oluşturabilmelidir.

## Yöntem 1 — Drag & Drop

Windows Explorer'dan:

- dosya
- klasör
- EXE
- script
- shortcut

sürüklenip uygulamaya bırakılabilir.

## Yöntem 2 — Yeni Pin

Buton:

```text
+ Yeni Pin
```

Form:

```text
Başlık
Tür
Hedef
Parametre
Çalışma dizini
Açıklama
Etiketler
Koleksiyon
İkon
```

## Yöntem 3 — Explorer Context Menu

İsteğe bağlı shell extension:

```text
Pin Anything'e Sabitle
```

Bu özellik uygulamanın ana sürümünde güvenli ve modüler şekilde ele alınmalıdır.

## Yöntem 4 — Kısayol ile

Kullanıcı:

```text
Ctrl + K
```

ile hızlı pin oluşturma penceresini açabilmelidir.

---

# 9. PIN TİPLERİ

## 9.1 Application

Örnek:

```text
Visual Studio Code
C:\Program Files\Microsoft VS Code\Code.exe
```

Aksiyonlar:

- Aç
- Yönetici olarak aç
- Konumunu aç
- Kaldır
- Düzenle

## 9.2 Folder

Örnek:

```text
C:\Projects
```

Aksiyon:

```text
Explorer'da aç
```

## 9.3 File

Dosyayı varsayılan uygulamayla açmalıdır.

## 9.4 Website

Örnek:

```text
https://github.com
```

Varsayılan tarayıcıda açılmalıdır.

## 9.5 Command

Örnek:

```text
ipconfig /all
```

Komutun çalıştırılacağı terminal seçilebilir.

## 9.6 PowerShell

Örnek:

```powershell
Get-Process
```

PowerShell içinde çalıştırılmalıdır.

## 9.7 Windows Setting

Windows URI protokolü veya uygun Windows API kullanılmalıdır.

Örneğin:

```text
ms-settings:network
ms-settings:display
ms-settings:bluetooth
```

## 9.8 System Tool

Örnek:

```text
devmgmt.msc
services.msc
eventvwr.msc
diskmgmt.msc
taskschd.msc
```

## 9.9 Network Path

Örnek:

```text
\\SERVER\Share
```

---

# 10. ANA ARAYÜZ

Ana pencere yaklaşık olarak aşağıdaki yapıya sahip olmalıdır:

```text
┌─────────────────────────────────────────────────────────────┐
│ KTYSoft / Pin Anything       🔎 Ne yapmak istiyorsun?      │
├──────────────┬──────────────────────────────┬───────────────┤
│              │                              │               │
│ Tüm Pinler   │       Pin Kartları            │ Detay        │
│ Uygulamalar  │                              │               │
│ Klasörler    │  ┌──────┐ ┌──────┐ ┌──────┐ │ VS Code       │
│ Dosyalar     │  │ VS   │ │Proj. │ │ Web  │ │               │
│ Web Siteleri │  └──────┘ └──────┘ └──────┘ │ [Aç]          │
│ Komutlar     │                              │               │
│ Ayarlar      │  ┌──────┐ ┌──────┐ ┌──────┐ │ Yönetici      │
│ Scripts      │  │ PS   │ │Device│ │Force │ │ Konum         │
│              │  └──────┘ └──────┘ └──────┘ │ Düzenle       │
│ Koleksiyonlar│                              │               │
│              │                              │               │
└──────────────┴──────────────────────────────┴───────────────┘
```

---

# 11. SIDEBAR

Sidebar kategorileri:

```text
📌 Tüm Pinler
▦ Uygulamalar
📁 Klasörler
📄 Dosyalar
🌐 Web Siteleri
>_ Komutlar
⚙ Ayarlar
📜 Scripts
```

Alt bölüm:

```text
Koleksiyonlar

+ Yeni Koleksiyon

Geliştirme
Sistem
İnternet
Oyunlar
Kişisel
```

Kullanıcı kendi koleksiyonlarını oluşturabilmelidir.

---

# 12. PIN KARTI

Kartta:

```text
[ICON]

Visual Studio Code
Uygulama

⭐
```

Gösterilebilecek bilgiler:

- ikon
- başlık
- tip
- favori durumu
- koleksiyon
- küçük etiket
- son kullanım

Kart hover olduğunda:

```text
[Aç]
[⋮]
```

gibi hızlı aksiyonlar gösterilebilir.

---

# 13. PIN DETAY PANELİ

Bir pin seçildiğinde sağ panel açılmalıdır.

Örnek:

```text
Visual Studio Code

Uygulama

C:\Users\User\AppData\Local\
Programs\Microsoft VS Code\
Code.exe

[ Aç ]

Yönetici olarak aç
Konumunu aç
Başlangıçta çalıştır
Kısayol oluştur

────────────────

Koleksiyon
Geliştirme

Not
Ana editörüm.

Etiketler
editor
kod
development
```

---

# 14. ARAMA

Arama uygulamanın en önemli özelliklerinden biridir.

Arama:

```text
Ctrl + K
```

ile açılabilmelidir.

Arama sadece başlığa bakmamalıdır.

Arama alanları:

- Title
- Description
- Tags
- Target
- Type
- Collection
- Path

Örneğin:

```text
server
```

şunları bulabilmelidir:

```text
Server
Server Scripts
\\SERVER\Share
Server Dashboard
```

Fuzzy search desteklenmelidir.

---

# 15. HIZLI KOMUTLAR

Arama kutusu sadece arama değil, komut merkezi de olabilir.

Örnek:

```text
> add
> settings
> collection
> pin
> recent
```

Daha sonra gelişmiş command palette eklenebilir.

---

# 16. FAVORİLER

Her pin:

```text
⭐ Favori
```

olarak işaretlenebilir.

Favoriler:

- üstte gösterilebilir
- ayrı filtre olabilir
- hızlı erişim kısayoluna bağlanabilir.

---

# 17. KOLEKSİYONLAR

Kullanıcı:

```text
Geliştirme
Sistem Yönetimi
Sunucular
Okul
Oyun
Kişisel
```

gibi koleksiyonlar oluşturabilmelidir.

Bir pin birden fazla koleksiyona ait olabilirse veri modeli buna uygun tasarlanmalıdır.

Koleksiyonlar:

- isim
- ikon
- renk
- sıralama
- pin listesi

içermelidir.

---

# 18. SMART COLLECTION

İleri aşamada otomatik koleksiyonlar eklenebilir.

Örneğin:

```text
Scripts
```

koleksiyonu:

```text
Type = PowerShell
OR
Type = Batch
```

ile otomatik oluşturulabilir.

Başka örnek:

```text
Recently Used
```

son kullanılan pinleri otomatik gösterir.

---

# 19. SON KULLANILANLAR

Uygulama pin kullanımını takip etmelidir.

```text
Son Kullanılanlar

Visual Studio Code     14:32
PowerShell             14:20
Projects               13:55
GitHub                 13:21
Device Manager         12:40
```

Kullanıcı geçmişi temizleyebilmelidir.

---

# 20. PIN İSTATİSTİKLERİ

İleri aşamada:

```text
En çok kullandıkların

Visual Studio Code    182
PowerShell             96
Projects               74
Chrome                 53
```

gösterilebilir.

Bu özellik varsayılan olarak tamamen lokal çalışmalıdır.

Analitik veriler internete gönderilmemelidir.

---

# 21. KISAYOLLAR

Kullanıcı pinlere global kısayol atayabilmelidir.

Örnek:

```text
Ctrl + Alt + V
```

→ Visual Studio Code

```text
Ctrl + Alt + P
```

→ PowerShell

Kısayol çakışmaları tespit edilmelidir.

---

# 22. SAĞ TIK MENÜSÜ

Her pin için:

```text
Aç
Yönetici olarak aç
Konumunu aç
Yolunu kopyala
Kısayol oluştur
Koleksiyona ekle
Favorilere ekle
Düzenle
Kopyala
Sil
```

gibi seçenekler bulunmalıdır.

Pin tipine göre menü dinamik olmalıdır.

Örneğin web sitesinde:

```text
Tarayıcıda aç
URL'yi kopyala
```

scriptte:

```text
Çalıştır
Düzenle
PowerShell ile aç
```

---

# 23. SAĞ TIK MENÜSÜNÜN GÜVENLİĞİ

Kullanıcıya gösterilen işlemler hedef türüne göre doğrulanmalıdır.

Özellikle:

- shell command injection
- kötü niyetli parametre
- bozuk path
- silme işlemi
- yönetici yetkisi

kontrol edilmelidir.

Kullanıcıdan gelen komutlar otomatik olarak güvenilir kabul edilmemelidir.

---

# 24. YÖNETİCİ OLARAK ÇALIŞTIRMA

Bazı pinlerde:

```text
☐ Yönetici olarak çalıştır
```

seçeneği bulunmalıdır.

Uygulama UAC'yi atlatmaya çalışmamalıdır.

Windows'un standart elevation mekanizması kullanılmalıdır.

---

# 25. İKON SİSTEMİ

Pin için ikon kaynakları:

1. EXE ikonunu otomatik alma
2. Dosya ikonunu alma
3. URL favicon'u
4. Windows sistem ikonu
5. Dahili ikon seti
6. Kullanıcının özel PNG/SVG ikonu

İkonlar cache'lenmelidir.

Her açılışta ikonlar yeniden çıkarılmamalıdır.

---

# 26. TEMA

En az:

- Dark
- Light
- System

temaları desteklenmelidir.

Varsayılan:

```text
System
```

olmalıdır.

Koyu tema modern Windows 11 estetiğinde olmalıdır.

---

# 27. RENKLER

Ana vurgu rengi kullanıcı tarafından değiştirilebilir.

Örneğin:

- Blue
- Purple
- Green
- Orange
- Red

Ancak renk seçimi erişilebilirlik kontrastını bozmamalıdır.

---

# 28. GRID / LIST GÖRÜNÜMÜ

Pinler:

```text
Grid
```

ve:

```text
List
```

olarak görüntülenebilmelidir.

Grid kart boyutu:

```text
Small
Medium
Large
```

olarak değiştirilebilir.

---

# 29. SIRALAMA

Desteklenmesi gereken sıralamalar:

- İsim
- Son kullanılan
- En çok kullanılan
- Oluşturulma tarihi
- Güncellenme tarihi
- Tür
- Özel sıralama

---

# 30. WINDOWS ENTEGRASYONU

Uygulama Windows ile mümkün olduğunca doğal entegre olmalıdır.

Desteklenebilecek özellikler:

- Windows URI'leri
- ShellExecute
- Explorer entegrasyonu
- dosya ilişkilendirmeleri
- EXE ikonları
- shortcut çözümleme
- UNC yolları
- environment variable çözümleme
- Windows Terminal
- PowerShell
- CMD

---

# 31. TAŞINABİLİR SÜRÜM

Uygulama mümkün olduğunca portable çalışmalıdır.

Önerilen:

```text
PinAnything.exe
Data\
    pins.db
    settings.json
    icons\
    logs\
```

Portable modda kullanıcı verileri uygulamanın bulunduğu dizinde tutulabilir.

Installer sürümünde Windows AppData kullanılabilir.

---

# 32. VERİ DEPOLAMA

Önerilen:

**SQLite**

Neden:

- hafif
- hızlı
- lokal
- portable
- güvenilir
- kolay backup
- çok sayıda pin için yeterli

Şema migration sistemi bulunmalıdır.

---

# 33. IMPORT / EXPORT

Kullanıcı bütün pinlerini dışarı aktarabilmelidir.

Destek:

```text
JSON
ZIP
```

Örnek JSON:

```json
{
  "version": 1,
  "pins": [
    {
      "title": "Visual Studio Code",
      "type": "Application",
      "target": "C:\\Program Files\\Microsoft VS Code\\Code.exe",
      "favorite": true
    }
  ]
}
```

Export edilen veride mümkün olduğunca makineye özel bilgiler normalize edilmelidir.

---

# 34. BACKUP

Kullanıcı:

```text
Ayarlar
→ Veri
→ Yedekle
```

diyebilmelidir.

Otomatik backup isteğe bağlı olmalıdır.

Backup dosyası:

```text
PinAnything-Backup-2026-09-08.zip
```

şeklinde oluşturulabilir.

---

# 35. BOZUK PIN TESPİTİ

Bir dosya taşındığında:

```text
⚠ Hedef bulunamadı

C:\Projects\Test.exe
```

gösterilmelidir.

Seçenekler:

```text
[Yeni Konum Bul]
[Düzenle]
[Sil]
```

Mümkünse otomatik path recovery yapılabilir.

Ancak dosyanın yanlış bir dosyayla eşleştirilmesi engellenmelidir.

---

# 36. DOSYA TAŞINMASINI ALGILAMA

İleri aşamada:

- dosya watcher
- path validation
- shortcut metadata
- file ID

kullanılarak hedefin taşındığı tespit edilebilir.

Her zaman otomatik düzeltme yapmak yerine kullanıcıya doğrulama sunulmalıdır.

---

# 37. OFFLINE-FIRST

Uygulamanın temel özellikleri internetsiz çalışmalıdır.

İnternet gerektirmemesi gerekenler:

- pin oluşturma
- pin açma
- arama
- koleksiyon
- ayarlar
- import/export
- kullanım geçmişi
- sistem araçları

Web sitesi favicon'u gibi özellikler internet yoksa graceful fallback kullanmalıdır.

---

# 38. TELEMETRİ

Varsayılan olarak:

**TELEMETRİ YOK**

Kullanıcının:

- pinleri
- dosya yolları
- komutları
- kullanım geçmişi
- koleksiyonları

sunucuya gönderilmemelidir.

İleri aşamada anonim hata raporlama eklenirse:

- opt-in
- açıkça açıklanmış
- kapatılabilir
- hassas veri filtreli

olmalıdır.

---

# 39. GİZLİLİK

Uygulama lokal pin verilerini kullanıcı bilgisayarı üzerinde tutmalıdır.

Özellikle şu veriler loglanmamalıdır:

- parolalar
- tokenlar
- API keyleri
- kişisel dosya içerikleri
- clipboard içerikleri

Komutların kendisi dahi loglarda gereksiz yere açık metin tutulmamalıdır.

---

# 40. LOGGING

Log sistemi olmalıdır.

Seviyeler:

```text
Debug
Info
Warning
Error
Critical
```

Loglarda:

- zaman
- component
- işlem
- hata kodu

bulunabilir.

Hassas veriler maskelenmelidir.

---

# 41. HATA YÖNETİMİ

Uygulama çökmek yerine kullanıcıya anlaşılır hata göstermelidir.

Kötü:

```text
NullReferenceException
```

İyi:

```text
Pin açılamadı.

Dosya artık bulunamıyor:
C:\Projects\Test.exe

[Konumu Değiştir]
[Pin'i Düzenle]
```

Teknik detaylar ayrı:

```text
Detayları göster
```

alanında bulunabilir.

---

# 42. GÜNCELLEME

İleri aşamada otomatik güncelleme sistemi eklenebilir.

Güncelleme:

- imzalı
- doğrulanmış
- rollback destekli

olmalıdır.

Sessiz ve güvensiz executable değiştirme mekanizması kullanılmamalıdır.

---

# 43. KLAVYE ODAKLI KULLANIM

Temel işlemler klavyeyle yapılabilmelidir.

Önerilen:

```text
Ctrl + K     Arama
Ctrl + N     Yeni Pin
Enter        Aç
Delete       Sil
F2           Düzenle
Ctrl + E     Export
Ctrl + I     Import
Ctrl + ,     Ayarlar
Esc          Paneli kapat
```

Liste/grid navigasyonu:

```text
Arrow Keys
Enter
Space
Delete
```

ile çalışmalıdır.

---

# 44. ACCESSIBILITY

Destek:

- keyboard navigation
- screen reader uyumluluğu
- yeterli kontrast
- tooltip
- anlamlı accessibility name
- ölçeklendirme
- Windows display scaling

olmalıdır.

---

# 45. PERFORMANS HEDEFLERİ

Hedef:

```text
Cold start:
< 1.5 saniye

Normal açılış:
< 1 saniye

Arama:
< 100 ms

Pin açma:
Windows API gecikmesi dışında minimum

Idle RAM:
mümkün olduğunca düşük
```

Yüzlerce hatta binlerce pin uygulamayı yavaşlatmamalıdır.

---

# 46. LAZY LOADING

İkonlar, metadata ve detaylar gerektiğinde yüklenmelidir.

Uygulama açılırken:

```text
Tüm EXE ikonlarını tarama
```

gibi pahalı işlemler yapılmamalıdır.

---

# 47. PIN VALIDATION

Pin oluşturulurken:

- hedef var mı?
- URL geçerli mi?
- komut boş mu?
- script mevcut mu?
- Windows URI geçerli mi?

kontrol edilmelidir.

Ancak ağ kaynakları gibi durumlarda erişilemiyor olması doğrudan pinin geçersiz olduğu anlamına gelmemelidir.

---

# 48. SEARCH ENGINE

İlk sürüm için SQLite üzerinde optimize edilmiş arama yeterlidir.

İleri aşamada:

- fuzzy matching
- ranking
- recent boost
- favorite boost
- exact match boost

eklenebilir.

Örneğin:

```text
vs
```

arama sonucu:

```text
Visual Studio Code
Visual Studio
VS Scripts
```

olabilir.

---

# 49. AKILLI SIRALAMA

Sonuç puanı:

```text
Score =
TextMatch
+ ExactMatch
+ FavoriteBoost
+ RecentUsageBoost
+ FrequencyBoost
+ CollectionBoost
```

mantığıyla hesaplanabilir.

Bu değerler konfigüre edilebilir olmalıdır.

---

# 50. UYGULAMA MENÜSÜ

Menü:

```text
Pin Anything
├── Yeni Pin
├── Yeni Koleksiyon
├── İçe Aktar
├── Dışa Aktar
├── Ayarlar
├── Yedekle
└── Çıkış
```

---

# 51. SYSTEM TRAY

Uygulama system tray'de çalışabilmelidir.

Tray menüsü:

```text
Pin Anything

🔎 Hızlı Ara

⭐ Favoriler
🕘 Son Kullanılanlar

────────────

Ana Pencere
Ayarlar
Çıkış
```

Kullanıcı isterse:

```text
Windows başlangıcında çalıştır
```

özelliğini açabilir.

---

# 52. GLOBAL HOTKEY

İsteğe bağlı:

```text
Ctrl + Space
```

veya kullanıcı tarafından belirlenen kombinasyonla:

```text
Quick Launcher
```

açılmalıdır.

Örnek:

```text
┌──────────────────────────────────┐
│ 🔎 Bir şey ara...                │
├──────────────────────────────────┤
│ Visual Studio Code               │
│ PowerShell                       │
│ Projects                         │
│ Device Manager                   │
│ GitHub                           │
└──────────────────────────────────┘
```

Bu pencere ana uygulamadan bağımsız hızlı açılmalıdır.

---

# 53. DYNAMIC CONTEXT ACTIONS

Pin türüne göre aksiyonlar değişmelidir.

### EXE

```text
Aç
Yönetici olarak aç
Konumunu aç
Kısayol oluştur
```

### Folder

```text
Aç
Terminal burada aç
PowerShell burada aç
Yolu kopyala
```

### Website

```text
Aç
URL kopyala
Tarayıcıda gizli aç
```

### PowerShell

```text
Çalıştır
Düzenle
Terminalde aç
```

---

# 54. KULLANICI DENEYİMİ

Yeni kullanıcı ilk açılışta boş ekran görmemelidir.

Onboarding:

```text
Pin Anything'e hoş geldin.

Sık kullandığın şeyleri buraya sabitle.

[İlk Pinimi Oluştur]
```

Örnek pinler otomatik oluşturulmamalıdır.

Kullanıcı isterse:

```text
Örnek Pinleri Ekle
```

seçeneğine basabilir.

---

# 55. DEMO / SAMPLE PINS

İsteğe bağlı demo verisi:

```text
Notepad
Calculator
PowerShell
Windows Terminal
Downloads
Documents
Settings
```

Ancak gerçek yollar sistemden dinamik olarak bulunmalıdır.

Hard-coded kullanıcı yolları kullanılmamalıdır.

---

# 56. WINDOWS SÜRÜMLERİ

Hedef:

```text
Windows 10 22H2+
Windows 11
```

Windows 10 desteği için WinUI 3 / Windows App SDK özelliklerinin uyumluluğu ayrıca kontrol edilmelidir.

Windows 11'e özel görsel özellikler fallback ile çalışmalıdır.

---

# 57. GÜVENLİK KURALLARI

AI geliştirici aşağıdaki kuralları ihlal etmemelidir:

- Kullanıcı onayı olmadan dosya silme
- Kullanıcı onayı olmadan sistem ayarı değiştirme
- UAC atlatma
- Defender/Firewall devre dışı bırakma
- gizli persistence mekanizması
- credential toplama
- keylogging
- clipboard verisini dışarı gönderme
- kullanıcı verisini sunucuya gönderme

Komut çalıştırma özelliğinde kullanıcı tarafından tanımlanan komut ile uygulamanın kendi sistem komutları ayrıştırılmalıdır.

---

# 58. TEST

Test katmanları:

```text
Unit Tests
Integration Tests
UI Tests
Security Tests
Performance Tests
```

Özellikle:

- pin CRUD
- import/export
- path validation
- search
- collection
- shortcut
- command execution
- broken pin
- admin elevation
- settings
- database migration

test edilmelidir.

---

# 59. EDGE CASELER

AI aşağıdaki durumları özellikle test etmelidir:

- dosya taşınmış
- dosya silinmiş
- klasör silinmiş
- ağ yolu offline
- URL erişilemiyor
- EXE yönetici istiyor
- komut bulunamıyor
- PowerShell yok / farklı sürüm
- kullanıcı profil yolu değişmiş
- OneDrive yolu değişmiş
- removable drive harfi değişmiş
- duplicate pin
- aynı isimli farklı pin
- invalid URL
- invalid shortcut
- bozuk database
- yarım kalmış import
- import sırasında duplicate
- çok uzun path
- Unicode path
- Türkçe karakterli dosya adı

---

# 60. DUPLICATE DETECTION

Aynı hedef tekrar pinlenirse:

```text
Bu öğe zaten pinlenmiş.

Visual Studio Code

[Mevcut Pini Aç]
[Yine de Ekle]
[İptal]
```

gösterilebilir.

Duplicate detection:

- target
- normalized target
- type

üzerinden yapılmalıdır.

---

# 61. DOSYA YOLLARI

Windows path işlemlerinde:

- environment variables
- `%USERPROFILE%`
- `%APPDATA%`
- `%LOCALAPPDATA%`
- `%PROGRAMFILES%`

gibi değişkenler gerektiğinde desteklenebilir.

Kullanıcı export yaptığında makineye özel pathlerin başka bilgisayarda sorun çıkarabileceği açıkça belirtilmelidir.

---

# 62. INTERNATIONALIZATION

İlk dil:

```text
Türkçe
```

İkinci dil:

```text
English
```

Tüm UI metinleri localization sistemi üzerinden gelmelidir.

Kod içine doğrudan UI stringleri gömülmemelidir.

---

# 63. AYARLAR

Ayarlar:

### Genel

- başlangıçta çalıştır
- system tray
- minimize to tray
- son pencere boyutunu hatırla

### Görünüm

- tema
- accent color
- grid/list
- kart boyutu

### Arama

- fuzzy search
- son kullanılanları öne çıkar
- favorileri öne çıkar

### Kısayollar

- global hotkey
- uygulama kısayolları

### Veri

- import
- export
- backup
- database location

### Gizlilik

- usage statistics
- diagnostics
- log retention

Varsayılan gizlilik ayarları minimum veri toplamalı olmalıdır.

---

# 64. MVP

İlk çalışan sürüm aşağıdakilerden oluşmalıdır:

## MVP-1

- [ ] Ana pencere
- [ ] Sidebar
- [ ] Pin CRUD
- [ ] Application
- [ ] Folder
- [ ] File
- [ ] Website
- [ ] Windows System Tool
- [ ] SQLite
- [ ] Search
- [ ] Favorites
- [ ] Collections
- [ ] Context menu
- [ ] Dark/Light/System
- [ ] Import/Export
- [ ] Keyboard navigation

MVP tamamlanmadan gelişmiş AI özellikleri eklenmemelidir.

---

# 65. V2

- [ ] PowerShell pins
- [ ] CMD pins
- [ ] Script pins
- [ ] Global hotkey
- [ ] Quick Launcher
- [ ] System Tray
- [ ] Broken Pin detection
- [ ] Recent items
- [ ] Usage statistics
- [ ] Advanced fuzzy search
- [ ] Custom icons
- [ ] Pin shortcuts

---

# 66. V3

- [ ] Explorer context menu
- [ ] Smart Collections
- [ ] Dynamic context actions
- [ ] automatic path recovery
- [ ] advanced Windows URI integration
- [ ] plugin architecture
- [ ] update system
- [ ] portable mode improvements

---

# 67. GELECEKTEKİ AI ÖZELLİKLERİ

AI temel ürünün zorunlu parçası olmamalıdır.

İleride:

```text
"Sunucu araçlarımı göster"
```

veya:

```text
"Bugün en çok kullandığım programları göster"
```

gibi doğal dil araması yapılabilir.

Ancak AI özelliği:

- lokal parser
- isteğe bağlı cloud AI
- privacy-first

şeklinde tasarlanmalıdır.

AI olmadan ürün tam işlevsel kalmalıdır.

---

# 68. GELİŞTİRME SÜRECİ

AI geliştirici projeyi tek seferde yazmaya çalışmamalıdır.

Her aşamada:

1. Planla
2. Mimariyi kontrol et
3. Küçük bir feature geliştir
4. Build al
5. Test et
6. Hataları düzelt
7. Kod incelemesi yap
8. Sonraki feature'a geç

şeklinde ilerlenmelidir.

---

# 69. AI GELİŞTİRİCİ İÇİN ZORUNLU KURAL

Bir feature geliştirirken mevcut çalışan özellikleri bozmamak zorunludur.

Her değişiklikten sonra:

```text
Build
Unit Tests
Relevant Integration Tests
```

çalıştırılmalıdır.

Derleme hatası bırakılarak sonraki feature'a geçilmemelidir.

---

# 70. KOD KALİTESİ

Kod:

- SOLID
- DRY
- KISS
- clear naming
- dependency injection
- async/await
- cancellation
- structured logging

prensiplerine uygun olmalıdır.

Gereksiz abstraction oluşturulmamalıdır.

Özellikle tek satırlık işlemler için gereksiz service/interface zinciri kurulmasından kaçınılmalıdır.

---

# 71. UI KALİTESİ

UI:

- tutarlı spacing
- tutarlı typography
- keyboard focus
- responsive layout
- hover states
- pressed states
- disabled states
- loading states
- empty states
- error states

içermelidir.

Her ekranın:

```text
Loading
Empty
Error
Success
```

durumları düşünülmelidir.

---

# 72. EMPTY STATE

Örneğin hiç pin yoksa:

```text
📌

Henüz pin yok.

Sık kullandığın uygulama, klasör,
dosya veya web sitesini buraya ekle.

[+ İlk Pinini Oluştur]
```

---

# 73. SEARCH EMPTY STATE

```text
Sonuç bulunamadı.

"server" için eşleşen pin yok.

[Yeni Pin Oluştur]
```

---

# 74. VISUAL REFERENCE

UI tasarımında bu dokümanla birlikte verilen Pin Anything konsept ekran görüntüsü referans alınmalıdır.

Tasarım yönü:

- Windows 11 benzeri
- dark-first
- modern
- rounded corners
- subtle transparency
- güçlü fakat sade sidebar
- kart tabanlı pin görünümü
- sağda detay paneli
- mavi/cyan vurgu
- okunabilir typography

Ancak görseldeki tasarım birebir kopyalanmamalıdır.

UI, gerçek uygulamada kullanılabilirlik açısından optimize edilmelidir.

---

# 75. KTYSoft MARKA KURALLARI

Ürün:

```text
Pin Anything
by KTYSoft
```

olarak markalanmalıdır.

KTYSoft logosu uygulamanın uygun alanlarında kullanılabilir.

Marka kullanıcı deneyiminin önüne geçmemelidir.

---

# 76. GITHUB / OPEN SOURCE

Repository:

```text
pin-anything
```

Önerilen:

```text
src/
tests/
docs/
assets/
installer/
.github/
```

Dosyalar:

```text
README.md
LICENSE
CONTRIBUTING.md
SECURITY.md
CHANGELOG.md
CODE_OF_CONDUCT.md
```

MIT lisansı kullanılmalıdır.

---

# 77. README İÇERİĞİ

README:

- ürün açıklaması
- ekran görüntüsü
- özellikler
- kurulum
- portable sürüm
- kullanım
- keyboard shortcuts
- build instructions
- contribution
- license

içermelidir.

---

# 78. RELEASE KONTROLÜ

Release öncesi:

```text
[ ] Build başarılı
[ ] Testler başarılı
[ ] Release configuration test edildi
[ ] Portable test edildi
[ ] Installer test edildi
[ ] Windows 10 test edildi
[ ] Windows 11 test edildi
[ ] Import/export test edildi
[ ] Upgrade test edildi
[ ] Uninstall test edildi
[ ] Log kontrol edildi
[ ] Güvenlik kontrolü yapıldı
[ ] README güncellendi
[ ] CHANGELOG güncellendi
```

---

# 79. AI'NIN UYGULAMAMASI GEREKENLER

AI geliştirici:

- gereksiz özellik eklememeli
- kullanıcı istemeden UI'ı radikal değiştirmemeli
- mevcut API'leri sebepsiz değiştirmemeli
- testleri silmemeli
- compiler warninglerini görmezden gelmemeli
- güvenlik uyarılarını bastırmamalı
- hard-coded kullanıcı pathleri kullanmamalı
- administrator yetkisini gereksiz kullanmamalı
- telemetry eklememeli
- dış servislere kullanıcı verisi göndermemeli

---

# 80. ÖNCELİKLENDİRME

Öncelik:

```text
P0 = Kritik / önce yapılacak
P1 = MVP için önemli
P2 = V2
P3 = Gelecek
```

İlk geliştirme sırası:

```text
P0
├── Architecture
├── Database
├── Pin model
├── Main UI
└── Pin CRUD

P1
├── Search
├── Collections
├── Favorites
├── Pin execution
├── Context menu
└── Settings

P2
├── Quick Launcher
├── Global Hotkey
├── Tray
├── Scripts
├── Import/Export
└── Broken Pin detection

P3
├── Explorer integration
├── Smart Collections
├── Plugin architecture
├── AI natural language
└── Advanced automation
```

---

# 81. GELİŞTİRME AJANINA BAŞLANGIÇ TALİMATI

Bu projeyi geliştirirken önce kod yazmaya başlama.

Önce:

1. Repository'yi incele.
2. Mevcut dosya yapısını analiz et.
3. Projenin mevcut teknoloji stack'ini belirle.
4. Bu dokümandaki gereksinimleri kontrol et.
5. Eksik mimari parçaları belirle.
6. Uygulanabilir bir implementation planı çıkar.
7. Planı küçük milestone'lara böl.
8. İlk milestone için gerekli dosyaları oluştur.
9. Build al.
10. Test et.
11. Sonucu raporla.

Her milestone tamamlandığında:

```text
Implemented
Tested
Known Issues
Next Step
```

formatında kısa rapor hazırlanmalıdır.

---

# 82. ÖNEMLİ: ÖNCE ÇALIŞAN ÜRÜN

Ürünü gereksiz şekilde büyütme.

İlk hedef:

> Kullanıcı bir uygulamayı, klasörü, dosyayı veya web sitesini pinleyebilsin ve daha sonra tek hareketle açabilsin.

Bu temel deneyim kusursuz çalışmadan:

- AI
- plugin
- gelişmiş analytics
- karmaşık automation
- cloud sync

gibi özelliklere geçilmemelidir.

---

# 83. BAŞARI KRİTERİ

Pin Anything başarılı sayılabilmesi için kullanıcı şu soruyu sorduğunda:

> "Bunu Windows'ta nasıl bulacağım?"

cevap:

> "Pin Anything'e sabitle."

olmalıdır.

Ürünün amacı Windows'a yeni bir dosya yöneticisi eklemek değil;

**kullanıcının Windows üzerindeki kişisel erişim katmanını oluşturmak**tır.

---

# 84. SON TALİMAT

Bu dokümanı proje gereksinimlerinin ana kaynağı olarak kabul et.

Ancak kod tabanının gerçek durumu bu dokümandan daha yüksek öncelikli teknik gerçekliktir.

Mevcut kodu incelemeden dosyaları ezme.

Mevcut çalışan özellikleri koru.

Her değişiklikte:

```text
Analyze → Plan → Implement → Build → Test → Review
```

döngüsünü uygula.

Bir feature tamamlanmış görünse bile edge case'leri kontrol et.

Özellikle Windows API, process execution, shell integration, file system ve administrator elevation işlemlerinde güvenli ve resmi Windows mekanizmalarını kullan.

**Amaç: gösterişli bir mockup değil, günlük kullanımda gerçekten hızlı, güvenilir ve profesyonel bir Windows uygulaması üretmektir.**
