# Security Policy

## Güvenlik Açığı Bildirimi

Bir güvenlik açığı tespit ettiyseniz lütfen GitHub üzerinden **private** bir
issue açın veya doğrudan KTYSoft ile iletişime geçin. Açığı kamuya açık
ortamlarda paylaşmayın.

Bildirimde şunları belirtin:

- Etkilenen sürüm
- Açığın türü ve etkisi
- Yeniden üretme adımları (mümkünse)
- Önerilen düzeltme (varsa)

## Desteklenen Sürümler

Aktif geliştirme `main` dalında yapılır. Güvenlik düzeltmeleri en son yayın ve
`main` için önceliklidir.

## Güvenlik Notları

- Bu uygulama **telemetri toplamaz**; pin, yol, komut ve kullanım verisi
  cihaz dışına gönderilmez.
- Yaklaşan pin hedefleri varsayılan olarak `ShellExecute` ile başlatılır.
  Pin hedefi yalnızca kullanıcının bilgisayarında saklanır.
- Komut çalıştırmayı gerektiren pin türlerinde (Command/PowerShell/Batch)
  girdinin kullanıcı tarafından oluşturulduğu varsayılır; üçüncü taraf veriyi
  pin hedefi olarak kabul etmeyin.