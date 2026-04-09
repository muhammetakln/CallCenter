# GlobalExceptionHandlingMiddleware

Uygulamanın tüm exception'larını merkezi olarak yönetmek için kullanılan middleware.

## 📋 İçerik

### GlobalExceptionHandlingMiddleware.cs
Tüm HTTP request'lerinde oluşan exception'ları yakalar ve standart hata yanıtı döner.

---

## 🎯 Özellikler

- ✅ Global exception handling
- ✅ Merkezi error logging
- ✅ Standart JSON error response
- ✅ HTTP status code mapping
- ✅ Exception type'a göre auto response
- ✅ Async/await desteği

---

## 🔧 Kurulum

### 1. Middleware Sınıfı (Zaten var)

```csharp
// Utilities.Middlewares/GlobalExceptionHandlingMiddleware.cs
public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> logger;

    public GlobalExceptionHandlingMiddleware(
        RequestDelegate next, 
        ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        this.next = next;
        this.logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Sistemde global bir hata yakalandı!");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = ex switch
        {
            ArgumentNullException => StatusCodes.Status400BadRequest,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError
        };
        
        var errorDetails = new ErrorDetails
        {
            StatusCode = context.Response.StatusCode,
            Message = ex.Message,
        };
        
        return context.Response.WriteAsync(errorDetails.ToString());
    }
}
```

### 2. ErrorDetails Modeli

```csharp
// Models/ErrorDetails.cs
using System.Text.Json.Serialization;

namespace LogTest.Models
{
    public class ErrorDetails
    {
        [JsonPropertyName("statusCode")]
        public int StatusCode { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public override string ToString()
        {
            return System.Text.Json.JsonSerializer.Serialize(this);
        }
    }
}
```

### 3. Program.cs'e Ekle

```csharp
// Program.cs
var builder = WebApplicationBuilder.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddLogging();

var app = builder.Build();

// Middleware'i ekle (En başa koy!)
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

---

## 📚 Nasıl Çalışır?

```
1. HTTP Request gelir
    ↓
2. Middleware InvokeAsync metodunu çalıştırır
    ↓
3. try-catch bloğunda pipeline devam eder
    ↓
4. Controller methodunda Exception oluşur
    ↓
5. catch bloğu exception'ı yakalar
    ↓
6. Logger exception'ı kaydeder
    ↓
7. HandleExceptionAsync çağrılır
    ↓
8. Exception type'a göre Status Code belirlenir
    ↓
9. JSON error response döndürülür
```

---

## 💻 Kullanım Örnekleri

### Örnek 1: ArgumentNullException (400 Bad Request)

```csharp
[HttpPost("user")]
public IActionResult CreateUser(UserDto user)
{
    if (user == null)
        throw new ArgumentNullException(nameof(user)); // 400 döner
    
    return Ok("Kullanıcı oluşturuldu");
}

// Response:
// {
//   "statusCode": 400,
//   "message": "Value cannot be null. (Parameter 'user')",
//   "timestamp": "2026-04-09T10:30:00Z"
// }
```

### Örnek 2: KeyNotFoundException (404 Not Found)

```csharp
[HttpGet("user/{id}")]
public IActionResult GetUser(int id)
{
    var user = _repository.GetUserById(id);
    
    if (user == null)
        throw new KeyNotFoundException($"Kullanıcı ID: {id} bulunamadı"); // 404 döner
    
    return Ok(user);
}

// Response:
// {
//   "statusCode": 404,
//   "message": "Kullanıcı ID: 5 bulunamadı",
//   "timestamp": "2026-04-09T10:30:00Z"
// }
```

### Örnek 3: UnauthorizedAccessException (401 Unauthorized)

```csharp
[HttpDelete("user/{id}")]
public IActionResult DeleteUser(int id)
{
    var currentUserId = GetCurrentUserId();
    
    if (currentUserId != id)
        throw new UnauthorizedAccessException("Bu işlemi yapma yetkiniz yok"); // 401 döner
    
    return Ok("Kullanıcı silindi");
}

// Response:
// {
//   "statusCode": 401,
//   "message": "Bu işlemi yapma yetkiniz yok",
//   "timestamp": "2026-04-09T10:30:00Z"
// }
```

### Örnek 4: Generic Exception (500 Internal Server Error)

```csharp
[HttpPost("process")]
public IActionResult ProcessData()
{
    try
    {
        // Bilinmeyen hata
        int result = 1 / int.Parse("0"); // DivideByZeroException
    }
    catch
    {
        throw new Exception("Veri işleme sırasında hata oluştu");
    }
    
    return Ok();
}

