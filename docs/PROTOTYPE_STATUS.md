# Afterrust — Prototip Durumu (2026-11-06)

## Yön
Distopik hayatta kalma (bkz. `DECISIONS.md`, `GAME_DESIGN.md` v2.0): yaşamsal ihtiyaçlar → yağma → üretim → gelişim.

## Oyunda olanlar (kodda; Unity içinde henüz test edilmedi)
- Sığınak: avlu, çit, harap sığınak; altın saat ışığı + post-processing.
- Yaşamsal değerler: sağlık, açlık (~25 dk), susuzluk (~15 dk), dayanıklılık (koşma/zıplama); açlık/susuzluk sıfırda sağlık düşer, ölüm ekranı, R ile yeniden başlama.
- Envanter: ağırlık sınırı (35 kg, büyük çanta ile 50 kg), Tab ile envanter, 1 ye / 2 iç / 3 bandaj.
- Yağma: karton kutular, alet kutusu, sandık, variller, konteynerler (E basılı tut); her kap bir kez rastgele dolar, sığmayanlar içeride kalır.
- Su: yağmur varili kirli su verir; tezgâhta kömür + bezle süzülür.
- Üretim (tezgâh): bandaj, temiz su, kablo kesici, büyük sırt çantası, alternatör geri dönüşümü.
- Söküm: akü kıskacı → akü, alternatör kablosu (kablo kesici) → alternatör.
- Tüccar: yalnız hurda/söküm parçası satın alır; 60 krediye kablo kesici.
- Kayıt/yükleme: F5 kaydet, F9 son kayda dön, 3 dakikada bir ve çıkışta otomatik kayıt, açılışta kayıttan devam; ölünce R son kayıt, N yeni oyun. Kaydedilenler: konum, değerler, envanter/aletler, aranmış kaplar, sökülen parçalar, yağmur varili, saat ve hava. Dosya: `Application.persistentDataPath/afterrust_save.json`.
- Gün-gece: 24 dakikada bir gün, 07:30'da başlar; gece ay ışığı ve çok karanlık, lambalar karanlıkta yanar, L ile fener.
- Hava: sürekli kapalı ve kasvetli; koyu fırtına bulutları kayar, ara sıra sağanak (çatılara çarpıp kaybolan yağmur), sağanakta şimşek; yağmurda varil 4 kat hızlı dolar. Soğuk "Storm" renk düzenlemesi.
- Duvar birleşimlerindeki titreme (üst üste binen aynı düzlemli yüzeyler) giderildi: duvarlar artık birbirine yalnız değiyor.
- Sığınak: tamir atölyesi yerine kapısı açılıp kapanan sığınak (yatak, masa, soba); motor dışarıda hurda köşesinde, tüccar tenteli takas tezgâhında.
- Isı ve enerji değerleri; soba (odun/kömür, ısıtma, duman, pişirme: kaynat, ısıt, kömür yap); yatakta uyku (gece sabaha kadar, gündüz şekerleme, sonra otomatik kayıt); odun yığını ve palet yığını.
- Ufuk: sise gömülen yıkık şehir siluetleri, elektrik direkleri, su kulesi; zemin ufka kadar. Lamba ışığı duvardan sızmıyor. Hurdacı artık tüccar; diğer iki karakter kaldırıldı.
- Sahne her Pull sonrası otomatik güncellenir (`AfterrustSceneSync`, sürüm 7).

## Test
- EditMode: `SalvageRulesTests`, `SurvivalRulesTests`, `SaveAndWorldTests`, `FireAndSleepTests`, `ExpeditionRulesTests` (Window > General > Test Runner). Bu ortamda Unity yok; testler henüz çalıştırılmadı.

## Sonraki
Unity testleri, uGUI arayüz ve eşya ikonları, profesyonel ses/miks, çoklu keşif bölgeleri, sığınak yükseltmeleri, yapay zekâ tehditleri.

## 6 Kasım — teknik sağlamlaştırma
- Kayıtlar önce geçici dosyaya yazılır; önceki kayıt `.bak` olarak korunur. Ana kayıt bozuksa yedekten geri okumaya çalışılır; yeni oyun iki dosyayı da temizler.
- Üretimde farklı kütleli aynı eşya türlerinin ağırlık hesabı ile envanterden çıkarılma sırası eşleştirildi; hatalı tarifler güvenli reddedilir.
- `AfterrustSceneSync` güncel olmayan sahnede Play'i engeller.
- QA planı mevcut v6 çevre/sığınak/tüccara göre sekiz kareye güncellendi; mevcut GitHub PNG'leri otomatik olarak güncellenmez.
- GitHub Actions statik kontrollerine ek doğrulama eklendi. Unity EditMode işi **yalnızca lisans ve UNITY_CI_ENABLED yapılandırılırsa** çalışır.
- Bu değişiklikler GitHub kod incelemesine dayalıdır; yerel Unity derlemesi ve sahne/screenshot yenilemesi henüz yapılmadı.

## 6 Kasım — ilk keşif ve ses (sahne v7)
- Güney avlu kapısından yürüyerek ulaşılabilen iki yağmalanabilir yapı (harap yardım büfesi ve açık nakliye konteyneri), yeni erzak/sarf ve değerli hurda sandıkları.
- Görünür kimyasal sızıntı: oyuncu yaklaştıkça sağlık kaybı, ekranda uyarı; bölge yanından güvenle geçilebilir.
- Basit keşif görevi: kapıdan çık → yardım noktasında bir kap ara → sığınağa dön; aranan kapların durumu mevcut SaveSystem ile saklanır.
- Procedural prototip ses sistemi: adımlar, rüzgâr, yağmur, etkileşim, üretim ve tehlike uyarısı; sonradan kaliteli WAV/OGG klipler takılabilir.
- Otomatik sahne güncelleme v7, üç yeni QA kamera açısı (toplam 11), yeni script GUID'leri ve temel regresyon testleri.
- **Kod GitHub'a eklendi; Unity Editor'da derleme, sahne üretimi, gerçek oynanış, ses ve yeni PNG görüntüleri henüz doğrulanmadı.**

## 6 Kasım — yağmur ve ses düzeltmesi (sahne v8)
- Sığınağın kaplamalı çatı parçaları fiziken yağmuru engellemiyordu. Çatıya ve tüccar tentesine görünmez sürekli çarpışma yüzeyi eklendi. Eski v7 sahnesi, eşyaları/kapıları yeniden üretmeden v8'e taşınır.
- Yağmur sistemi, her damlanın düşüş sütununa raycast yaparak çatı ve objelerin **üstünde** sonlanan ömür verir. Sığınaktan dışarı bakıldığında dışarıda yağmur görünmeye devam eder; damlalar çatının altına geçmez.
- Rüzgâr/yağmur seslerinde aynı 2–3 saniyelik kaydın tekrar etmesi kaldırıldı; PCM akışı sürekli yeni ses üretir. İçeride yağmur/rüzgâr sesleri filtrelenir, adımlarda farklı ses ve perde varyasyonları vardır.
- Yağmur ve sürekli ses üretimi için EditMode regresyon testleri eklendi. Görsel/işitsel çalışma ve performans hâlâ Unity Editor'da doğrulanmalıdır.
