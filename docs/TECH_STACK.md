# Afterrust — Teknik Yığın (2026-10-31)
## Seçilen motor
- Unity 6.3 LTS (6000.3.x), C#.
- İlk hedef platform: Windows 64-bit PC.
- İlk oyun: birinci şahıs tek oyuncu; ağ/çok oyunculu sistem kapsam dışı.
- Render pipeline: URP (öneri); motor sürümü kurulduğunda tam patch versiyonu `ProjectSettings/ProjectVersion.txt` ile sabitlenir.
- Unity Input System, CharacterController tabanlı ilk prototip; fizik tabanlı sökülebilir parçalara özel collider/rigidbody.
- ScriptableObject veri tanımları: ItemDefinition, SalvageableDefinition, PartDefinition, ToolDefinition.
- JSON save sürümleme ve testler; UI Toolkit veya uGUI seçimi prototip sırasında gerekçeyle yapılır.

## Ana mimari ilkeler
- Tekil nesneler yerine tekrar kullanılabilir söküm davranışı.
- Her parçanın kalıcı ID'si, durum yüzdesi, kütlesi, gerekli aleti, söküm önkoşulları ve kaynak çıktısı.
- Dünya kesintisiz algılanacak; bölgesel yükleme/LOD ihtiyaç doğrultusunda eklenecek.
- İlk sprintte büyük harita ve detaylı model değil: çalışan otomobil → söküm → envanter → tamir → satış → kaydet/yükle.

## Araç entegrasyonu
- ChatGPT görev ve tasarım yöneticisi.
- Claude Code ana kod uygulayıcı; Codex inceleme ve hedefli iyileştirme; Gemini yalnızca gerekli bağımsız denetim.
- Meshy varlık üretimi ve optimizasyon; otomatik bağlantısı için hesap ve API yetkileri ayrıca kurulmalı.
- GitHub Issues/PR ve Actions kontrol noktaları.
- Harici API anahtarları hiçbir zaman repoya yazılmaz; yalnızca GitHub Secrets veya güvenilir secret manager.
- Oyunun kodunu güncellemek ile otomatik harici ajan çalıştırmak farklı işlemlerdir: erişim yapılandırılmadan otomatik ajan çağrısı varmış gibi davranılmaz.

## GitHub Actions
- Başlangıçta belge ve proje yapısı doğrulaması.
- Unity Test Runner / CI, Unity projesi eklenip gerekli lisans etkinleştirilince ayrı aşamada.
- Önce test; sonra PR inceleme ve birleştirme.
