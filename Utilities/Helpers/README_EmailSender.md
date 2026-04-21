# Utilities.Helpers - Veri İthalatı ve Email Gönderimi

Kapsamlı bir yardımcı kütüphane paketi. CSV dosyalarını okumak ve email gönderimi için açık kaynaklı çözümleri içerir.

---

## 📚 İçindekiler

1. [DataImporters - CSV İthalatı](#dataimporters---csv-ithalatı)
2. [EmailSender - Email Gönderimi](#emailsender---email-gönderimi)
3. [Kurulum ve Yapılandırma](#kurulum-ve-yapılandırma)
4. [Kullanım Örnekleri](#kullanım-örnekleri)
5. [Hata Çözümleri](#hata-çözümleri)
6. [En İyi Uygulamalar](#en-iyi-uygulamalar)

---

## 🎯 Genel Özellikler

- ✅ **.NET 8.0** tam uyumlu
- ✅ **Async/Await** desteği
- ✅ **Exception Handling** ve hata yönetimi
- ✅ **Logging** mekanizması
- ✅ **Dependency Injection** desteği
- ✅ **XML Documentation** comments
- ✅ **Validation** kontrolleri

---

# DataImporters - CSV İthalatı

## 📋 Açıklama

`DataImporters` sınıfı, CSV dosyalarını asynchronously okuyup .NET nesnelerine dönüştüren statik yardımcı sınıftır. **CsvHelper** kütüphanesini kullanarak, CSV verilerini türü belirtilmiş (strongly-typed) C# nesnelerine kolayca eşleştirmenizi sağlar.

## 🎯 Özellikler

- ✅ **Asynchronous İşlem**: Non-blocking CSV okuma
- ✅ **Generic Tip Desteği**: Herhangi bir veri sınıfını kullanabilirsiniz
- ✅ **Otomatik Mapping**: CSV sütunları otomatik olarak property'lere eşlenir
- ✅ **Hafif ve Verimli**: Stream tabanlı işleme
- ✅ **Culture Invariant**: Uluslararası formatları destekler

## 📦 Bağımlılıklar

```bash
dotnet add package CsvHelper
```

**Desteklenen Sürümler:**
- CsvHelper: 30.0.0 veya sonrası
- Framework: .NET 8.0+

## 🚀 Kullanım - DataImporters

### Adım 1: Veri Modeli Oluşturun

```csharp
namespace YourApp.Models
{
    public class Person
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        public string Email { get; set; }
        public DateTime BirthDate { get; set; }
    }
}
```

### Adım 2: CSV Dosyasını İçe Aktarın

```csharp
using Utilities.Helpers;
using System.IO;

// Dosya yolu ile kullanım
using var stream = new FileStream("people.csv", FileMode.Open, FileAccess.Read);
var people = await DataImporters.ImportCsvAsync<Person>(stream);

// Sonuçları işleyin
foreach (var person in people)
{
    Console.WriteLine($"{person.FirstName} {person.LastName} - {person.Age}");
}
```

### Adım 3: CSV Dosya Formatı

Örnek CSV dosyası (`people.csv`):

```csv
FirstName,LastName,Age,Email,BirthDate
John,Doe,30,john.doe@example.com,1994-05-15
Jane,Smith,28,jane.smith@example.com,1996-08-22
Bob,Johnson,35,bob.johnson@example.com,1989-12-03
```

## 📊 CSV Okuma Senaryoları

### Senaryo 1: Web Upload ile CSV İçe Aktarma

```csharp
[HttpPost("upload-csv")]
public async Task<IActionResult> UploadCsv(IFormFile file)
{
    try
    {
        if (file == null || file.Length == 0)
            return BadRequest("Dosya yüklenmedi.");

        using var stream = file.OpenReadStream();
        var employees = await DataImporters.ImportCsvAsync<Employee>(stream);
        
        // Veritabanına kaydedin
        await _context.Employees.AddRangeAsync(employees);
        await _context.SaveChangesAsync();
        
        return Ok(new 
        { 
            message = "Başarıyla içe aktarıldı",
            count = employees.Count()
        });
    }
    catch (Exception ex)
    {
        _logger.LogError($"CSV Upload hatası: {ex.Message}");
        return BadRequest(new { error = ex.Message });
    }
}
```

### Senaryo 2: Toplu Dosya İçe Aktarma

```csharp
public async Task ImportMultipleCsvFilesAsync(string directoryPath)
{
    var csvFiles = Directory.GetFiles(directoryPath, "*.csv");
    var results = new List<(string file, int count, bool success)>();

    foreach (var filePath in csvFiles)
    {
        try
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var records = await DataImporters.ImportCsvAsync<SalesRecord>(stream);
            
            await ProcessRecords(records);
            
            results.Add((Path.GetFileName(filePath), records.Count(), true));
        }
        catch (Exception ex)
        {
            _logger.LogError($"Dosya işlenemedi: {filePath}. Hata: {ex.Message}");
            results.Add((Path.GetFileName(filePath), 0, false));
        }
    }

    LogImportSummary(results);
}
```

### Senaryo 3: Veri Doğrulama ve Filtreleme

```csharp
public async Task<List<Person>> ImportAndValidateAsync(Stream stream)
{
    var people = await DataImporters.ImportCsvAsync<Person>(stream);

    var validPeople = people
        .Where(p => !string.IsNullOrWhiteSpace(p.Email))
        .Where(p => p.Age >= 18 && p.Age <= 120)
        .Where(p => Regex.IsMatch(p.Email, @"^[^\s@]+@[^\s@]+\.[^\s@]+$"))
        .ToList();

    _logger.LogInformation($"Toplam: {people.Count()}, Geçerli: {validPeople.Count()}");

    return validPeople;
}
```

### Senaryo 4: Toplu İşleme (Batch Processing)

```csharp
public async Task ImportWithBatchProcessingAsync(Stream stream, int batchSize = 1000)
{
    var people = await DataImporters.ImportCsvAsync<Person>(stream);

    var batches = people
        .Select((item, index) => new { item, index })
        .GroupBy(x => x.index / batchSize)
        .Select(g => g.Select(x => x.item).ToList());

    foreach (var batch in batches)
    {
        try
        {
            await _context.People.AddRangeAsync(batch);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Batch işlendi: {batch.Count} kayıt");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Batch işleme hatası: {ex.Message}");
            throw;
        }
    }
}
```

---

# EmailSender - Email Gönderimi

## 📋 Açıklama

`EmailSender` sınıfı, SMTP protokolü kullanarak email gönderimi sağlayan yardımcı sınıftır. **Microsoft.AspNetCore.Identity.UI** arayüzünü implement eder ve dependency injection ile entegre çalışır.

## 🎯 Özellikler

- ✅ SMTP protokolü desteği
- ✅ HTML email desteği
- ✅ Async/await desteği
- ✅ Options Pattern kullanımı
- ✅ SSL/TLS şifreleme
- ✅ Microsoft Identity entegrasyonu
- ✅ Exception handling ve logging
- ✅ Email validasyonu
- ✅ Timeout ayarı
- ✅ Configuration validation

## 📦 Bağımlılıklar

```bash
dotnet add package Microsoft.AspNetCore.Identity.UI
```

## 🔧 Kurulum - EmailSender

### Adım 1: EmailSettings Modelini Oluşturun

```csharp
// Utilities/Models/EmailSettings.cs
namespace Utilities.Models
{
    public class EmailSettings
    {
        /// <summary>
        /// SMTP sunucusu adresi (örn: smtp.gmail.com)
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// SMTP port numarası (genellikle 587 veya 465)
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// SMTP kullanıcı adı (genellikle email adresi)
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// SMTP şifresi veya uygulama parolası
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Email gönderenin adı (alıcıda gösterilecek isim)
        /// </summary>
        public string DisplayName { get; set; }
    }
}
```

### Adım 2: appsettings.json Yapılandırması

```json
{
  "EmailSettings": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "UserName": "your-email@gmail.com",
    "Password": "your-app-password",
    "DisplayName": "Uygulamanız Adı"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

### Adım 3: Dependency Injection Yapılandırması

```csharp
// Program.cs
var builder = WebApplicationBuilder.CreateBuilder(args);

// EmailSettings yapılandırmasını kaydet
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings")
);

// IEmailSender arayüzünü EmailSender ile kaydet
builder.Services.AddScoped<IEmailSender, EmailSender>();

// Logging yapılandırması
builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.AddDebug();
});

var app = builder.Build();
```

## 📚 Metodlar - EmailSender

### SendEmailAsync

```csharp
public async Task SendEmailAsync(string email, string subject, string htmlMessage)
```

**Parametreler:**
| Parametre | Tip | Açıklama |
|-----------|-----|----------|
| `email` | string | Alıcı email adresi |
| `subject` | string | Email başlığı |
| `htmlMessage` | string | Email içeriği (HTML formatında) |

**İstisnalar:**
- `ArgumentException`: Email adresi geçersiz ise
- `InvalidOperationException`: Email gönderme başarısız oldu ise
- `ArgumentNullException`: Settings null ise

## 💻 Kullanım Örnekleri - EmailSender

### Örnek 1: Hoş Geldiniz Emaili

```csharp
public class AccountService
{
    private readonly IEmailSender _emailSender;

    public AccountService(IEmailSender emailSender)
    {
        _emailSender = emailSender;
    }

    public async Task SendWelcomeEmailAsync(string email, string userName)
    {
        string subject = "Hoş Geldiniz!";
        string htmlMessage = $@"
            <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h1>Hoş Geldiniz, {userName}!</h1>
                    <p>Sitemize kaydınız başarıyla tamamlandı.</p>
                    <p>Hesabınızı tam kullanabilmek için email adresinizi doğrulamanız gerekiyor.</p>
                    <a href='https://example.com/verify' style='background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>
                        Email Adresini Doğrula
                    </a>
                </body>
            </html>
        ";
        
        await _emailSender.SendEmailAsync(email, subject, htmlMessage);
    }
}
```

### Örnek 2: Email Doğrulama

```csharp
public async Task SendConfirmationEmailAsync(string email, string callbackUrl)
{
    string subject = "Email Adresinizi Doğrulayın";
    string htmlMessage = $@"
        <html>
            <body>
                <h2>Email Doğrulaması Gerekli</h2>
                <p>Lütfen hesabınızı doğrulamak için aşağıdaki linke tıklayın:</p>
                <a href='{callbackUrl}'>Hesabı Doğrula</a>
                <p style='color: gray; font-size: 12px;'>
                    Bu link 24 saat geçerlidir.
                </p>
            </body>
        </html>
    ";
    
    await _emailSender.SendEmailAsync(email, subject, htmlMessage);
}
```

### Örnek 3: Şifre Sıfırlama

```csharp
public async Task SendPasswordResetEmailAsync(string email, string resetToken)
{
    string subject = "Şifre Sıfırlama Talebi";
    string resetUrl = $"https://example.com/reset-password?token={Uri.EscapeDataString(resetToken)}";
    
    string htmlMessage = $@"
        <html>
            <body>
                <h2>Şifre Sıfırlama Talebi</h2>
                <p>Şifrenizi sıfırlamak için aşağıdaki butona tıklayın:</p>
                <a href='{resetUrl}' style='background-color: #28a745; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; display: inline-block;'>
                    Şifresini Sıfırla
                </a>
                <p><strong>Uyarı:</strong> Bu link 2 saat geçerlidir.</p>
                <p>Bu talebi siz yapmadıysanız bu emaili görmezden gelebilirsiniz.</p>
            </body>
        </html>
    ";
    
    await _emailSender.SendEmailAsync(email, subject, htmlMessage);
}
```

### Örnek 4: Bildirim Emaili

```csharp
public async Task SendNotificationAsync(
    string email, 
    string title, 
    string message, 
    string actionUrl, 
    string actionText = "Detayları Görüntüle")
{
    string subject = title;
    string htmlMessage = $@"
        <html>
            <body>
                <h3>{title}</h3>
                <p>{message}</p>
                <a href='{actionUrl}' style='background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>
                    {actionText}
                </a>
            </body>
        </html>
    ";
    
    await _emailSender.SendEmailAsync(email, subject, htmlMessage);
}
```

### Örnek 5: Sipariş Onay Emaili

```csharp
public async Task SendOrderConfirmationAsync(string email, Order order)
{
    string subject = $"Siparişiniz Onaylandı - #{order.OrderId}";
    
    string htmlMessage = $@"
        <html>
            <body>
                <h2>Siparişiniz Onaylandı!</h2>
                <p>Siparişiniz başarıyla oluşturulmuştur.</p>
                
                <h3>Sipariş Detayları</h3>
                <table style='border-collapse: collapse; width: 100%;'>
                    <tr style='border-bottom: 1px solid #ddd;'>
                        <th style='text-align: left; padding: 8px;'>Ürün</th>
                        <th style='text-align: left; padding: 8px;'>Miktar</th>
                        <th style='text-align: right; padding: 8px;'>Fiyat</th>
                    </tr>
                    {string.Join("", order.Items.Select(item => $@"
                        <tr style='border-bottom: 1px solid #eee;'>
                            <td style='padding: 8px;'>{item.ProductName}</td>
                            <td style='padding: 8px;'>{item.Quantity}</td>
                            <td style='text-align: right; padding: 8px;'>{item.Price:C}</td>
                        </tr>
                    "))}
                </table>
                
                <h3 style='text-align: right;'>Toplam: {order.TotalPrice:C}</h3>
                <p>Takip Numarası: {order.TrackingNumber}</p>
            </body>
        </html>
    ";
    
    await _emailSender.SendEmailAsync(email, subject, htmlMessage);
}
```

### Örnek 6: Service'te Hata Yönetimi ile Kullanım

```csharp
public class UserService
{
    private readonly IEmailSender _emailSender;
    private readonly ILogger<UserService> _logger;

    public UserService(IEmailSender emailSender, ILogger<UserService> logger)
    {
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task RegisterUserAsync(User user)
    {
        try
        {
            // Kullanıcıyı kaydet
            await SaveUserAsync(user);

            // Onay emaili gönder
            string subject = "Hesap Oluşturma Onayı";
            string confirmationUrl = GenerateConfirmationUrl(user.Id);
            
            await _emailSender.SendEmailAsync(
                user.Email,
                subject,
                $"<a href='{confirmationUrl}'>Hesabınızı Doğrulayın</a>"
            );

            _logger.LogInformation($"Kullanıcı kaydı başarılı: {user.Email}");
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning($"Geçersiz email: {ex.Message}");
            throw;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError($"Email gönderme başarısız: {ex.Message}");
            throw;
        }
    }
}
```

---

# Kurulum ve Yapılandırma

## 📥 NuGet Paketleri

```bash
dotnet add package CsvHelper
dotnet add package Microsoft.AspNetCore.Identity.UI
```

## 🔐 Güvenlik - Credentials Yönetimi

### Seçenek 1: User Secrets (Development)

```bash
dotnet user-secrets init
dotnet user-secrets set "EmailSettings:Password" "your-secure-password"
dotnet user-secrets set "EmailSettings:UserName" "your-email@example.com"
```

### Seçenek 2: Environment Variables (Production)

```bash
export EMAIL_SETTINGS__PASSWORD="your-secure-password"
export EMAIL_SETTINGS__USERNAME="your-email@example.com"
```

### Seçenek 3: Azure Key Vault

```csharp
var keyVaultUrl = new Uri($"https://{Environment.GetEnvironmentVariable("KEY_VAULT_NAME")}.vault.azure.net/");
var credential = new DefaultAzureCredential();
builder.Configuration.AddAzureKeyVault(keyVaultUrl, credential);
```

---

# Hata Çözümleri

## DataImporters Hataları

| Hata | Sebep | Çözüm |
|------|-------|-------|
| `CsvHelper not found` | NuGet paketi yüklü değil | `dotnet add package CsvHelper` |
| `Column not found` | CSV sütun adı property adı ile uymuyor | Sütun adlarını kontrol edin |
| `Invalid cast` | Veri tipi uyumsuz | Veri formatını düzeltin |
| `Stream is disposed` | Stream kapatıldı | `using` ifadesinde kullanın |

## EmailSender Hataları

| Hata | Sebep | Çözüm |
|------|-------|-------|
| `ArgumentNullException` | EmailSettings null | appsettings.json kontrolü |
| `ArgumentException` | Email formatı geçersiz | Email adresini kontrol edin |
| `SmtpException` | SMTP bağlantısı başarısız | Host, Port, Credentials kontrol |
| `Timeout` | Sunucu yanıt vermedi | Timeout ayarını artırın |
| `Authentication failed` | Credentials yanlış | UserName ve Password kontrol |

## Gmail Yapılandırması Sorunları

**Problem:** "Less secure apps" hatası

**Çözüm:**
1. Google Account → Security
2. "App passwords" kısmına git
3. Yeni bir app password oluştur
4. 16 karakterlik parolayı `appsettings.json`'da kullan

## Outlook/Office 365 Yapılandırması

```json
{
  "EmailSettings": {
    "Host": "smtp-mail.outlook.com",
    "Port": 587,
    "UserName": "your-email@outlook.com",
    "Password": "your-password",
    "DisplayName": "Your Name"
  }
}
```

---

# En İyi Uygulamalar

## 1. CSV İçe Aktarma

```csharp
// ✅ DOĞRU
public async Task<List<T>> SafeImportAsync<T>(Stream stream) where T : class
{
    if (stream == null || stream.Length == 0)
        throw new ArgumentException("Stream boş olamaz.");

    try
    {
        var records = await DataImporters.ImportCsvAsync<T>(stream);
        return records.ToList();
    }
    catch (Exception ex)
    {
        _logger.LogError($"CSV import hatası: {ex.Message}");
        throw;
    }
}

// ❌ YANLIŞ
public async Task<IEnumerable<T>> ImportAsync<T>(Stream stream)
{
    return await DataImporters.ImportCsvAsync<T>(stream);
}
```

## 2. Email Gönderimi

```csharp
// ✅ DOĞRU
public async Task SendEmailSafelyAsync(string email, string subject, string body)
{
    try
    {
        await _emailSender.SendEmailAsync(email, subject, body);
    }
    catch (ArgumentException ex)
    {
        _logger.LogWarning($"Geçersiz email: {email}");
        throw;
    }
    catch (InvalidOperationException ex)
    {
        _logger.LogError($"Email gönderme başarısız: {ex.Message}");
        // Retry logic veya fallback mekanizması ekle
        throw;
    }
}

// ❌ YANLIŞ
public async Task SendEmail(string email, string subject, string body)
{
    await _emailSender.SendEmailAsync(email, subject, body);
}
```

## 3. Batch Processing

```csharp
// ✅ Büyük dosyalar için batch işleme
const int BATCH_SIZE = 1000;

public async Task ImportLargeCsvAsync(Stream stream)
{
    var records = await DataImporters.ImportCsvAsync<T>(stream);
    
    var batches = records
        .Batch(BATCH_SIZE)
        .ToList();

    foreach (var batch in batches)
    {
        await _context.AddRangeAsync(batch);
        await _context.SaveChangesAsync();
    }
}
```

---

## 🧪 Unit Test Örnekleri

### DataImporters Test

```csharp
[TestClass]
public class DataImportersTests
{
    [TestMethod]
    public async Task ImportCsvAsync_WithValidData_ReturnsCorrectCount()
    {
        // Arrange
        var csvContent = "FirstName,LastName,Age\nJohn,Doe,30\nJane,Smith,28";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

        // Act
        var result = await DataImporters.ImportCsvAsync<Person>(stream);

        // Assert
        Assert.AreEqual(2, result.Count());
        Assert.AreEqual("John", result.First().FirstName);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public async Task ImportCsvAsync_WithNullStream_ThrowsException()
    {
        await DataImporters.ImportCsvAsync<Person>(null);
    }
}
```

### EmailSender Test

```csharp
[TestClass]
public class EmailSenderTests
{
    private EmailSender _emailSender;
    private ILogger<EmailSender> _logger;

    [TestInitialize]
    public void Setup()
    {
        var settings = new EmailSettings
        {
            Host = "smtp.gmail.com",
            Port = 587,
            UserName = "test@example.com",
            Password = "test-password",
            DisplayName = "Test App"
        };

        var options = Options.Create(settings);
        _logger = new Mock<ILogger<EmailSender>>().Object;
        _emailSender = new EmailSender(options, _logger);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public async Task SendEmailAsync_WithInvalidEmail_ThrowsException()
    {
        await _emailSender.SendEmailAsync("invalid-email", "Subject", "Body");
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public async Task SendEmailAsync_WithNullSubject_ThrowsException()
    {
        await _emailSender.SendEmailAsync("test@example.com", null, "Body");
    }
}
```

---

## 📋 Checklist

### DataImporters Checklist
- [ ] CsvHelper NuGet paketi yüklü
- [ ] Veri modeli oluşturuldu
- [ ] CSV dosyası doğru formatında
- [ ] Stream doğru şekilde yönetiliyor
- [ ] Exception handling eklendi
- [ ] Logging yapılandırıldı
- [ ] Unit testler yazıldı

### EmailSender Checklist
- [ ] EmailSettings modeli oluşturuldu
- [ ] appsettings.json yapılandırıldı
- [ ] DI container'ında kayıt yapıldı
- [ ] SMTP credentials doğru
- [ ] Logging yapılandırıldı
- [ ] Exception handling eklendi
- [ ] HTML şablonları hazırlandı
- [ ] Unit testler yazıldı
- [ ] Production'da credentials secure saklı

---

## 🔗 İlgili Kaynaklar

- [CsvHelper Dokumentasyonu](https://joshclose.github.io/CsvHelper/)
- [ASP.NET Core Logging](https://learn.microsoft.com/en-us/dotnet/core/extensions/logging)
- [Dependency Injection](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection)
- [Options Pattern](https://learn.microsoft.com/en-us/dotnet/core/extensions/options)
- [SMTP Protocol](https://tools.ietf.org/html/rfc5321)

---

## 📞 Destek ve İletişim

Sorunlar veya öneriler için lütfen issues bölümüne yorum yapın.

---

**Sürüm:** 2.0  
**Güncellenme Tarihi:** 2026  
**Framework:** .NET 8.0  
**Lisans:** MIT  
**Durum:** Production Ready ✅