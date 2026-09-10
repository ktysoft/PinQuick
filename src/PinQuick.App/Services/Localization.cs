namespace PinQuick.App.Services;

/// <summary>
/// Uygulama geneli dil desteği. Şu an Türkçe ve İngilizce içerir.
/// Görünüm başlangıcında <see cref="Language"/> değerine göre seçim yapılır.
/// </summary>
public static class Loc
{
    public static string Language { get; set; } = "tr";

    public static string T(string key)
    {
        if (Language == "en" && _en.TryGetValue(key, out var en))
        {
            return en;
        }

        return _tr.TryGetValue(key, out var tr) ? tr : key;
    }

    private static readonly Dictionary<string, string> _tr = new()
    {
        // Header
        ["HeadNewPin"] = "Yeni Pin",
        ["HeadMenuTooltip"] = "Dışa / içe aktarma",
        ["MenuExport"] = "Pins dışa aktar (JSON)",
        ["MenuImport"] = "Pins içe aktar (JSON)",
        ["MenuBackup"] = "Yedek Al (ZIP)",
        ["MenuRestore"] = "Yedekten Geri Yükle",
        ["MenuShowTour"] = "Eğitimi Göster",
        ["HeadSettingsTooltip"] = "Ayarlar",
        ["SearchPlaceholder"] = "Ne yapmak istiyorsun?  (Ctrl + K)",
        ["SearchHistoryLabel"] = "Son Aramalar",
        ["SearchHistoryClear"] = "Geçmişi temizle",

        // Sidebar
        ["SideCategories"] = "Kategoriler",
        ["SideRecent"] = "Son Kullanılanlar",
        ["SideCollections"] = "Koleksiyonlar",
        ["SideToggleCollectionsTooltip"] = "Koleksiyonları göster / gizle",
        ["SideNewCollectionTooltip"] = "Yeni koleksiyon",
        ["SideDeleteCollectionTooltip"] = "Seçili koleksiyonu sil",
        ["SideAbout"] = "Hakkında",
        ["SideAboutTooltip"] = "Uygulama hakkında",
        ["GridReorderTooltip"] = "Ana görünümde kartları sürükleyerek sıralayabilirsin",

        // Filters
        ["FilterAll"] = "Tüm Pinler",
        ["FilterFavorites"] = "Favoriler",
        ["FilterRecent"] = "Son Kullanılanlar",
        ["FilterBroken"] = "Bozuk Pinler",
        ["FilterApplications"] = "Uygulamalar",
        ["FilterFolders"] = "Klasörler",
        ["FilterFiles"] = "Dosyalar",
        ["FilterWebsites"] = "Web Siteleri",
        ["FilterCommands"] = "Komutlar",
        ["FilterScripts"] = "Scripts",
        ["FilterSettings"] = "Ayarlar",

        // Pin types
        ["TypeApplication"] = "Uygulama",
        ["TypeFile"] = "Dosya",
        ["TypeFolder"] = "Klasör",
        ["TypeWebsite"] = "Web Sitesi",
        ["TypeUrl"] = "URL",
        ["TypeCommand"] = "Komut",
        ["TypePowerShell"] = "PowerShell",
        ["TypeBatch"] = "Komut Dosyası",
        ["TypeWindowsSetting"] = "Windows Ayarı",
        ["TypeSystemTool"] = "Sistem Aracı",
        ["TypeNetworkPath"] = "Ağ Yolu",
        ["TypeCustom"] = "Özel",

        // Empty state & detail panel
        ["EmptyNoPins"] = "Henüz pin yok.",
        ["EmptyDescription"] = "Sık kullandığın uygulama, klasör, dosya veya web sitesini buraya ekle.",
        ["EmptyFirstPin"] = "İlk Pini Oluştur",
        ["DetailOpen"] = "Aç",
        ["DetailFavorite"] = "Favori",
        ["DetailRunAsAdmin"] = "Yönetici olarak çalıştır",
        ["DetailOpenLocation"] = "Konumu aç",
        ["DetailEdit"] = "Düzenle",
        ["DetailDelete"] = "Sil",
        ["DetailLastUsed"] = "Son kullanım",
        ["DetailUseCount"] = "Kullanım sayısı",
        ["DetailBroken"] = "Hedef şu an bulunamıyor",
        ["BrokenBadgeTooltip"] = "Hedef bulunamadı veya geçersiz",
        ["ContextAddFavorite"] = "Favorilere ekle",
        ["ContextRemoveFavorite"] = "Favorilerden çıkar",
        ["ContextAddToCollection"] = "Koleksiyona Ekle",
        ["ContextNoCollection"] = "Koleksiyonsuz",

        // Status messages
        ["MsgNoPinsFound"] = "Pin bulunamadı.",
        ["MsgNothingToAdd"] = "Eklenecek yeni pin bulunamadı.",
        ["MsgPinAddedOne"] = "1 pin eklendi.",
        ["MsgPinAddedMany"] = "{0} pin eklendi.",
        ["MsgAlreadyPinned"] = "Bu öğe zaten pinlenmiş.",
        ["MsgCollectionExists"] = "Bu isimde bir koleksiyon zaten var.",
        ["MsgImportDone"] = "{0} pin içe aktarıldı.",
        ["MsgImportNothing"] = "Import edilecek yeni pin bulunamadı.",
        ["MsgExportDone"] = "{0} pin dışa aktarıldı.",
        ["MsgExportNothing"] = "Dışa aktarılacak pin yok.",
        ["MsgLaunchFailed"] = "Pin başlatılamadı.",
        ["MsgOpenLocationFailed"] = "Konum açılamadı.",
        ["MsgBackupCreated"] = "Yedek oluşturuldu.",
        ["MsgBackupFailed"] = "Yedek oluşturulamadı.",
        ["MsgRestoreDone"] = "Yedek geri yüklendi. Uygulama yeniden başlatılacak.",
        ["MsgRestoreFailed"] = "Yedek geri yüklenemedi.",
        ["RestoreConfirmMessage"] = "Bu işlem mevcut pinleri seçilen yedekteki verilerle değiştirecek. Emin misiniz?",
        ["RestoreButton"] = "Geri Yükle",

        // Confirmation dialogs
        ["DeletePinTitle"] = "Pini sil",
        ["DeletePinPrompt"] = "'{0}' silinsin mi?",
        ["DeleteSelectedPrompt"] = "Seçili {0} pin silinsin mi?",
        ["SelBarCount"] = "{0} pin seçildi",
        ["SelBarAddToCollection"] = "Koleksiyona ekle",
        ["SelBarClear"] = "Seçimi temizle",
        ["DeleteCollectionTitle"] = "Koleksiyonu sil",
        ["DeleteCollectionPrompt"] = "'{0}' koleksiyonu silinsin mi?",
        ["SilButton"] = "Sil",
        ["CancelButton"] = "İptal",

        // Pin dialog
        ["NewPinDialogTitle"] = "Yeni Pin",
        ["EditPinDialogTitle"] = "Pini Düzenle",
        ["SaveButton"] = "Kaydet",
        ["ValidationInvalidUrl"] = "Geçerli bir URL girin (http veya https ile başlamalı).",
        ["ValidationInvalidMsSettings"] = "Hedef 'ms-settings:' ile başlamalı.",
        ["ValidationInvalidNetworkPath"] = "Ağ yolu '\\\\sunucu\\paylaşım' biçiminde olmalı.",
        ["ValidationTargetMissing"] = "Hedef şu an bulunamadı. Yine de eklemek istiyorsan Kaydet'e tekrar bas.",
        ["FieldTitle"] = "Başlık",
        ["TitlePlaceholder"] = "Pinin görünen adı",
        ["FieldType"] = "Tür",
        ["FieldTarget"] = "Hedef",
        ["TargetPlaceholder"] = "Uygulama, klasör, dosya, URL veya komut",
        ["BrowseButton"] = "Gözat",
        ["FieldArguments"] = "Parametreler",
        ["ArgumentsPlaceholder"] = "İsteğe bağlı komut satırı parametreleri",
        ["FieldWorkingDirectory"] = "Çalışma dizini",
        ["OptionalPlaceholder"] = "İsteğe bağlı",
        ["FieldDescription"] = "Açıklama",
        ["FieldTags"] = "Etiketler",
        ["TagsPlaceholder"] = "virgülle ayrılmış, ör: kod, sunucu",
        ["FieldCollection"] = "Koleksiyon",
        ["NoCollectionPlaceholder"] = "Koleksiyonsuz",
        ["FieldFavorite"] = "Favori",
        ["FieldAdmin"] = "Yönetici olarak çalıştır",
        ["FieldIcon"] = "İkon",
        ["IconAutoPlaceholder"] = "Boş bırakılırsa otomatik (uygulamanın ikonu)",
        ["LibraryButton"] = "Kütüphane",
        ["ClearButton"] = "Temizle",
        ["ValidationTitleRequired"] = "Başlık boş olamaz.",
        ["ValidationTypeRequired"] = "Tür seçilmelidir.",
        ["ValidationTargetRequired"] = "Hedef boş olamaz.",
        ["ValidationCollectionNameRequired"] = "Koleksiyon adı boş olamaz.",
        ["TargetHeaderUrl"] = "Hedef (URL)",
        ["TargetHeaderCommand"] = "Komut",
        ["TargetHeaderMsSettings"] = "Hedef (ms-settings:...)",
        ["TargetHeaderNetwork"] = "Hedef (\\\\sunucu\\paylaşım)",
        ["TargetHeaderSystemTool"] = "Hedef (devmgmt.msc vb.)",

        // Collection dialog
        ["NewCollectionDialogTitle"] = "Yeni Koleksiyon",
        ["EditCollectionTitle"] = "Koleksiyonu Düzenle",
        ["CreateButton"] = "Oluştur",
        ["FieldCollectionName"] = "Koleksiyon adı",
        ["CollectionNamePlaceholder"] = "Geliştirme, Sistem, Kişisel...",
        ["SideEditCollectionTooltip"] = "Seçili koleksiyonu yeniden adlandır",
        ["FieldCollectionColor"] = "Renk",
        ["NoColorButton"] = "Renksiz",

        // Single instance
        ["AlreadyRunningTitle"] = "PinQuick zaten açık",
        ["AlreadyRunningMessage"] = "Uygulama zaten çalışıyor.",

        // Settings dialog
        ["SettingsDialogTitle"] = "Ayarlar",
        ["ThemeLabel"] = "Tema",
        ["ThemeSystem"] = "Sistem",
        ["ThemeDark"] = "Koyu",
        ["ThemeLight"] = "Açık",
        ["LanguageLabel"] = "Dil",
        ["LanguageTurkish"] = "Türkçe",
        ["LanguageEnglish"] = "İngilizce",
        ["StartupLabel"] = "Windows ile başlat",
        ["CardSizeLabel"] = "Kart boyutu",
        ["CardSizeSmall"] = "Küçük",
        ["CardSizeMedium"] = "Orta",
        ["CardSizeLarge"] = "Büyük",
        ["MinimizeToTrayLabel"] = "Pencere kapatıldığında tepsiye küçült",
        ["GlobalHotkeyLabel"] = "Global kısayol (Ctrl+Space pencereyi göster/gizle)",
        ["RestartRequiredMessage"] = "Dil değişikliği için uygulamanın yeniden başlatılması gerekir. Şimdi yeniden başlatılsın mı?",
        ["RestartButton"] = "Yeniden Başlat",

        // Onboarding turu
        ["TourTitle"] = "PinQuick Turu",

        ["TourStep1Title"] = "Arama Kutusu",
        ["TourStep1Subtitle"] = "Pinleri başlıklarına, hedeflerine veya etiketlerine göre anında arayın.",
        ["TourStep2Title"] = "Yeni Pin",
        ["TourStep2Subtitle"] = "Dosya, klasör, uygulama, web sitesi ve daha fazlasını tek tıkla pinleyin.",
        ["TourStep3Title"] = "Kategoriler",
        ["TourStep3Subtitle"] = "Tümünü, favorileri, son kullanılanları veya bozuk pinleri buradan filtreleyin.",
        ["TourStep4Title"] = "Koleksiyonlar",
        ["TourStep4Subtitle"] = "Pinleri mantıklı gruplar halinde düzenleyin; koleksiyon ekleyin, yeniden adlandırın veya silin.",
        ["TourStep5Title"] = "Pinler",
        ["TourStep5Subtitle"] = "Bir pini açmak için çift tıklayın veya sağ tıklayarak gelişmiş seçeneklere ulaşın.",
        ["TourStep6Title"] = "Ayarlar",
        ["TourStep6Subtitle"] = "Tema, dil, başlangıç, sistem tepsisi ve global kısayolu buradan yönetin.",
        ["TourStep7Title"] = "Menü",
        ["TourStep7Subtitle"] = "Yedekleme, geri yükleme, dışa ve içe aktarma işlemlerine buradan erişin.",

        ["TourNext"] = "Sonraki",
        ["TourFinish"] = "Tamamla",
        ["TourClose"] = "Kapat",
        ["TourSkip"] = "Atla",
        ["TourProgress"] = "Adım {0}/{1}",

        // About
        ["AboutTitle"] = "{0} hakkında",
        ["AboutDescription"] = "Sık kullandığın uygulama, klasör, dosya ve web sitelerine tek pencereden hızlı erişim.",
        ["AboutVersion"] = "Sürüm: {0}",
        ["AboutDeveloper"] = "Geliştirici: {0}",
        ["AboutWebsite"] = "Web Sitesi: {0}",
        ["AboutEmail"] = "E-posta: {0}",
        ["AboutLicense"] = "Lisans: {0}",
        ["CloseButton"] = "Kapat",
        ["ErrorTitle"] = "Hata",
        ["TrayShow"] = "PinQuick'i Göster",
        ["TrayExit"] = "Çıkış",
        ["TrayTooltip"] = "PinQuick",
        ["ThemeToggleTooltip"] = "Temayı değiştir",
        ["DragToCollection"] = "Koleksiyona sürükle",
        ["ExportFilterTitle"] = "Dışa Aktarılacak Pinleri Seç",
        ["ExportFilterAll"] = "Tüm Pinler",
        ["ExportFilterCurrent"] = "Mevcut Filtre",
        ["MsgExportFiltered"] = "{0} pin dışa aktarıldı (filtreli).",
        ["AutoBackupLabel"] = "Otomatik yedekleme",
        ["AutoBackupNone"] = "Kapalı",
        ["AutoBackupDaily"] = "Günlük",
        ["AutoBackupWeekly"] = "Haftalık",
        ["MsgAutoBackupCreated"] = "Otomatik yedek oluşturuldu.",
    };

