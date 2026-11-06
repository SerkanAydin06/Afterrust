# Afterrust — Prototip Durumu (2026-11-05)

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
- Sahne her Pull sonrası otomatik güncellenir (`AfterrustSceneSync`, sürüm 6).

## Test
- EditMode: `SalvageRulesTests`, `SurvivalRulesTests`, `SaveAndWorldTests`, `FireAndSleepTests` (Window > General > Test Runner). Bu ortamda Unity yok; testler henüz çalıştırılmadı.

## Sonraki
Kayıt/yükleme, ses, gün-gece, uyku/sıcaklık, ocak/yemek pişirme, uGUI arayüz ve eşya ikonları, avlu dışı keşif bölgeleri, tehlikeler.
