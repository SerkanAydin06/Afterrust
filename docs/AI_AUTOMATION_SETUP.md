# Afterrust — Otomatik geliştirme altyapısı

## Gerçek durum (2026-11-06)
- GitHub deposu, karar belgeleri ve AGENTS.md mevcut.
- Unity projesi oluşturuldu; sürüm `6000.3.27f2` (`ProjectSettings/ProjectVersion.txt`).
- `AfterrustPrototype.unity` kaynak sahnesi GitHub'da ilk hâliyle bulunabilir. `AfterrustSceneSync`, sahne Unity Editor'da açıldığında eksik yapım adımlarını (şu anda v6) uygular, sonra sahneyi kaydeder. **Dönüştürülmüş sahne ve üretilen mesh/prefab dosyaları ayrıca commit/push edilmelidir.** Play başlangıcı, güncelleme tamamlanmadan engellenir.
- `.github/workflows/docs-check.yml` dokümantasyonu; `unity-project-check.yml` temel Unity proje dosyaları ve QA kamera planını doğrular. Bu işler **C# derlemesi yapmaz.**
- `unity-project-check.yml` içinde Unity 6.3 EditMode test işi hazır, fakat repo değişkeni `UNITY_CI_ENABLED=true` yapılmadıkça **çalışmaz**. GameCI için uygun Unity lisansı ve GitHub Actions Secrets (`UNITY_LICENSE` ve gereken hesap kimlik bilgileri) tanımlanmalıdır. Secrets hiçbir zaman depoya yazılmaz.
- Claude Code / Codex / Gemini / Meshy servisleri arasında kendi kendine çalışan bir görev orkestrasyonu henüz bulunmuyor. GitHub entegrasyonunun olması tek başına bu otomasyonu etkinleştirmez.
- QA ekran görüntüsü yakalama Unity Editor Play ile otomatik çalışır. `QA/Screenshots/latest/` GitHub'a otomatik gönderilmez; yeni görüntüler oluşturulduktan sonra commit/push gerekir.

## Geliştirme ve doğrulama akışı
1. Değişikliği küçük bir görev dalında uygula; kaynak kodla birlikte test, doküman ve QA kamera noktalarını güncelle.
2. GitHub yapı kontrollerini çalıştır. Lisanslı Unity CI etkinse EditMode sonuçlarını değerlendir.
3. Yerel Unity Editor'da projeyi `6000.3.27f2` ile aç. Console derleme hatalarını gider; `[Afterrust] Scene updated` mesajını ve sahne sürümünü kontrol et.
4. EditMode ve PlayMode smoke testlerini yap; yeni sahne dosyalarını ve `QA/Screenshots/latest/` klasöründeki güncel görüntüleri commit/push et.
5. Hata varsa önce düzelt; sonra PR'ı birleştir. Hata ve kaynak tüketimi kayıtlarını gözden geçir.

## Unity CI'ı açma
1. GitHub > Settings > Secrets and variables > Actions: GameCI'nin istediği Unity lisans bilgilerini Secrets olarak ekle.
2. Aynı bölümde Variables altında `UNITY_CI_ENABLED` değerini `true` yap.
3. Workflow'daki `unity-editmode` işinin derleme ve EditMode test sonuçlarını takip et. Lisans ayarlanmadıysa işin atlanması testlerin geçtiği anlamına gelmez.

## İlkeler
- Üretilmiş varlıkları, değişmiş Unity sahnesini ve `.meta` dosyalarını kontrol etmeden merge etme.
- API anahtarlarını sohbete veya Issue/commit içine koyma.
- Token ve maliyet optimizasyonunu, ölçülmüş kalite/başarılı test oranlarıyla birlikte değerlendir.
