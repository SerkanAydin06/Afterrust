# Afterrust — Oyun Tasarım Belgesi v2.0 (hayatta kalma)
## Tanım
Enerji, tedarik zinciri ve savaş krizleriyle çöken bir dünyada nüfusun büyük kısmı yok olmuştur. Oyuncu, harap bir sığınakta yaşayan tek bir hayatta kalandır. Yaşamak için yiyecek ve su bulmalı, terk edilmiş yerleri yağmalamalı, bulduklarından ihtiyaç duyduğunu üretmeli ve kendini/sığınağını geliştirerek daha tehlikeli bölgelere açılmalıdır.

## Temel ilkeler
1. **Her şeyin bir bedeli var:** zaman açlık ve susuzluk demektir; koşmak dayanıklılık, taşımak ağırlık ister.
2. **Yağma anlamlı:** her kap bir kez aranır, içerik rastgele ama bağlama uygun (alet kutusunda bant/tel, rafta konserve).
3. **Üretim ihtiyaçtan doğar:** bandaj, temiz su, alet ve çanta gibi çıktılar hayatta kalmayı doğrudan uzatır.
4. **Gelişim kalıcıdır:** daha büyük çanta, yeni aletler, tezgâh ve sığınak yükseltmeleri yeni bölgeleri mümkün kılar.
5. **Ölüm gerçek bir sonuçtur:** prototipte sahne yeniden başlar; ileride eşya kaybı ve kurtarma.
6. Zombi/fantastik yaratık yok; tehlike insanlar, çevre ve kıtlıktır.

## Ana döngü
Uyan → ihtiyaçlarını kontrol et (açlık, susuzluk, sağlık) → yakın çevreyi ara/yağmala → sığınağa dön → üret (bandaj, temiz su, alet, çanta) → tüket/iyileş → daha uzağa keşfe çık.

## Yaşamsal değerler (prototip)
- **Sağlık:** açlık veya susuzluk sıfırda azalır; ikisi de iyiyken yavaşça yenilenir; bandajla artar; 0 = ölüm.
- **Açlık:** ~25 dakikada biter; konserve ve krakerle dolar.
- **Susuzluk:** ~15 dakikada biter; temiz su veya (hastalık riskiyle) kirli suyla dolar.
- **Dayanıklılık:** koşma ve zıplama harcar, dinlenince dolar.
- **Isı:** gece ve yağmurda açıkta düşer; çatı altında ve yanan sobanın yanında yükselir. 30 altı üşüme (açlık hızlanır, dayanıklılık yavaş dolar), 15 altı sağlık kaybı. Sıcak yemek ısıtır.
- **Enerji:** uyanıkken ~22 dakikada biter; 15 altı yorgunluk (dayanıklılık en fazla 50), 0'da sağlık kaybı. Yatakta uyuyarak dolar.
- Sonraki: hastalık, yaralanma türleri, ıslanma.

## Soba ve uyku
- Soba odun (4 dk) veya kömürle (6 dk) yanar; 5 m içini ısıtır, bacadan duman çıkar. Yanarken pişirme: suyu kaynat, konserveyi ısıt (sıcak yemek), odundan kömür yap.
- Yatak: gece 07:00'ye kadar, gündüz (yorgunsan) 3 saat. Zaman ve soba ilerler; açlık/susuzluk biraz düşer, enerji dolar; uyanınca kayıt alınır. Çok aç/susuz ya da çok üşürken uyunamaz.
- Odun: sığınak yanındaki odun yığını, palet yığını, sandık ve konteynerler.

## Yağma
Kaplar (karton kutu, alet kutusu, varil, sandık, konteyner) basılı tutarak aranır; her kabın kendi ganimet tablosu vardır. Çanta doluysa kalan eşyalar kapta kalır. Söküm (akü, alternatör, kablo) ve hurda yığınları malzeme kaynağıdır.

## Üretim (tezgâh)
- Bandaj = 2 bez
- Suyu süz: kirli su + kömür + bez = temiz su
- Kablo kesici = metal hurda + bant (söküm için alet)
- Büyük sırt çantası = 4 bez + 2 bant + 1 tel (taşıma kapasitesi +15 kg, bir kez)
- Alternatör geri dönüşümü = alternatör → 3 bakır tel
Sonraki: yağmur toplayıcı, ocak/ateş, tuzak, silah/savunma, sığınak yükseltmeleri.

## Su ve yiyecek kaynakları
Yağmur varili kirli su verir (bekleme süresiyle yeniden dolar). Yiyecek yalnızca yağmayla bulunur (sonra: avcılık/tuzak, bahçe).

## Ticaret
Tüccar tezgâhı ikincil: söküm parçaları ve hurda satılır, kredi ile alet alınır. Yiyecek/su satılmaz.

## Dünya
Başlangıç: çitle çevrili avlu ve kapısı kilitlenebilir harap sığınak. Sonra çevre bölgeler: Eski Mahalle, Otoyol Mezarlığı, Sanayi, Hastane, Alışveriş Merkezi, Askerî Tesis. Uzun vade: kesintisiz açık dünya.

## Tehlikeler (sonraki aşamalar)
Yağmacılar ve diğer hayatta kalanlar, vahşi hayvanlar, çevresel riskler (kimyasal, çökme), hava koşulları ve gece. Kaçış, saklanma ve pazarlık seçenekleri.

## Görsel-işitsel dil
Soğuk beton, pas, soluk yeşil, tozlu sarı; kullanılmış ve terk edilmiş dünya. Sıcak altın saat ışığı ile umut/kasvet karşıtlığı.

## Prototip başarı ölçütü
Aç ve susuz kalmadan önce avluyu yağmala → tezgâhta bandaj ve temiz su üret → çantayı büyüt → kablo kesiciyi üretip alternatörü sök → geri dönüştür; açlık/susuzluk sıfırlanırsa öl ve yeniden başla.
