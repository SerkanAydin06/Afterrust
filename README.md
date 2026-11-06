# Afterrust

**Çalışma adı:** Afterrust  
**Tür:** Birinci şahıs 3D • Distopik hayatta kalma • Yağma • Üretim • Kendini ve sığınağı geliştirme  
**Durum:** Oynanabilir hayatta kalma prototipi (Unity 6.3 LTS): yaşamsal değerler, yağma, üretim, söküm, sığınak, ilk keşif bölgesi ve prototip ortam sesleri.

## Oyun
Medeniyet çökmüş, geriye terk edilmiş şehirler ve az sayıda hayatta kalan kalmıştır. Oyuncu harap bir sığınakta yaşayan tek başına bir insandır: açlık ve susuzlukla mücadele eder, terk edilmiş yerleri yağmalar, bulduklarından ihtiyaç duyduğunu üretir, kendini ve sığınağını geliştirerek daha uzağa açılır.

**Onaylanan oyun yönü (2026-11-05):** distopik hayatta kalma — yaşamsal ihtiyaçlar → yağma → üretim → gelişim. Söküm malzeme kaynağıdır; ticaret ikincildir. Nihai hedef kesintisiz açık dünya.

**Kontroller:** WASD yürü, Shift koş, Space zıpla, E etkileşim (kapları aramak için basılı tut), F tüccardan alet al, Tab envanter, 1 ye, 2 iç, 3 bandaj, L fener, F5 kaydet, F9 yükle, Esc paneli kapat; ölünce R son kayıttan devam, N yeni oyun.

**İlk keşif (sahne v7; güvenli avlu geliştirilirken görevi geçici pasif):** Avlu kapısından güneye çık; harap yardım büfesi veya konteyneri yağmala. Kimyasal sızıntıdan uzak durarak bulduklarınla sığınağa dön. Görev hedefi ekranda görünür. Rüzgâr, yağmur, adım ve etkileşim sesleri için yerleşik prototip ses sistemi bulunur.

## Dokümantasyon
- [Karar günlüğü](docs/DECISIONS.md) — kesin ve öneri durumundaki kararlar
- [Oyun tasarımı](docs/GAME_DESIGN.md) — mekanikler, dünya, ekonomi ve ilerleme
- [Yol haritası](docs/ROADMAP.md) — P0–P3 ve prototip kabul koşulları
- [AI çalışma sözleşmesi](AGENTS.md) — ajanların uyması gereken kurallar
- [AI verimlilik ve öğrenme](docs/AI_EFFICIENCY.md) — minimum gereksiz token ve ölçülebilir süreç iyileştirmesi
- [Öğrenilen dersler](docs/LESSONS_LEARNED.md) — doğrulanmış teknik çözümler
- [3D varlık üretimi](docs/ASSET_PIPELINE.md) — Meshy ve asset kontrolü

## Geliştirme ilkesi
Önce küçük çalışan sürüm; kapsamlı açık dünya ve ağır lojistik daha sonra. Her özellik küçük değişiklikler, testler ve geri alınabilir Git commit'leriyle geliştirilecek.

**Marka notu:** Afterrust adının ticari kullanım uygunluğu resmî marka incelemesiyle henüz kesinleşmedi.

## Unity 6.3 LTS prototip (ilk proje iskeleti)
- [Bilgisayarda açılış adımları](docs/UNITY_QUICKSTART.md)
- [Otomasyon ve entegrasyon durumları](docs/AI_AUTOMATION_SETUP.md)
- [Unity teknik kararları](docs/TECH_STACK.md)
- [AI görev şablonu](.github/ISSUE_TEMPLATE/ai-task.yml)

**Doğrulama durumu:** Oyun kodunda hayatta kalma, yağma, üretim, sığınak, soba, uyku, kayıt ve hava sistemleri mevcut. Unity Editor derlemesi ve tüm EditMode testleri bu GitHub incelemesinde çalıştırılmadı. GitHub Actions varsayılan olarak belgeleri ve proje yapısını doğrular; lisanslı Unity EditMode işi isteğe bağlıdır (bkz. `docs/AI_AUTOMATION_SETUP.md`). Yeni sahne ve QA görüntüleri Editor'da üretildikten sonra ayrıca commit/push edilmelidir.

**Güvenli avlu (sahne v9):** Güney çit açıklığı kapalı başlayan çift kanatlı metal kapıyla kapatıldı. Kapıya bakıp **E** ile her iki kanadı açıp kapat; kapı durumu kayıtlı oyunda saklanır. Tüccarın çatısı destek direklerinin üzerine hizalandı. Adım sesinin ayrı düşük ses ayarı `AfterrustAudio.footstepsVolume` üzerinden yapılabilir.
