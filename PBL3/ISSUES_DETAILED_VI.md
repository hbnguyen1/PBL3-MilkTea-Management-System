# 📋 DANH SÁCH CÁC VẤN ĐỀ DỰ ÁN PBL3 - CHI TIẾT

**Cập nhật:** 2024 (sau khi implement OrderQueue)
**Trạng thái:** 15 issues (2 critical, 5 high, 5 medium, 3 low)

---

## 🔴 **CRITICAL (Phải fix ngay)**

### #1: Race Condition - GetNextItemID()
**Vị trí:** `PBL3\Interface\ItemService.cs` - method `GetNextItemID()`  
**Vấn đề:**
```csharp
// Lấy ID lớn nhất, cộng 1 - NHƯ VẬY CÓ THỂ 2 THREAD CÙNG LẤY ID GIỐ
int maxId = conn.Items.Max(i => i.itemID) ?? 0;
return maxId + 1;
```
- Nếu có 2 thread gọi cùng lúc → cùng lấy maxID → trùng ID → lỗi PRIMARY KEY VIOLATION
- **Ảnh hưởng:** Data integrity, khách hàng thêm 2 item cùng lúc → duplicate key error

**Giải pháp:**
- A) Dùng DB sequence/identity (best practice)
- B) Lock database level (pessimistic concurrency)
- C) Guid thay vì int (no conflict, nhưng thay đổi schema)

**Độ ưu tiên:** CRITICAL (khách hàng report lỗi → business down)

---

### #2: Password Plain Text
**Vị trí:** `PBL3\Models\Users.cs` - property `password`  
**Vấn đề:**
```csharp
public string password { get; set; } // ❌ LƯU PLAIN TEXT VÀO DB
```
- Database có thể bị dump → tất cả password leak
- Audit log, SQL Server backup → ai có access DC thấy hết password
- Không comply GDPR/SOC2

**Giải pháp:**
- Dùng `BCrypt` hoặc `Argon2` hash password trước save DB
- Login: hash input password → so sánh với DB hash

**Độ ưu tiên:** CRITICAL (security incident risk)

---

## 🟠 **HIGH (Nên fix tuần này)**

### #3: Không có Dependency Injection
**Vị trí:** Tất cả `*Service.cs` và `*Manager.cs`  
**Vấn đề:**
```csharp
// ❌ NHƯ VẬY
var itemService = new ItemService(); // Mỗi lần tạo mới → memory leak, DbContext leak
itemService.AddItem(...);

// Thay vì:
// ✅ DI từ constructor
public ItemManager(IItemService itemService) { ... }
```
- Mỗi `new ItemService()` → `new MilkTeaDBContext()` → DbContext ko dispose → Memory leak
- Không thể mock test (unit test khó)
- DbContext pooling (connection pool optimization) không work

**Giải pháp:**
- Setup Microsoft.Extensions.DependencyInjection
- Register `MilkTeaDBContext`, `IItemService`, `IOrderService`, etc. vào DI container
- Inject vào WPF windows qua constructor

**Độ ưu tiên:** HIGH (app chạy lâu → memory leak → crash)

---

### #4: DbContext Pooling Chưa Enable
**Vị trí:** `PBL3\Data\MilkTeaDB.cs` - method `OnConfiguring()`  
**Vấn đề:**
```csharp
// ❌ HIỆN TẠI
optionsBuilder.UseSqlServer(connectionString);
// Mỗi lần tạo context → new connection → slow

// ✅ NÊN
optionsBuilder
    .UseSqlServer(connectionString)
    .EnableSensitiveDataLogging() // For dev only
    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking); // For read-heavy
```

**Giải pháp:** Setup DbContextPooling + connection pooling SQL Server

**Độ ưu tiên:** HIGH (performance, user wait lâu khi order/item nhiều)

---

### #5: UserSession Thread-Safe Issues
**Vị trí:** `PBL3\Core\UserSession.cs`  
**Vấn đề:**
```csharp
public static Users? CurrentUser { get; set; } // ❌ Static + property = thread-unsafe
// Multi-thread access → race condition → crash or wrong user context
```

