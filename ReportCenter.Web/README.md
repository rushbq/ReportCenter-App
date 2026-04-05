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