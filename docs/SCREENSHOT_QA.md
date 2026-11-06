# Afterrust — otomatik QA ekran görüntüleri

## Çalışma
Unity Editor'da Play başlayınca `Assets/Resources/qa_screenshot_plan.json` yüklenir. Plan varsayılan olarak açık; 2 saniye sonra belirtilen noktalardan PNG'ler alınır. Yakalama **oyuncu kamerasını geçici olarak yeniden konumlandırır**, sonra önceki yerel pozisyonunu/rotasyonunu, görüş açısını ve hedef dokusunu geri yükler. Oyun dünyasındaki oyuncunun konumu değiştirilmez.

`QA/Screenshots/latest/` içindeki PNG'ler ve `manifest.json` yeni Play'de yeniden oluşturulur; yalnızca bu klasördeki eski PNG'ler ve manifest silinir. Plan şu an **11** açı içerir: eski sığınak/avlu karelerine ek olarak v7 giriş rotası, yardım büfesi ve kimyasal konteyner/tehlike alanı.

## Önemli: GitHub'a gönderilen görsellerin güncelliği
1 Kasım 2026 tarihli manifest, eski prototipin yedi açısını gösteriyor; yeni v13 kodunun çıktıları değildir. Yeni Unity Play çalıştırılana kadar bunları güncel sahnenin kanıtı olarak kullanmayın. Otomatik sistem dosyaları **yerelde üretir**, GitHub'a kendiliğinden push etmez.

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

## Tek açık yağmur varili (v10)
- QA planında `13_open_rain_collector` açısı, varilin üstünün kapalı olmadığını ve su yüzeyinin yerini denetler. Toplam 13 açı tanımlıdır.
- Yağmur yokken varili 30 saniye gözle: doluluk artmamalı.
- Yağmurdayken su yüzeyi çok yavaş yükselmeli. İlerleme hızını geliştirme testi için bir EditMode testi de denetler (tam dolum: 15 dakika aktif yağış).
- Varilin üstünden bakıldığında boş iç metal dibi, doldukça su yüzeyi görünmelidir. İsteğe bağlı dış seviye çizgisi de suyla beraber yükselir.
- `F5` ile örneğin %20 doluyken kaydedip `F9` ile yükleyerek doluluğun aynen geri geldiğini kontrol et.

## v11: Gece kaydından bağımsız QA gündüz çekimi
- 6 Kasım 2026 tarihli Unity v10 PNG'leri karanlık gece koşullarında üretildiği için tasarım kontrolüne yeterli değil; özellikle tüccar ve varil tamamen karanlık.
- Planın `captureAtFixedDaytime=true`, `captureHour=11` alanları QA çekimini sabah/öğle aydınlığına sabitler. Çekim bitince gerçek oyun saati, hava ve yağış zamanlayıcısı geri yüklenir.
- `manifest.json` yeni çekimde `capturedGameHour`, `capturedRain`, `lightingMode` bilgilerini içerir.
- Gece görsel kontrolü ayrıca Play'de yapılmalı; gündüz QA gece karanlığını test etmez. Eski PNG'ler, Play ve commit edilene kadar eski gece görüntüleridir.

## v12: Kapı tabelası / FPS kontrolü
- Güney kapısında `GÜVENLİ BÖLGE` yazısı ve tabela levhası görünmemeli. Kapı ve üst taşıyıcı kiriş yerinde kalmalı.
- **FPS sayacı otomatik QA PNG'lerinde görünmez:** kamera çıktısı IMGUI çizimini kapsamaz. FPS'yi doğrudan Unity **Game** penceresinin sağ üst köşesinde, saat/hava durumunun altında kontrol et.
- Yaklaşık 2 saniye durarak, ardından avluda yürüyüp koşarak FPS ve ms değerlerini ayrı not et. Kısa süreli düşüşleri daha sonra profiler ile araştır.

## V13: İç mekân depolama alanı
- `14_shelter_storage` açısı sığınağın sağ tarafına yerleştirilen sandığı, kapı geçiş açıklığını ve sandık kapağını göstermelidir.
- Otomatik sistem **14 açı** çeker. Diğer bütün kareler olduğu gibi eski PNG'ler silinir ve yeni kareler yerlerine yazılır.
- Depo UI'sı IMGUI ile çizildiğinden Camera.Render QA PNG'lerinde görünmez; Play/Game içinde `E` ile açıp **Koy**, **Al**, dolu kapasite ve F5/F9 kaydını manuel kontrol et.

## v14: Tüccar ekranını Game penceresinde doğrula
- Güncel `manifest.json` 14 Unity görüntüsü içerir; Play sırasında sabit gündüz koşulunda üretildiler. Bu, yeni `v14` ticaret panelinin çalıştığının kanıtı **değildir**.
- Ticaret ekranı ve FPS sayacı IMGUI ile çizildiği için kamera tabanlı QA PNG'lerine girmez. Gerçek oyun **Game** penceresinden `E` ile açılıp iki sütun, tekli satış, satın alma ve Esc/E kapatma kontrol edilmeli.
- Bu aşamada yeni otomatik kamera açısı eklenmedi: tüccarın fiziksel modeli değişmedi, mevcut `04_trade_stall` ve `07_scavenger_trader` açıları kullanılabilir.

## v15.1 — sığınak içi materyal kontrolü
- `02_shelter_interior`: beton duvar/zemin, iç giriş, ahşap tezgâh ve metal rafların ayrıştığını kontrol et.
- `08_stove_and_bed`: yatak kumaşları ve sobanın kararmış metalini hem gündüz hem gerçek gecede denetle.
- `14_shelter_storage`: sandık ahşabı/metal şeritler; kapı geçişi, collider ve yakın duvarla fiziksel örtüşme kontrolü.
- Yeni yüzeyler mevcut `concrete_*`, `wood_planks`, `rusty_steel`, `cast_metal`, `pegboard` texture/normal setleriyle üretilir. Dış avlu yüzeyleri değişmemeli; FPS ve draw call bilgisi gerçek Unity testinden gelmeli.
- Mevcut üç QA açısı yeniden kullanıldı; kamera sayısı 14 olarak korunuyor. Yerel Play ve GitHub push olmadan görsellerin güncellendiği varsayılmaz.

## v15.2 — sığınak ışık geçişi (sahne 16)
- Oyun Play'de **gece**: sarkıt lamba yatak/depo/orta alanı okunur tutmalı, tezgâh ayrı sıcak ışık almalı, dışarı ışık sızmamalı.
- Soba ateşlendiğinde sağ arka köşe sıcak görünmeli; söndürülünce kapaktaki parlama ve ışık kaybolmalı, siyah metal kaplama kalmalı.
- **Gündüz**: iki iç lamba normal gün/gece döngüsüyle kapanmalı, dış avlunun ve tüccarın ışığı etkilenmemeli.
- `02_shelter_interior`, `08_stove_and_bed`, `14_shelter_storage` mevcut açılar yeniden kullanılır. Otomatik QA sadece **11:00 gündüz** çektiği için gece testi ayrıca doğrudan Unity Play/Game penceresinde yapılmalı.
- FPS/ms değerlerini iç/dış, gündüz/gece, soba açık/kapalı durumlarında karşılaştır. GitHub CI sayısal FPS sonucu üretemez.