**Giải pháp:**
```csharp
private static readonly object _lock = new object();
public static Users? CurrentUser
{
    get { lock (_lock) { return _currentUser; } }
    set { lock (_lock) { _currentUser = value; } }
}
```

**Độ ưu tiên:** HIGH (multi-user, app crash in edge case)

---

### #6: Exception Handling Minimal
**Vị trí:** Tất cả `*Service.cs`, `*Manager.cs`, `*.xaml.cs` windows  
**Vấn đề:**
- Hầu hết code không có try-catch
- Nếu DB down → unhandled exception → app crash
- User không biết gì xảy ra → chỉ thấy white screen

**Giải pháp:**
- Wrap DB operations vào try-catch
- Log errors vào file/logger
- Show MessageBox or status bar cho user (friendly error)

**Độ ưu tiên:** HIGH (production stability, user experience)

---

### #7: Input Validation Missing
**Vị trí:** `PBL3\Manangers\ItemManager.cs`, `OrderProcessingManager.cs`, etc.  
**Vấn đề:**
- Không kiểm tra null, empty, negative price
- Nếu nhân viên nhập `-100` giá → lưu DB → nonsense data

**Giải pháp:**
- Validate input trước gọi Service
- Show error message nếu invalid

**Độ ưu tiên:** HIGH (data quality, business logic integrity)

---

## 🟡 **MEDIUM (Nên fix trong tháng này)**

### #8: Composite Key (itemID, size) Design Ambiguity
**Vị trí:** `PBL3\Data\MilkTeaDB.cs` - `OnModelCreating()`  
**Vấn đề:**
```csharp
modelBuilder.Entity<Item>().HasKey(i => new { i.itemID, i.size });
```
- Hiện tại item có 2 key: itemID (định danh item), size (M/L)
- Nếu thêm size mới (XL) → thêm row mới với same itemID? Hay cập nhật existing?
- ItemService.GetItemById(1) → return M hay L hay both?

**Giải pháp:**
- Option A: Single identity key (itemID), size là attribute riêng (CLEANER)
- Option B: Keep composite nhưng document rõ semantic

**Độ ưu tiên:** MEDIUM (design debt, confusing logic)

---

### #9: N+1 Query Problem
**Vị trí:** `PBL3\Interface\ItemService.cs` - method `isAvailable()`  
**Vấn đề:**
```csharp
// ❌ N+1: Nếu loop 100 items → 101 queries (1 for items + 100 cho từng item)
foreach (var item in items)
{
    var hasRecipe = conn.Recipes.Where(r => r.itemID == item.itemID).FirstOrDefault();
}
```

**Giải pháp:** `.Include(i => i.Recipes)` hoặc batch query

**Độ ưu tiên:** MEDIUM (performance khi có nhiều items)

---