    private static readonly Dictionary<string, string> _en = new()
    {
        ["HeadNewPin"] = "New Pin",
        ["HeadMenuTooltip"] = "Import / export",
        ["MenuExport"] = "Export pins (JSON)",
        ["MenuImport"] = "Import pins (JSON)",
        ["MenuBackup"] = "Backup (ZIP)",
        ["MenuRestore"] = "Restore from backup",
        ["MenuShowTour"] = "Show tutorial",
        ["HeadSettingsTooltip"] = "Settings",
        ["HeadRefreshTooltip"] = "Refresh (Ctrl + R): re-scan names and icons",
        ["SearchPlaceholder"] = "What do you want to do?  (Ctrl + K)",
        ["SearchHistoryLabel"] = "Recent Searches",
        ["SearchHistoryClear"] = "Clear history",

        ["SideCategories"] = "Categories",
        ["SideRecent"] = "Recently Used",
        ["SideCollections"] = "Collections",
        ["SideToggleCollectionsTooltip"] = "Show / hide collections",
        ["SideNewCollectionTooltip"] = "New collection",
        ["SideDeleteCollectionTooltip"] = "Delete selected collection",
        ["SideAbout"] = "About",
        ["SideAboutTooltip"] = "About the app",
        ["GridReorderTooltip"] = "Drag cards to reorder in the main view",

        ["FilterAll"] = "All Pins",
        ["FilterFavorites"] = "Favorites",
        ["FilterRecent"] = "Recent",
        ["FilterBroken"] = "Broken Pins",
        ["FilterApplications"] = "Applications",
        ["FilterFolders"] = "Folders",
        ["FilterFiles"] = "Files",
        ["FilterWebsites"] = "Websites",
        ["FilterCommands"] = "Commands",
        ["FilterScripts"] = "Scripts",
        ["FilterSettings"] = "Settings",

        ["TypeApplication"] = "Application",
        ["TypeFile"] = "File",
        ["TypeFolder"] = "Folder",
        ["TypeWebsite"] = "Website",
        ["TypeUrl"] = "URL",
        ["TypeCommand"] = "Command",
        ["TypePowerShell"] = "PowerShell",
        ["TypeBatch"] = "Batch",
        ["TypeWindowsSetting"] = "Windows Setting",
        ["TypeSystemTool"] = "System Tool",
        ["TypeNetworkPath"] = "Network Path",
        ["TypeCustom"] = "Custom",

        ["EmptyNoPins"] = "No pins yet.",
        ["EmptyDescription"] = "Add apps, folders, files or websites you use often.",
        ["EmptyFirstPin"] = "Create your first pin",
        ["DetailOpen"] = "Open",
        ["DetailFavorite"] = "Favorite",
        ["DetailRunAsAdmin"] = "Run as administrator",
        ["DetailOpenLocation"] = "Open location",
        ["DetailEdit"] = "Edit",
        ["DetailDelete"] = "Delete",
        ["DetailLastUsed"] = "Last used",
        ["DetailUseCount"] = "Use count",
        ["DetailBroken"] = "Target currently not found",
        ["BrokenBadgeTooltip"] = "Target not found or invalid",
        ["ContextAddFavorite"] = "Add to favorites",
        ["ContextRemoveFavorite"] = "Remove from favorites",
        ["ContextAddToCollection"] = "Add to collection",
        ["ContextNoCollection"] = "No collection",

        ["MsgNoPinsFound"] = "No pins found.",
        ["MsgNothingToAdd"] = "No new pins to add.",
        ["MsgPinAddedOne"] = "1 pin added.",
        ["MsgPinAddedMany"] = "{0} pins added.",
        ["MsgAlreadyPinned"] = "This item is already pinned.",
        ["MsgCollectionExists"] = "A collection with this name already exists.",
        ["MsgImportDone"] = "Imported {0} pins.",
        ["MsgImportNothing"] = "No new pins to import.",
        ["MsgExportDone"] = "Exported {0} pins.",
        ["MsgExportNothing"] = "No pins to export.",
        ["MsgLaunchFailed"] = "Failed to launch pin.",
        ["MsgOpenLocationFailed"] = "Failed to open location.",
        ["MsgBackupCreated"] = "Backup created.",
        ["MsgBackupFailed"] = "Failed to create backup.",
        ["MsgRestoreDone"] = "Backup restored. The app will restart.",
        ["MsgRestoreFailed"] = "Failed to restore backup.",
        ["RestoreConfirmMessage"] = "This will replace your current pins with the data from the selected backup. Are you sure?",
        ["RestoreButton"] = "Restore",

        ["DeletePinTitle"] = "Delete pin",
        ["DeletePinPrompt"] = "Delete '{0}'?",
        ["DeleteSelectedPrompt"] = "Delete {0} selected pins?",
        ["SelBarCount"] = "{0} pins selected",
        ["SelBarAddToCollection"] = "Add to collection",
        ["SelBarClear"] = "Clear selection",
        ["DeleteCollectionTitle"] = "Delete collection",
        ["DeleteCollectionPrompt"] = "Delete collection '{0}'?",
        ["SilButton"] = "Delete",
        ["CancelButton"] = "Cancel",

        ["NewPinDialogTitle"] = "New Pin",
        ["EditPinDialogTitle"] = "Edit Pin",
        ["SaveButton"] = "Save",
        ["ValidationInvalidUrl"] = "Enter a valid URL (must start with http or https).",
        ["ValidationInvalidMsSettings"] = "Target must start with 'ms-settings:'.",
        ["ValidationInvalidNetworkPath"] = "Network path must look like '\\\\server\\share'.",
        ["ValidationTargetMissing"] = "Target not found. Click Save again to keep it anyway.",
        ["FieldTitle"] = "Title",
        ["TitlePlaceholder"] = "Display name of the pin",
        ["FieldType"] = "Type",
        ["FieldTarget"] = "Target",
        ["TargetPlaceholder"] = "App, folder, file, URL or command",
        ["BrowseButton"] = "Browse",
        ["FieldArguments"] = "Parameters",
        ["ArgumentsPlaceholder"] = "Optional command line parameters",
        ["FieldWorkingDirectory"] = "Working directory",
        ["OptionalPlaceholder"] = "Optional",
        ["FieldDescription"] = "Description",
        ["FieldTags"] = "Tags",
        ["TagsPlaceholder"] = "comma separated, e.g: code, server",
        ["FieldCollection"] = "Collection",
        ["NoCollectionPlaceholder"] = "No collection",
        ["FieldFavorite"] = "Favorite",
        ["FieldAdmin"] = "Run as administrator",
        ["FieldIcon"] = "Icon",
        ["IconAutoPlaceholder"] = "Leave empty for automatic (app icon)",
        ["LibraryButton"] = "Library",
        ["ClearButton"] = "Clear",
        ["ValidationTitleRequired"] = "Title cannot be empty.",
        ["ValidationTypeRequired"] = "Select a type.",
        ["ValidationTargetRequired"] = "Target cannot be empty.",
        ["ValidationCollectionNameRequired"] = "Collection name cannot be empty.",
        ["TargetHeaderUrl"] = "Target (URL)",
        ["TargetHeaderCommand"] = "Command",
        ["TargetHeaderMsSettings"] = "Target (ms-settings:...)",
        ["TargetHeaderNetwork"] = "Target (\\\\server\\share)",
        ["TargetHeaderSystemTool"] = "Target (devmgmt.msc etc.)",

        ["NewCollectionDialogTitle"] = "New Collection",
        ["EditCollectionTitle"] = "Edit Collection",
        ["CreateButton"] = "Create",
        ["FieldCollectionName"] = "Collection name",
        ["CollectionNamePlaceholder"] = "Development, System, Personal...",
        ["SideEditCollectionTooltip"] = "Rename selected collection",
        ["FieldCollectionColor"] = "Color",
        ["NoColorButton"] = "No color",

        ["AlreadyRunningTitle"] = "PinQuick is already running",
        ["AlreadyRunningMessage"] = "The application is already running.",

        ["SettingsDialogTitle"] = "Settings",
        ["ThemeLabel"] = "Theme",
        ["ThemeSystem"] = "System",
        ["ThemeDark"] = "Dark",
        ["ThemeLight"] = "Light",
        ["LanguageLabel"] = "Language",
        ["LanguageTurkish"] = "Turkish",
        ["LanguageEnglish"] = "English",
        ["StartupLabel"] = "Launch at Windows startup",
        ["CardSizeLabel"] = "Card size",
        ["CardSizeSmall"] = "Small",
        ["CardSizeMedium"] = "Medium",
        ["CardSizeLarge"] = "Large",
        ["MinimizeToTrayLabel"] = "Minimize to tray when the window is closed",
        ["GlobalHotkeyLabel"] = "Global hotkey (Ctrl+Space show/hide window)",
        ["RestartRequiredMessage"] = "The app must restart to apply the language change. Restart now?",
        ["RestartButton"] = "Restart",

        ["AboutTitle"] = "About {0}",
        ["AboutDescription"] = "Quick access to your frequently used apps, folders, files and websites in one window.",
        ["AboutVersion"] = "Version: {0}",
        ["AboutDeveloper"] = "Developer: {0}",
        ["AboutWebsite"] = "Website: {0}",
        ["AboutEmail"] = "Email: {0}",
        ["AboutLicense"] = "License: {0}",
        ["CloseButton"] = "Close",
        ["ErrorTitle"] = "Error",
        ["TrayShow"] = "Show PinQuick",
        ["TrayExit"] = "Exit",
        ["TrayTooltip"] = "PinQuick",
        ["ThemeToggleTooltip"] = "Switch theme",
        ["DragToCollection"] = "Drag to collection",
        ["ExportFilterTitle"] = "Select Pins to Export",
        ["ExportFilterAll"] = "All Pins",
        ["ExportFilterCurrent"] = "Current Filter",
        ["MsgExportFiltered"] = "{0} pins exported (filtered).",
        ["AutoBackupLabel"] = "Automatic backup",
        ["AutoBackupNone"] = "Off",
        ["AutoBackupDaily"] = "Daily",
        ["AutoBackupWeekly"] = "Weekly",
        ["MsgAutoBackupCreated"] = "Automatic backup created.",
        ["TourTitle"] = "PinQuick Tour",

        ["TourStep1Title"] = "Search Box",
        ["TourStep1Subtitle"] = "Instantly search pins by title, target or tags.",
        ["TourStep2Title"] = "New Pin",
        ["TourStep2Subtitle"] = "Pin files, folders, apps, websites and more with a single click.",
        ["TourStep3Title"] = "Categories",
        ["TourStep3Subtitle"] = "Filter all pins, favorites, recently used or broken pins here.",
        ["TourStep4Title"] = "Collections",
        ["TourStep4Subtitle"] = "Organize pins into meaningful groups; add, rename or delete collections.",
        ["TourStep5Title"] = "Pins",
        ["TourStep5Subtitle"] = "Double-click a pin to launch it, or right-click for advanced options.",
        ["TourStep6Title"] = "Settings",
        ["TourStep6Subtitle"] = "Manage theme, language, startup, system tray and the global shortcut here.",
        ["TourStep7Title"] = "Menu",
        ["TourStep7Subtitle"] = "Access backup, restore, export and import from here.",

        ["TourNext"] = "Next",
        ["TourFinish"] = "Finish",
        ["TourClose"] = "Close",
        ["TourSkip"] = "Skip",
        ["TourProgress"] = "Step {0}/{1}",
    };
}

