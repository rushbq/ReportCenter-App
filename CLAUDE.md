# ReportCenter-App

企業報表中心 (Enterprise Report Center) - 台灣寶工營運數據視覺化儀表板

ASP.NET Core 10 Razor Pages + Dapper；前端 Tailwind 3.4.17 (CDN) + Alpine.js + Chart.js + HTMX + Lucide，**不使用 npm/webpack，所有前端依賴以固定版本走 CDN**。

## 文件索引（用到時再讀）

本檔只放每次都要遵守的硬規則。架構、功能、資料庫、部署細節在 `docs/`：

| 要找的東西 | 文件 |
|-----------|------|
| 專案結構、分層與 DI、Pipeline、身分解析、快取、設定與機密載入順序、Serilog | `docs/01-系統架構.md` |
| 各頁功能與業務邏輯（埋點、目錄管理、使用分析、權限管理） | `docs/02-功能與業務邏輯.md` |
| 業務規則總表（**修改行為時必須同步更新**） | `docs/03-業務規則總表.md` |
| 資料表、SP、PKSYS、Migration | `docs/04-資料庫.md`；DDL 在 `ReportCenter.Web/docs/開發/db-ddl.sql` |
| 本機開發、User Secrets、IIS 部署與排查 | `docs/05-開發與部署.md` |
| 已知問題、已修正、刻意保留、已移除的功能 | `docs/06-已知問題與待辦.md` |
| 前端版面 / 元件 / RWD 規格、色彩 Token | `ReportCenter.Web/docs/frontend-spec.md`、`DESIGN.md` |

## 開發指令

```bash
cd ReportCenter.Web && dotnet run    # http://localhost:5276
cd ReportCenter.Web && dotnet build
```

Development 環境首頁戰情固定用 Mock Repository、身分由 `DevWindowsAuthMiddleware` 讀 `appsettings.Development.json` 的 `MockWindowsAuth` 模擬（`UserName` 留空即改走真正的登入頁，見 `docs/05` §1.5）；目前沒有自動化測試專案。

## 開發規範

- 語言：繁體中文 (UI 文字、註解、commit message)
- 三層式 Page → Service → Repository，皆透過 DI 注入介面；商業邏輯放 `Services/`，資料存取放 `Repositories/`
- View (`.cshtml`) 僅負責呈現，不放 LINQ 商業邏輯；需計算的資料在 PageModel 預先處理後透過屬性傳遞
- 資料模型集中在 `Models/ReportModels.cs`；元件化使用 Razor Partial View (`_*.cshtml`)
- POST handler 中的字串解析邏輯抽為 `private static` 方法；Service 中的長方法拆分為具名私有方法
- 所有 Service 介面與 PageModel 加入 XML `<summary>` 註解
- 樣式優先使用 Tailwind utility class，避免自訂 CSS；互動行為使用 Alpine.js
- 複雜 Alpine 邏輯抽成 `function xxxPage()` 放在 `@section Scripts` 內，避免 Razor `@` 衝突
- `<script>` 區塊內的中文字串須透過 `@Html.Raw(JsonSerializer.Serialize(..., jsonOpts))` 注入，避免 Razor HTML 編碼產生亂碼
- JSON 序列化統一使用 `JavaScriptEncoder.UnsafeRelaxedJsonEscaping` 避免中文被轉義為 `\uXXXX`
- 動態圖示（如收藏星號）、`x-for` 重建的列內圖示一律 inline SVG（搭配 Alpine `:class`）。`lucide.createIcons()` 不會對重建的列再跑，用 `data-lucide` 篩選後圖示會消失
- 管理頁 UI 改版時，同步更新 `/Admin/Help` 管理者使用手冊（截圖在 `wwwroot/images/help/`，用 `python tools/help-screenshots.py` 重拍，見 `docs/05` §1.7）

## 身分、權限與公司別

- 登入：DataHubApi JWT（RS256，與 PKPortal 共用）換本站 Cookie `rc_auth`，**不使用 Windows 驗證**；規格見 `docs/內部站台登入規格.md`。全站 FallbackPolicy 須登入，新增免登入頁面必須明確加 `[AllowAnonymous]`
- 未登入的 fetch / sendBeacon 回 401（不轉址），由 `site.js` 的 fetch 包裝函式重新整理頁面；新增 AJAX 不要自行處理轉址
- 未通過驗證時 `GetCurrentUser()` 回傳空使用者 (Id = "")：非管理員、無任何權限。**不可退回固定的 Mock 使用者**
- 所有 `Pages/Admin/*` 繼承 `AdminPageModel`（GET 顯示權限不足、POST 回 403 JSON）。新增管理頁務必繼承它
- 新增收藏 / 釘選前檢查報表存在、啟用且使用者有權限 (`SqlReportService.CanAccessReport`)；取消一律允許
- `AdminUsers` 不放 `appsettings.json`：開發放 `appsettings.Development.json`，正式放 `RC_CONFIG_PATH` 外部機密檔
- 目前公司別**只能**透過 `IReportService.GetCurrentCompanyId()` 取得，**不可直接讀 `companyId` cookie**（沒切換過公司的使用者 cookie 是空的）
- 外部報表連結**只能**透過 `IReportService.BuildReportUrl()` 由後端組好再交給前端，前端不可自行拼接網址

