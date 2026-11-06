# Afterrust — otomatik QA ekran görüntüleri

## Çalışma
Unity Editor'da Play başlayınca `Assets/Resources/qa_screenshot_plan.json` yüklenir. Plan varsayılan olarak açık; 2 saniye sonra belirtilen noktalardan PNG'ler alınır. Yakalama **oyuncu kamerasını geçici olarak yeniden konumlandırır**, sonra önceki yerel pozisyonunu/rotasyonunu, görüş açısını ve hedef dokusunu geri yükler. Oyun dünyasındaki oyuncunun konumu değiştirilmez.

`QA/Screenshots/latest/` içindeki PNG'ler ve `manifest.json` yeni Play'de yeniden oluşturulur; yalnızca bu klasördeki eski PNG'ler ve manifest silinir. Plan şu an **11** açı içerir: eski sığınak/avlu karelerine ek olarak v7 giriş rotası, yardım büfesi ve kimyasal konteyner/tehlike alanı.

## Önemli: GitHub'a gönderilen görsellerin güncelliği
1 Kasım 2026 tarihli manifest, eski prototipin yedi açısını gösteriyor; yeni v9 kodunun çıktıları değildir. Yeni Unity Play çalıştırılana kadar bunları güncel sahnenin kanıtı olarak kullanmayın. Otomatik sistem dosyaları **yerelde üretir**, GitHub'a kendiliğinden push etmez.

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

## Yağmurun çatıdan geçmesini gözle test et (v8)
- Play sırasında yağmur başladığında sığınağın içine gir: parçacıkların çatının altına inmemesi gerekir.
- Açık kapıdan dışarıdaki yağmuru gözle; tüm yağmurun kapatılması istenmiyor.
- Tüccar tezgâhı tentesi ve keşif alanı konteyneri/büfesinin altında aynı davranışı kontrol et.
- Bu test için normal QA sahne fotoğrafları yeterli değildir: yağışlı hava ve oyuncu konumu koşulu gerekir. Ek gözlem/ekran görüntüsü oluştur.

## Güvenli avlu kontrolü (v9)
- Güney girişine yaklaş: çift kanat kapalıyken çitle birleşmeli, aradan yürüyerek geçilememeli; **E** ile iki kanat beraber içeri açılmalı.
- Tüccar tezgâhının tavanı dört direğin üstünde kalmalı; hem ön hem arka travers görünmeli, havada yüzer hissi olmamalı.
- İlk keşif görevi güvenli avlu düzenlemesi sırasında pasif tutulur.
- Adımlar sessiz/arka planda kalmalı; yağmur ve rüzgâr ambiyansı hâlâ duyulmalı.
