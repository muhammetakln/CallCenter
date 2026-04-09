# EmailSender Helper

SMTP kullanarak email göndermek için yapılandırılmış yardımcı sınıf.

## 📋 İçerik

### EmailSender.cs
`IEmailSender` arayüzünü implement eden email gönderme sınıfı.

---

## 🎯 Özellikler

- ✅ SMTP protokolü desteği
- ✅ HTML email desteği
- ✅ Async/await desteği
- ✅ Options Pattern kullanımı
- ✅ SSL/TLS şifreleme
- ✅ Microsoft Identity entegrasyonu

---

## 🔧 Kurulum

### 1. EmailSettings Modeli Oluştur

```csharp
// Utilities.Models/EmailSettings.cs
namespace Utilities.Models
{
    public class EmailSettings
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string DisplayName { get; set; }
    }
}
```

### 2. appsettings.json Yapılandır

```json
{
  "EmailSettings": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "UserName": "your-email@gmail.com",
    "Password": "your-app-password",
    "DisplayName": "Uygulamanız Adı"
  }
}
```

### 3. Dependency Injection Kaydet

```csharp
// Program.cs
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings")
);

builder.Services.AddScoped<IEmailSender, EmailSender>();
```

---

## 📚 Metodlar

### SendEmailAsync

```csharp
public async Task SendEmailAsync(string email, string subject, string htmlMessage)
```

**Parametreler:**
- `email` (string) - Alıcı email adresi
- `subject` (string) - Email konusu
- `htmlMessage` (string) - Email içeriği (HTML destekli)

**Dönüş:** Task (async işlem)

---

## 💻 Kullanım Örnekleri

### Örnek 1: Basit Email Gönderme

```csharp
public class UserService
{
    private readonly IEmailSender _emailSender;

    public UserService(IEmailSender emailSender)
    {
        _emailSender = emailSender;
    }

    public async Task SendWelcomeEmailAsync(string email, string userName)
    {
        string subject = "Hoş Geldiniz!";
        string htmlMessage = $@"
            <h1>Hoş Geldiniz {userName}!</h1>
            <p>Sitemize kaydınız başarıyla tamamlandı.</p>
            <a href='https://example.com/verify'>E-mailini Doğrula</a>
        ";
        
        await _emailSender.SendEmailAsync(email, subject, htmlMessage);
    }
}
```

### Örnek 2: Email Doğrulama

```csharp
public async Task SendConfirmationEmailAsync(string email, string callbackUrl)
{
    string subject = "Email Doğrulama";
    string htmlMessage = $@"
        <p>Lütfen hesabınızı doğrulamak için aşağıdaki linke tıklayın:</p>
        <a href='{callbackUrl}'>Hesabı Doğrula</a>
    ";
    
    await _emailSender.SendEmailAsync(email, subject, htmlMessage);
}
```

### Örnek 3: Şifre Sıfırlama

```csharp
public async Task SendPasswordResetEmailAsync(string email, string resetToken)
{
    string subject = "Şifre Sıfırlama";
    string resetUrl = $"https://example.com/reset-password?token={resetToken}";
    string htmlMessage = $@"
        <h2>Şifrenizi Sıfırlayın</h2>
        <p>Şifrenizi sıfırlamak için aşağıdaki linke tıklayın:</p>
        <a href='{resetUrl}' style='background-color: blue; color: white; padding: 10px; text-decoration: none;'>
            Şifresini Sıfırla
        </a>
        <p>Bu link 24 saat geçerlidir.</p>
    ";
    
    await _emailSender.SendEmailAsync(email, subject, htmlMessage);
}
```

### Örnek 4: Bildirim Emaili

```csharp
public async Task SendNotificationAsync(string email, string message, string actionUrl)
{
    string subject = "Yeni Bildirim";
    string htmlMessage = $@"
        <h3>Sizin için yeni bir güncelleme var!</h3>
        <p>{message}</p>
        <a href='{actionUrl}'>Detayları Görüntüle</a>
    ";
    
    await _emailSender.SendEmailAsync(email, subject, htmlMessage);
}
```

---

## ⚠️ Bilinen Sorunlar

### Problem 1: Null Reference Exception
```csharp
// ❌ Settings null olabilir
private readonly EmailSettings settings;
```

**Çözüm:**
```csharp
// ✅ Null check ekle
private readonly EmailSettings settings;

public EmailSender(IOptions<EmailSettings> options)
{
    settings = options?.Value ?? throw new ArgumentNullException(nameof(options));
}
```

---

### Problem 2: Exception Handling Yok
Email gönderme başarısız olduğunda hata yakalanmıyor.

**Çözüm:**
```csharp
public async Task SendEmailAsync(string email, string subject, string htmlMessage)
{
    try
    {
        using var client = new SmtpClient(settings.Host, settings.Port)
        {
            Credentials = new NetworkCredential(settings.UserName, settings.Password),
            EnableSsl = true
        };
        
        using var mailMessage = new MailMessage
        {
            From = new MailAddress(settings.UserName, settings.DisplayName),
            Subject = subject,
            Body = htmlMessage,
            IsBodyHtml = true
        };
        
        mailMessage.To.Add(email);
        await client.SendMailAsync(mailMessage);
    }
    catch (SmtpException ex)
    {
        throw new InvalidOperationException("Email gönderme başarısız oldu.", ex);
    }
    catch (Exception ex)
    {
        throw new InvalidOperationException("Beklenmeyen bir hata oluştu.", ex);
    }
}
```

---

### Problem 3: Timeout Ayarı Yok

**Çözüm:**
```csharp
using var client = new SmtpClient(settings.Host, settings.Port)
{
    Credentials = new NetworkCredential(settings.UserName, settings.Password),
    EnableSsl = true,
    Timeout = 10000  // 10 saniye
};
```

