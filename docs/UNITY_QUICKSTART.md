# Afterrust — Unity 6.3 LTS hızlı başlangıç

1. GitHub'dan `SerkanAydin06/Afterrust` deposunu klonla veya güncelle.
2. Unity Hub'da klasörü aç ve **6000.3.27f2** editörünü kullan (`ProjectSettings/ProjectVersion.txt`).
3. Paketler ve C# derlemesi bitince `Assets/Scenes/AfterrustPrototype.unity` sahnesini aç.
4. `AfterrustSceneSync` sahne sürümünü kontrol eder ve eksik yapım adımlarını otomatik uygular (mevcut hedef v11). Console'da **`[Afterrust] Scene updated to version 11 and saved.`** mesajını gör. Hata görürsen önce düzelt; sahne güncellenmeden Play engellenir.
5. Play aç ve ana sistemleri doğrula: hareket, yağma, açlık/susuzluk, sığınak kapısı, tezgâh, soba, uyku, gece/yağmur, F5/F9 kayıt ve yükleme.
6. `Window > General > Test Runner > EditMode` içindeki testleri çalıştır. Testlerin varlığı, çalıştırıldıklarını veya geçtiklerini tek başına kanıtlamaz.
7. Gece güvenli bölgeyi kontrol et: çevre konturları seçilebilmeli, tüccar tezgâhında küçük lamba yanmalı ve GÜVENLİ BÖLGE yazısı tabela içine sığmalı. Yenilenen QA ekran görüntüleri gündüz 11:00'de alınmalı; çekimden sonra oyun zamanı aynı kalmalı.
8. Güvenli bölgenin güney çitindeki iki kanatlı metal kapının kapalı başladığını kontrol et. Bir kanada bakıp **E** ile her iki kanadı birlikte açıp kapat; F5 ile açık halde kaydet, F9 ile yükleyip durumu kontrol et. Tüccar tentesinin dört direğe oturduğunu ve yatay desteklerini incele. Adım sesinin diğer sesleri bastırmadığını doğrula.
9. Sağanak sırasında sığınağa gir: kapalı çatının altında damla görünmemeli, kapıdan bakıldığında dışarıdaki yağmur sürmelidir. Tüccar tentesini de kontrol et. Rüzgâr/yağmur sesinde tekrar/kesinti dinle.
10. Güvenli avluda eski üç varil yerine tek, dik ve üstü açık varil göründüğünü kontrol et. İlk başta boş olmalı. Inspector'da `World Environment > DayNightWeather > raining` açıkken gerçek zamanlı doluluk artışını izle: %100 için toplam 900 saniye yağış gerekir. Yağmuru kapatınca seviye sabit kalmalı. En az %34 olunca `E` basılı tutarak kirli su al; seviye düşmeli. F5/F9 ile kısmi doluluğun korunduğunu kontrol et.
11. Güney kapısından avluyu terk et, yardım büfesini veya konteyneri ara, kimyasal sızıntıya yaklaşmadan sığınağa dön. Adım/yağmur/rüzgâr ve üretim seslerini kontrol et.
12. `QA/Screenshots/latest/` altında **bu sahne sürümüne ait** PNG'lerin üretildiğini kontrol et. Gerekli sahne, mesh, prefab, .meta ve QA değişikliklerini GitHub'a commit/push et.

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
