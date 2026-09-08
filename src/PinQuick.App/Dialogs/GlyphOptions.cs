using PinQuick.App.ViewModels;

namespace PinQuick.App.Dialogs;

/// <summary>
/// İkon kütüphanesinde sunulan tek bir simge seçeneği.
/// </summary>
public sealed class GlyphOption
{
    public string Code { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string Glyph => PinItemViewModel.GlyphFromCode(Code);
}

/// <summary>
/// Kullanıcının seçebileceği dahili simge kataloğu.
/// </summary>
public static class GlyphOptions
{
    public static IReadOnlyList<GlyphOption> All { get; } = new[]
    {
        new GlyphOption { Code = "E7F4", Name = "Uygulama" },
        new GlyphOption { Code = "E8B7", Name = "Klasör" },
        new GlyphOption { Code = "E7C3", Name = "Dosya" },
        new GlyphOption { Code = "E8A5", Name = "Belge" },
        new GlyphOption { Code = "E774", Name = "Web Sitesi" },
        new GlyphOption { Code = "E71B", Name = "Bağlantı" },
        new GlyphOption { Code = "E756", Name = "Komut" },
        new GlyphOption { Code = "E713", Name = "Ayarlar" },
        new GlyphOption { Code = "E968", Name = "Sistem Aracı" },
        new GlyphOption { Code = "E8A7", Name = "Özel" },
        new GlyphOption { Code = "E734", Name = "Yıldız" },
        new GlyphOption { Code = "E735", Name = "Dolu Yıldız" },
        new GlyphOption { Code = "EEB5", Name = "Kalp" },
        new GlyphOption { Code = "E710", Name = "Ekle" },
        new GlyphOption { Code = "E74D", Name = "Çöp Kutusu" },
        new GlyphOption { Code = "E70F", Name = "Düzenle" },
        new GlyphOption { Code = "E72C", Name = "Yenile" },
        new GlyphOption { Code = "E721", Name = "Ara" },
        new GlyphOption { Code = "E8C8", Name = "Kopyala" },
        new GlyphOption { Code = "E77F", Name = "Yapıştır" },
        new GlyphOption { Code = "E896", Name = "İndir" },
        new GlyphOption { Code = "E898", Name = "Yükle" },
        new GlyphOption { Code = "E715", Name = "E-posta" },
        new GlyphOption { Code = "E823", Name = "Saat" },
        new GlyphOption { Code = "E787", Name = "Takvim" },
        new GlyphOption { Code = "E77B", Name = "Kişi" },
        new GlyphOption { Code = "E765", Name = "Klavye" },
        new GlyphOption { Code = "E722", Name = "Kamera" },
        new GlyphOption { Code = "E8D6", Name = "Müzik" },
        new GlyphOption { Code = "EB9F", Name = "Resim" },
        new GlyphOption { Code = "E714", Name = "Video" },
        new GlyphOption { Code = "E7FC", Name = "Oyun" },
        new GlyphOption { Code = "E72E", Name = "Kitap" },
        new GlyphOption { Code = "E70B", Name = "Not" },
        new GlyphOption { Code = "E8EF", Name = "Hesap Makinesi" },
        new GlyphOption { Code = "E946", Name = "Bilgi" },
        new GlyphOption { Code = "E7BA", Name = "Uyarı" },
        new GlyphOption { Code = "E783", Name = "Hata" },
        new GlyphOption { Code = "E73E", Name = "Onay" },
        new GlyphOption { Code = "E80F", Name = "Ev" },
        new GlyphOption { Code = "E81D", Name = "Konum" },
        new GlyphOption { Code = "E71A", Name = "Durdur" },
        new GlyphOption { Code = "E7A7", Name = "Geri Al" },
        new GlyphOption { Code = "E7A8", Name = "Yeniden Yap" },
        new GlyphOption { Code = "E72A", Name = "Ok" },
        new GlyphOption { Code = "E9F9", Name = "Soru" },
    };
}