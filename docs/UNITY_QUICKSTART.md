# Afterrust — Unity 6.3 LTS hızlı başlangıç

1. GitHub'dan `SerkanAydin06/Afterrust` deposunu klonla veya güncelle.
2. Unity Hub'da klasörü aç ve **6000.3.27f2** editörünü kullan (`ProjectSettings/ProjectVersion.txt`).
3. Paketler ve C# derlemesi bitince `Assets/Scenes/AfterrustPrototype.unity` sahnesini aç.
4. `AfterrustSceneSync` sahne sürümünü kontrol eder ve eksik yapım adımlarını otomatik uygular (mevcut hedef v6). Console'da **`[Afterrust] Scene updated to version 6 and saved.`** mesajını gör. Hata görürsen önce düzelt; sahne güncellenmeden Play engellenir.
5. Play aç ve ana sistemleri doğrula: hareket, yağma, açlık/susuzluk, sığınak kapısı, tezgâh, soba, uyku, gece/yağmur, F5/F9 kayıt ve yükleme.
6. `Window > General > Test Runner > EditMode` içindeki testleri çalıştır. Testlerin varlığı, çalıştırıldıklarını veya geçtiklerini tek başına kanıtlamaz.
7. `QA/Screenshots/latest/` altında **bu sahne sürümüne ait** PNG'lerin üretildiğini kontrol et. Gerekli sahne, mesh, prefab, .meta ve QA değişikliklerini GitHub'a commit/push et.

## Kontroller
- **WASD** hareket; fareyle bak; **Shift** koş; **Space** zıpla.
- **E** etkileşim; yağmalanabilir kapta basılı tut.
- **Tab** envanter; **1** ye, **2** iç, **3** bandaj; **L** fener.
- **F** tüccarda kablo kesici satın al; **F5** kaydet, **F9** yükle.
- **Esc** paneli kapat; ölüm ekranında **R** son kayıttan devam, **N** yeni oyun.

## Bilinen sınırlar
- GitHub Actions statik dosya kontrolü, Unity derlemesinin yerine geçmez. Opsiyonel GameCI EditMode işi lisans ve `UNITY_CI_ENABLED` değişkeni gerektirir.
- GitHub'daki eski sahne / eski QA fotoğrafları yeni kodun Unity'de çalıştığını kanıtlamaz.
- Prototip bir avlu ve sığınak etrafında; kesintisiz açık dünya, düşman AI, nihai UI ve kapsamlı ses sistemi henüz tamamlanmadı.
- Kayıtlar `Application.persistentDataPath/afterrust_save.json` dosyasında tutulur; tamamlanmış önceki kayıt `.bak` olarak korunur.
