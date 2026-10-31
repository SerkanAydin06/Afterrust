# Afterrust — Unity 6.3 LTS ilk açılış
## Unity Hub
1. GitHub deposunu bilgisayara klonla (GitHub Desktop: File > Clone repository > SerkanAydin06/Afterrust).
2. Unity Hub > Projects > Add project from disk > klonlanan `Afterrust` klasörünü seç.
3. Unity 6.3 LTS editörünü seç. Projedeki `ProjectVersion.txt` ilk açılış için 6000.3.27f2 tabanını kullanır. Projenin sabitlenmiş sürümü 6000.3.27f2'dir; Unity Hub'da bunu seç.
4. Package Manager bağımlılıkları indirdikten sonra script import/compilation bitmesini bekle.
5. İlk açılışta editör `Assets/Scenes/AfterrustPrototype.unity` sahnesini otomatik üretir. Üretmezse üst menüden `Afterrust > Create or Rebuild Prototype Scene` seç.
6. Play'e bas. WASD yürü; fareyle bak; Shift koş; Space zıpla; E hedef parçayı sök veya satış noktasına sat; ESC fareyi serbest bırak.
7. Oluşturulan sahne ve .meta dosyaları GitHub'a eklenebilir.

## İlk durumda neler var?
- Basit yer tutucu geometrili sanayi alanı.
- İlk kişi hareket/kamera, raycast etkileşimi.
- Üç hurda nesnesi: akü, alternatör, metal.
- Basit ağırlık, durum ve kredi; E ile toplama ve satış.
- Henüz ayrıntılı söküm, gerçek otomobil, survival, kayıt/yükleme, 3D assetler, araç sürüşü veya açık dünya bulunmuyor.

## Olası ilk açılış engelleri
- Paket indirme için internet bağlantısı gerekir.
- Unity Input System etkinleştirilsin uyarısı çıkarsa editörü yeniden başlat.
- Bu kod Unity Editor içinde henüz derlenip test edilmedi; Console hataları GitHub issue'ya eklenmeli.
- Windows build/Unity CI için lisans, kurulu modüller ve yetkili runner ayrıca gerekli.

## Ekip notu
`AGENTS.md` ve `docs/AI_EFFICIENCY.md` görevden önce okunur. Bütün görevler küçük ve test edilebilir PR'larla ilerler.
