# Meshy karakter testi — Scavenger 01

## Amaç
Afterrust dünyasına ait ilk **insan karakteri** (hareketsiz, NPC görünüm testi). Oyuncumuz birinci şahıs olduğu için önce karakterin ölçüsünü, malzemelerini ve Unity'ye aktarımını test ediyoruz. Animasyon/rigging için ileride ayrı görev açılabilir.

## Meshy text-to-3D istemi (İngilizce)
"Full body adult post-collapse scrapyard mechanic survivor, practical layered worn work jacket in muted olive and charcoal, faded work trousers with reinforced knees, heavy dusty work boots, simple worn gloves, utility belt with pliers and small tools, small canvas backpack, subtle grease and rust stains, realistic grounded 3D video game NPC, natural anatomy, believable functional clothing, no weapon, no logos, no readable text, no floating accessories, symmetrical neutral A-pose, centered single character, clean silhouette, full body including feet and hands, suitable for a post-apocalyptic industrial survival game."

## Önerilen üretim/aktarım
- Önce tek bir karakter; ek varyasyon üretme.
- Meshy Text to 3D → textured model (preview + refine gerekiyorsa).
- Tek karakter, neutral A-pose; mümkünse rigging uyumlu.
- Unity'nin yerel içe aktarımı için **FBX**, dokularla birlikte. GLB tek başına mevcut Unity projesinde doğrudan içe aktarılmaz; ayrıca importer gerektirir.
- Hedef boy: 1.75 metre. Model karmaşıklığı prototip hedefi için tercihen 10–30 bin üçgen (gerçek geometriyi ölç; garanti değil).
- Yüzey: mat/yarı mat; aşırı parlak PBR yok. Dokuların lisans ve kaynak kayıtları tutulur.
- FBX adı: `Scavenger.fbx`.
- Hedef yol: `Assets/Art/Characters/Scavenger/Scavenger.fbx`; doku ve materyaller de bu klasörde.
- Unity'de `Afterrust > Create or Rebuild Prototype Scene` çalıştırınca FBX model, (8,0,8) noktasındaki test platformuna otomatik yerleştirilir. Dosya yoksa yer tutucu kapsül görünür.
- Unity Editor derlemesi ve model görüntüsü otomatik ekran görüntüsü raporuyla doğrulanır.

## Kontrol listesi
1. Model sahnede gözüküyor mu? (Pembe/mor malzeme var mı?)
2. Yaklaşık 1.75 metre boyunda mı?
3. Ayaklar platforma değiyor mu?
4. Modelde kopuk eller, çakışan giysiler veya aşırı poligon var mı?
5. Ayrı rigging isteniyorsa modelin nötr pozu buna uygun mu?

## Gerçek durum
Meshy CLI bu çalışma ortamında zaman aşımına uğradı; **henüz gerçek Meshy modeli üretilmedi**. Bu dosya ve Unity'ye otomatik yerleştirme kodu hazırlanmış entegrasyon aşamasıdır.