---

### Problem 4: Email Validasyon Yok

**Çözüm:**
```csharp
public async Task SendEmailAsync(string email, string subject, string htmlMessage)
{
    // ✅ Email validasyonu
    if (string.IsNullOrWhiteSpace(email))
        throw new ArgumentException("Email boş olamaz.", nameof(email));
    
    if (!email.Contains("@"))
        throw new ArgumentException("Geçersiz email formatı.", nameof(email));
    
    // ... kalan kod ...
}
```

---

### Problem 5: Logging Yok

**Çözüm:**
```csharp
public class EmailSender : IEmailSender
{
    private readonly EmailSettings settings;
    private readonly ILogger<EmailSender> logger;

    public EmailSender(IOptions<EmailSettings> options, ILogger<EmailSender> logger)
    {
        settings = options.Value;
        this.logger = logger;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        logger.LogInformation($"Email gönderiliyor: {email}");
        
        try
        {
            // ... email gönderme kodu ...
            logger.LogInformation($"Email başarıyla gönderildi: {email}");
        }
        catch (Exception ex)
        {
            logger.LogError($"Email gönderilemedi: {email}. Hata: {ex.Message}");
            throw;
        }
    }
}
```

---

## 🚀 İyileştirmeler

### Şablon Desteği Ekle

```csharp
public class EmailTemplate
{
    public string Subject { get; set; }
    public string Body { get; set; }
}

public class EmailSender : IEmailSender
{
    private readonly Dictionary<string, EmailTemplate> templates;
    
    public async Task SendEmailFromTemplateAsync(
        string email, 
        string templateName, 
        Dictionary<string, string> variables)
    {
        var template = templates[templateName];
        
        string body = template.Body;
        foreach (var variable in variables)
        {
            body = body.Replace($"{{{variable.Key}}}", variable.Value);
        }
        
        await SendEmailAsync(email, template.Subject, body);
    }
}
```

### Batch Email Gönderme

```csharp
public async Task SendBatchEmailAsync(
    IEnumerable<string> emails, 
    string subject, 
    string htmlMessage)
{
    var tasks = emails.Select(email => 
        SendEmailAsync(email, subject, htmlMessage)
    );
    
    await Task.WhenAll(tasks);
}
```

### Background Job ile Gönderme

```csharp
// Hangfire ile entegrasyon
public async Task SendEmailAsyncBackground(string email, string subject, string htmlMessage)
{
    BackgroundJob.Enqueue(() => SendEmailAsync(email, subject, htmlMessage));
}
```

---

## 📧 SMTP Sağlayıcıları Yapılandırması

### Gmail
```json
{
  "EmailSettings": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "UserName": "your-email@gmail.com",
    "Password": "your-app-password",
    "DisplayName": "Your App Name"
  }
}
```

### Outlook
```json
{
  "EmailSettings": {
    "Host": "smtp-mail.outlook.com",
    "Port": 587,
    "UserName": "your-email@outlook.com",
    "Password": "your-password",
    "DisplayName": "Your App Name"
  }
}
```

### SendGrid
```json
{
  "EmailSettings": {
    "Host": "smtp.sendgrid.net",
    "Port": 587,
    "UserName": "apikey",
    "Password": "SG.your-api-key",
    "DisplayName": "Your App Name"
  }
}
```

### AWS SES
```json
{
  "EmailSettings": {
    "Host": "email-smtp.region.amazonaws.com",
    "Port": 587,
    "UserName": "your-smtp-username",
    "Password": "your-smtp-password",
    "DisplayName": "Your App Name"
  }
}
```

---

## 🔒 Güvenlik Notları

⚠️ **Şifreleri .env veya User Secrets ile sakla:**

```csharp
// Program.cs - Development
builder.Configuration.AddUserSecrets<Program>();

// Production
builder.Configuration.AddEnvironmentVariables();
```

⚠️ **Gmail için App Password kullan (2FA):**
- Google Account → Security → App Passwords

⚠️ **Credentials'ı hardcode etme!**

---

## 🧪 Unit Test Örneği

```csharp
[TestFixture]
public class EmailSenderTests
{
    private IEmailSender _emailSender;
    private IOptions<EmailSettings> _options;

    [SetUp]
    public void Setup()
    {
        var settings = new EmailSettings
        {
            Host = "smtp.gmail.com",
            Port = 587,
            UserName = "test@gmail.com",
            Password = "test-password",
            DisplayName = "Test App"
        };
        
        _options = Options.Create(settings);
        _emailSender = new EmailSender(_options);
    }

    [Test]
    public async Task SendEmailAsync_WithValidEmail_ShouldSendSuccessfully()
    {
        // Arrange
        string email = "recipient@example.com";
        string subject = "Test";
        string message = "<p>Test message</p>";

        // Act & Assert
        Assert.DoesNotThrowAsync(async () => 
            await _emailSender.SendEmailAsync(email, subject, message)
        );
    }
}
```

---

## 📌 Gereksinimler

- **.NET 8.0** veya üzeri
- **Microsoft.AspNetCore.Identity.UI** paket
- **System.Net.Mail** namespace (built-in)
- **SMTP sunucusu** erişimi

---

## 🎯 Checklist

- [ ] EmailSettings yapılandırması
- [ ] appsettings.json setup
- [ ] Dependency Injection kaydı
- [ ] Exception handling ekle
- [ ] Logging mekanizması ekle
- [ ] Email validasyonu yap
- [ ] Timeout ayarı set et
- [ ] Unit testleri yaz
- [ ] HTML email şablonları oluştur

---

**Son Güncelleme:** 2026  
**Framework:** .NET 8.0  
**Paket:** Microsoft.AspNetCore.Identity.UI
