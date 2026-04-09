# Generic Repository Pattern (.NET 8.0)

## 📋 İçindekiler
1. [Proje Açıklaması](#proje-açıklaması)
2. [Mimari Yapı](#mimari-yapı)
3. [Dosya Yapısı](#dosya-yapısı)
4. [Arayüzler ve Metodlar](#arayüzler-ve-metodlar)
5. [Kullanım Örnekleri](#kullanım-örnekleri)
6. [Tespit Edilen Sorunlar](#tespit-edilen-sorunlar)
7. [Önerilen İyileştirmeler](#önerilen-iyileştirmeler)
8. [En İyi Uygulamalar](#en-iyi-uygulamalar)

---

## 🎯 Proje Açıklaması

Bu proje, **Entity Framework Core 8.0** ile çalışan bir **Generic Repository Pattern** implementasyonudur. Veri tabanı işlemlerini soyutlayarak, uygulamanızın veri erişim katmanını merkezi bir şekilde yönetmenizi sağlar.

### Amaçlar:
- ✅ Veri tabanı operasyonlarının soyutlanması
- ✅ Tekrar kullanılabilir generic kod
- ✅ CRUD (Create, Read, Update, Delete) işlemlerinin standartlaştırılması
- ✅ Dependency Injection desteği
- ✅ Async/await desteği

---

## 🏗️ Mimari Yapı

```
Utilities.Generics
├── IRepository<T>        (Arayüz)
└── Repository<T>         (Implementasyon)
```

### Tasarım Deseni: Repository Pattern
- **Benefit**: Veri erişim logic'ini iş logic'inden ayırır
- **Esneklik**: Mock'lama ve unit testing kolaylaşır
- **Bakım**: Merkezi değişiklikler tüm repository'leri etkiler

---

## 📁 Dosya Yapısı

### `IRepository<T>` (Arayüz)
```
Utilities.Generics/
└── IRepository.cs        ← Tüm repository operasyonlarının sözleşmesi
```

### `Repository<T>` (Implementasyon)
```
Utilities.Generics/
└── Repository.cs         ← IRepository<T>'yi implement eden generic sınıf
```

---

## 🔧 Arayüzler ve Metodlar

### IRepository<T> Arayüzü

#### **CREATE (Oluşturma) Operasyonları**

```csharp
// Tekil entiti oluştur
Task CreateAsync(T entity);

// Birden fazla entiti oluştur
Task CreateManyAsync(IEnumerable<T> entities);
```

#### **READ (Okuma) Operasyonları**

```csharp
// Birincil anahtarla oku
Task<T?> ReadByKeyAsync(object entityKey);

// İlk eşleşen entiti bul
Task<T?> FindFirstAsync(Expression<Func<T, bool>>? expression = null);

// Koşula göre birden fazla entiti oku (include desteği)
Task<IEnumerable<T>> ReadManyAsync(
    Expression<Func<T, bool>>? expression = null, 
    params string[] includes);
```

#### **UPDATE (Güncelleme) Operasyonları**

```csharp
// Tekil entiti güncelle
Task UpdateAsync(T entity);

// Birden fazla entiti güncelle
Task UpdateManyAsync(IEnumerable<T> entities);

// Koşula göre güncelle
Task UpdateManyAsync(Expression<Func<T, bool>>? expression = null);
```

#### **DELETE (Silme) Operasyonları**

```csharp
// Tekil entiti sil
Task DeleteAsync(T entity);

// Birden fazla entiti sil
Task DeleteManyAsync(IEnumerable<T> entities);

// Koşula göre sil
Task DeleteManyAsync(Expression<Func<T, bool>>? expression = null);
```

#### **UTILITY (Yardımcı) Operasyonları**

```csharp
// Kaç entiti var sayısını al
Task<int> CountAsync(Expression<Func<T, bool>>? expression = null);

// Herhangi bir entiti var mı kontrol et
Task<bool> AnyAsync(Expression<Func<T, bool>>? expression = null);
```

---

## 💻 Kullanım Örnekleri

### 1️⃣ **Proje Kurulumu**

```csharp
// Program.cs
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
```

### 2️⃣ **Entiti Tanımlama**

```csharp
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### 3️⃣ **Repository Enjeksiyonu ve Kullanım**

```csharp
public class UserService
{
    private readonly IRepository<User> _userRepository;

    public UserService(IRepository<User> userRepository)
    {
        _userRepository = userRepository;
    }

    // CREATE - Yeni kullanıcı ekle
    public async Task AddUserAsync(User user)
    {
        await _userRepository.CreateAsync(user);
    }

    // READ - Kullanıcı bul
    public async Task<User?> GetUserAsync(int userId)
    {
        return await _userRepository.ReadByKeyAsync(userId);
    }

    // READ - E-mailden ara
    public async Task<User?> FindByEmailAsync(string email)
    {
        return await _userRepository.FindFirstAsync(u => u.Email == email);
    }

    // READ - Tüm aktif kullanıcılar
    public async Task<IEnumerable<User>> GetActiveUsersAsync()
    {
        return await _userRepository.ReadManyAsync(
            u => u.CreatedAt > DateTime.Now.AddMonths(-1)
        );
    }

    // UPDATE - Kullanıcı güncelle
    public async Task UpdateUserAsync(User user)
    {
        await _userRepository.UpdateAsync(user);
    }

    // DELETE - Kullanıcı sil
    public async Task DeleteUserAsync(User user)
    {
        await _userRepository.DeleteAsync(user);
    }

    // COUNT - Toplam kullanıcı sayısı
    public async Task<int> GetTotalUsersAsync()
    {
        return await _userRepository.CountAsync();
    }

    // ANY - Belirli emaile sahip kullanıcı var mı?
    public async Task<bool> UserExistsAsync(string email)
    {
        return await _userRepository.AnyAsync(u => u.Email == email);
    }
}
```

### 4️⃣ **DbContext Kurulumu**

```csharp
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Product> Products { get; set; }
}
```

---

## ⚠️ Tespit Edilen Sorunlar

### 🔴 **Problem 1: Dönüş Tipi Tutarsızlığı**

**Sorun:**
```csharp
// IRepository.cs
public interface IRepository<T> where T : class
{
    Task<T?> ReadByKeyAsync(object entityKey);      // T? döndürüyor
    Task<T?> FindFirstAsync(...);                    // T? döndürüyor
}

// Repository.cs
public class Repository<T> : IRepository<T> where T : class
{
    public virtual async Task<T> ReadByKeyAsync(object entityKey) 
        => await _set.FindAsync(entityKey);          // T döndürüyor (nullable değil!)
    
    public virtual async Task<T> FindFirstAsync(...) 
        => await _set.FirstOrDefaultAsync(...);      // T döndürüyor
}
```

**Etki:** Null reference exception riski, type safety ihlali

---

### 🔴 **Problem 2: Yapıcı Metodun Korumalı (Protected) Olması**

```csharp
protected Repository(DbContext context)  // ❌ Protected
{
    _context = context;
    _set = context.Set<T>();
}
```

**Sorun:** DI container tarafından somutlaştırılamaz

---

### 🔴 **Problem 3: SaveChanges Çağrılmıyor**

```csharp
public virtual async Task CreateAsync(T entity)
    => await _set.AddAsync(entity);  // ❌ DbContext.SaveChangesAsync() yok!
```

**Etki:** Veriler veritabanına kaydedilmez

---

### 🔴 **Problem 4: Task.Run Gereksiz Kullanımı**

```csharp
public virtual async Task DeleteAsync(T entity)
    => await Task.Run(() => _set.Remove(entity));  // ❌ Gereksiz threading
```

**Sorun:** Performans düşüşü, thread pool'u işgal etme

---

### 🔴 **Problem 5: Include Metodunda String Başvurusu**

```csharp
public virtual async Task<IEnumerable<T>> ReadManyAsync(
    Expression<Func<T, bool>>? expression = null, 
    params string[] includes)
{
    var entities = _set.Where(expression ?? (x => true));
    foreach(var include in includes)
    {
        entities = entities.Include(include);  // ❌ String include risk
    }
    return await entities.ToListAsync();
}
```

**Sorun:** 
- Include adı yanlış yazıldığında compile-time hatası yok
- Reflection gereksiz
- Tip güvenliği yok

---

### 🔴 **Problem 6: IDisposable Yok**

```csharp
public class Repository<T> : IRepository<T>
{
    protected readonly DbContext _context;
    // ❌ DbContext'i dispose etmiyor
}
```

**Etki:** Bellek sızıntısı, veritabanı bağlantısı açık kalabilir

---

## ✨ Önerilen İyileştirmeler

### ✅ **Çözüm 1: Dönüş Tipini Düzelt**

```csharp
// IRepository.cs
public interface IRepository<T> where T : class
{
    Task<T?> ReadByKeyAsync(object entityKey);
    Task<T?> FindFirstAsync(Expression<Func<T, bool>>? expression = null);
}

// Repository.cs
public virtual async Task<T?> ReadByKeyAsync(object entityKey) 
    => await _set.FindAsync(entityKey);

public virtual async Task<T?> FindFirstAsync(Expression<Func<T, bool>>? expression = null) 
    => await _set.FirstOrDefaultAsync(expression ?? (x => true));
```

---

### ✅ **Çözüm 2: Yapıcıyı Public Yap**

```csharp
public Repository(DbContext context)  // ✅ Public
{
    _context = context ?? throw new ArgumentNullException(nameof(context));
    _set = context.Set<T>();
}
```

---

### ✅ **Çözüm 3: SaveChanges Ekle**

```csharp
public interface IRepository<T> where T : class
{
    // ... diğer metodlar ...
    Task SaveChangesAsync();  // ✅ YENİ
}

public class Repository<T> : IRepository<T>
{
    public virtual async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}

// Kullanım
public async Task AddUserAsync(User user)
{
    await _userRepository.CreateAsync(user);
    await _userRepository.SaveChangesAsync();  // ✅
}
```

---

### ✅ **Çözüm 4: Task.Run Kaldır**

```csharp
// ❌ Yanlış
public virtual async Task DeleteAsync(T entity)
    => await Task.Run(() => _set.Remove(entity));

// ✅ Doğru
public virtual async Task DeleteAsync(T entity)
{
    _set.Remove(entity);
    await Task.CompletedTask;
}

// Veya daha temiz
public virtual Task DeleteAsync(T entity)
{
    _set.Remove(entity);
    return Task.CompletedTask;
}
```

---

### ✅ **Çözüm 5: Lambda Include Kullan**

```csharp
// ❌ Yanlış
public virtual async Task<IEnumerable<T>> ReadManyAsync(
    Expression<Func<T, bool>>? expression = null, 
    params string[] includes)

// ✅ Doğru
public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> ReadManyAsync(
        Expression<Func<T, bool>>? expression = null,
        Func<IQueryable<T>, IQueryable<T>>? include = null);
}

public virtual async Task<IEnumerable<T>> ReadManyAsync(
    Expression<Func<T, bool>>? expression = null,
    Func<IQueryable<T>, IQueryable<T>>? include = null)
{
    IQueryable<T> query = _set.Where(expression ?? (x => true));
    
    if (include != null)
        query = include(query);
    
    return await query.ToListAsync();
}

// Kullanım
var users = await _userRepository.ReadManyAsync(
    u => u.IsActive,
    q => q.Include(u => u.Orders)
         .Include(u => u.Profile)
);
```

---

### ✅ **Çözüm 6: IDisposable Ekle**

```csharp
public class Repository<T> : IRepository<T>, IDisposable where T : class
{
    private bool _disposed = false;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _context?.Dispose();
            }
            _disposed = true;
        }
    }

    ~Repository()
    {
        Dispose(false);
    }
}

// Kullanım
using (var repository = new Repository<User>(dbContext))
{
    // işlemler
}
```

---

## 📚 En İyi Uygulamalar

### 1️⃣ **Unit of Work Pattern Ekle**

```csharp
public interface IUnitOfWork : IDisposable
{
    IRepository<User> Users { get; }
    IRepository<Product> Products { get; }
    Task<int> SaveChangesAsync();
}

public class UnitOfWork : IUnitOfWork
{
    private readonly DbContext _context;
    private IRepository<User> _users;
    private IRepository<Product> _products;

    public IRepository<User> Users 
        => _users ??= new Repository<User>(_context);
    
    public IRepository<Product> Products 
        => _products ??= new Repository<Product>(_context);

    public async Task<int> SaveChangesAsync()
        => await _context.SaveChangesAsync();

    public void Dispose() => _context?.Dispose();
}
```

---

### 2️⃣ **Specification Pattern Kullan**

```csharp
public class Specification<T>
{
    public Expression<Func<T, bool>> Criteria { get; set; }
    public List<Expression<Func<T, object>>> Includes { get; } = new();
    public int Take { get; set; } = 0;
    public int Skip { get; set; } = 0;
    public bool IsPagingEnabled { get; set; } = false;
}

public class UserSpecification : Specification<User>
{
    public UserSpecification(string email)
    {
        Criteria = u => u.Email == email;
        Includes.Add(u => u.Orders);
    }
}
```

---

### 3️⃣ **Exception Handling Ekle**

```csharp
public virtual async Task CreateAsync(T entity)
{
    try
    {
        await _set.AddAsync(entity);
    }
    catch (DbUpdateException ex)
    {
        throw new ApplicationException("Veri eklenirken hata oluştu", ex);
    }
}
```

---

### 4️⃣ **Logging Ekle**

```csharp
private readonly ILogger<Repository<T>> _logger;

public Repository(DbContext context, ILogger<Repository<T>> logger)
{
    _context = context;
    _set = context.Set<T>();
    _logger = logger;
}

public virtual async Task CreateAsync(T entity)
{
    _logger.LogInformation($"Creating entity of type {typeof(T).Name}");
    await _set.AddAsync(entity);
}
```

---

## 🎓 Sonuç

Bu Generic Repository Pattern, veri erişim katmanını soyutlamak için iyi bir temel sağlar. Ancak, yukarıda belirtilen sorunların düzeltilmesi ve iyileştirmelerin uygulanması, üretim ortamında daha güvenilir ve bakım yapılabilir bir sistem oluşturur.

### 📌 Özet Kontrol Listesi:

- [ ] Dönüş tiplerini nullable yap
- [ ] Constructor'ı public yap
- [ ] SaveChangesAsync ekle
- [ ] Task.Run kullanımını kaldır
- [ ] String Include yerine Lambda kullan
- [ ] IDisposable implement et
- [ ] Unit of Work pattern ekle
- [ ] Exception handling ekle
- [ ] Logging ekle
- [ ] Unit testler yaz

---

**Hazırlanma Tarihi:** 2026  
**Framework:** .NET 8.0  
**ORM:** Entity Framework Core 8.0
