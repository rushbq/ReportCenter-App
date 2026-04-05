
### 三層式架構 (SOLID)

```
Presentation (UI)
└── Pages/Admin/Catalog.cshtml(.cs)
└── Pages/Admin/Index.cshtml(.cs)

Business Logic (BLL)
├── Services/ICatalogService.cs     ← 目錄管理介面
├── Services/CatalogService.cs      ← 目錄管理實作
├── Services/IReportService.cs      ← 儀表板/報表介面 (精簡)
└── Services/SqlReportService.cs    ← 儀表板/報表實作

Data Access (DAL)
├── Repositories/ICatalogRepository.cs   ← 目錄資料存取介面
└── Repositories/SqlCatalogRepository.cs ← SQL Server 實作
```

**SOLID 原則應用：**
- **S** (單一職責): `IReportService` 只負責儀表板/報表，`ICatalogService` 只負責目錄管理
- **O** (開放封閉): 透過介面擴展，不修改既有程式碼
- **L** (里氏替換): Repository/Service 均可替換實作
- **I** (介面隔離): 拆分大介面為兩個專責介面
- **D** (依賴反轉): Page → Service → Repository，均透過 DI 注入介面