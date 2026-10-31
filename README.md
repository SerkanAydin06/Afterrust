# Afterrust

**Çalışma adı:** Afterrust  
**Tür:** Birinci şahıs 3D • Post-collapse hurdacılık • Hayatta kalma • Ticaret • Üs geliştirme  
**Durum:** Tasarım ve prototip planlama; oyun henüz geliştirilmedi.

## Oyun
Medeniyetin çökmesinin ardından eski dünyanın makineleri hayatta kalanların en değerli kaynakları hâline gelmiştir. Oyuncu küçük bir tamirhaneden başlayıp araçları ve makineleri sökerek, onararak, geri dönüştürerek ve ticaret yaparak kendi hurdacılık işletmesini büyütür.

**Onaylanan oyun yönü:** Dengeli ve ayrıntılı hurdacılık; dengeli hayatta kalma/çatışma; nihai hedef kesintisiz açık dünya. İlk prototip küçük ve yoğun bir bölgede temel döngüyü kanıtlar.

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

Not: İlk FPS/hurda prototip kodları depoya eklendi; Unity Editor içinde derleme ve çalışma testi henüz yapılmadı. GitHub Actions yalnız belge ve yapı doğruluyor, gerçek Unity derlemesi henüz etkin değil.
