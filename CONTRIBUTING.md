# Contributing to PinQuick

Teşekkürler! PinQuick'e katkıda bulunmak istediğiniz için harika.

## Katkı Süreci

1. Bir **issue** açın veya mevcut issue üzerinde çalışmaya başlayın.
2. Kolayca gözden geçirilebilmesi için **küçük, odaklı** değişiklikler yapın.
3. Kod stiline ve `AGENTS.md` bölümündeki geliştirme standartlarına uyun.
4. Değişikliklerinize uygun **testler** ekleyin.

## Geliştirme

```powershell
dotnet build PinQuick.slnx
dotnet test tests/PinQuick.Tests
```

## Commit Kuralları

- Kısa ve açıklayıcı commit mesajları yazın.
- Çözümlediğiniz issue numarasını belirtin (ör. `fixes #12`).
- Secret, API key veya kullanıcıya özel veri commit etmeyin.

## Kod Stili

- Mevcut proje stilini birebir takip edin; gereksiz abstraction eklemeyin.
- Tüm yeni hayranlık/forms Türkçe UI metinleri ile uyumlu olmalıdır.

## Derecelendirme

- Yeni özellik geliştirmeden önce lisans ve güvenlik değerlendirmesi yapın
  (bkz. `AGENTS.md`, bölüm 3-7).

## Lisans

Katkılarınız MIT lisansı altında (bkz. `LICENSE`) kabul edilir.