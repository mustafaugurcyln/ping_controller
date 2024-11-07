Ağ İzleme Uygulaması

Bu uygulama sayesinde ağınızdaki sunucular, kameralar, firewall'lar ve diğer ping atılabilen tüm cihazları gerçek zamanlı olarak izleyebilirsiniz. Uygulama, cihazların durumunu sürekli kontrol ederek çevrimdışı olduklarında anında e-posta bildirimi gönderiyor ve tekrar çevrimiçi olduklarında da bilgilendiriyor.

🚀 Özellikler

Gerçek Zamanlı İzleme: 

Cihazların anlık durumlarını sürekli ping atarak kontrol eder.
Otomatik E-posta Bildirimleri:

Seviye 1 Uyarı: Cihaz ilk kez çevrimdışı olduğunda hemen bildirim gönderir.

Seviye 2 Uyarı: Cihaz 5 dakika boyunca çevrimdışı kalırsa, daha ciddi bir uyarı gönderir.

Seviye 3 Uyarı: Cihaz 55 dakika boyunca çevrimdışı kalırsa, kritik bir uyarı gönderir.

Çevrimiçi Bildirimi: Cihaz tekrar çevrimiçi olduğunda bilgilendirme e-postası gönderir.

Esnek Cihaz Yönetimi: IP adreslerini ve açıklamalarını kolayca ekleyebilir veya düzenleyebilirsiniz.

Veri Kalıcılığı: Cihaz bilgileri yerel bir dosyada saklanır ve uygulama yeniden başlatıldığında otomatik olarak yüklenir.

SMTP Entegrasyonu: E-posta bildirimleri için SMTP protokolü kullanılır ve Office365 gibi servislerle entegre edilebilir.

💻 Gereksinimler

.NET Framework

SMTP destekli bir e-posta hesabı (Örneğin, Office365)

🔧 Kurulum
Projeyi Klonlayın veya İndirin

git clone https://github.com/kullaniciadi/projeadi.git

Proje Dosyalarını Açın

Visual Studio veya benzeri bir IDE kullanarak projeyi açın.
Gerekli Kütüphanelerin Yüklü Olduğundan Emin Olun

Proje bağımlılıklarını kontrol edin ve eksik olanları yükleyin.

🛠️ Yapılandırma ve Kullanım

1. SMTP Ayarlarını Yapılandırın
   
Form1.cs dosyasında aşağıdaki değişkenleri kendi e-posta ayarlarınızla güncelleyin:

private string smtpServer = "smtp.office365.com";
private int smtpPort = 587;
private string senderEmail = "sizinemail@ornek.com";
private string senderPassword = "sifreniz";
private string recipientEmail = "aliciemail@ornek.com";

2. Uygulamayı Çalıştırın

Debug menüsünden veya F5 tuşuna basarak uygulamayı başlatın.

3. Cihazları Ekleyin
   
Açılan arayüzde, IP Adresi ve Açıklama alanlarına izlemek istediğiniz cihazların bilgilerini girin.

4. İzlemeyi Başlatın

Başlat düğmesine tıklayarak izlemeyi başlatın.
Uygulama, cihazların durumunu gerçek zamanlı olarak kontrol etmeye başlayacaktır.

5. Bildirimleri Alın

Cihazların durumuna göre belirlenen e-posta adresine otomatik bildirimler gönderilecektir.

Çevrimdışı olduğunda ve belirli süreler boyunca çevrimdışı kalmaya devam ettiğinde.

Tekrar çevrimiçi olduğunda.

📝 Notlar

Veri Kalıcılığı:

Girdiğiniz cihaz bilgileri, uygulama kapanırken otomatik olarak TextBoxData.txt dosyasına kaydedilir ve uygulama yeniden açıldığında otomatik olarak yüklenir.
Loglama:

Uygulama arayüzündeki konsol bölümünde anlık logları görüntüleyebilirsiniz.

⭐ Teşekkürler
Projeyi beğendiyseniz yıldız vermeyi unutmayın!
