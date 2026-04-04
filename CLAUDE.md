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
├── Models/ReportModels.cs          # 資料模型 (Company, UserInfo, Department, Report, KPI, ChartData 等)
├── Services/
│   ├── IReportService.cs           # 資料服務介面 (切換 API 時實作此介面)
│   └── MockReportService.cs        # Mock 實作 (模擬資料)
├── Pages/
│   ├── Index.cshtml(.cs)           # 首頁儀表板 (KPI、圖表、篩選、快速存取)
│   ├── Department.cshtml(.cs)      # 部門報表列表 (搜尋、篩選、排序、卡片/表格切換)
│   ├── Report.cshtml(.cs)          # 報表明細 (圖表切換、篩選、收藏、匯出、分頁)
│   └── Shared/
│       ├── _Layout.cshtml          # 主版面配置 (TopNav + Sidebar + Content + 搜尋 Modal)
│       ├── _TopNav.cshtml          # 頂部導航列 (Logo、公司選單、搜尋、使用者資訊)
│       ├── _Sidebar.cshtml         # 側邊欄 (部門選單、收藏、最近瀏覽)
│       └── _KpiCard.cshtml         # KPI 卡片元件
├── wwwroot/
│   ├── images/logo.png             # 公司 Logo
│   ├── favicon.svg                 # 品牌 Favicon
│   ├── css/site.css                # 自訂樣式 (極少，主要用 Tailwind)
│   └── js/site.js                  # Alpine Store (搜尋、收藏、最近瀏覽)
└── docs/frontend-spec.md           # 前端技術規格文件
```

## 路由

| 路由 | 頁面 | 說明 |
|------|------|------|
| `/` | Index | 營運總覽儀表板 |
| `/Department?dept={id}` | Department | 部門報表列表 |
| `/Report?dept={id}&name={name}&page={n}` | Report | 報表明細頁 |

## 資料架構 (Service Layer)

```
IReportService (介面)
  └── MockReportService (目前使用)
  └── ApiReportService (未來 — 實作此類別並在 Program.cs 替換即可)
```

**切換真實 API 只需：**
1. 建立 `ApiReportService : IReportService`
2. 在 `Program.cs` 將 `MockReportService` 換成 `ApiReportService`
3. 頁面與前端完全不需修改

## Alpine.js Store (site.js)

| Store | 用途 | 持久化 |
|-------|------|--------|
| `$store.search` | 全站搜尋 Modal、⌘K 快捷鍵 | 否 |
| `$store.favorites` | 報表收藏管理 | localStorage |
| `$store.recent` | 最近瀏覽紀錄 (最多 20 筆) | localStorage |

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
