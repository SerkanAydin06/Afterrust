# Meshy karakter testi — Scavenger 01

## Mevcut varlık doğrulaması
- GitHub'a kullanıcı tarafından `Assets/Arts/Characters/Scavenger/Meshy_AI_scrapyard_mechanic_mo_1031200119_image-to-3d-texture.glb` yüklendi.
- Dosya Git LFS'de; pointer gerçek modelin **53.603.452 bayt** olduğunu bildiriyor. Gerçek modelin yerel bilgisayara indirilmesi için Git LFS gerekir.
- Modelin gerçek geometrisi ve dokusu henüz görsel olarak değerlendirilmedi.
- Karakter şu an **riglenmemiş**, statik NPC gösterim testi için kullanılacak.

## Unity otomatik içe aktarma
- Projeye Unity'nin `com.unity.cloud.gltfast` **6.20.0** kararlı paketi eklendi. Unity Package Manager internete bağlanıp bağımlılıkları indirmeli.
- Editor, `Assets/Arts/Characters/Scavenger/` altındaki GLB'yi veya FBX'i model varlığı olarak tanırsa, `Afterrust > Create or Rebuild Prototype Scene` karakteri test kaidesine yerleştirir.
- Yer: (8,0,8). Modelin yüksekliği yaklaşık 1,75 m olacak şekilde ölçeklenir.
- GLB import tamamlanmazsa veya Git LFS indirmemişse otomatik yer tutucu görünür.

## Unity'de test et
1. GitHub Desktop'ta **Pull origin**.
2. Unity'ye dönüp paketlerin yüklenmesini ve projenin derlenmesini bekle.
3. Project panelinden `Assets/Arts/Characters/Scavenger` içindeki GLB'nin Unity tarafından model varlığı olarak tanındığını kontrol et.
4. **Afterrust > Create or Rebuild Prototype Scene** (mevcut sahneyi yeniden yazar), ardından Play.
5. `QA/Screenshots/latest/05_character_test.png` görselini GitHub'a gönder.

## Kontrol kriterleri
- Statik karakter görünüyor mu? Kayıp parça veya pembe malzeme var mı?
- Yaklaşık 1,75 m ölçü korunmuş mu ve ayakları platforma değiyor mu?
- Oyun için poligon sayısı makul mü? Rigging/animasyon daha sonra ayrı değerlendirilecek.
- Sahne oluşturma Console hatası varsa ekran görüntüsü ya da hata çıktısı üzerinden düzeltilecek.

## İlerideki üretim hedefi
Gerçekçi, mat dokulu, yıpranmış iş kıyafetleri, alet kemeri ve sırt çantası olan bir hurdacı NPC. İlk görsel testten sonra ağırlıklar/rigging bozuksa ayrıca düzeltilir.
