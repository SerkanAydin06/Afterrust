# Afterrust — Özellik Öncelikleri ve Yol Haritası
Durum: Planlandı; uygulama başlamadı.

## P0: Temel prototip
- Unity proje iskeleti, GitHub Actions derleme/doğrulama.
- FPS hareket, kamera, etkileşim.
- Veri odaklı nesne/parça/söküm sistemi.
- 3 ayrıntılı sökülebilir nesne sınıfı, yaklaşık 20 parça tanımı.
- Envanter, ağırlık, doğru alet kısıtları.
- Küçük tamirhane, basit tamir masası, 1 tüccar.
- Satış/alış, basit ilerleme, kayıt/yükleme.
- Geçici geometrik modeller ile çalışır dikey dilim.

## P1: Oyun döngüsü
- Açlık/susuzluk/sağlık ve çevresel riskler.
- Sipariş görevleri, yerleşimlere ilk bağ.
- Temel modüler üs yükseltmeleri.
- Meshy modüler model üretimi/optimizasyonu ve oyun içi kalite.

## P2: Açık dünya ve tehlikeler
- Bölgesel yükleme ve kesintisiz alan genişleme.
- Yağmacı AI / çatışma / saklanma ve kaçış.
- Sürülebilir araç, yakıt, yükleme ve bakım.
- Yerleşim ekonomisi, itibar, daha çok keşif bölgesi.

## P3: Uzun vadeli
- Ağır endüstriyel kurtarma, vinç/lojistik.
- Yardımcı çalışanlar ve işletme yönetimi.
- Yeniden inşa projeleri, ekosistem ve ekonomi sonuçları.

## Prototip kabul koşulları
1. Oyunu açma/kapatma, kaydetme/yükleme doğru çalışır.
2. En az bir araç veya makinenin bağımlı parçaları uygun aletlerle sökülür.
3. Sökülen parçaların kondisyonu ve ağırlığı korunur.
4. Parça tamirhaneye taşınır ve onarılır/satılır.
5. Kazanç yeni ekipmana çevrilir.
6. Kritik hata yok; otomatik test/derleme raporu bulunur.