/// <summary>
/// XAML statik bağlama (x:Bind) için kullanılan görünüm metinleri.
/// Her property <see cref="Loc.T(string)"/> üzerinden o anki dilde döner.
/// </summary>
public static class Strings
{
    public static string HeadNewPin => Loc.T("HeadNewPin");
    public static string HeadMenuTooltip => Loc.T("HeadMenuTooltip");
    public static string MenuExport => Loc.T("MenuExport");
    public static string MenuImport => Loc.T("MenuImport");
    public static string MenuBackup => Loc.T("MenuBackup");
    public static string MenuRestore => Loc.T("MenuRestore");
    public static string MenuShowTour => Loc.T("MenuShowTour");
    public static string TourProgress => Loc.T("TourProgress");
    public static string TourNext => Loc.T("TourNext");
    public static string TourSkip => Loc.T("TourSkip");
    public static string HeadSettingsTooltip => Loc.T("HeadSettingsTooltip");
    public static string HeadRefreshTooltip => Loc.T("HeadRefreshTooltip");
    public static string SearchPlaceholder => Loc.T("SearchPlaceholder");
    public static string SearchHistoryLabel => Loc.T("SearchHistoryLabel");
    public static string SearchHistoryClear => Loc.T("SearchHistoryClear");

    public static string SideCategories => Loc.T("SideCategories");
    public static string SideRecent => Loc.T("SideRecent");
    public static string SideCollections => Loc.T("SideCollections");
    public static string SideToggleCollectionsTooltip => Loc.T("SideToggleCollectionsTooltip");
    public static string SideNewCollectionTooltip => Loc.T("SideNewCollectionTooltip");
    public static string SideDeleteCollectionTooltip => Loc.T("SideDeleteCollectionTooltip");
    public static string SideEditCollectionTooltip => Loc.T("SideEditCollectionTooltip");
    public static string SideAbout => Loc.T("SideAbout");
    public static string SideAboutTooltip => Loc.T("SideAboutTooltip");
    public static string GridReorderTooltip => Loc.T("GridReorderTooltip");
    public static string SelBarAddToCollection => Loc.T("SelBarAddToCollection");
    public static string SelBarClear => Loc.T("SelBarClear");

