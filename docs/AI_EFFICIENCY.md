# Yapay Zekâ Ekibi — Verimlilik ve Sürekli İyileştirme
Amaç: Doğruluğu, kaliteyi ve ilerleme hızını artırırken gereksiz bağlam/token/API giderlerini azaltmak. **Modeller kendi ağırlıklarını eğitmez**; süreç, hafıza ve araç seçimleri ölçülerek gelişir.

## İş başına tek sorumlu
- ChatGPT: gereksinim, öncelik, kabul ölçütü ve görev dağıtımı.
- Claude Code: ana uygulayıcı (ilgili işte tek kod sahibi).
- Codex: riskli kod ve test incelemesi; gerekirse ayrı branch'te düzeltme.
- Gemini: yalnız önemli tasarım, performans veya güvenlik konularında bağımsız ikinci görüş; aynı soruyu her görevde üç modele sorma.
- Meshy: sadece onaylı varlık tanımından gerekli 3D assetleri üretir.
- GitHub Issues ve PR: görev ve kararların kalıcı gerçeği.

## Token verim kuralları
1. **Önce görev kapsamı**: amaç, hangi dosyalar, sınırlar, test ve kabul kriteri. Bir görev bir net çıktı.
2. **Dar bağlam**: önce AGENTS.md + ilgili docs + değişecek dosyalar; bütün repo veya uzun logları körlemesine okumayın.
3. **Keşif kademeli**: dosya adı/arama → ilgili fonksiyon → çevresi; gerekmedikçe tüm dosyayı taşımayın.
4. **Kısa rapor**: ne değişti, test sonucu, risk, commit/PR. Gerekirse ayrıntı log/artifact olarak saklanır.
5. **Tekrar önleme**: daha önce denenmiş çözüm ve sonucunu docs/LESSONS_LEARNED.md'ye kaydedin.
6. **Önbellek ve tasarım varlıkları**: kabul edilmiş mimari kararlarını, şemaları, asset ID'lerini tekrar üretmeyin.
7. **Model yönlendirme**: basit işler için hafif/ucuz model; zor hata/mimari için güçlü model; sınırları sağlayıcının erişimine göre ayarla.
8. **Durma koşulu**: testler/kabul maddeleri tamamlanınca gereksiz iyileştirme döngüsünü sonlandır.
9. **Maliyet bütçesi**: görev başında azami çağrı/deneme sayısı belirle; aşıma yaklaşınca kısa durum+blokaj raporu üret.
10. **Güvenlik**: tokenı düşürmek için test, yedek, inceleme, lisans ve güvenlik kontrolünü atlama.

## Görev iş akışı
Issue aç → ilgili belgeleri seç → tek programcı branch'inde uygula → otomatik test/derleme → gerekirse bağımsız inceleme → PR → kontroller geçerse merge → ilgili dersleri kaydet.

## Ölçüm
Her PR'de kaydet: tamamlanan kabul maddeleri, değişen dosya sayısı, test süresi, derleme durumu, yeniden açılan hata, AI çağrı sayısı ve (sağlayıcı bildiriyorsa) giriş/çıkış tokenları ile maliyet. Uydurma metrik kullanma.
Haftalık iyileştirme: en pahalı/çok tekrarlanan işleri bul → şablon veya araç akışı sadeleştir → kalite ve maliyet karşılaştır → işe yarayan yöntemi AGENTS.md'ye ekle. Güncelleme PR incelemesiyle yapılır; sistem kendi yetkilerini genişletmez.

## Yasaklar
- Aynı dosyada iki ajanın eşzamanlı yetkisiz çalışması.
- Sonsuz kendi kendine düzeltme döngüsü.
- Test edilmemiş değişiklikleri main'e otomatik birleştirme.
- Gizli anahtarları prompt, GitHub veya günlük dosyasına yazma.
- AI modelinin kendi öğrenmesi hakkında gerçekte olmayan bir iddia.
