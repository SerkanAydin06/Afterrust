# Afterrust — Yapay Zekâ Geliştirme Sözleşmesi
Önce `docs/DECISIONS.md`, ilgili `docs/GAME_DESIGN.md` bölümü, `docs/ROADMAP.md` ve `docs/AI_EFFICIENCY.md` okunur.

## Hedef
Sahibi kod yazmadan, güvenli ve test edilebilir küçük iterasyonlarla oyunu yönlendirir.

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