    public static string EmptyNoPins => Loc.T("EmptyNoPins");
    public static string EmptyDescription => Loc.T("EmptyDescription");
    public static string EmptyFirstPin => Loc.T("EmptyFirstPin");

    public static string DetailOpen => Loc.T("DetailOpen");
    public static string DetailFavorite => Loc.T("DetailFavorite");
    public static string DetailRunAsAdmin => Loc.T("DetailRunAsAdmin");
    public static string DetailOpenLocation => Loc.T("DetailOpenLocation");
    public static string DetailEdit => Loc.T("DetailEdit");
    public static string DetailDelete => Loc.T("DetailDelete");
    public static string BrokenBadgeTooltip => Loc.T("BrokenBadgeTooltip");

    public static string NewPinDialogTitle => Loc.T("NewPinDialogTitle");
    public static string SaveButton => Loc.T("SaveButton");
    public static string CancelButton => Loc.T("CancelButton");
    public static string FieldTitle => Loc.T("FieldTitle");
    public static string TitlePlaceholder => Loc.T("TitlePlaceholder");
    public static string FieldType => Loc.T("FieldType");
    public static string FieldTarget => Loc.T("FieldTarget");
    public static string TargetPlaceholder => Loc.T("TargetPlaceholder");
    public static string BrowseButton => Loc.T("BrowseButton");
    public static string FieldArguments => Loc.T("FieldArguments");
    public static string ArgumentsPlaceholder => Loc.T("ArgumentsPlaceholder");
    public static string FieldWorkingDirectory => Loc.T("FieldWorkingDirectory");
    public static string OptionalPlaceholder => Loc.T("OptionalPlaceholder");
    public static string FieldDescription => Loc.T("FieldDescription");
    public static string FieldTags => Loc.T("FieldTags");
    public static string TagsPlaceholder => Loc.T("TagsPlaceholder");
    public static string FieldCollection => Loc.T("FieldCollection");
    public static string NoCollectionPlaceholder => Loc.T("NoCollectionPlaceholder");
    public static string FieldFavorite => Loc.T("FieldFavorite");
    public static string FieldAdmin => Loc.T("FieldAdmin");
    public static string FieldIcon => Loc.T("FieldIcon");
    public static string IconAutoPlaceholder => Loc.T("IconAutoPlaceholder");
    public static string LibraryButton => Loc.T("LibraryButton");
    public static string ClearButton => Loc.T("ClearButton");