### #10: GetNextItemID() Dups with Composite Key
**Vị trí:** `PBL3\Manangers\ItemManager.cs`  
**Vấn đề:**
```csharp
int itemID = itemService.GetNextItemID(); // Gọi 1 lần
// Sau đó tạo 2 items M và L với cùng itemID
Item itemM = new Item { itemID = itemID, size = "M", ... };
Item itemL = new Item { itemID = itemID, size = "L", ... };
```
- Nếu 2 admin thêm item cùng lúc → GetNextItemID() race condition (vấn đề #1)

**Giải pháp:** Fix #1 trước (DB sequence), rồi code này sẽ ổn

**Độ ưu tiên:** MEDIUM (depends on #1)

---

### #11: Naming Convention Inconsistency
**Vị trí:** Folder `PBL3\Manangers\` (typo: should be `Managers`)  
**Vấn đề:**
```
Manangers/  ← typo (missing 'a')
```

**Giải pháp:** Rename folder → `Managers`

**Độ ưu tiên:** MEDIUM (code cleanliness, but breaking change)

---

### #12: OrderQueue Persistence Not Implemented
**Vị trí:** `PBL3\Core\OrderQueue.cs`  
**Vấn đề:**
- Queue là in-memory (Queue<T>)
- Khi app restart → tất cả orders trong queue mất
- Staff chưa xử lý → customer hang hơi

**Giải pháp:**
- A) Load queue from JSON file on startup
- B) Load queue from OrderQueueLog DB table
- C) Keep in-memory (hiện tại, accept data loss)

**Độ ưu tiên:** MEDIUM (depends on business requirement)

---

### #13: Missing OrderQueueLog Database Table
**Vị trí:** `PBL3\Data\MilkTeaDB.cs` (nếu dùng DB persistence)  
**Vấn đề:**
- Nếu muốn persist queue → cần OrderQueueLog table
- Migration chưa có

**Giải pháp:**
```csharp
// Add to DbContext OnModelCreating()
modelBuilder.Entity<OrderQueueLog>().HasKey(x => x.queueLogID);
```
Then: `dotnet ef migrations add AddOrderQueueLog` → `dotnet ef database update`

**Độ ưu tiên:** MEDIUM (if persistence chosen)

---

## 🟢 **LOW (Nice to have)**

### #14: UpdateItem Logic Bug (Potential)
**Vị trị:** `PBL3\Interface\ItemService.cs` - method `UpdateItem()` (if exists)  
**Vấn đề:**
- Nếu update itemID → violate FK (recipes, orderDetails pointing to old itemID)

**Giải pháp:**
- Không allow update itemID (primary key = immutable)
- hoặc cascade update FK (risky)

**Độ ưu tiên:** LOW (rare operation, need to verify if UpdateItem used)

---

### #15: Logging Not Implemented
**Vị trị:** Tất cả services, managers  
**Vấn đề:**
- Khi error → không biết gì xảy ra
- Troubleshooting khó

**Giải pháp:**
- Setup Serilog hoặc NLog
- Log to console + file + event viewer

**Độ ưu tiên:** LOW (nice-to-have in prod, not blocking)

---

## 📊 **SUMMARY TABLE**

| # | Tiêu đề | Vị trí | Độ ưu tiên | Effort | Status |
|---|---------|--------|-----------|--------|--------|
| 1 | Race condition GetNextItemID | ItemService.cs | 🔴 CRITICAL | 2-3h | TODO |
| 2 | Password plain text | Users.cs | 🔴 CRITICAL | 2-3h | TODO |
| 3 | No DI setup | All Services | 🟠 HIGH | 4-5h | TODO |
| 4 | DbContext pooling | MilkTeaDB.cs | 🟠 HIGH | 1h | TODO |
| 5 | UserSession thread-safe | UserSession.cs | 🟠 HIGH | 1h | TODO |
| 6 | Exception handling | All files | 🟠 HIGH | 3-4h | TODO |
| 7 | Input validation | Managers | 🟠 HIGH | 2-3h | TODO |
| 8 | Composite key design | MilkTeaDB.cs | 🟡 MEDIUM | 1-2h | TODO |
| 9 | N+1 queries | ItemService.cs | 🟡 MEDIUM | 1-2h | TODO |
| 10 | GetNextItemID dups | ItemManager.cs | 🟡 MEDIUM | depends #1 | TODO |
| 11 | Naming: Manangers typo | Folder | 🟡 MEDIUM | 0.5h | TODO |
| 12 | OrderQueue persistence | OrderQueue.cs | 🟡 MEDIUM | 2-3h | PENDING |
| 13 | OrderQueueLog table | MilkTeaDB.cs | 🟡 MEDIUM | 1h | PENDING |
| 14 | UpdateItem bug | ItemService.cs | 🟢 LOW | 0.5h | VERIFY |
| 15 | Logging setup | All files | 🟢 LOW | 2h | TODO |

---

## 🎯 **RECOMMENDED FIX ORDER**

### **Phase 1 (This week) - CRITICAL:**
1. Fix #1 (Race condition) → Enable DB sequence for itemID
2. Fix #2 (Password hashing) → Use BCrypt

### **Phase 2 (Next week) - HIGH:**
3. Fix #3 (DI setup) → Register services
4. Fix #4 (DbContext pooling)
5. Fix #5 (UserSession thread-safe)
6. Fix #6 (Exception handling)
7. Fix #7 (Input validation)

### **Phase 3 (Month) - MEDIUM:**
8. Fix #8 (Composite key review)
9. Fix #9 (N+1 queries)
10. Fix #11 (Naming convention)
11. Decide #12, #13 (Queue persistence)

---

**Cập nhật:** Đợi hướng dẫn từ user về fix priority.
