# Meshy / 3D Varlık Kuralları
- Her varlık için kısa teknik tanım: amaç, boyut (metre), hedef motor, parça ayrımı, pivot, collider, poligon/texture bütçesi.
- Sökülebilir mekanik parçaları mümkün olduğunca bağımsız GLB/FBX nesneleri olarak temin et; tek mesh nesneye zorla yapıştırma.
- Görsel kalite kadar oyun içi maliyet, UV, doku çözünürlüğü, doğru yüzey normalleri ve telif/tescil durumu önemlidir.
- Her üretilen modele benzersiz asset ID, kaynak, prompt özeti, revizyon ve lisans notu ekle.
- Üretilen modeli otomatik kabul etme: ölçek, fizik, bağlantı, performans ve oyun içi ekran görüntüsüyle doğrula.
- Önce prototip geometrisi, sonra ihtiyaç netleşince yüksek kaliteli asset; gereksiz yeniden üretimden kaçın.

## Sahne otomatik güncellenir (menü yok)
- `Assets/Editor/AfterrustSceneSync.cs`: Unity açılınca / Pull sonrası betikler yeniden yüklenince / sahne açılınca `AfterrustPrototype.unity` sahnesinin sürümüne (`[Scene Build Info]` nesnesi) bakar; eksik adımları bir kez uygular ve sahneyi kaydeder. Kullanıcının menü çalıştırması gerekmez.
- Yeni içerik eklerken: `Latest` sürümünü artır, yeni `if (current < N)` bloğuna adımı ekle. Adımlar tekrar çalıştırılabilir (aynı adlı kökü silip yeniden kurar).
- Sürüm 1: avlu ve çit (`ScrapyardEnvironmentBuilder`), tamir atölyesi (`RepairShopBuilder`), yürüyen NPC'ler (scavenger, Character1, Neutral Figure; malzemeler FBX'e gömülü dokular yerine çıkarılmış dokulardan), eski kutu/silindir nesnelerin kaldırılması, Golden Hour ışığı + post-processing.

## Tamir atölyesi
- 8,5 × 6 m, ön tarafı oyuncuya bakan küçük harap dükkân: boyası dökülmüş briket duvarlar (sol ön köşe yıkık, sağda tahta çakılı kırık pencere), bir levhası eksik oluklu sac çatı (motor sehpasına güneş düşer), yarı açık kepenk, "TAMİR ATÖLYESİ" tabelası.
- İç: tezgâh + alet panosu, raf, motor sehpası (sökülebilir alternatör ve kablosu), sandık üstünde akü ve kıskacı, tüccar tezgâhı (`Trade Post - E to Sell`, E ile sat / F ile kablo kesici).
- Dış: lastik yığını, variller, palet yığını, hurda yığını (`Loose Metal`), konteynerler, sarkan titrek ampul ve duvar lambaları.
- Oyun nesneleri (sökülebilir parçalar, tüccar) bileşen ve verilerini korur; yalnız görünümleri değişti.

## Dokular
- Prosedürel, dikişsiz, özgün: `Assets/Arts/Environment/Textures/` (toprak, beton zemin/duvar, briket, oluklu sac, paslı çelik, ahşap, lastik, boyalı metal, alet panosu, döküm metal, tabela). Üretici: `python3 tools/textures/generate_scrapyard_textures.py Assets/Arts/Environment/Textures 1024`.
- Karakter dokuları FBX'ten çıkarıldı: `Characters/character1/Textures/`, `Characters/1/Textures/`.

## Işık ve post-processing
- Golden Hour (varsayılan): alçak sıcak güneş, prosedürel gökyüzü, üç tonlu ortam ışığı, tozlu sis; Overcast alternatifi `ScrapyardAtmosphereSetup.ApplyOvercast` ile.
- Post Processing v2: ACES HDR renk, bloom, MSVO AO, vignette, hafif gren. Linear renk uzayı, MSAA 4x.
- Önizlemeler (Blender, yaklaşık): `QA/Environment/`.

## Hava ve gün-gece (sahne sürümü 3)
- `World Environment` (`DayNightWeather`): güneş/ay, prosedürel gökyüzü (çalışırken kopyası), iki bulut katmanı (`storm_clouds.png`, 55 m ve 85 m; sis onları silmesin diye alçak), oyuncuyu izleyen yağmur parçacıkları (dünyayla çarpışır, çatı altında yağmaz), şimşek ışığı, karanlıkta lambalar.
- Ayarlar Inspector'da: gün uzunluğu, kuru/yağmurlu süre aralıkları, şimşek aralığı, yağmur yoğunluğu.
- Kalıcı kural: modüler parçalar **birbirine değer, üst üste binmez**. Aynı yöne bakan çakışık yüzeyler z-fighting (titreme) yapar; parçalar arasında en az 1–1,5 cm boşluk ya da açık bir iç içe geçme olmalı.
