# Changelog

Bu proje [Keep a Changelog](https://keepachangelog.com/tr/1.0.0/) ve
[SemVer](https://semver.org/spec/v2.0.0.html) biçimine uyar.

## [0.1.0] - 2026-09-08

### Eklendi

- Çözüm yapısı: `PinAnything.Core`, `PinAnything.Storage`, `PinAnything.Windows`,
  `PinAnything.App` (WinUI 3 / Windows App SDK, MVVM), `PinAnything.Tests` (xUnit)
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