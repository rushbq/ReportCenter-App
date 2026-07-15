# ReportCenter-App

企業報表中心 (Enterprise Report Center) - 台灣寶工營運數據視覺化儀表板

## 技術架構

- **後端:** ASP.NET Core 10 Razor Pages (C#)
- **前端:** Tailwind CSS (CDN) + Alpine.js 3.x + Chart.js 4 + HTMX 2.0.4
- **圖示:** Lucide Icons (unpkg CDN)
- **字體:** Noto Sans TC (Google Fonts)
- **日誌:** Serilog (Console + File + Seq)
- **資料:** MockReportService（DI 注入），可快速切換為 API 實作

## 專案結構

```
ReportCenter.Web/
├── Models/ReportModels.cs          # 資料模型 (Company, UserInfo, Department, Report, KPI, ChartData, ReportCatalogItem 等)
├── Middleware/
│   ├── DevWindowsAuthMiddleware.cs # 開發環境模擬 Windows AD 驗證中介層
│   └── UnhandledExceptionLoggingMiddleware.cs # 未處理例外結構化記錄
├── Models/Settings/
│   ├── ReportBaseUrlSettings.cs    # 報表外部連結 BaseUrl 設定
│   └── MockWindowsAuthSettings.cs  # 開發環境模擬 AD 使用者設定
├── Repositories/
│   ├── ICatalogRepository.cs       # 報表目錄 DAL 介面 (ReportCenter DB)
│   ├── SqlCatalogRepository.cs     # 報表目錄 Dapper 實作
│   ├── IPksysRepository.cs         # PKSYS DAL 介面 (User_Dept + User_Profile)
│   ├── SqlPksysRepository.cs       # PKSYS Dapper 實作
│   ├── IPermissionRepository.cs    # 權限 DAL 介面 (ReportCenter DB)
│   └── SqlPermissionRepository.cs  # 權限 Dapper 實作
├── Services/
│   ├── IReportService.cs           # 資料服務介面
│   ├── SqlReportService.cs         # SQL 實作 (含權限過濾)
│   ├── MockReportService.cs        # Mock 實作 (儀表板 KPI/圖表)
│   ├── IPermissionService.cs       # 權限管理 BLL 介面
│   └── PermissionService.cs        # 權限管理 BLL 實作
├── Pages/
│   ├── Index.cshtml(.cs)           # 首頁 (快速存取釘選報表；營運總覽儀表板規劃中，暫已移除)
│   ├── Department.cshtml(.cs)      # 部門報表列表 (搜尋、分類 Tab、卡片檢視)
│   ├── Admin/
│   │   ├── Index.cshtml(.cs)       # 管理總覽 (KPI、部門矩陣、孤立報表、相依性分析)
│   │   ├── Catalog.cshtml(.cs)     # 報表目錄管理 (CRUD、篩選、編輯 Modal)
│   │   └── Permission.cshtml(.cs)  # 權限管理 (批次指派、個人權限)
│   └── Shared/
│       ├── _Layout.cshtml          # 主版面配置 (TopNav + Sidebar + Content + 搜尋 Modal)
│       ├── _TopNav.cshtml          # 頂部導航列 (Logo、公司選單、搜尋、使用者資訊)
│       ├── _Sidebar.cshtml         # 側邊欄 (部門選單、系統管理、收藏)
│       └── _KpiCard.cshtml         # KPI 卡片元件
├── wwwroot/
│   ├── images/logo.png             # 公司 Logo
│   ├── favicon.svg                 # 品牌 Favicon
│   ├── css/site.css                # 自訂樣式 (極少，主要用 Tailwind)
│   └── js/site.js                  # Alpine Store (搜尋、收藏、Toast)
├── appsettings.json                # 共用設定 (base)
├── appsettings.Development.json    # 開發環境設定
├── appsettings.Staging.json        # 測試環境設定
├── appsettings.Production.json     # 正式環境設定 (範本；實際部署走外部路徑)
├── web.config                      # IIS 部署設定 (Windows 驗證)
├── logs/                           # Serilog File Sink 輸出 (gitignore)
└── docs/
    ├── frontend-spec.md            # 前端技術規格文件
    ├── db-ddl.sql                  # ReportCenter DB 完整 DDL
    ├── migration-001-permission.sql # 權限表 Migration
    ├── shared-infra.Production.template.json        # 伺服器共用機密範本
    └── reportWebApp-Site.Production.template.json   # 站台專用機密範本
```

## 路由

| 路由 | 頁面 | 說明 |
|------|------|------|
| `/` | Index | 首頁 (快速存取釘選報表) |
| `/Department?dept={id}` | Department | 部門報表列表 |
| `/Admin` | Admin/Index | 報表管理總覽 (KPI、部門矩陣、孤立報表) |
| `/Admin/Catalog` | Admin/Catalog | 報表目錄管理 (CRUD) |
| `/Admin/Permission` | Admin/Permission | 權限管理 (批次指派、個人權限) |

## 資料架構 (Service Layer)

```
IReportService (介面)               # 首頁/部門頁資料
  └── SqlReportService              # Dapper + 權限過濾
  └── MockReportService             # Mock 資料 (儀表板 KPI/圖表仍沿用)

ICatalogService (介面)              # 報表目錄管理 BLL
  └── CatalogService                # 依賴 ICatalogRepository + IPksysRepository + DepartmentDisplaySettings
      ├── GetAdminStats()           → 統計 KPI + 部門矩陣 (分 TW/SH，依 appsettings 過濾)
      ├── BuildDeptUsagesForRegion() → 私有方法：建置單一區域部門矩陣
      └── CRUD / 分類 / 部門 / 資料夾 / BaseUrl

IPermissionService (介面)           # 權限管理 BLL
  └── PermissionService             # 依賴 IPermissionRepository + IPksysRepository + ICatalogRepository
      ├── GrantBatch()              → 批次授權 (冪等)
      ├── GetDeptUserTree()         → 部門→使用者樹 (UI 用)
      └── GetReportTree()           → 分類→報表樹 (UI 用)

ICatalogRepository (介面)           # 資料存取層 (ReportCenter DB)
  └── SqlCatalogRepository          # Dapper + SQL Server

IPksysRepository (介面)             # PKSYS 資料存取層 (User_Dept + User_Profile)
  └── SqlPksysRepository            # Dapper + SQL Server (PKSYS 連線)

IPermissionRepository (介面)        # 權限資料存取層 (ReportCenter DB)
  └── SqlPermissionRepository       # Dapper + SQL Server
```

## 雙連線字串

| 連線名稱 | 資料庫 | 用途 |
|----------|--------|------|
| `ReportCenter` | ReportCenter | 報表目錄、權限、收藏、釘選 |
| `PKSYS` | PKSYS | User_Dept (部門)、User_Profile (使用者) |

- **開發環境**: `secrets.json` (ASP.NET Core User Secrets)
- **正式環境**: IIS `web.config` 環境變數指向外部 JSON 檔（載入順序：共用 → 站台，後者覆蓋前者）

| 環境變數 | 用途 | 範例路徑 |
|----------|------|---------|
| `SHARED_INFRA_PATH` | 伺服器各站台共用的機密設定 (如共用連線字串、Seq) | `D:\CompanySecrets\shared-infra.Production.json` |
| `RC_CONFIG_PATH` | 本站台專用的機密設定 (如站台連線字串、AdminUsers) | `D:\CompanySecrets\reportWebApp-Site.Production.json` |

**切換真實 API 只需：**
1. 建立 `ApiReportService : IReportService`
2. 在 `Program.cs` 將 `MockReportService` 換成 `ApiReportService`
3. 頁面與前端完全不需修改

## Logging 架構

使用 **Serilog** 作為結構化日誌框架，所有環境統一輸出至 File Sink，非開發環境額外輸出至 Seq。

### Sink 配置

| Sink | 環境 | 說明 |
|------|------|------|
| **Console** | Development | 精簡格式，含 SourceContext，方便本機除錯 |
| **File** | Staging / Production | `logs/rc-YYYYMMDD.log`，每日滾動，保留 30 天，單檔 50 MB |
| **Seq** | Staging / Production | 結構化查詢，選配 (有設定 `Seq:ServerUrl` 才啟用) |

### Log 等級策略

| 環境 | Default | Microsoft | Microsoft.AspNetCore |
|------|---------|-----------|---------------------|
| Development | Debug | Information | Warning |
| Staging | Information | Warning | Warning |
| Production | Information | Warning | Warning |

### 結構化屬性 (Enricher)

每筆 Log 自動附帶：`Application`、`Environment`、`MachineName` (由 `WithProperty` 注入)；`SourceContext` (由 Serilog 框架自動注入)

Request Logging (`UseSerilogRequestLogging`) 額外附帶：`TraceId`、`UserName`、`ClientIp`

### 智慧過濾

- 靜態資源 (`/css`, `/js`, `/images`, `/favicon`) 降為 Verbose 等級，不寫入檔案
- 慢請求 (> 3s) 自動升級為 Warning
- 4xx 回應為 Warning，5xx / Exception 為 Error

### 例外記錄 (`UnhandledExceptionLoggingMiddleware`)

- 結構化屬性推入 LogContext：`TraceId`、`RequestPath`、`HttpMethod`、`UserName`、`QueryString`、`ClientIp`
- `SqlException` 額外記錄：`SqlNumber`、`Procedure`、`SqlLine`、`SqlState`

### File Sink 輸出格式

```
2026-04-08 14:30:15.123 +08:00 [INF] [ReportCenter.Web.Pages.IndexModel] TraceId=abc123 User=PROSKIT\10255 首頁載入完成
```

## appsettings 環境設定

| 檔案 | 用途 | 包含內容 |
|------|------|---------|
| `appsettings.json` | 共用 base 設定 | Serilog 基底等級、ReportBaseUrls、AdminUsers、DepartmentDisplay |
| `appsettings.Development.json` | 開發環境 | MockWindowsAuth 模擬使用者、DetailedErrors |
| `appsettings.Staging.json` | 測試環境 | Seq 設定(空，由外部機密檔填入) |
| `appsettings.Production.json` | 正式環境範本 | Seq 設定(空，由外部機密檔填入) |

**注意事項：**
- Development 連線字串放在 `secrets.json` (User Secrets)
- Staging / Production 實際機密設定由 IIS `web.config` 環境變數指向外部 JSON 檔：
  - `SHARED_INFRA_PATH` → 伺服器共用設定 (如 Seq、共用連線字串)
  - `RC_CONFIG_PATH` → 站台專用設定 (如本站連線字串、AdminUsers)
  - 載入順序：`appsettings.json` → `appsettings.{env}.json` → `SHARED_INFRA_PATH` → `RC_CONFIG_PATH` (後者覆蓋前者)

## Alpine.js Store (site.js)

| Store | 用途 | 持久化 |
|-------|------|--------|
| `$store.search` | 全站搜尋 Modal、⌘K 快捷鍵 | 否 |
| `$store.favorites` | 報表收藏管理（toggle 時自動顯示 Toast） | localStorage |
| `$store.toast` | 全域 Toast 提示（`show(msg)` 呼叫，2.5 秒後自動消失） | 否 |

> 「最近瀏覽」(`$store.recent`) 已隨 `/Report` 明細頁一併移除：報表皆為 SmartQuery / SSRS 外部連結、
> 開新視窗，本站沒有可記錄瀏覽的頁面載入時機。舊使用者的 `localStorage` key `rc_recent` 不再讀寫。

## 公司切換機制

- 使用 Cookie (`companyId`) 儲存選擇的公司
- TopNav 切換時寫入 Cookie 並重新載入頁面
- 後端透過 `IHttpContextAccessor` 讀取 Cookie 決定資料來源
- 未來兩個公司別的報表權限將分開管理

## Windows 驗證機制

### 環境切換策略

| 環境 | 驗證方式 | 實作 |
|------|---------|------|
| Development (macOS) | 模擬 Windows AD 身份 | `DevWindowsAuthMiddleware` 讀取 `appsettings.Development.json` 的 `MockWindowsAuth` 區段 |
| Production (IIS) | Windows 驗證 (Negotiate) | `Microsoft.AspNetCore.Authentication.Negotiate` + IIS Windows Authentication |

### 開發環境設定 (`appsettings.Development.json`)

```json
"MockWindowsAuth": {
  "UserName": "PROSKIT\\10255",
  "DisplayName": "高先生",
  "EmployeeId": "10255",
  "Department": "資訊部",
  "DepartmentId": "109"
}
```

### IIS 部署

- `web.config` 已設定 `windowsAuthentication enabled="true"` 與 `anonymousAuthentication enabled="false"`
- `forwardWindowsAuthToken="true"` 確保 Windows 驗證 Token 傳遞至 ASP.NET Core
- 部署前需在 IIS 伺服器安裝「Windows Authentication」功能

### 使用者資訊解析流程

1. **Development**: `DevWindowsAuthMiddleware` → Claims → `SqlReportService.GetCurrentUser()`
2. **Production**: IIS Negotiate → Claims (僅 `ClaimTypes.Name`) → `SqlReportService.GetCurrentUser()`
3. Production 環境僅提供 Windows 帳號名稱 (`DOMAIN\username`)，未來可透過 AD/DB 查詢補充 DisplayName、Department 等詳細資料

## 權限管理

### 權限架構

- **以人為主**：針對人給予報表存取權限，不設計部門權限
- **預設無權限**：未指派權限的使用者看不到任何報表
- **權限 DB**：`UserReportPermission` 表在 ReportCenter 資料庫
- **使用者來源**：PKSYS 資料庫的 `User_Profile` 表

### 資料模型

- `UserReportPermission` — 使用者報表權限 (EmployeeId + ReportID，UNIQUE)
- `UserProfileItem` — User_Profile 對應 (AccountName, DisplayName, DeptID, DeptName)
- `DeptWithUsers` — 部門含使用者清單 (人員樹節點)
- `CategoryWithReports` — 分類含報表清單 (報表樹節點)
- `ReportTreeItem` — 報表樹葉節點

### 權限管理 UI (`/Admin/Permission`)

兩種模式：
1. **功能→多人 (批次)**：左右雙欄樹狀選擇，勾選報表+人員後批次指派
2. **單一人→功能 (個人)**：搜尋人員，查看/新增/移除其權限

### 權限檢查流程

1. `SqlReportService.GetReports()` — 依 `GetAuthorizedReportIds()` 過濾報表列表
2. Admin 頁面目前不限制存取

## 資料庫結構 (DDL)

完整 DDL 定義於 `docs/db-ddl.sql`，Migration 腳本依序編號於 `docs/migration-*.sql`。

### ReportCenter 資料庫

| 資料表 | 用途 | PK |
|--------|------|-----|
| `ReportCategory` | 報表分類 | `CategoryID` (IDENTITY) |
| `ReportCatalog` | 報表目錄主表 | `ReportID` (IDENTITY) |
| `ReportDepartment` | 報表×部門指派 (多對多) | `(ReportID, DeptID)` |
| `ReportDependency` | 報表相依物件 | `(ReportID, DependsOn)` |
| `UserReportPermission` | 使用者報表權限 | `PermissionID` (IDENTITY) + UQ `(EmployeeId, ReportID)` |
| `UserFavorite` | 使用者收藏 | `(UserID, ReportID)` |
| `UserPin` | 使用者釘選 (快速存取) | `(UserID, ReportID)` |

### PKSYS 資料庫 (外部系統，唯讀)

| 資料表 | 用途 |
|--------|------|
| `User_Dept` | 部門主檔 (Area, DeptID, DeptName) |
| `User_Profile` | 人員主檔 (Account_Name, Display_Name, DeptID) |

## 篩選功能

| 頁面 | 篩選器 | 實作方式 |
|------|--------|---------|
| Department | 搜尋 + 分類 Tab | 前端 Alpine 篩選（固定依更新日期新到舊排序） |
| Admin/Catalog | 工具類型（全部/SmartQuery/SSRS） | 前端 Alpine 篩選 |
| Admin/Catalog | 狀態（全部/啟用/停用） | 前端 Alpine 篩選 |
| Admin/Catalog | 搜尋（名稱、來源、路徑） | 前端 Alpine 篩選 |

## 報表目錄管理

### 報表來源類型

| 類型 | 說明 | 色彩標記 |
|------|------|---------|
| SmartQuery | 外部 SmartQuery 報表，開新視窗連結 | amber (橙) |
| SSRS | SQL Server Reporting Services，開新視窗連結 | indigo (靛藍) |

`Internal` (開發於本系統的報表，emerald 綠) 短期內不開發，已從報表目錄管理的選項與篩選中移除，
`/Report` 明細頁亦已刪除。DB 欄位、`AdminStats.InternalCount`、badge 配色與 `IReportService`
的 `GetMaterialRows` / `GetReportChartData` 等 Mock 方法仍保留，未來要重啟開發時再接回。

### 資料模型

- `ReportCatalogItem` — 報表清冊主表 (對應 DB `ReportCatalog`)
- `DeptAssignment` — 部門指派 (對應 DB `ReportDepartment`)
- `Dependencies` — 相依物件清單 (對應 DB `ReportDependency`)
- `AdminStats` — 管理總覽 KPI ViewModel，含 `DeptUsagesTW` / `DeptUsagesSH` (依區域分開)
- `DeptUsage` — 部門 → 報表對照
- `DependencyGroup` — 相依性分析 ViewModel (Admin/Index.cshtml.cs)

### 管理功能

- **總覽頁 (`/Admin`)**: KPI 卡片、部門使用矩陣 (台灣/上海雙欄)、孤立報表警示、最近異動、相依性分析
- **目錄管理 (`/Admin/Catalog`)**: 搜尋篩選 (工具類型/啟停狀態/資料夾)、新增/編輯 Modal (Tab 分頁: 基本設定、部門指派、進階設定)、啟停切換、刪除確認

### 部門過濾規則

**所有 Service 層取得部門的方法，都必須依 `appsettings.json` → `DepartmentDisplay.Regions` 設定過濾，僅回傳設定中存在的 DeptID。** 這是全域規則，不限於特定頁面。

目前遵循此規則的方法：

| Service | 方法 | 過濾方式 |
|---------|------|---------|
| `SqlReportService` | `GetDepartments()` | 迴圈比對 `config.Depts` |
| `CatalogService` | `BuildDeptUsagesForRegion()` | `allowedDeptIds` HashSet |
| `CatalogService` | `GetCatalogDepartments()` | `allowedDeptIds` HashSet |

新增任何取得部門的方法時，務必加入相同的過濾邏輯。
若需新增/移除顯示的部門，修改 `appsettings.json` 的 `DepartmentDisplay` 區段即可。

## 匯出功能

- **Excel:** 原按鈕僅存在於 `/Report` 明細頁 (只顯示 Toast 提示)，該頁移除後目前全站無匯出入口；
  未來若重啟內部報表開發，再改用後端套件（如 ClosedXML）實作實際下載
- **PDF:** 已移除，未來不開放

## 設計系統 (Tailwind 色彩 Token)

| Token | 色碼 | 用途 |
|-------|------|------|
| `pri` | `#005758` | 主色 (深青) |
| `pri-hover` | `#006d6e` | 主色 Hover |
| `pri-light` | `#e8f4f4` | 主色淺底 |
| `acc` | `#00b4b6` | 強調色 (青) |
| `surface` | `#f5f7f8` | 頁面背景 |
| `txt` | `#1a2e2f` | 主要文字 |
| `txt-sec` | `#5c7576` | 次要文字 |
| `ok` | `#0d9668` | 正向趨勢 (綠) |
| `bad` | `#dc4a4a` | 負向趨勢 (紅) |
| `bdr` | `#e2e8ea` | 邊框 |

## 開發指令

```bash
# 啟動開發伺服器
cd ReportCenter.Web && dotnet run    # http://localhost:5276

# 建置
cd ReportCenter.Web && dotnet build
```

## 開發規範

- 語言：繁體中文 (UI 文字、註解、commit message)
- 樣式優先使用 Tailwind utility class，避免自訂 CSS
- 互動行為使用 Alpine.js (`x-data`, `x-show`, `@click` 等)
- 複雜 Alpine 邏輯抽成 `function xxxPage()` 放在 `@section Scripts` 內，避免 Razor `@` 衝突
- 圖表使用 Chart.js，初始化放在 `<script>` 區塊內
- 元件化使用 Razor Partial View (`_*.cshtml`)
- 資料模型集中在 `Models/ReportModels.cs`
- 商業邏輯放在 `Services/` 層，透過 DI 注入
- 頁面邏輯放在對應的 `.cshtml.cs` PageModel
- 不使用 npm/webpack，所有前端依賴走 CDN
- 動態圖示（如收藏星號）使用 inline SVG 搭配 Alpine `:class`，不使用 Lucide 動態渲染（避免 `createIcons()` 替換後失去響應性）
- `<script>` 區塊內的中文字串須透過 `@Html.Raw(JsonSerializer.Serialize(..., jsonOpts))` 注入，避免 Razor HTML 編碼產生亂碼
- JSON 序列化統一使用 `JavaScriptEncoder.UnsafeRelaxedJsonEscaping` 避免中文被轉義為 `\uXXXX`
- View (`.cshtml`) 僅負責呈現，不放 LINQ 商業邏輯；需計算的資料在 PageModel 預先處理後透過屬性傳遞
- POST handler 中的字串解析邏輯抽為 `private static` 方法，保持 handler 簡潔
- Service 中的長方法拆分為具名私有方法 (如 `BuildDeptUsagesForRegion`)，提升可讀性與可測試性
- 所有 Service 介面與 PageModel 加入 XML `<summary>` 註解
