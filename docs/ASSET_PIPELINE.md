# Meshy / 3D Varlık Kuralları
- Her varlık için kısa teknik tanım: amaç, boyut (metre), hedef motor, parça ayrımı, pivot, collider, poligon/texture bütçesi.
- Sökülebilir mekanik parçaları mümkün olduğunca bağımsız GLB/FBX nesneleri olarak temin et; tek mesh nesneye zorla yapıştırma.
- Görsel kalite kadar oyun içi maliyet, UV, doku çözünürlüğü, doğru yüzey normalleri ve telif/tescil durumu önemlidir.
- Her üretilen modele benzersiz asset ID, kaynak, prompt özeti, revizyon ve lisans notu ekle.
- Üretilen modeli otomatik kabul etme: ölçek, fizik, bağlantı, performans ve oyun içi ekran görüntüsüyle doğrula.
- Önce prototip geometrisi, sonra ihtiyaç netleşince yüksek kaliteli asset; gereksiz yeniden üretimden kaçın.

## Modüler hurdalık çevresi (prosedürel dokular)
- Dokular: `Assets/Arts/Environment/Textures/` — `ground_dirt`, `concrete_slab`, `concrete_wall`, `corrugated_rust`, `rusty_steel` (albedo + OpenGL normal, 1024 px, dikişsiz). Özgün, kodla üretildi: `python3 tools/textures/generate_scrapyard_textures.py Assets/Arts/Environment/Textures 1024`. Lisans sorunu yok.
- Unity: **Afterrust > Build Scrapyard Environment** → malzemeler (`Materials/`), metre-UV modüler mesh'ler (`Meshes/`), prefablar (`Prefabs/`: beton zemin 4×4, beton duvar 4×3, kolon, oluklu sac çit 4×2,4, çelik direk) ve yerleşim (beton çalışma alanı, kapılı çevre çiti, atölye köşesi). Mevcut zemin ve harabe blokları yalnız yeni malzemeyle kaplanır; karakter/araç/oyun nesnelerine dokunulmaz. Tekrar çalıştırmak sadece `Scrapyard Environment` kökünü yeniler.
- Ağ izni verilirse aynı dosya adlarıyla ambientCG/Poly Haven (CC0) dokuları konup araç yeniden çalıştırılarak değiştirilebilir.
- Önizleme (Blender): `QA/Environment/`.

## Işık ve atmosfer
- **Afterrust > Lighting > Golden Hour (default)**: alçak sıcak akşam güneşi (yumuşak gölge), prosedürel gökyüzü (`Materials/SKY_Scrapyard.mat`), üç tonlu ortam ışığı, tozlu üstel sis, kameralar gökyüzüne geçer, atölye duvarlarında ve kapı direklerinde 5 sıcak lamba (`Scrapyard Lamps`).
- **Afterrust > Lighting > Overcast**: gri, kasvetli, yoğun sis; lambalar daha belirgin.
- Yalnız RenderSettings, `Sun`, kameralar ve `Scrapyard Lamps` değişir. `Create or Rebuild Prototype Scene` eski ışığı geri yükler; ardından preset tekrar uygulanmalı.
- Önizleme (Blender, yaklaşık): `QA/Environment/preview_golden_hour.jpg`.
