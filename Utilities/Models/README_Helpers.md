# Helpers Klasörü

Bu klasör, uygulamanın tüm yardımcı sınıflarını ve utility'lerini içerir.

## 📁 İçerik

### Generics
Generic Repository Pattern implementasyonu ve ilgili arayüzleri içerir.

#### Dosyalar:
- **IRepository.cs** - Repository arayüzü
- **Repository.cs** - Repository implementasyonu

#### Açıklama:
Entity Framework Core ile veri tabanı işlemlerini soyutlamak için kullanılan generic repository pattern'i sağlar.

---

## 🚀 Hızlı Başlangıç

### 1. Dependency Injection Kurulumu

```csharp
// Program.cs
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
```

### 2. Servis Kullanımı

```csharp
public class UserService
{
    private readonly IRepository<User> _repository;

    public UserService(IRepository<User> repository)
    {
        _repository = repository;
    }

    public async Task<User?> GetUserAsync(int id)
    {
        return await _repository.ReadByKeyAsync(id);
    }
}
```

---

## 📚 Mevcut Metodlar

### CREATE
```csharp
Task CreateAsync(T entity)
Task CreateManyAsync(IEnumerable<T> entities)
```

### READ
```csharp
Task<T?> ReadByKeyAsync(object entityKey)
Task<T?> FindFirstAsync(Expression<Func<T, bool>>? expression = null)
Task<IEnumerable<T>> ReadManyAsync(Expression<Func<T, bool>>? expression = null, params string[] includes)
```

### UPDATE
```csharp
Task UpdateAsync(T entity)
Task UpdateManyAsync(IEnumerable<T> entities)
Task UpdateManyAsync(Expression<Func<T, bool>>? expression = null)
```

### DELETE
```csharp
Task DeleteAsync(T entity)
Task DeleteManyAsync(IEnumerable<T> entities)
Task DeleteManyAsync(Expression<Func<T, bool>>? expression = null)
```

### UTILITY
```csharp
Task<int> CountAsync(Expression<Func<T, bool>>? expression = null)
Task<bool> AnyAsync(Expression<Func<T, bool>>? expression = null)
```

---

## ⚠️ Bilinen Sorunlar ve Çözümler

### Problem: SaveChanges Çağrılmıyor
Veri ekledikten sonra `SaveChangesAsync()` eklemek gerekebilir.

### Problem: Task.Run Gereksiz Kullanımı
Update ve Delete işlemleri sırasında Thread Pool'u işgal ediyor.

### Problem: String Include Kullanımı
Include adları compile-time'da kontrol edilmiyor.

---

## 📝 Kullanım Örnekleri

### Örnek 1: Kullanıcı Ekleme
```csharp
var user = new User { Name = "Ali", Email = "ali@example.com" };
await _repository.CreateAsync(user);
```

### Örnek 2: Koşula Göre Arama
```csharp
var activeUsers = await _repository.ReadManyAsync(
    u => u.IsActive == true
);
```

### Örnek 3: Koşula Göre Silme
```csharp
await _repository.DeleteManyAsync(u => u.CreatedAt < DateTime.Now.AddYears(-1));
```

### Örnek 4: Sayma
```csharp
int count = await _repository.CountAsync(u => u.Status == "Active");
```

---

## 🔧 Gereksinimler

- **.NET 8.0** veya üzeri
- **Entity Framework Core 8.0** veya üzeri
- Async/Await desteği

---

## 📌 Notlar

- Tüm metodlar `async/await` desteğine sahiptir
- Generic `<T>` sınıfları ile herhangi bir entity tipi ile çalışabilir
- Expression<Func<T, bool>> kullanarak LINQ sorguları destekler
- DbContext injection gereklidir

---

## 🎯 Sonraki Adımlar

- Unit of Work pattern'i implement et
- Specification pattern'i ekle
- Exception handling'i geliştir
- Logging mekanizması ekle
- Unit testleri yaz

---

**Son Güncelleme:** 2026  
**Framework:** .NET 8.0  
**ORM:** Entity Framework Core 8.0
