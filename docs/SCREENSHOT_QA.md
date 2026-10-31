# Afterrust — Otomatik QA ekran görüntüleri

## Çalışma
Unity 6.3 Editor'da Play'e basınca `Assets/Resources/qa_screenshot_plan.json` okunur. Oyun başladıktan yaklaşık 2 saniye sonra ayrı bir geçici kamera dört açıdan PNG üretir. Oyuncunun kamerası ve pozisyonu değişmez. Görüntüler `QA/Screenshots/latest/` klasöründedir.

Her yeni Play başlangıcında yalnız bu klasördeki önceki PNG ve manifest.json silinir. Yeni görüntüler ve bir konum/açı manifest dosyası yazılır. Farklı klasörlerdeki dosyalar silinmez.

## AI tarafından yeni kamera noktaları belirleme
Yapay zekâ GitHub üzerinden `Assets/Resources/qa_screenshot_plan.json` içindeki `views` listesini günceller:
- `id`: tekil ve kısa dosya adı (ör. `car_engine_closeup`)
- `description`: görünmesini istediğimiz sistem ve açı
- `position`: dünya koordinatı (x,y,z)
- `rotation`: Euler derece (x,y,z)
- `fieldOfView`: 25–100 arası görüş açısı
- `delaySeconds`: sahnenin hazırlanması için bekleme

Sadece gerekli yerleri seçin. Her oyuna girişte rastgele onlarca görüntü çekmek yerine, mevcut hata/özellik incelemesi için 3–6 açı yeterlidir. Başka bir sahne oluşturulunca konumlar güncellenmeli; eski sahne koordinatları yeni sahnede boş alanı gösterebilir.

## GitHub'a gönderme
1. Unity Play'de bir kez çalıştır ve Console'da `[Afterrust QA] Saved ... screenshots` mesajını kontrol et.
2. GitHub Desktop'ta `QA/Screenshots/latest` altındaki PNG + manifest.json değişikliklerini commit et.
3. Push origin yap. Sonraki Play önceki fotoğrafları yerel klasörden temizler; GitHub Desktop'ta silmeler de değişiklik olarak görünür. Yeni commit sadece güncel fotoğrafları içerir.

## Sınırlar
- Bu yalnız Unity **Editor Play** modunda çalışır; dağıtılacak oyunda otomatik ekran görüntüsü kaydetmez.
- Ayrı kamera ile dünya render'ı alınır. OnGUI HUD/menü gibi ekran üstü UI görüntüde bulunmayabilir.
- Kamera açısı bir noktadan alınır; animasyonun belirli anlarını yakalamak için ileride zamanlanmış tetikleyiciler eklenebilir.
- Oyun GitHub'a kendisi push yapmaz; kullanıcının GitHub Desktop üzerinden commit/push yapması gerekir.
- GitHub'a gönderdiğin PNG'ler repo geçmişinde kalır. Son klasörde eskisi silinse de Git geçmişindeki eski commitler silinmez. Hassas bilgi varsa görüntüleri yükleme.
- `QA/Screenshots/latest` dışında hiçbir dosya temizlenmez.
