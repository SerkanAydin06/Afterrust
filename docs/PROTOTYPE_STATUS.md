# Afterrust — Prototip Durumu (2026-11-05)

## Yön
Distopik hayatta kalma (bkz. `DECISIONS.md`, `GAME_DESIGN.md` v2.0): yaşamsal ihtiyaçlar → yağma → üretim → gelişim.

## Oyunda olanlar (kodda; Unity içinde henüz test edilmedi)
- Sığınak: avlu, çit, harap tamir atölyesi; altın saat ışığı + post-processing.
- Yaşamsal değerler: sağlık, açlık (~25 dk), susuzluk (~15 dk), dayanıklılık (koşma/zıplama); açlık/susuzluk sıfırda sağlık düşer, ölüm ekranı, R ile yeniden başlama.
- Envanter: ağırlık sınırı (35 kg, büyük çanta ile 50 kg), Tab ile envanter, 1 ye / 2 iç / 3 bandaj.
- Yağma: karton kutular, alet kutusu, sandık, variller, konteynerler (E basılı tut); her kap bir kez rastgele dolar, sığmayanlar içeride kalır.
- Su: yağmur varili kirli su verir; tezgâhta kömür + bezle süzülür.
- Üretim (tezgâh): bandaj, temiz su, kablo kesici, büyük sırt çantası, alternatör geri dönüşümü.
- Söküm: akü kıskacı → akü, alternatör kablosu (kablo kesici) → alternatör.
- Tüccar: yalnız hurda/söküm parçası satın alır; 60 krediye kablo kesici.
- Sahne her Pull sonrası otomatik güncellenir (`AfterrustSceneSync`, sürüm 2).

## Test
- EditMode: `SalvageRulesTests`, `SurvivalRulesTests` (Window > General > Test Runner). Bu ortamda Unity yok; testler henüz çalıştırılmadı.

## Sonraki
Kayıt/yükleme, ses, gün-gece, uyku/sıcaklık, ocak/yemek pişirme, uGUI arayüz ve eşya ikonları, avlu dışı keşif bölgeleri, tehlikeler.
