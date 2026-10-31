# Afterrust — Otomatik Geliştirme Altyapısı
## Durum
1. GitHub repo, kararlar, AGENTS.md, token-verim kuralları: hazır.
2. Belge doğrulama GitHub Actions: hazırlanıyor / kontrol edilecek.
3. Unity 6.3 LTS proje iskeleti: henüz oluşturulmadı.
4. Unity otomatik build/test ve lisans yapılandırması: henüz etkin değil.
5. Claude Code, Codex, Gemini ve Meshy servis hesaplarının otomatik orkestrasyonu: henüz etkin değil.

## Hedef iş akışı
Kullanıcı görev söyler → ChatGPT kabul koşulu ile Issue tanımlar → tek uygulayıcı görev dalında çalışır → otomatik test → gerektiğinde ikinci ajan incelemesi → güvenli PR → onaylı merge → öğrenilen dersler kaydedilir.

## Yetkilendirme ve güvenlik
Her sağlayıcı için ayrı yetki ve resmi API erişimi gerekebilir. Sadece bağlantı kurulması yetmez; model kullanım ücretleri ayrıca doğabilir. Gizli anahtarları sohbette, Issue'da, repoda veya çalışma günlüklerinde paylaşma. Repo güvenlik ayarları ve dal koruması ayrıca açılmalı.

## İyileşme ölçümü
Başarı kriteri: geçen testler, düzelen hata sayısı, tekrar edilen iş, token tüketimi ve API maliyeti (sağlayıcının verdiği gerçek veriler). Ölçülmeyen maliyet tahmin olarak raporlanmaz. Pahalı model ancak yüksek zorlukta kullanılmalı.
