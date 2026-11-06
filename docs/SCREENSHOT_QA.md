# Afterrust — otomatik QA ekran görüntüleri

## Çalışma
Unity Editor'da Play başlayınca `Assets/Resources/qa_screenshot_plan.json` yüklenir. Plan varsayılan olarak açık; 2 saniye sonra belirtilen noktalardan PNG'ler alınır. Yakalama **oyuncu kamerasını geçici olarak yeniden konumlandırır**, sonra önceki yerel pozisyonunu/rotasyonunu, görüş açısını ve hedef dokusunu geri yükler. Oyun dünyasındaki oyuncunun konumu değiştirilmez.

`QA/Screenshots/latest/` içindeki PNG'ler ve `manifest.json` yeni Play'de yeniden oluşturulur; yalnızca bu klasördeki eski PNG'ler ve manifest silinir. Plan şu an **8** açı içerir: sığınak önü/içi, hurda köşesi, ticaret tezgâhı, avlu, uzak harabeler, yeni tüccar ve soba/yatak.

## Önemli: GitHub'a gönderilen görsellerin güncelliği
1 Kasım 2026 tarihli manifest, eski prototipin yedi açısını gösteriyor; yeni v6 kodunun çıktıları değildir. Yeni Unity Play çalıştırılana kadar bunları güncel sahnenin kanıtı olarak kullanmayın. Otomatik sistem dosyaları **yerelde üretir**, GitHub'a kendiliğinden push etmez.

## Yeni kamera noktaları
`Assets/Resources/qa_screenshot_plan.json` dosyasındaki `views` dizisini düzenle:
- `id` benzersiz dosya adı; `description` amaç.
- `position` dünya koordinatı; `rotation` Euler açıları; `fieldOfView` 25–100.
- `enabled` ve `delaySeconds` otomatik yakalamayı belirler.

Görüntünün doğru yeri gösterdiğini Play içinde kontrol et. Her yeni sahne kurulumunda kamera noktaları eski konumları hedefleyebilir.

## GitHub'a gönderme
1. Play'i başlat, Console'da `[Afterrust QA] Saved ... screenshots` yazdığını doğrula.
2. Dosyaları incele; bozuk/boş kareler varsa açıları değiştir.
3. PNG ve `manifest.json` değişikliklerini GitHub Desktop ile commit/push et. Güncel klasörde eskiler silinir ama Git geçmişindeki görüntüler saklanır.

## Sınırlar
- Sadece Unity **Editor Play** modunda çalışır.
- Kamera render'ı IMGUI HUD/menüleri içermez; bunların ayrı ekran yakalaması gerekir.
- Resimler kendiliğinden GitHub'a gönderilmez.
