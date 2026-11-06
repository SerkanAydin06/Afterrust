# Afterrust — Yapay Zekâ Geliştirme Sözleşmesi
Önce `docs/DECISIONS.md`, ilgili `docs/GAME_DESIGN.md` bölümü, `docs/ROADMAP.md` ve `docs/AI_EFFICIENCY.md` okunur.

## Hedef
Sahibi kod yazmadan, güvenli ve test edilebilir küçük iterasyonlarla oyunu yönlendirir.

## Zorunlu canlı geliştirme listesi
- **Her Afterrust oturumunda ilk açılacak dosya:** `docs/DEVELOPMENT_PLAN.md`.
- Yeni bir ihtiyaç/bug görülürse aynı PR ile görev ekle, sırasını güncelle, tamamlananı işaretle. Bir sonraki uygulanabilir göreve sırayla geç.
- Görev durumları: `[ ]` başlanmadı, `[~]` kodlandı ama Unity doğrulaması bekliyor, `[x]` gerçek Unity kabul kontrolünden geçti. Statik GitHub CI tek başına `[x]` için yeterli değildir.
- `docs/ROADMAP.md` uzun vadeli kapsam; sürüm bazındaki gerçek ilerleme için canlı plan esas alınır. Kullanıcı kısa sonuç ister: yapılan iş, kalan test ve PR.
- Bu düzen bir **oturum içi iş akışıdır**; bağımsız bir otomasyon başlatılmadıkça sohbet dışında kendiliğinden güncelleme yapıldığı iddia edilemez.

## İş disiplini
- Her görev: hedef + kabul kriterleri + değiştirilmesi gereken minimum dosya + risk + test.
- Önce mevcut kodu ara, çalışanın yerini yeniden yazma.
- Ayrı dal ve PR; main için test zorunlu; tek dosyada eşzamanlı çakışan ajan yok.
- Tasarım kararlarını kendiliğinden değiştirme; gerekirse öneri PR/issue'su aç.
- Kısa Türkçe ilerleme/sonuç raporu: yapılan, test edilen, kalan, PR.
- Gereksiz token sarfiyatını önle: tam dosya/uzun log dökümü yerine hedefli sorgu ve özet.
- Öğrenme = belgelenmiş süreç iyileştirmesi; model ağırlıkları kendi kendine değişmez.
- Gizli anahtarları asla commit etme, dışa aktarırken lisans/kaynak kontrolü yap.
- Ayrıntılı kurallar: `docs/AI_EFFICIENCY.md`; varlıklar: `docs/ASSET_PIPELINE.md`.

## Ajan rol dağılımı
Claude Code ana uygulama, Codex bağımsız kod/test incelemesi, Gemini seçici ikinci görüş, Meshy varlık üretimi, ChatGPT görev ve ürün yönetimi. Entegrasyonlar yapılandırılana kadar otomatik çalıştıkları varsayılmaz.

## Görsel kalite kontrol otomasyonu
- Unity Editor Play modunda `Assets/Resources/qa_screenshot_plan.json` içindeki açılardan ekran görüntüsü üretilir.
- AI yeni görev için yalnız gerekli kamera açılarını plan dosyasında düzenler; kısa, sabit kimlikler ve açıklamalar kullanır.
- Çıktılar yalnız `QA/Screenshots/latest/` altında tutulur; kullanıcı commit/push yaptıktan sonra GitHub üzerinden incelenir.
- Ekran görüntülerini gerçekten görmeden veya Unity'de derleme yapmadan kalite kontrolünün geçtiğini iddia etme.
- Talimatlar: `docs/SCREENSHOT_QA.md`.
