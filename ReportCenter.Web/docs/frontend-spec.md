# ReportCenter.Web 前端技術規格文件

> **專案名稱：** 台灣寶工 報表中心
> **框架：** ASP.NET Core Razor Pages + Tailwind CSS + Alpine.js
> **文件版本：** 1.0
> **最後更新：** 2026/04/01

---

## 目錄

1. [專案概覽](#1-專案概覽)
2. [技術架構](#2-技術架構)
3. [CDN 套件與版本](#3-cdn-套件與版本)
4. [設計系統](#4-設計系統)
5. [版面結構](#5-版面結構)
6. [頁面規格](#6-頁面規格)
7. [元件規格](#7-元件規格)
8. [Alpine.js 互動模式](#8-alpinejs-互動模式)
9. [Chart.js 圖表設定](#9-chartjs-圖表設定)
10. [RWD 響應式設計](#10-rwd-響應式設計)
11. [資料模型](#11-資料模型)
12. [路由結構](#12-路由結構)
13. [檔案清單](#13-檔案清單)

---

## 1. 專案概覽

ReportCenter.Web 是一套企業內部報表中心系統的前端展示層，提供：

- **年度目標戰情** — 銷售／接單、月結／即時累計、三區 KPI 與逐月達成率
- **部門報表列表** — 分類、搜尋、收藏及 SmartQuery / SSRS 外部連結
- **快速存取** — 釘選管理與全站搜尋
- **系統管理** — 報表目錄、使用分析與權限管理

系統以 Service / Repository 分層存取 SQL Server；Development 可依 DI 設定切換 Mock 實作。

---

## 2. 技術架構

| 層級 | 技術 | 說明 |
|------|------|------|
| **Server** | ASP.NET Core (.NET 10) | Razor Pages 架構 |
| **CSS** | Tailwind CSS 3.4.17 (CDN) | 透過 `<script>` 標籤引入，含自訂 config |
| **JS 框架** | Alpine.js 3.15.12 | 輕量互動（sidebar、Dialog、篩選等） |
| **圖表** | Chart.js 4.5.1 | 年度達成率與使用量折線圖 |
| **圖標** | Lucide Icons 1.25.0 | SVG icon library |
| **動態載入** | HTMX 2.0.4 | 已引入但尚未深度使用 |
| **字體** | Noto Sans TC | Google Fonts，支援繁體中文 |

---

## 3. CDN 套件與版本

| 套件 | 版本 | CDN URL |
|------|------|---------|
| Tailwind CSS | 3.4.17 | `https://cdn.tailwindcss.com/3.4.17` |
| Alpine.js | 3.15.12 | `https://cdn.jsdelivr.net/npm/alpinejs@3.15.12/dist/cdn.min.js` |
| Chart.js | 4.5.1 | `https://cdn.jsdelivr.net/npm/chart.js@4.5.1/dist/chart.umd.min.js` |
| Lucide Icons | 1.25.0 | `https://unpkg.com/lucide@1.25.0/dist/umd/lucide.min.js` |
| HTMX | 2.0.4 | `https://unpkg.com/htmx.org@2.0.4/dist/htmx.min.js` |
| Noto Sans TC | 400-800 | `https://fonts.googleapis.com/css2?family=Noto+Sans+TC:wght@400;500;600;700;800` |

---

## 4. 設計系統

### 4.1 色彩配置

Tailwind `extend.colors` 自訂色彩：

| Token | 色碼 | 用途 |
|-------|------|------|
| `pri` | `#005758` | 主色（深青綠） |
| `pri-hover` | `#006d6e` | 主色 hover 狀態 |
| `pri-light` | `#e8f4f4` | 主色淺底（active 背景） |
| `acc` | `#00b4b6` | 強調色（青色） |
| `surface` | `#f5f7f8` | 頁面底色 |
| `txt` | `#1a2e2f` | 主文字色（深墨綠） |
| `txt-sec` | `#5c7576` | 次要文字色 |
| `txt-ter` | `#94aeb0` | 第三層文字色 |
| `ok` | `#087a55` | 正向趨勢（綠色，白底對比 5.35:1） |
| `bad` | `#b93838` | 負向趨勢（紅色，白底對比 5.70:1） |
| `bdr` | `#e2e8ea` | 邊框色 |
| `bdr-light` | `#eef2f3` | 淺邊框色 |

### 4.2 Chart.js 色彩 Token

```css
:root {
  --chart-export: #005758;
  --chart-tw: #c27600;
  --chart-cn: #6366f1;
}
```

### 4.3 字體與字級

**字體家族：**
```css
font-family: 'Noto Sans TC', -apple-system, BlinkMacSystemFont, sans-serif;
-webkit-font-smoothing: antialiased;
```

**字級規範：**

| 用途 | Class | 像素 |
|------|-------|------|
| 標籤/徽章 | `text-[10px]` | 10px |
| 小標/metadata | `text-[11px]` | 11px |
| UI 標籤/chip | `text-[12px]` | 12px |
| 內文/列表項 | `text-[13px]` | 13px |
| 小標題 | `text-sm` / `text-[15px]` | 14-15px |
| 區塊標題 | `text-lg` / `text-[18px]` | 18px |
| 頁面標題 | `text-[20px]` - `text-[22px]` | 20-22px |
| KPI 數值 | `text-[26px]` | 26px |

**字重：**

| 用途 | 字重 |
|------|------|
| 內文 | 400 (regular) |
| 標籤 | 500 (medium) |
| Active 狀態/強調 | 600 (semibold) |
| 標題 | 700 (bold) |
| KPI 大數值 | 800 (extrabold) |

### 4.4 間距系統

| 層級 | Class | 像素 |
|------|-------|------|
| XS | `gap-1` / `gap-1.5` | 4px / 6px |
| SM | `gap-2` / `gap-2.5` | 8px / 10px |
| MD | `gap-3` / `gap-3.5` | 12px / 14px |
| LG | `gap-4` | 16px |

### 4.5 邊框與陰影

**邊框：**
- 預設：`border border-bdr`
- 淺色：`border-bdr-light`
- 白色透明：`border-white/10`、`border-white/[0.12]`
- 虛線：`border: 1.5px dashed`

**陰影：**
- 卡片：`shadow-sm`
- Hover：`hover:shadow-md`
- 導航列：`shadow-[0_2px_8px_rgba(0,87,88,0.15)]`

### 4.6 全域 CSS Reset

```css
* { margin: 0; padding: 0; box-sizing: border-box; }
[x-cloak] { display: none !important; }
```

### 4.7 Chip 元件樣式

```css
.chip {
  font-size: 12px;
  padding: 5px 12px;
  border-radius: 6px;
  border: 1px solid #e2e8ea;
  background: white;
  color: #5c7576;
  white-space: nowrap;
  user-select: none;
  display: inline-flex;
  align-items: center;
  gap: 4px;
  cursor: pointer;
}
.chip-active {
  border-color: #005758;
  background: #e8f4f4;
  color: #005758;
  font-weight: 600;
}
.chip-dashed {
  border: 1.5px dashed #e2e8ea;
}
```

### 4.8 共用 Component Classes

為避免重複的 Tailwind utility 組合散落各頁面，專案將常用組合抽取為 `@apply` component class，統一定義於 `Pages/Shared/_Layout.cshtml` 的 `<style type="text/tailwindcss">` 區塊（Tailwind Play CDN 支援 `@apply` 指令於 component layer）。

```html
<style type="text/tailwindcss">
  @layer components {
    /* 頁面外層容器：≥1536px (2xl) 自動延展 */
    .page-container      { @apply p-4 md:p-6 pb-10 max-w-[1200px] 2xl:max-w-[1600px]; }
    .page-container-wide { @apply p-4 md:p-6 pb-10 max-w-[1400px] 2xl:max-w-[1800px]; }
    /* 下拉選單／彈出清單項目 (顏色與 hover 由 :class 或額外 class 指定) */
    .menu-item           { @apply w-full text-left px-4 py-2 text-[13px] cursor-pointer border-0 bg-transparent; }
    /* 白色區塊卡片 */
    .card                { @apply bg-white border border-bdr rounded-xl p-5 shadow-sm; }
    /* 徽章基本形狀（10px chip，顏色自行補） */
    .badge               { @apply text-[10px] font-semibold px-2 py-0.5 rounded-full; }
  }
</style>
```

| Class | 用途 | 備註 |
|-------|------|------|
| `.page-container` | 頁面外層容器（預設寬度） | `≥2xl (1536px)` 延展至 1600px |
| `.page-container-wide` | 寬版外層容器 | 權限管理等寬表頁使用；`≥2xl` 延展至 1800px |
| `.menu-item` | 下拉選單／彈出清單項目 | 顏色由 `:class` 或額外 class 指定 |
| `.card` | 白色區塊卡片 | 圖表、列表等區塊容器 |
| `.badge` | 徽章形狀（10px chip） | 顏色自行補（例：`badge bg-pri-light text-pri`） |

**使用範例：**

```html
<!-- 頁面外層 -->
<div class="page-container" x-data="xxxPage()">...</div>

<!-- 白色卡片 + 內含徽章 -->
<div class="card">
  <span class="badge bg-pri-light text-pri">採購部</span>
  <span class="badge bg-bdr-light text-txt-ter">已停用</span>
  ...
</div>

<!-- 下拉選單項（配合 Alpine :class 動態配色） -->
<button :class="active ? 'bg-pri-light text-pri font-semibold' : 'text-txt hover:bg-surface'"
        class="menu-item">本月</button>
```

**其他站台複製提示：** 所有 component class 都透過 `@apply` 展開為 Tailwind utility，無純手寫 CSS 魔法數字。要調整（例如改變容器最大寬、卡片圓角），只需修改 `_Layout.cshtml` 中 `@apply` 後的 utility 即可，不需動到頁面。

---

## 5. 版面結構

### 5.1 整體佈局

```
┌──────────────────────────────────────────────┐
│  _TopNav (h-14, sticky top-0, z-50)         │
├──────────┬───────────────────────────────────┤
│          │                                   │
│ _Sidebar │         Main Content              │
│ (w-248px)│   (flex-1, overflow-y-auto)       │
│ (sticky) │   height: calc(100vh - 56px)      │
│          │                                   │
│          │   ┌─ Index.cshtml ─────────┐      │
│          │   │  or Department.cshtml  │      │
│          │   │  or Admin/*.cshtml     │      │
│          │   └────────────────────────┘      │
│          │                                   │
└──────────┴───────────────────────────────────┘
```

### 5.2 _Layout.cshtml 結構

- `<html lang="zh-TW">`
- Body: `flex flex-col min-h-screen`
- Sidebar + Main 容器: flex 排列
- 手機版 Sidebar: `fixed inset-0 z-30` + 黑色半透明覆蓋 (`bg-black/40`)
- 桌面版 Sidebar: `lg:relative lg:top-0 lg:translate-x-0`
- Transition: `duration-200 ease-out`
- Section 插槽: `@RenderSectionAsync("Styles")` 和 `@RenderSectionAsync("Scripts")`
- **共用 component class 定義區塊**: `<style type="text/tailwindcss">` 含 `.page-container`、`.menu-item`、`.card`、`.badge` 等（見 4.8）

### 5.3 Lucide Icons 初始化

```javascript
document.addEventListener('DOMContentLoaded', () => lucide.createIcons());
document.addEventListener('htmx:afterSwap', () => lucide.createIcons());
```

---

## 6. 頁面規格

### 6.1 首頁 — Index.cshtml

**路由：** `/`

**區塊結構：**

```
┌─ 年度目標戰情 ──────────────────────────┐
│ 2026年度目標戰情 [銷售|接單] [月結|即時] [i] │
├─ KPI Cards (grid-cols-1 md:3) ─────────┤
│ ┌─外銷 USD─┐ ┌─台灣 NTD─┐ ┌─中國 RMB─┐ │
│ │達成率/目標│ │達成率/目標│ │達成率/目標│ │
│ │累計/年增率│ │累計/年增率│ │累計/年增率│ │
│ └─────────┘ └─────────┘ └─────────┘ │
├─ 累計達成率走勢 ────────────────────────┤
│ Chart.js Line Chart + 無障礙摘要 + 文字資料表 │
├─ 快速存取 (grid-cols-2 lg:4) ──────────┤
│ 已釘選報表卡片                 [管理釘選] │
└────────────────────────────────────────┘
```

**年度戰情控制：**

| 控制 | 值 | 行為 |
|------|----|------|
| 模式 | `S` 銷售 / `O` 接單 | 呼叫 `/Api/HomeDashboard?handler=Data` |
| 區間 | `M` 月結累計 / `R` 即時累計 | 呼叫 `/Api/HomeDashboard?handler=Data` |
| 初始值 | 銷售 + 即時累計 | 由 `IndexModel.Dashboard` 伺服器端注入 |

切換模式或區間時保留上一份有效資料、顯示載入狀態，並以 `AbortController`
取消過期請求；失敗時顯示可重試訊息。

**KPI 卡片資料：**

| 區塊 | 幣別 | 顯示欄位 |
|------|------|----------|
| 外銷 (`EXPORT`) | USD | 累計達成率、年度目標、累計金額、去年同期比較 |
| 台灣 (`TW`) | NTD | 累計達成率、年度目標、累計金額、去年同期比較 |
| 中國 (`CN`) | RMB | 累計達成率、年度目標、累計金額、去年同期比較 |

不同區塊使用原幣顯示，金額不可跨區加總；跨區比較以達成率為主。

**快速存取卡片：**
- 顯示部門、報表名稱及另開視窗提示
- Hover: `hover:shadow-md hover:border-pri`
- 連結依 `ReportTool` 組成 SmartQuery / SSRS 外部網址

**管理釘選 Dialog：**
- 原生 `<dialog>`，支援 Esc、背景點擊關閉及焦點歸還
- 預設範圍為「已釘選」，可切換「全部報表」
- 支援名稱、部門、分類搜尋與篩選
- 透過 `/Api/Pin?handler=Toggle` 更新釘選狀態

---

### 6.2 部門報表 — Department.cshtml

**路由：** `/Department?dept={deptId}&cat={category}`

**Alpine.js 狀態：**
```javascript
x-data="deptPage()"
// tab、searchQuery、reports、favoriteIds
```

**區塊結構：**

```
┌─ Header ────────────────────────────┐
│ 首頁 / 部門報表 / 採購部             │
│ [icon] 採購部 報表  [搜尋部門報表]    │
├─ Tabs ──────────────────────────────┤
│ 桌面: 全部 | 成本分析 | 供應商管理 |..│
│ 手機: [全部] [成本分析] [供應商管理]  │
├─ Cards (grid 1→2→3 cols) ──────────┤
│ ┌──────┐ ┌──────┐ ┌──────┐         │
│ │報表卡│ │報表卡│ │報表卡│         │
│ └──────┘ └──────┘ └──────┘         │
└─────────────────────────────────────┘
```

**Tab 樣式：**

| 裝置 | 樣式 | 特徵 |
|------|------|------|
| 桌面 (`hidden md:flex`) | 底線式 Tab | `border-b-2 border-pri` active |
| 手機 (`flex md:hidden`) | Pill 膠囊按鈕 | `rounded-full` + 換行 `flex-wrap` |

**報表卡片：**
- Grid: `grid-cols-1 sm:grid-cols-2 lg:grid-cols-3`
- 分類徽章 + 收藏星號
- 報表名稱 + 說明 + 更新日期
- 依 `ReportTool` 連至 SmartQuery / SSRS，另開新視窗
- 搜尋與分類共用同一份 `filtered` 計算結果
- 固定依更新日期新到舊排序

---

## 7. 元件規格

### 7.1 _TopNav.cshtml — 頂部導航列

**Alpine.js 狀態：**
```javascript
x-data="{ companyOpen: false, selectedCompany: '台灣寶工' }"
```

**高度：** `h-14` (56px) — sticky top-0 z-50

**背景：** `bg-pri` (主色)

**組成元素：**

| 元素 | 顯示條件 | 說明 |
|------|----------|------|
| 漢堡選單 | `lg:hidden` | 觸發 `sidebarOpen` |
| Logo | 常駐 | `/images/logo.png`，連回首頁 |
| 公司選擇器 | `hidden md:block` | 寫入 `companyId` Cookie 後重新載入 |
| 桌面搜尋按鈕 | `hidden sm:flex` | 開啟全站搜尋 Dialog，含 `⌘K` 提示 |
| 手機搜尋按鈕 | `sm:hidden` | 開啟相同的全站搜尋 Dialog |
| 使用者資訊 | `hidden md:flex` | 顯示姓名與暱稱 |

### 7.2 _Sidebar.cshtml — 側邊欄

**Alpine.js 狀態：**
```javascript
x-data="{ expanded: '@currentDept' }"
```

**尺寸：** `w-[248px] min-w-[248px]`

**定位：** `sticky top-14`，高度 `calc(100vh - 56px)`

**區塊：**

1. **首頁連結**
   - Active 判斷: `currentPage == "/Index"`
   - Active 樣式: `font-semibold text-pri bg-pri-light`
   - 預設樣式: `text-txt hover:bg-[#f8fafa]`

2. **部門分類（accordion）**
   - 5 個部門，每個含子分類
   - 點擊展開/收合: `@@click="expanded = (expanded === '@d.Id') ? '' : '@d.Id'"`
   - 展開時顯示 `chevron-down`，收合時 `chevron-right`
   - 子分類連結: `/Department?dept=@d.Id&cat=@sub`

3. **快速存取**
   - 「我的收藏」(star icon，數量由目前使用者收藏資料計算)

4. **Footer**
   - 狀態燈: `w-1.5 h-1.5 rounded-full bg-ok`
   - 文字: `報表總數: {目前公司可見報表數} 份`

### 7.3 _KpiCard.cshtml — KPI 卡片

此 Partial 目前保留供管理頁 KPI 使用；首頁年度戰情採用專用三區卡片，不透過此 Partial 呈現。

---

## 8. Alpine.js 互動模式

### 8.1 Sidebar Toggle（手機版）

```
_Layout.cshtml: x-data="appShell()"
  └── _TopNav 漢堡按鈕: toggleSidebar($el)
  └── Overlay: x-show="sidebarOpen" @@click="closeSidebar()"
  └── Sidebar 關閉時於手機套用 inert / aria-hidden
  └── 開啟後移入焦點，關閉後將焦點歸還觸發按鈕
```

### 8.2 Sidebar Accordion

```
_Sidebar.cshtml: x-data="{ expanded: '@currentDept' }"
  └── 部門按鈕: @@click="expanded = (expanded === '@d.Id') ? '' : '@d.Id'"
  └── 子分類列表: x-show="expanded === '@d.Id'" x-cloak
  └── Chevron icon: 依 expanded 切換 down/right
```

### 8.3 部門搜尋與分類

```
Department.cshtml: x-data="deptPage()"
  └── 搜尋框: x-model="searchQuery"
  └── 分類按鈕: @@click="tab='@sub'"
  └── filtered: 先搜尋，再套用分類，最後依 updated 倒序
  └── tabCount(): 計算搜尋條件下各分類數量
```

### 8.4 收藏切換

```
Department.cshtml: toggleFavorite(reportId)
  └── POST /Api/Favorite?handler=Toggle
  └── 成功後更新 favoriteIds 並顯示 Toast
  └── 支援滑鼠點擊與 Enter / Space 鍵盤操作
```

### 8.5 全站搜尋 Dialog

```
_Layout.cshtml: <dialog id="globalSearchDialog">
  └── TopNav 搜尋按鈕 / Ctrl+K: $store.search.openDialog($el)
  └── Esc / 背景點擊 / 關閉按鈕: $store.search.close()
  └── 開啟後焦點移入搜尋框，關閉後歸還觸發元素
  └── 原生 showModal() 使背景內容不可互動
```

---

## 9. Chart.js 圖表設定

### 9.1 累計達成率走勢（Index — `#ytdTrendChart`）

| 屬性 | 值 |
|------|-----|
| 類型 | `line` |
| 高度 | `h-[260px]` |
| responsive | `true` |
| maintainAspectRatio | `false` |
| Y 軸 | 百分比，從 `0` 起始 |
| Tooltip | 一位小數百分比 |

**資料集：**

| 區塊 | 色彩 Token | 線條 | 點標記 |
|------|------------|------|--------|
| 外銷 | `--chart-export` | 實線 | circle |
| 台灣 | `--chart-tw` | 虛線 `[7,4]` | rectRot |
| 中國 | `--chart-cn` | 點線 `[2,3]` | triangle |

圖表外另提供：
- `role="img"` 與螢幕閱讀器摘要
- 可展開的完整文字資料表
- 無資料狀態
- `prefers-reduced-motion` 時停用 Chart.js 動畫

### 9.2 使用量趨勢（Admin/Usage — `#usageTrendChart`）

| 屬性 | 值 |
|------|-----|
| 類型 | `line` |
| 資料集 | 每日報表點擊數 |
| 線色 | `#005758` |
| 填色 | `rgba(0, 180, 182, 0.08)` |
| Y 軸 | `beginAtZero: true`、整數刻度 |
| X 軸 | 最多顯示 10 個刻度 |

---

## 10. RWD 響應式設計

### 10.1 斷點定義

| 斷點 | Tailwind 前綴 | 寬度 | 用途 |
|------|---------------|------|------|
| Base | (無) | < 640px | 手機 |
| sm | `sm:` | ≥ 640px | 平板直立 |
| md | `md:` | ≥ 768px | 平板橫向 |
| lg | `lg:` | ≥ 1024px | 桌面 |
| 2xl | `2xl:` | ≥ 1536px | 大螢幕（頁面容器延展至 1600/1800px） |

### 10.2 響應式行為對照表

| 元素 | 手機 (Base) | 平板 (sm/md) | 桌面 (lg) |
|------|-------------|-------------|-----------|
| Sidebar | 隱藏，漢堡選單呼出 | 同左 | 常駐顯示 |
| TopNav Logo 文字 | 隱藏 | 顯示 (`sm:inline`) | 顯示 |
| 公司選擇器 | 隱藏 | 隱藏 | 顯示 (`md:flex`) |
| 桌面搜尋框 | 隱藏 | 顯示 (`sm:flex`) | 顯示 |
| 手機搜尋按鈕 | 顯示 (`sm:hidden`) | 隱藏 | 隱藏 |
| 年度戰情卡片 | 1 欄 | 3 欄 (`md:grid-cols-3`) | 3 欄 |
| 年度走勢圖 | 滿寬 | 滿寬 | 滿寬 |
| 報表卡片 | 1 欄 | 2 欄 (`sm:grid-cols-2`) | 3 欄 (`lg:grid-cols-3`) |
| 快速存取 | 2 欄 | 2 欄 | 4 欄 (`lg:grid-cols-4`) |
| 部門 Tab | Pill 膠囊 (`flex md:hidden`) | 底線式 (`hidden md:flex`) | 底線式 |
| 走勢文字資料表 | 水平捲動 (`overflow-x-auto`) | 同左 | 完整顯示 |
| 內容 padding | `p-4` | `md:p-6` | `md:p-6` |
| 容器最大寬 | 100% | 100% | `max-w-[1200px]`（`≥2xl` 延展至 `1600px`；寬版頁 1400/1800px） |

---

## 11. 資料模型

### 11.1 C# Model 定義

| Model | 用途 | 主要欄位 |
|-------|------|----------|
| `Department` | 部門導覽與分類 | `Id`, `Label`, `Icon`, `Count`, `Subs` |
| `Report` | 報表卡片與外部連結 | `ReportID`, `Name`, `Desc`, `Cat`, `Updated`, `ReportTool`, `ReportCode` |
| `HomeYtdDashboard` | 首頁年度戰情 | `Mode`, `CumulativeType`, `ReportYear`, `EndMonth`, `Blocks`, `Trend` |
| `YtdBlockKpi` | 外銷／台灣／中國 KPI | 年度目標、累計金額、達成率、去年同期成長率 |
| `YtdTrendPoint` | 逐月達成率走勢 | 區塊、月份、累計金額、年度目標、達成率 |
| `ReportCatalogItem` | 管理端報表目錄 | 報表來源、工具類型、部門指派與相依性 |

### 11.2 資料來源

| 功能 | Development | Staging / Production |
|------|-------------|----------------------|
| 部門、報表、收藏、釘選 | `SqlReportService` | `SqlReportService` |
| 首頁年度目標戰情 | `MockHomeDashboardRepository` | `SqlHomeDashboardRepository` |
| 報表目錄、權限、使用記錄 | SQL Repository | SQL Repository |

首頁戰情的 Repository 由 `Program.cs` 依環境註冊，正式環境不回傳 Mock 資料。

### 11.3 Page Model

**IndexModel**
- 透過 `IHomeDashboardService` 載入「銷售 + 即時累計」初始戰情
- 透過 `IReportService` 組裝可釘選清單、已釘選 ID 與外部報表網址
- 頁面切換戰情條件時改呼叫 `/Api/HomeDashboard?handler=Data`

**DepartmentModel**
```csharp
public IActionResult OnGet(string dept)
// Query: ?dept=100
// 驗證目前公司可用部門 → 載入報表與收藏狀態
```

---

## 12. 路由結構

| 路徑 | 頁面 | 參數 | 說明 |
|------|------|------|------|
| `/` | Index.cshtml | — | 年度目標戰情與快速存取 |
| `/Department` | Department.cshtml | `dept` | 部門報表列表 |
| `/Admin` | Admin/Index.cshtml | — | 管理總覽 |
| `/Admin/Catalog` | Admin/Catalog.cshtml | — | 報表目錄管理 |
| `/Admin/Usage` | Admin/Usage.cshtml | — | 使用分析 |
| `/Admin/Permission` | Admin/Permission.cshtml | — | 權限管理 |
| `/Api/HomeDashboard?handler=Data` | Api/HomeDashboard.cshtml | `mode`, `cumType` | 首頁戰情 JSON |
| `/Api/Pin?handler=Toggle` | Api/Pin.cshtml | `reportId` | 切換釘選 |
| `/Error` | Error.cshtml | — | 錯誤頁面 |

**範例 URL：**
- `http://localhost:5276/`
- `http://localhost:5276/Department?dept=100`
- `http://localhost:5276/Admin/Usage`
- `http://localhost:5276/Api/HomeDashboard?handler=Data&mode=S&cumType=R`

---

## 13. 檔案清單

```
ReportCenter.Web/
├── Program.cs                          # ASP.NET Core 啟動設定
├── ReportCenter.Web.csproj             # 專案檔
├── Models/
│   └── ReportModels.cs                 # 報表、目錄與首頁戰情模型
├── Services/
│   ├── IReportService.cs               # 報表查詢介面
│   ├── SqlReportService.cs             # SQL + 權限過濾
│   ├── IHomeDashboardService.cs        # 首頁戰情介面
│   └── HomeDashboardService.cs         # 首頁戰情 BLL
├── Pages/
│   ├── _ViewImports.cshtml             # 全域 using + TagHelpers
│   ├── _ViewStart.cshtml               # 預設 Layout 指定
│   ├── Index.cshtml                    # 年度目標戰情與快速存取
│   ├── Index.cshtml.cs                 # 首頁 PageModel
│   ├── Department.cshtml               # 部門報表列表
│   ├── Department.cshtml.cs            # 部門 PageModel
│   ├── Admin/                           # 目錄、使用分析、權限管理
│   ├── Api/                             # 首頁戰情、收藏、釘選、使用埋點
│   ├── Error.cshtml                    # 錯誤頁面
│   ├── Error.cshtml.cs                 # 錯誤 PageModel
│   └── Shared/
│       ├── _Layout.cshtml              # 主版面、CDN、Sidebar、搜尋 Dialog
│       ├── _TopNav.cshtml              # 頂部導航列（Partial）
│       ├── _Sidebar.cshtml             # 側邊欄（Partial）
│       └── _KpiCard.cshtml             # KPI 卡片（Partial）
├── wwwroot/
│   ├── css/site.css                    # 自訂 CSS
│   └── js/site.js                      # Alpine Store、版面互動與使用埋點
└── docs/
    └── frontend-spec.md                # 本文件
```

---

## Lucide Icons 主要使用清單

| Icon 名稱 | 使用位置 |
|-----------|----------|
| `menu` | TopNav 漢堡選單 |
| `building-2` | 公司選擇器 |
| `search` / `x` | 搜尋與清除／關閉操作 |
| `chevron-down` | 下拉、Sidebar 展開 |
| `chevron-right` | Sidebar 收合 |
| `home` | Sidebar 首頁連結 |
| `star` | 收藏 |
| `settings` / `bar-chart-3` / `shield-check` | 系統管理導覽 |
| `info` / `trending-up` / `line-chart` | 首頁年度戰情 |
| 部門設定的動態 icon | Department 與 Sidebar 部門項目 |