// Response:
// {
//   "statusCode": 500,
//   "message": "Veri işleme sırasında hata oluştu",
//   "timestamp": "2026-04-09T10:30:00Z"
// }
```

---

## ⚠️ Bilinen Sorunlar

### Problem 1: Exception Message'i Açığa Çıkarma (Security)

```csharp
// ❌ Yanlış - Detaylı hata messag'ı user'a gösterilir
{
  "statusCode": 500,
  "message": "Connection string: Server=localhost;Password=123456"
}
```

**Çözüm:**
```csharp
private static Task HandleExceptionAsync(HttpContext context, Exception ex)
{
    context.Response.ContentType = "application/json";
    
    string message = ex switch
    {
        ArgumentNullException => "Gerekli alan boş olamaz",
        KeyNotFoundException => "İstenen kaynak bulunamadı",
        UnauthorizedAccessException => "Bu işlemi yapma yetkiniz yok",
        ArgumentException => "Geçersiz parametre",
        _ => context.RequestServices.GetRequiredService<IHostEnvironment>().IsDevelopment()
            ? ex.Message
            : "Sistem hatası oluştu. Lütfen daha sonra tekrar deneyin."
    };
    
    context.Response.StatusCode = ex switch
    {
        ArgumentNullException or ArgumentException => StatusCodes.Status400BadRequest,
        KeyNotFoundException => StatusCodes.Status404NotFound,
        UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
        _ => StatusCodes.Status500InternalServerError
    };
    
    var errorDetails = new ErrorDetails
    {
        StatusCode = context.Response.StatusCode,
        Message = message
    };
    
    return context.Response.WriteAsync(errorDetails.ToString());
}
```

---

### Problem 2: Stack Trace Kaydedilmiyor

```csharp
// ❌ Yanlış
logger.LogError(ex, "Sistemde global bir hata yakalandı!");

// ✅ Doğru - Stack trace ile log et
logger.LogError(ex, "Sistemde global bir hata yakalandı! Stack: {StackTrace}", ex.StackTrace);
```

---

### Problem 3: Custom Exception Support Yok

```csharp
// ✅ Çözüm - Custom exception sınıfları ekle
public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
}

public class BusinessException : Exception
{
    public BusinessException(string message) : base(message) { }
}

// Middleware'de handle et
context.Response.StatusCode = ex switch
{
    ArgumentNullException => StatusCodes.Status400BadRequest,
    ValidationException => StatusCodes.Status400BadRequest,
    BusinessException => StatusCodes.Status422UnprocessableEntity,
    KeyNotFoundException => StatusCodes.Status404NotFound,
    UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
    _ => StatusCodes.Status500InternalServerError
};
```

---

### Problem 4: Request Bilgisi Loglanmıyor

**Çözüm:**
```csharp
public async Task InvokeAsync(HttpContext context)
{
    try
    {
        logger.LogInformation(
            "Request başladı: {Method} {Path}",
            context.Request.Method,
            context.Request.Path
        );
        
        await next(context);
    }
    catch (Exception ex)
    {
        logger.LogError(
            ex,
            "Exception oluştu: {Method} {Path} - {Message}",
            context.Request.Method,
            context.Request.Path,
            ex.Message
        );
        
        await HandleExceptionAsync(context, ex);
    }
}
```

---

### Problem 5: Response Yazılamadı Exception'ı

```csharp
// ❌ Response başka şekilde yazılmışsa hata oluşur
private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
{
    if (context.Response.HasStarted) // ✅ Check ekle
        return;
    
    // ... error response yaz ...
}
```

---

## 🚀 İyileştirmeler

### 1. Custom Exception Classes

```csharp
// Exceptions/ValidationException.cs
public class ValidationException : Exception
{
    public List<string> Errors { get; set; }
    
    public ValidationException(string message, List<string> errors = null)
        : base(message)
    {
        Errors = errors ?? new List<string>();
    }
}

// Exceptions/BusinessException.cs
public class BusinessException : Exception
{
    public BusinessException(string message) : base(message) { }
}

// Exceptions/NotFoundException.cs
public class NotFoundException : Exception
{
    public string ResourceName { get; set; }
    public object ResourceId { get; set; }
    
    public NotFoundException(string resourceName, object resourceId)
        : base($"{resourceName} (ID: {resourceId}) bulunamadı")
    {
        ResourceName = resourceName;
        ResourceId = resourceId;
    }
}
```

---

### 2. Extended ErrorDetails

```csharp
public class ErrorDetails
{
    [JsonPropertyName("statusCode")]
    public int StatusCode { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("path")]
    public string Path { get; set; }

    [JsonPropertyName("traceId")]
    public string TraceId { get; set; }

    [JsonPropertyName("errors")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string> Errors { get; set; }

    [JsonPropertyName("stackTrace")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string StackTrace { get; set; }

    public override string ToString()
    {
        return System.Text.Json.JsonSerializer.Serialize(this);
    }
}
```

---

### 3. Geliştirilmiş Middleware

```csharp
public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> logger;
    private readonly IHostEnvironment environment;

    public GlobalExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        this.next = next;
        this.logger = logger;
        this.environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Exception: {Method} {Path} - {Message}",
                context.Request.Method,
                context.Request.Path,
                ex.Message
            );
            
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        if (context.Response.HasStarted)
            return;

        context.Response.ContentType = "application/json";

        var (statusCode, message, errors) = GetErrorDetails(ex);
        context.Response.StatusCode = statusCode;

        var errorDetails = new ErrorDetails
        {
            StatusCode = statusCode,
            Message = message,
            Path = context.Request.Path,
            TraceId = context.TraceIdentifier,
            Errors = errors,
            StackTrace = environment.IsDevelopment() ? ex.StackTrace : null
        };