## 部門過濾規則

**所有 Service 層取得部門的方法，都必須依 `appsettings.json` → `DepartmentDisplay.Regions` 白名單過濾，僅回傳設定中存在的 DeptID。** 新增任何取得部門的方法時，務必加入相同的過濾邏輯（現有範例：`SqlReportService.GetDepartments()`、`CatalogService.BuildDeptUsagesForRegion()`）。
DeptID 必須為數字（`ReportDepartment.DeptID` 為 INT），`DepartmentDisplay` 以 `ValidateOnStart` 驗證。

## 報表使用記錄（埋點）

- 新增報表連結時務必加上 `data-report-id` 與 `data-report-source`（`department` / `pin` / `favorite` / `search`），否則不會記錄使用
- 動態建立後 `.click()` 的 `<a>`（如搜尋 Dialog 的 `go()`）不經委派監聽，須直接呼叫 `window.trackReportClick(reportId, source)`
- 30 秒防重複以後端 `UsageService` 為權威，前端節流只是省請求
- 汰除報表用 `IsActive=0` 停用；硬刪除會 FK CASCADE 清掉權限、收藏、釘選與使用記錄

## 刻意保留 / 不要重複提出

- `ReportCategory` **刻意不提供管理 UI**（僅 3 個、由 IT 以 SQL 維護；理由見 `ICatalogService` 註解與 `docs/06` P3）
- `Internal` 報表類型的 DB 欄位、`AdminStats.InternalCount`、badge 配色、`IReportService` 的 Mock 方法保留給未來重啟內部報表開發，不要當死碼刪除
- 不在 `web.config` 寫 Windows 驗證 / 匿名驗證設定（`<security><authentication>` 預設鎖定，寫入會 HTTP 500.19）；IIS 管理員設定為只開匿名驗證 + `FallbackPolicy` 兜底
- `DevWindowsAuthMiddleware`、`MockWindowsAuth`、`WindowsAuthDebug`、`/_diag/windows-auth` 是舊命名，刻意保留，不要為了改名而改動
- 首頁戰情 SP `usp_Rpt_Home_YtdTarget` 以 `INSERT…EXEC` **依欄位位置**擷取底層 YTD SP，底層 SP 欄位異動時必須同步修改（`docs/06` S1）

## SQL Server 相容性規範 ⚠️

**正式環境為 SQL Server 2008 R2 (compatibility level 100)，開發環境版本較新，
本機測試不會踩到相容性問題——寫 SQL 時必須主動確認，不能靠跑得起來判斷。**

- 所有 SQL / T-SQL / Dapper 查詢 / Stored Procedure / View / migration script 皆須相容 2008 R2
- 不確定是否相容時，先明講風險，再改用 2008 R2 可執行的寫法
- 涉及 SQL 變更時，回覆中需明確說明是否已依 `SQL Server 2008 R2 / compatibility level 100` 檢查

| 禁止 (2012+ 語法) | 改用 |
|------------------|------|
| `OFFSET ... FETCH` | `ROW_NUMBER() OVER (...)` 包成子查詢後篩 RowNum（範例：`SqlUsageRepository.GetLogs()`） |
| `IIF`, `CHOOSE` | `CASE WHEN` |
| `CONCAT` | `ISNULL(a, '') + ISNULL(b, '')` |
| `TRY_CAST`, `TRY_CONVERT` | 應用層先驗證 |
| `FORMAT`, `DATEFROMPARTS`, `EOMONTH` | `CONVERT` / `DATEADD` / `DATEDIFF` |
| `LEAD`, `LAG`, `THROW`, `SEQUENCE` | — |

## Exception Logging 規範

- 所有未預期例外都必須能留下可追查 log；不可只有 500 頁面而沒有對應例外紀錄
- 全域例外處理（`UnhandledExceptionLoggingMiddleware`）至少記錄 `TraceId`、`RequestPath`、`HttpMethod`、登入帳號、例外訊息、stack trace；`SqlException` 另記 `Number`、`Procedure`、`LineNumber`、`State`
- `catch` 不可只吞例外；需補 context 時 `LogError(...)` 後重新拋出，或轉成具明確意圖的錯誤回應
- 新增資料存取或關鍵流程時，先確認 500 錯誤在 Development 與 Production 都有可查的 logging 路徑
