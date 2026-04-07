# ReportCenter-App

企業報表中心 (Enterprise Report Center) - 台灣寶工營運數據視覺化儀表板

## 技術架構

- **後端:** ASP.NET Core 10 Razor Pages (C#)
- **前端:** Tailwind CSS (CDN) + Alpine.js 3.x + Chart.js 4 + HTMX 2.0.4
- **圖示:** Lucide Icons (unpkg CDN)
- **字體:** Noto Sans TC (Google Fonts)
- **資料:** MockReportService（DI 注入），可快速切換為 API 實作

## 專案結構

```
ReportCenter.Web/
├── Models/ReportModels.cs          # 資料模型 (Company, UserInfo, Department, Report, KPI, ChartData, ReportCatalogItem 等)
├── Middleware/
│   └── DevWindowsAuthMiddleware.cs # 開發環境模擬 Windows AD 驗證中介層
├── Models/Settings/
│   ├── ReportBaseUrlSettings.cs    # 報表外部連結 BaseUrl 設定
│   └── MockWindowsAuthSettings.cs  # 開發環境模擬 AD 使用者設定
├── Services/
│   ├── IReportService.cs           # 資料服務介面 (切換 API 時實作此介面)
│   └── MockReportService.cs        # Mock 實作 (模擬資料，含 14 筆報表目錄 seed data)
├── Pages/
│   ├── Index.cshtml(.cs)           # 首頁儀表板 (KPI、圖表、篩選、快速存取)
│   ├── Department.cshtml(.cs)      # 部門報表列表 (搜尋、篩選、排序、卡片/表格切換)
│   ├── Report.cshtml(.cs)          # 報表明細 (圖表切換、篩選、收藏、匯出、分頁)
│   ├── Admin/
│   │   ├── Index.cshtml(.cs)       # 管理總覽 (KPI、部門矩陣、孤立報表、相依性分析)
│   │   └── Catalog.cshtml(.cs)     # 報表目錄管理 (CRUD、篩選、編輯 Modal)
│   └── Shared/
│       ├── _Layout.cshtml          # 主版面配置 (TopNav + Sidebar + Content + 搜尋 Modal)
│       ├── _TopNav.cshtml          # 頂部導航列 (Logo、公司選單、搜尋、使用者資訊)
│       ├── _Sidebar.cshtml         # 側邊欄 (部門選單、系統管理、收藏、最近瀏覽)
│       └── _KpiCard.cshtml         # KPI 卡片元件
├── wwwroot/
│   ├── images/logo.png             # 公司 Logo
│   ├── favicon.svg                 # 品牌 Favicon
│   ├── css/site.css                # 自訂樣式 (極少，主要用 Tailwind)
│   └── js/site.js                  # Alpine Store (搜尋、收藏、最近瀏覽)
├── web.config                         # IIS 部署設定 (Windows 驗證)
└── docs/frontend-spec.md           # 前端技術規格文件
```

## 路由

| 路由 | 頁面 | 說明 |
|------|------|------|
| `/` | Index | 營運總覽儀表板 |
| `/Department?dept={id}` | Department | 部門報表列表 |
| `/Report?dept={id}&name={name}&page={n}` | Report | 報表明細頁 |
| `/Admin` | Admin/Index | 報表管理總覽 (KPI、部門矩陣、孤立報表) |
| `/Admin/Catalog` | Admin/Catalog | 報表目錄管理 (CRUD) |

## 資料架構 (Service Layer)

```
IReportService (介面)               # 首頁/部門頁資料
  └── MockReportService (目前使用)
  └── ApiReportService (未來)

ICatalogService (介面)              # 報表目錄管理 BLL
  └── CatalogService                # 依賴 ICatalogRepository + DepartmentDisplaySettings
      ├── GetAdminStats()           → 統計 KPI + 部門矩陣 (分 TW/SH，依 appsettings 過濾)
      ├── BuildDeptUsagesForRegion() → 私有方法：建置單一區域部門矩陣
      └── CRUD / 分類 / 部門 / 資料夾 / BaseUrl

ICatalogRepository (介面)           # 資料存取層
  └── SqlCatalogRepository          # Dapper + SQL Server
```

**切換真實 API 只需：**
1. 建立 `ApiReportService : IReportService`
2. 在 `Program.cs` 將 `MockReportService` 換成 `ApiReportService`
3. 頁面與前端完全不需修改

## Alpine.js Store (site.js)

| Store | 用途 | 持久化 |
|-------|------|--------|
| `$store.search` | 全站搜尋 Modal、⌘K 快捷鍵 | 否 |
| `$store.favorites` | 報表收藏管理（toggle 時自動顯示 Toast） | localStorage |
| `$store.recent` | 最近瀏覽紀錄 (最多 20 筆) | localStorage |
| `$store.toast` | 全域 Toast 提示（`show(msg)` 呼叫，2.5 秒後自動消失） | 否 |

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

## 篩選功能

| 頁面 | 篩選器 | 實作方式 |
|------|--------|---------|
| Index | 期間（本月/上月/本季/本年） | 前端 Alpine 切換 Mock KPI 資料集 |
| Index | 區域（全部/北中南東） | 前端依區域比例調整數值與圖表 |
| Index | 部門（全部/各部門） | 前端篩選部門比較圖表 |
| Department | 日期（全部/本週/本月/本季） | 前端依報表更新日期相對篩選 |
| Department | 排序（最近更新/最早更新/名稱 A-Z） | 前端排序 |
| Report | 日期期間（2026/03 等） | 前端依 `period` 欄位篩選 |
| Report | 物料類別（原料/包材/設備/耗材） | 前端依 `category` 欄位篩選 |
| Report | 供應商 | 前端依 `supplier` 欄位篩選 |
| Admin/Catalog | 工具類型（全部/Internal/SmartQuery/SSRS） | 前端 Alpine 篩選 |
| Admin/Catalog | 狀態（全部/啟用/停用） | 前端 Alpine 篩選 |
| Admin/Catalog | 搜尋（名稱、來源、路徑） | 前端 Alpine 篩選 |

## 報表目錄管理

### 報表來源類型

| 類型 | 說明 | 色彩標記 |
|------|------|---------|
| Internal | 開發於本系統的報表 | emerald (綠) |
| SmartQuery | 外部 SmartQuery 報表，開新視窗連結 | amber (橙) |
| SSRS | SQL Server Reporting Services，開新視窗連結 | indigo (靛藍) |

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

- **Excel:** UI 按鈕已存在，目前僅顯示 Toast 提示，未來改用後端套件（如 ClosedXML）實作實際下載
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

## SQL Server 相容性規範

- 本專案所有 SQL / T-SQL / Dapper 查詢 / Stored Procedure / View / migration script，必須相容於 **SQL Server 2008 R2**，並以 **database compatibility level 100** 為最低標準。
- 不可假設資料庫可使用 SQL Server 2012 以上語法；若需求不確定是否相容，必須先明講風險，再改用 SQL Server 2008 R2 可執行的寫法。
- 涉及 SQL 變更時，回覆中需明確說明是否已依 `SQL Server 2008 R2 / compatibility level 100` 檢查。

### 禁止使用的較新語法 / 函式

- `OFFSET ... FETCH`
- `TRY_CAST`, `TRY_CONVERT`
- `CONCAT`
- `IIF`, `CHOOSE`
- `FORMAT`
- `DATEFROMPARTS`, `EOMONTH`
- `LEAD`, `LAG`
- `THROW`
- `SEQUENCE`

### 優先採用的相容寫法

- 分頁：使用 `ROW_NUMBER() OVER (...)`
- 條件判斷：使用 `CASE WHEN`
- 字串串接：使用 `ISNULL(a, '') + ISNULL(b, '')`
- 日期處理：使用 `CONVERT` / `DATEADD` / `DATEDIFF`
- 型別轉換驗證：優先在應用層先驗證，不依賴 `TRY_CAST` / `TRY_CONVERT`

## Exception Logging / Observability 規範

- 所有未預期例外都必須能留下可追查 log；不可只有 500 頁面而沒有對應例外紀錄。
- 全域例外處理需記錄至少以下欄位：`TraceId`、`RequestPath`、`HTTP Method`、登入帳號或識別資訊、例外訊息、stack trace。
- 若為 SQL 相關錯誤，需額外記錄：`SqlException.Number`、`Procedure`、`LineNumber`、`State`。
- `catch` 不可只吞例外；若需補 context，應 `LogError(...)` 後重新拋出，或轉成具明確意圖的錯誤回應。
- 新增資料存取或關鍵流程時，需先確認 500 錯誤在 Development 與 Production 都有可查的 logging 路徑。