        await context.Response.WriteAsync(errorDetails.ToString());
    }

    private (int StatusCode, string Message, List<string> Errors) GetErrorDetails(Exception ex)
    {
        return ex switch
        {
            ValidationException ve => (
                StatusCodes.Status400BadRequest,
                "Doğrulama hatası",
                ve.Errors
            ),
            NotFoundException => (
                StatusCodes.Status404NotFound,
                ex.Message,
                null
            ),
            ArgumentNullException or ArgumentException => (
                StatusCodes.Status400BadRequest,
                environment.IsDevelopment() ? ex.Message : "Geçersiz istek parametresi",
                null
            ),
            UnauthorizedAccessException => (
                StatusCodes.Status401Unauthorized,
                ex.Message,
                null
            ),
            _ => (
                StatusCodes.Status500InternalServerError,
                environment.IsDevelopment() ? ex.Message : "Sistem hatası oluştu",
                null
            )
        };
    }
}
```

---

### 4. Async Request Body Logging

```csharp
public async Task InvokeAsync(HttpContext context)
{
    // Request body'yi oku
    context.Request.EnableBuffering();
    var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
    context.Request.Body.Position = 0;

    try
    {
        logger.LogInformation(
            "Request: {Method} {Path} Body: {Body}",
            context.Request.Method,
            context.Request.Path,
            body
        );
        
        await next(context);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Exception caught");
        await HandleExceptionAsync(context, ex);
    }
}
```

---

## 📊 HTTP Status Code Mapping

| Exception Type | Status Code | Açıklama |
|---|---|---|
| ArgumentNullException | 400 | Boş parametre |
| ArgumentException | 400 | Geçersiz parametre |
| ValidationException | 400 | Validasyon hatası |
| KeyNotFoundException | 404 | Kaynak bulunamadı |
| NotFoundException | 404 | Kaynak bulunamadı |
| UnauthorizedAccessException | 401 | Yetkisiz erişim |
| BusinessException | 422 | İşletme kuralı ihlali |
| Exception (Diğer) | 500 | İç sunucu hatası |

---

## 🧪 Unit Test Örneği

```csharp
[TestFixture]
public class GlobalExceptionHandlingMiddlewareTests
{
    private GlobalExceptionHandlingMiddleware middleware;
    private HttpContext httpContext;
    private ILogger<GlobalExceptionHandlingMiddleware> logger;

    [SetUp]
    public void Setup()
    {
        logger = Mock.Of<ILogger<GlobalExceptionHandlingMiddleware>>();
        var next = new RequestDelegate(async context => 
        {
            throw new ArgumentNullException("test");
        });
        
        middleware = new GlobalExceptionHandlingMiddleware(next, logger);
        httpContext = new DefaultHttpContext();
    }

    [Test]
    public async Task InvokeAsync_WithArgumentNullException_ShouldReturn400()
    {
        // Act
        await middleware.InvokeAsync(httpContext);

        // Assert
        Assert.AreEqual(StatusCodes.Status400BadRequest, httpContext.Response.StatusCode);
    }

    [Test]
    public async Task InvokeAsync_WithKeyNotFoundException_ShouldReturn404()
    {
        // Arrange
        var next = new RequestDelegate(async context => 
        {
            throw new KeyNotFoundException("Not found");
        });
        
        middleware = new GlobalExceptionHandlingMiddleware(next, logger);

        // Act
        await middleware.InvokeAsync(httpContext);

        // Assert
        Assert.AreEqual(StatusCodes.Status404NotFound, httpContext.Response.StatusCode);
    }
}
```

---

## 📌 Kurulum Checklist

- [ ] GlobalExceptionHandlingMiddleware sınıfını ekle
- [ ] ErrorDetails modelini ekle
- [ ] Custom exception sınıflarını ekle
- [ ] Program.cs'e middleware'i ekle
- [ ] Exception mapping'i konfigure et
- [ ] Logging mekanizmasını ayarla
- [ ] Environment'a göre detay ayarla
- [ ] Unit testleri yaz
- [ ] Integration testleri yaz
- [ ] Error response örnek dokümantasyonu

---

## 🔒 Güvenlik Notları

⚠️ **Hassas bilgileri expose etme:**
- Veritabanı connection string'leri
- API Key'ler
- İç sistem detayları

⚠️ **Production'da detaylı hata vermeme:**
- Stack trace'i sakla
- Generic mesajlar kullan
- Development'da detay göster

⚠️ **Logging'de sensitif veri dikkat:**
- Password'ları log etme
- Token'ları log etme
- Kişisel verileri log etme

---

## 🎯 Sonraki Adımlar

- [ ] Rate limiting middleware ekle
- [ ] Request/Response logging middleware ekle
- [ ] Custom exception filter ekle
- [ ] API versioning ekle
- [ ] CORS policy ekle
- [ ] Authentication middleware ekle

---

**Son Güncelleme:** 2026  
**Framework:** .NET 8.0  
**Pattern:** Middleware
