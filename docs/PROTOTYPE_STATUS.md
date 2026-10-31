# Afterrust — Prototip Durumu (2026-10-31)

## Kanıtlanan
- Kullanıcı Unity 6000.3.27f2'de prototip sahnesini oluşturdu.
- GitHub'a 4 PNG ve manifest yüklendi; manifest sahnenin Unity sürümünü ve ekran görüntüsü konumlarını doğruluyor.
- PNG dosyaları GitHub API üzerinden listelendi, ancak bu çalışmada görsel piksel analizi yapılamadı.

## Bu değişikliklerle eklendi; Unity içinde henüz yeniden test edilmedi
- Sökülebilir parçalarda uygun alet, kondisyon ve önkoşul desteği.
- Akü bağlantısı önce, ardından akü; alternatör kablosu önce, ardından alternatör.
- Başlangıçta anahtar ve tornavida mevcut; kablo kesici satıcıdan 60 krediye alınabilir.
- Envanter nesneleri kondisyon bilgisini korur.
- Hedef ekranında eksik alet/önkoşul/hacim hatalarının açıklanması.
- Unity EditMode testleri (gerçek Test Runner sonucu henüz alınmadı).
- QA otomatik kamera planı güncellendi.

## Kullanıcı doğrulama akışı
1. GitHub Desktop ile Pull origin.
2. Unity'de Play modundan çık.
3. Afterrust > Create or Rebuild Prototype Scene (var olan sahneyi üzerine yazar; elle yaptığın düzenlemeleri korumak istiyorsan önce yedekle).
4. Play. E ile akü bağlantısını ve aküyü sök; metal hurdayı topla.
5. Satıcıya E ile sat; F ile 60 kredilik kablo kesiciyi al.
6. Araca dön; alternatör kablosunu ve alternatörü sırayla sök.
7. Window > General > Test Runner > EditMode bölümünden testleri çalıştır.
8. Play başlangıcı ekran görüntülerini yeniler; GitHub'a yalnız güncel QA görüntülerini gönder.

## Sınırlar
- Sökme işlemi şu an tek tuşla, fakat alet ve parça sırası denetimli; gerçek söküm animasyonu yok.
- Yer tutucu otomobil formu ve temel ticaret ekranı mevcut; Meshy varlıkları henüz eklenmedi.
- Gerçek Unity derleme, oyun testi veya maliyetli harici AI entegrasyonları henüz bu sistem tarafından çalıştırılmadı.