    public static string NewCollectionDialogTitle => Loc.T("NewCollectionDialogTitle");
    public static string CreateButton => Loc.T("CreateButton");
    public static string FieldCollectionName => Loc.T("FieldCollectionName");
    public static string CollectionNamePlaceholder => Loc.T("CollectionNamePlaceholder");
    public static string FieldCollectionColor => Loc.T("FieldCollectionColor");
    public static string NoColorButton => Loc.T("NoColorButton");
    public static string AlreadyRunningTitle => Loc.T("AlreadyRunningTitle");
    public static string AlreadyRunningMessage => Loc.T("AlreadyRunningMessage");

    public static string SettingsDialogTitle => Loc.T("SettingsDialogTitle");
    public static string ThemeLabel => Loc.T("ThemeLabel");
    public static string ThemeSystem => Loc.T("ThemeSystem");
    public static string ThemeDark => Loc.T("ThemeDark");
    public static string ThemeLight => Loc.T("ThemeLight");
    public static string LanguageLabel => Loc.T("LanguageLabel");
    public static string LanguageTurkish => Loc.T("LanguageTurkish");
    public static string LanguageEnglish => Loc.T("LanguageEnglish");
    public static string StartupLabel => Loc.T("StartupLabel");
    public static string CardSizeLabel => Loc.T("CardSizeLabel");
    public static string CardSizeSmall => Loc.T("CardSizeSmall");
    public static string CardSizeMedium => Loc.T("CardSizeMedium");
    public static string CardSizeLarge => Loc.T("CardSizeLarge");
    public static string MinimizeToTrayLabel => Loc.T("MinimizeToTrayLabel");
    public static string GlobalHotkeyLabel => Loc.T("GlobalHotkeyLabel");
    public static string RestartButton => Loc.T("RestartButton");

    public static string CloseButton => Loc.T("CloseButton");
    public static string ErrorTitle => Loc.T("ErrorTitle");
    public static string ThemeToggleTooltip => Loc.T("ThemeToggleTooltip");
    public static string DragToCollection => Loc.T("DragToCollection");
    public static string ExportFilterTitle => Loc.T("ExportFilterTitle");
    public static string ExportFilterAll => Loc.T("ExportFilterAll");
    public static string ExportFilterCurrent => Loc.T("ExportFilterCurrent");
    public static string AutoBackupLabel => Loc.T("AutoBackupLabel");
    public static string AutoBackupNone => Loc.T("AutoBackupNone");
    public static string AutoBackupDaily => Loc.T("AutoBackupDaily");
    public static string AutoBackupWeekly => Loc.T("AutoBackupWeekly");
}