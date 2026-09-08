# Pin Anything

> Windows için kişisel hızlı erişim katmanı — **by KTYSoft**

Pin Anything, Windows üzerinde sık eriştiğiniz her şeyi (uygulamalar, klasörler,
dosyalar, web siteleri, komutlar, sistem araçları, ağ yolları…) tek, hızlı ve
modern bir merkezde toplayıp tek hareketle açmanızı sağlayan hafif bir masaüstü
uygulamasıdır.

> **Temel fikir:** *"Windows'ta ulaşmak istediğim herhangi bir şeyi bir kez
> pinleyeyim ve daha sonra tek hareketle ulaşayım."*

## Özellikler (MVP)

- **Pin CRUD** — uygulama, klasör, dosya, web sitesi, URL, komut, PowerShell,
  Batch, Windows ayarı, sistem aracı, ağ yolu ve özel pinler
- **Çalıştırma** — ShellExecute tabanlı güvenli başlatma; `Yönetici olarak çalıştır`
  (standart UAC `runas` elevation)
- **Sidebar kategorileri ve koleksiyonlar** — Tüm Pinler, Uygulamalar, Klasörler,
  Dosyalar, Web Siteleri, Komutlar, Ayarlar, Scripts + kullanıcı koleksiyonlar
- **Arama** — başlık, açıklama, etiket, hedef ve tür üzerinden
- **Favoriler** — her pin favori olarak işaretlenebilir, sıralamada öne gelir
- **Tema** — Sistem / Koyu / Açık
- **Import / Export** — JSON (sürüm 1)
- **Klavye gezinme** — Ctrl + K arama, Ctrl + N yeni pin, F2 düzenle, Delete sil,
  Ctrl + E/I export/import
- **Lokal & gizli** — tüm veri SQLite'da yerel tutulur; telemetri yoktur

## Mimari

```text
src/
├── PinAnything.Core       Pin modeli, PinManager, CollectionManager, validasyon, export
├── PinAnything.Storage    SQLite veritabanı, migration, repository'ler
├── PinAnything.Windows    ProcessLauncher (ShellExecute / URI / CMD / PowerShell)
└── PinAnything.App        WinUI 3 (Windows App SDK) arayüz, MVVM
tests/
└── PinAnything.Tests      xUnit testler
```

## Gereksinimler

- Windows 10 22H2 veya Windows 11
- .NET 10 SDK (derleme için)
- WinUI 3 / Windows App SDK (WindowsAppSDK 2.x)

## Build

```powershell
dotnet restore
dotnet build PinAnything.slnx
dotnet test tests/PinAnything.Tests
```

Uygulamayı çalıştırmak için (WinUI paketli uygulama, geliştirici modu gerekir):

```powershell
dotnet run --project src/PinAnything.App
```

## Veri

Veriler varsayılan olarak `%LOCALAPPDATA%\PinAnything\Data\pins.db` içinde
SQLite tabanlı tutulur. Uygulama portablesını desteklemek için veri dizini
taşınabilir şekilde yapılandırılabilir.

## Klavye Kısayolları

| Kısayol     | İşlem            |
|-------------|------------------|
| `Ctrl + K`  | Aramaya odaklan  |
| `Ctrl + N`  | Yeni pin         |
| `Enter`     | Seçili pini aç   |
| `F2`        | Pini düzenle     |
| `Delete`    | Pini sil         |
| `Ctrl + E`  | Dışa aktar       |
| `Ctrl + I`  | İçe aktar        |
| `Esc`       | Aramayı temizle  |

## Lisans

[MIT](LICENSE)