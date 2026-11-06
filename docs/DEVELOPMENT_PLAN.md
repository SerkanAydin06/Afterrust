# Afterrust — Canlı Yapım Planı

**Tek çalışma listesi:** Bu dosya geliştirmenin öncelik ve ilerleme kaynağıdır. Önceki `docs/ROADMAP.md` uzun vadeli hedefleri açıklar. Son gözden geçirme: **2026-11-07**. Etkin kapsam: **güvenli bölgeyi tamamlamak; sonrasında dış keşfe geçmek**.

## Çalışma kuralları

- Her Afterrust görevinde **önce bu dosyayı oku**; GitHub `main`, son Unity QA manifestosu, mevcut kod ve varsa hata raporuyla karşılaştır.
- Yeni hata, ihtiyaç veya bağımlılık çıkarsa **öncelik sırasını ve görevleri aynı PR'da güncelle**. Tamamlanmış görev silinmez; gerekirse yeniden açılır.
- Öncelik: **P0 oyun açılışı/veri kaybı/derleme > P1 oynanış ve FPS > P2 görsel kalite > P3 dış dünya/içerik**. Ancak kullanıcının açıkça seçtiği kapsamı önceliklendir.
- Her görev için `[ ]` bekliyor, `[~]` kodlandı ama Unity'de henüz doğrulanmadı, `[x]` doğrulandı biçimini kullan. Markdown kutucuğu yalnız `[ ]` ve `[x]` desteklediği için `[~]` özel ilerleme göstergesidir.
- Her PR'da **ilgili görev ID'si**, değişiklik özeti ve kontrol sonucu olsun. Görev ancak **Unity Editor gerçek Play, görünüm/etkileşim, geri yükleme ve performans** kabul koşulları gözden geçirilince `[x]` olur. GitHub statik CI tek başına yeterli değildir.
- Oyuncu kayıtları, mevcut sahne hiyerarşisi ve malzeme paylaşımı korunur. Görsel işler **yeniden kurma gerektirmeyen, idempotent** migration ile uygulanır. Dekor gözle kontrol edilmeli; düşük poligon, ortak materyal ve az sayıda ışık tercih edilir.
- Sonraki oturumda veya yeni kod PR'ında bu listeyi yeniden değerlendir ve ilk uygulanabilir açık maddeye devam et. **Sohbet/otomasyon çalışmıyorken kendiliğinden kod ilerlemesi yapıldığı varsayılmaz.**
- QA kaynağı `QA/Screenshots/latest/manifest.json` ve 14 açıdır; yeni görüntüler ancak Unity çalıştırılıp GitHub'a gönderildikten sonra geçerli sayılır. Gerçek FPS değeri oyun içi HUD/Profiler'dan gelmelidir; uydurulmaz.

## Etkin sprint: v15 — Sığınak içi kalite

| ID | Durum | Öncelik | Görev ve kabul ölçütü |
|---|---|---|---|
| AR-1501 | [~] | P2 | **İç mekân materyal geçişi:** yalnız sığınağın zemin/duvar/kapı, tezgâh, raf, yatak, masa, soba ve sandığına tutarlı kirli beton/ahşap/metal/kumaş materyalleri; normal haritalar ve paylaşılan asset. Dış mekan değişmez; collider, NPC, eşya ve kayıtlar korunur. Kodlandı; Unity gündüz/gece QA bekliyor. |
| AR-1502 | [ ] | P2 | **İç aydınlatma:** sıcak/soğuk bölgeler, soba etkisi, gece okunabilirlik; en az ek ışık ve shadow maliyeti. |
| AR-1503 | [ ] | P2 | **İç detay ve düzen:** raf, alet, kablo, kırık duvar geçişi; objeler oyuncu yolunu kapatmaz, mümkün olduğunca collider kullanılmaz. |
| AR-1504 | [ ] | P1 | **Etkileşim nesneleri:** depo, yatak, soba, tezgâhın görsel okunabilirliği, tutarlı kullanım ipuçları; yeni sistemi yanlışlıkla tetiklemez. |
| AR-1505 | [ ] | P1 | **v15 kalite kapısı:** Unity compile/Play, 02/08/14 QA görüntüleri, dururken/yürürken/yağmurda FPS-ms, F5/F9 depo ve kapı. Hata varsa önce düzelt. |

## Sonraki sprint: v16 — Güvenli avlu dışı

| ID | Durum | Öncelik | Görev |
|---|---|---|---|
| AR-1601 | [ ] | P2 | Tüccar tezgâhı çatısı/direkleri ve tezgâh objeleri; malzeme ve yerleşim. |
| AR-1602 | [ ] | P2 | Avlu zemin/çit/kapı/yağmur varili yüzeyleri, yağış sonrası görünüm; fiziksel taşma yok. |
| AR-1603 | [ ] | P1 | Yağmur sadece dışarıda; açık varilin çok yavaş dolması ve su seviyesinin gerçek Play doğrulaması. |
| AR-1604 | [ ] | P1 | Güvenli alan v16 gündüz/gece/yağmur FPS ve etkileşim QA. |

## Sonraki sprint: v17 — Etkileşim ve arayüz son pass

| ID | Durum | Öncelik | Görev |
|---|---|---|---|
| AR-1701 | [ ] | P1 | Soba yanma efekti, yatak/uyku, depo açılış hissi, tezgâh etkileşimi. |
| AR-1702 | [ ] | P1 | Tüccar ve depo panellerini daha okunaklı/ölçeklenebilir uGUI ile modernleştirme. |
| AR-1703 | [ ] | P1 | Ses dengesi, atmosfer döngüsüzlüğü, görsel kalite/performance QA. |

## Sürekli kalite ve teknik borç

| ID | Durum | Öncelik | Görev |
|---|---|---|---|
| AR-Q001 | [ ] | P0 | Lisanslı Unity EditMode CI etkinleştirilerek gerçek C# derleme ve EditMode testleri doğrulansın. Bugün GitHub yalnız statik proje ve belge kontrolü çalıştırıyor. |
| AR-Q002 | [ ] | P1 | Gerçek FPS değerleri (sabit/yürüme/koşma/yağmur/sığınak) ve Unity Profiler ölçümleri toplanıp bütçe oluşturulsun. |
| AR-Q003 | [ ] | P1 | Oyun kaydı geriye uyumluluk ve sandık-tüccar etkileşimlerinin gerçek Play'de testi. |
| AR-Q004 | [ ] | P1 | Mevcut QA kameralarının nesneyi gerçekten gösterdiği doğrulansın; karanlık/boş/yanlış kadraj yeniden ayarlansın. |

## Kapsam sırası

`v15 sığınak → v16 güvenli avlu → v17 kullanım/UI → güvenli alan kalite kapısı → dış keşif ve daha büyük dünya`.

**Son güncelleme kaydı:** 2026-11-07 — AR-1501 kodu başlatıldı. Başlangıç tetikleyicisi: kullanıcı sırayla, uzun açıklama olmadan, yapılanlar listesiyle geliştirme istedi.
