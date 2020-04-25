# SmartEE.WeatherForecastService
WEB API (.Net Core Web API)

Uygulama temel olarak kullanıcı tarafından gönderilen istekteki konum ismine (örneğin İstanbul) göre bugünkü sıcaklığı ve o hafta beklenen en düşük ve en yüksek sıcaklığı kullanıcıya dönmeli (Örneğin bugün 17, 7 gün içinde beklenen en düşük 15, en yüksek 18)

Akış:

Verilen konum ismine ait latitude longitude bilgileri locationiq APIsinden alınmalı (dönülen ilk lat, lon bilgisi kullanılabilir) Bu bilgilerle darksky APIsinden bugünkü ve o hafta beklenen en düşük ve en yüksek sıcaklık alınıp kullanıcıya dönülmeli Bu bilgiler ilişkisel bir veritabanında tutulmalı, gelen istekteki konum bilgisine ait bugün yapılmış sorgu varsa API çağrısı yapılmadan, önceden kaydedilmiş değerler veritabanından alınıp kullanıcıya gönderilmeli Bu bilgiler aynı zamanda bir caching katmanında da tutulmalı, caching katmanında en fazla 50 ve son 1 saat içinde yapılmış sorgular ve cevapları yer almalı. Cachete yer alan sorgular için veritabanına gidilmeden doğrudan cacheteki yanıt istemciye dönülmeli. Yapılan sorgu yeni bir sorguysa (cacheten ve veritabanından gelmediyse) açık olan monitoring applere bildirim olarak gelen istek, gönderilen yanıt ve sorgu süresi gönderilmeli Monitoring APP (.Net Core Console Application)

Uygulama WEB API üzerinden geçen sorguların izlenmesini sağlamaktadır. İlk açıldığı anda WEB API üzerinde o gün yapılmış ve kaydedilmiş tüm bilgileri (Yer ismi, o günkü sıcaklık, 7 gün içinde beklenen en düşük sıcaklık, 7 gün içinde beklenen en yüksek sıcaklık) ekrana yansıtmalı ve açık kaldığı müddetçe web apiye gelen yeni isteklerin sonuçlarını anlık olarak ekrana yansıtmalıdır, yansıtma işlemi için console uygulaması WebAPI’yi pollamamalıdır (sürekli istek gönderip yeni bilgi olup olmadığını sorgulamamalıdır). Bu uygulama aynı anda birden fazla çalışabilir, bilgilerin tamamı tüm açık olan ekranlarda görünebilmelidir.

Aşağıdaki hesaplar üzerinden erişim sağlayamazsanız ücretsiz olarak yeni hesap açabilirsiniz:

LocationIQ:

https://my.locationiq.com

Kullanıcı: d2152063@urhen.com

Token: a1779b7817b3b2

https://eu1.locationiq.com/v1/search.php?key=a1779b7817b3b2&q=[location]&format=json

DarkSKY:

https://darksky.net/dev/account

Kullanıcı: d2152063@urhen.com

Şifre: 123456ab

Key: f3146e0fc78b4930d41a60703c08e2ae

https://api.darksky.net/forecast/[key]/[latitude],[longitude]
