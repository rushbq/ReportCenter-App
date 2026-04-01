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

- **營運總覽儀表板** — KPI 卡片、營收趨勢圖、部門比較圖
- **部門報表列表** — 卡片/表格雙檢視、分類篩選、搜尋
- **報表明細頁** — 成本趨勢圖、圓餅圖、物料數據表、分頁

系統以 **靜態假資料** (hard-coded mock data) 驅動，尚未串接後端 API。

---

## 2. 技術架構

| 層級 | 技術 | 說明 |
|------|------|------|
| **Server** | ASP.NET Core (.NET 10) | Razor Pages 架構 |
| **CSS** | Tailwind CSS (CDN) | 透過 `<script>` 標籤引入，含自訂 config |
| **JS 框架** | Alpine.js 3.x | 輕量互動（sidebar toggle、tab 切換等） |
| **圖表** | Chart.js 4 | 折線圖、長條圖、甜甜圈圖 |
| **圖標** | Lucide Icons | SVG icon library |
| **動態載入** | HTMX 2.0.4 | 已引入但尚未深度使用 |
| **字體** | Noto Sans TC | Google Fonts，支援繁體中文 |

---

## 3. CDN 套件與版本

| 套件 | 版本 | CDN URL |
|------|------|---------|
| Tailwind CSS | Latest | `https://cdn.tailwindcss.com` |
| Alpine.js | 3.x | `https://cdn.jsdelivr.net/npm/alpinejs@3.x.x/dist/cdn.min.js` |
| Chart.js | 4 | `https://cdn.jsdelivr.net/npm/chart.js@4` |
| Lucide Icons | Latest | `https://unpkg.com/lucide@latest` |
| HTMX | 2.0.4 | `https://unpkg.com/htmx.org@2.0.4` |
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
| `ok` | `#0d9668` | 正向趨勢（綠色） |
| `bad` | `#dc4a4a` | 負向趨勢（紅色） |
| `bdr` | `#e2e8ea` | 邊框色 |
| `bdr-light` | `#eef2f3` | 淺邊框色 |

### 4.2 Chart.js 色彩常數

```javascript
const C = {
  pri:     '#005758',
  acc:     '#00b4b6',
  priH:    '#006d6e',
  ter:     '#94aeb0',
  borderL: '#eef2f3'
};
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
│          │   │  or Report.cshtml      │      │
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
┌─ Header ────────────────────────────┐
│ 營運總覽              [篩選] [重新整理] │
│ 台灣寶工 ・ 最後更新 2026/03/20      │
├─ KPI Cards (grid-cols-2 lg:4) ──────┤
│ ┌──────┐ ┌──────┐ ┌──────┐ ┌──────┐ │
│ │$12.8M│ │$4.2M │ │67.2% │ │1,847 │ │
│ │+8.3% │ │-3.1% │ │+2.4% │ │+12.6%│ │
│ └──────┘ └──────┘ └──────┘ └──────┘ │
├─ Charts (grid-cols-1 lg:[1.6fr_1fr])┤
│ ┌─ 營收趨勢圖 ──┐ ┌─ 部門比較圖 ─┐ │
│ │ Line Chart     │ │ Bar Chart    │ │
│ │ h-[220px]      │ │ h-[220px]    │ │
│ └────────────────┘ └──────────────┘ │
├─ 快速存取 (grid-cols-2 lg:4) ───────┤
│ ┌──────┐ ┌──────┐ ┌──────┐ ┌──────┐ │
│ │報表卡│ │報表卡│ │報表卡│ │報表卡│ │
│ └──────┘ └──────┘ └──────┘ └──────┘ │
└─────────────────────────────────────┘
```

**KPI 卡片資料：**

| 標題 | 數值 | 趨勢 | 對比 |
|------|------|------|------|
| 本月營收 | $12.8M | +8.3% | vs 上月 |
| 採購成本 | $4.2M | -3.1% | vs 上月 |
| 毛利率 | 67.2% | +2.4% | — |
| 訂單數 | 1,847 | +12.6% | vs 上月 |

**快速存取卡片：**
- 左上角：部門標籤 (`bg-pri-light text-pri`)
- 右上角：星號收藏圖示
- 報表名稱 + hashtag 標籤
- Hover: `hover:shadow-md hover:border-pri`
- 連結: `/Report?dept={deptId}&name={encodedName}`

---

### 6.2 部門報表 — Department.cshtml

**路由：** `/Department?dept={deptId}&cat={category}`

**Alpine.js 狀態：**
```javascript
x-data="{ view: 'card', tab: 'all' }"
```

**區塊結構：**

```
┌─ Header ────────────────────────────┐
│ 首頁 / 部門報表 / 採購部             │
│ [icon] 採購部 報表  共 24 份報表      │
├─ Controls ──────────────────────────┤
│ [搜尋框 w-220px]    [卡片] [表格]    │
├─ Tabs ──────────────────────────────┤
│ 桌面: 全部 | 成本分析 | 供應商管理 |..│
│ 手機: [全部] [成本分析] [供應商管理]  │
├─ Card View (grid 1→2→3 cols) ──────┤
│ ┌──────┐ ┌──────┐ ┌──────┐         │
│ │報表卡│ │報表卡│ │報表卡│         │
│ └──────┘ └──────┘ └──────┘         │
├─ Table View (CSS Grid) ────────────┤
│ 報表名稱 │ 說明 │ 分類 │ 更新 │ 操作│
│ ────────┼─────┼─────┼─────┼─────│
│ 月度採購 │ ... │ 成本 │03/20│ 檢視│
└─────────────────────────────────────┘
```

**Tab 樣式：**

| 裝置 | 樣式 | 特徵 |
|------|------|------|
| 桌面 (`hidden md:flex`) | 底線式 Tab | `border-b-2 border-pri` active |
| 手機 (`flex md:hidden`) | Pill 膠囊按鈕 | `rounded-full` + 換行 `flex-wrap` |

**卡片檢視：**
- Grid: `grid-cols-1 sm:grid-cols-2 lg:grid-cols-3`
- 分類徽章 + 收藏星號
- 報表名稱 + 說明 + 更新日期
- 「檢視」連結含 eye icon
- 篩選: `x-show="tab==='all' || tab==='@r.Cat'"`

**表格檢視：**
- CSS Grid: `grid-cols-[2.2fr_3fr_1fr_0.8fr_80px]`
- Header: `bg-surface text-[11px] font-bold uppercase`
- 行間分隔: `border-t border-bdr-light`
- RWD: `overflow-x-auto` + `min-w-[700px]`

---

### 6.3 報表明細 — Report.cshtml

**路由：** `/Report?dept={deptId}&name={reportName}`

**區塊結構：**

```
┌─ Breadcrumb ────────────────────────┐
│ 首頁 / 採購部 / 月度採購成本分析      │
├─ Header ────────────────────────────┤
│ [←] 月度採購成本分析                 │
│ 採購部 ・ 成本分析 ・ 最後更新 ...    │
│ [收藏] [Excel] [PDF]                │
├─ Filter Bar (sticky top-14) ────────┤
│ [2026/03] [物料類別:全部] [+ 更多]   │
├─ Charts (grid 1→[1.6fr_1fr]) ──────┤
│ ┌─ 成本趨勢圖 ──┐ ┌─ 分類佔比 ───┐ │
│ │ Line h-[240px] │ │ Donut h-200  │ │
│ └────────────────┘ └──────────────┘ │
├─ KPI Cards (grid-cols-2 lg:4) ──────┤
│ ┌──────┐ ┌──────┐ ┌──────┐ ┌──────┐ │
│ │$4.2M │ │原料62│ │47 家 │ │-2.1% │ │
│ └──────┘ └──────┘ └──────┘ └──────┘ │
├─ Data Table ────────────────────────┤
│ 物料名稱│供應商│數量│單價│金額│變化  │
│ ────────┼─────┼───┼───┼───┼─────│
│ 碳鋼板  │台灣鋼│5200│$85│$442k│-2.3%│
│ ...                                 │
├─ Pagination ────────────────────────┤
│ 每頁 20 筆   ◀ 1 2 3 … 8 ▶         │
└─────────────────────────────────────┘
```

**篩選列：**
- Position: `sticky top-14 z-40`
- 樣式: `bg-white border border-bdr rounded-[10px] shadow-sm`
- Chip: `.chip`, `.chip-active`, `.chip-dashed`
- 重設連結: `text-pri font-medium`

**數據表格：**
- Grid: `grid-cols-[2fr_1.5fr_1fr_0.8fr_1fr_0.8fr]`
- 變化欄位色彩：
  - 下降（負值）: `text-ok` + `arrow-down-right`
  - 上升（正值）: `text-bad` + `arrow-up-right`
- RWD: `overflow-x-auto` + `min-w-[700px]`

**分頁：**
- Active 按鈕: `bg-pri text-white font-semibold`
- 一般按鈕: `text-txt-ter`
- 尺寸: `px-2.5 py-1 text-[12px] rounded-md`

---

## 7. 元件規格

### 7.1 _TopNav.cshtml — 頂部導航列

**Alpine.js 狀態：**
```javascript
x-data="{ mobileSearch: false }"
```

**高度：** `h-14` (56px) — sticky top-0 z-50

**背景：** `bg-pri` (主色)

**組成元素：**

| 元素 | 顯示條件 | 說明 |
|------|----------|------|
| 漢堡選單 | `lg:hidden` | 觸發 `sidebarOpen` |
| Logo icon | 常駐 | `bar-chart-3`，30x30px `rounded-lg bg-white/15` |
| Logo 文字 | `hidden sm:inline` | "報表中心" |
| 公司選擇器 | `hidden md:flex` | `building-2` icon + "台灣寶工" |
| 桌面搜尋框 | `hidden sm:flex` | max-w-400px，含 `⌘K` 快捷鍵提示 |
| 手機搜尋 icon | `sm:hidden` | 觸發 `mobileSearch` overlay |
| 使用者頭像 | 常駐 | 34x34px circle, "CY" |

**手機搜尋 Overlay：**
- 位置: `absolute top-14` (導航列下方)
- 寬度: 100%
- 內容: 搜尋輸入框 + 「取消」按鈕
- 關閉方式: 按「取消」或 `@@click.outside`
- Transition: enter `ease-out 150ms`, leave `ease-in 100ms`

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
   - 「我的收藏」(star icon, count: 8)
   - 「最近瀏覽」(clock icon, count: 5)

4. **Footer**
   - 狀態燈: `w-1.5 h-1.5 rounded-full bg-ok`
   - 文字: "報表總數: 107 份"

### 7.3 _KpiCard.cshtml — KPI 卡片

**ViewData 參數：**

| 參數 | 型別 | 範例 |
|------|------|------|
| `title` | string | "本月營收" |
| `value` | string | "$12.8M" |
| `trend` | string | "8.3" |
| `note` | string (optional) | "vs 上月" |

**容器樣式：** `bg-white border border-bdr rounded-xl px-5 py-[18px] shadow-sm min-w-[155px]`

**趨勢判斷：**
- `trend > 0` → 綠色 (`text-ok`) + `arrow-up-right` + `+` 前綴
- `trend ≤ 0` → 紅色 (`text-bad`) + `arrow-down-right`

---

## 8. Alpine.js 互動模式

### 8.1 Sidebar Toggle（手機版）

```
_Layout.cshtml: x-data="{ sidebarOpen: false }"
  └── _TopNav 漢堡按鈕: @@click="sidebarOpen = !sidebarOpen"
  └── Overlay: x-show="sidebarOpen" @@click="sidebarOpen = false"
  └── Sidebar 容器: :class 依 sidebarOpen 切換 translate-x
```

### 8.2 Sidebar Accordion

```
_Sidebar.cshtml: x-data="{ expanded: '@currentDept' }"
  └── 部門按鈕: @@click="expanded = (expanded === '@d.Id') ? '' : '@d.Id'"
  └── 子分類列表: x-show="expanded === '@d.Id'" x-cloak
  └── Chevron icon: 依 expanded 切換 down/right
```

### 8.3 檢視模式切換

```
Department.cshtml: x-data="{ view: 'card', tab: 'all' }"
  └── Grid 按鈕: @@click="view='card'"
  └── List 按鈕: @@click="view='table'"
  └── Card View: x-show="view==='card'"
  └── Table View: x-show="view==='table'" x-cloak
```

### 8.4 Tab 篩選

```
Department.cshtml:
  └── Tab 按鈕: @@click="tab='@sub'"
  └── 「全部」按鈕: @@click="tab='all'"
  └── 報表卡片: x-show="tab==='all' || tab==='@r.Cat'"
```

### 8.5 手機搜尋 Overlay

```
_TopNav.cshtml: x-data="{ mobileSearch: false }"
  └── 搜尋 icon: @@click="mobileSearch = !mobileSearch"
  └── Overlay: x-show="mobileSearch" @@click.outside="mobileSearch = false"
  └── 取消按鈕: @@click="mobileSearch = false"
```

---

## 9. Chart.js 圖表設定

### 9.1 營收趨勢圖（Index — `#revenueChart`）

| 屬性 | 值 |
|------|-----|
| 類型 | `line` |
| 高度 | `h-[220px]` (container) |
| responsive | `true` |
| maintainAspectRatio | `false` |

**資料集：**

| 資料集 | 顏色 | 線條樣式 | 填滿 |
|--------|------|----------|------|
| 本年 | `pri` | 實線 `2px` | ✅ 漸層填滿 |
| 去年 | `ter` | 虛線 `[4,4]` | ❌ |
| 預算 | `acc` | 點線 `[2,2]` | ❌ |

**X 軸：** 10月、11月、12月、1月、2月、3月
**Y 軸：** 數值 + `'M'` 後綴

### 9.2 部門比較圖（Index — `#deptChart`）

| 屬性 | 值 |
|------|-----|
| 類型 | `bar` |
| 高度 | `h-[220px]` |
| barPercentage | `0.6` |
| borderRadius | `4` |

**資料集：**

| 資料集 | 顏色 |
|--------|------|
| 實際 | `pri` |
| 目標 | `borderL` (淺灰) |

**標籤：** 採購部、業務部、財務部、人資部、資訊部

### 9.3 成本趨勢圖（Report — `#costChart`）

| 屬性 | 值 |
|------|-----|
| 類型 | `line` |
| 高度 | `h-[240px]` |

**資料集：**

| 資料集 | 顏色 | 線條樣式 | 填滿 | 點標記 |
|--------|------|----------|------|--------|
| 原料 | `pri` | 實線 | ✅ | ❌ |
| 包材 | `acc` | 實線 | ✅ | ❌ |
| 設備 | `priH` | 實線 | ❌ | ✅ `r:3` |
| 其他 | `ter` | 虛線 `[3,3]` | ❌ | ❌ |

### 9.4 分類佔比圖（Report — `#pieChart`）

| 屬性 | 值 |
|------|-----|
| 類型 | `doughnut` |
| 高度 | `h-[200px]` |
| cutout | `'60%'` |
| legend | 隱藏（自訂 HTML legend） |

**資料：**

| 分類 | 百分比 | 顏色 |
|------|--------|------|
| 原料 | 62% | `pri` |
| 包材 | 18% | `acc` |
| 設備 | 12% | `priH` |
| 其他 | 8% | `ter` |

**自訂 Legend：**
```html
<div class="flex gap-3 justify-center flex-wrap mt-3">
  <span class="flex items-center gap-1.5 text-[11px] text-txt-sec">
    <span class="w-2 h-2 rounded-full bg-pri inline-block"></span>原料 62%
  </span>
  <!-- 其餘分類... -->
</div>
```

### 9.5 共用設定

所有圖表共用：
```javascript
{
  responsive: true,
  maintainAspectRatio: false,
  plugins: { legend: { display: false } },
  scales: {
    x: { grid: { display: false }, ticks: { font: { size: 11 }, color: '#94aeb0' } },
    y: { grid: { color: '#eef2f3' }, ticks: { callback: v => v + 'M' } }
  }
}
```

---

## 10. RWD 響應式設計

### 10.1 斷點定義

| 斷點 | Tailwind 前綴 | 寬度 | 用途 |
|------|---------------|------|------|
| Base | (無) | < 640px | 手機 |
| sm | `sm:` | ≥ 640px | 平板直立 |
| md | `md:` | ≥ 768px | 平板橫向 |
| lg | `lg:` | ≥ 1024px | 桌面 |

### 10.2 響應式行為對照表

| 元素 | 手機 (Base) | 平板 (sm/md) | 桌面 (lg) |
|------|-------------|-------------|-----------|
| Sidebar | 隱藏，漢堡選單呼出 | 同左 | 常駐顯示 |
| TopNav Logo 文字 | 隱藏 | 顯示 (`sm:inline`) | 顯示 |
| 公司選擇器 | 隱藏 | 隱藏 | 顯示 (`md:flex`) |
| 桌面搜尋框 | 隱藏 | 顯示 (`sm:flex`) | 顯示 |
| 手機搜尋 icon | 顯示 (`sm:hidden`) | 隱藏 | 隱藏 |
| KPI 卡片 | 2 欄 | 2 欄 | 4 欄 (`lg:grid-cols-4`) |
| 圖表區 | 單欄堆疊 | 單欄 | 雙欄 (`lg:grid-cols-[1.6fr_1fr]`) |
| 報表卡片 | 1 欄 | 2 欄 (`sm:grid-cols-2`) | 3 欄 (`lg:grid-cols-3`) |
| 快速存取 | 2 欄 | 2 欄 | 4 欄 (`lg:grid-cols-4`) |
| 部門 Tab | Pill 膠囊 (`flex md:hidden`) | 底線式 (`hidden md:flex`) | 底線式 |
| 數據表格 | 水平捲動 (`overflow-x-auto`) | 同左 | 完整顯示 |
| 內容 padding | `p-4` | `md:p-6` | `md:p-6` |
| 容器最大寬 | 100% | 100% | `max-w-[1200px]` |

---

## 11. 資料模型

### 11.1 C# Model 定義

**Department**
```csharp
public class Department
{
    public string Id { get; set; }       // "procurement"
    public string Label { get; set; }    // "採購部"
    public string Icon { get; set; }     // Lucide icon name: "package"
    public int Count { get; set; }       // 報表數量: 24
    public List<string> Subs { get; set; } // 子分類: ["成本分析", "供應商管理", ...]
}
```

**Report**
```csharp
public class Report
{
    public string Name { get; set; }     // "月度採購成本分析"
    public string Desc { get; set; }     // "各類物料採購金額與趨勢分析"
    public string Cat { get; set; }      // "成本分析"
    public string Updated { get; set; }  // "03/20"
    public bool Fav { get; set; }        // 是否收藏
}
```

**QuickAccess**
```csharp
public class QuickAccess
{
    public string Dept { get; set; }     // "採購部"
    public string DeptId { get; set; }   // "procurement"
    public string Name { get; set; }     // "月度採購成本分析"
    public string Tag { get; set; }      // "成本"
}
```

**MaterialRow**
```csharp
public class MaterialRow
{
    public string Material { get; set; }   // "碳鋼板 SUS304"
    public string Supplier { get; set; }   // "台灣鋼鐵"
    public string Qty { get; set; }        // "5,200 KG"
    public string UnitPrice { get; set; }  // "$85"
    public string Amount { get; set; }     // "$442,000"
    public double Change { get; set; }     // -2.3 (百分比)
}
```

### 11.2 靜態資料總覽

**部門列表 (5)：**

| Id | 名稱 | Icon | 報表數 | 子分類 |
|----|------|------|--------|--------|
| procurement | 採購部 | package | 24 | 成本分析、供應商管理、訂單追蹤、績效報告 |
| sales | 業務部 | bar-chart-3 | 31 | 客戶分析、業績排名、區域統計、產品銷售 |
| finance | 財務部 | dollar-sign | 18 | 收支分析、預算管理、帳齡分析、資金流向 |
| hr | 人資部 | users | 12 | 出勤管理、薪資統計、人力配置、招募進度 |
| it | 資訊部 | monitor | 22 | 系統監控、資安報告、設備管理、服務台統計 |

**報表總數：** 107 份（5 部門合計）

**快速存取項目 (8)：**

| 部門 | 報表名稱 | 標籤 |
|------|----------|------|
| 採購部 | 月度採購成本分析 | 成本 |
| 業務部 | 客戶銷售排名 | 銷售 |
| 財務部 | 應收帳款帳齡表 | 財務 |
| 業務部 | 區域營收分佈 | 銷售 |
| 採購部 | 供應商績效評比 | 供應商 |
| 人資部 | 人員出勤統計 | 人資 |
| 資訊部 | 系統可用性報告 | IT |
| 財務部 | 預算執行率追蹤 | 財務 |

**物料資料 (7 rows)：**

| 物料 | 供應商 | 數量 | 單價 | 金額 | 變化 |
|------|--------|------|------|------|------|
| 碳鋼板 SUS304 | 台灣鋼鐵 | 5,200 KG | $85 | $442,000 | -2.3% |
| PE 包裝膜 | 永豐塑膠 | 12,000 M | $12 | $144,000 | +1.1% |
| 電子控制模組 | 矽達科技 | 800 PCS | $320 | $256,000 | -5.2% |
| 潤滑油 ISO VG68 | 中油化學 | 2,400 L | $45 | $108,000 | +0.8% |
| 銅線 Ø1.2mm | 嘉義銅業 | 3,600 KG | $210 | $756,000 | -1.7% |
| 不鏽鋼螺栓 M10 | 正達五金 | 20,000 PCS | $3.5 | $70,000 | +2.4% |
| 矽膠密封圈 | 聯合橡膠 | 8,500 PCS | $8 | $68,000 | -0.9% |

### 11.3 Page Model

**IndexModel** — 無參數，靜態頁面

**DepartmentModel**
```csharp
public void OnGet(string dept)
// Query: ?dept=procurement
// 查找部門 → 載入該部門報表清單
```

**ReportModel**
```csharp
public void OnGet(string dept, string name)
// Query: ?dept=procurement&name=月度採購成本分析
// 查找部門 → 設定報表名稱
```

---

## 12. 路由結構

| 路徑 | 頁面 | 參數 | 說明 |
|------|------|------|------|
| `/` | Index.cshtml | — | 營運總覽儀表板 |
| `/Department` | Department.cshtml | `dept`, `cat` | 部門報表列表 |
| `/Report` | Report.cshtml | `dept`, `name` | 報表明細頁 |
| `/Error` | Error.cshtml | — | 錯誤頁面 |

**範例 URL：**
- `http://localhost:5276/`
- `http://localhost:5276/Department?dept=procurement`
- `http://localhost:5276/Department?dept=finance&cat=帳齡分析`
- `http://localhost:5276/Report?dept=procurement&name=月度採購成本分析`

---

## 13. 檔案清單

```
ReportCenter.Web/
├── Program.cs                          # ASP.NET Core 啟動設定
├── ReportCenter.Web.csproj             # 專案檔
├── Models/
│   └── ReportModels.cs                 # 資料模型 + 靜態假資料
├── Pages/
│   ├── _ViewImports.cshtml             # 全域 using + TagHelpers
│   ├── _ViewStart.cshtml               # 預設 Layout 指定
│   ├── Index.cshtml                    # 首頁儀表板
│   ├── Index.cshtml.cs                 # 首頁 PageModel
│   ├── Department.cshtml               # 部門報表列表
│   ├── Department.cshtml.cs            # 部門 PageModel
│   ├── Report.cshtml                   # 報表明細
│   ├── Report.cshtml.cs                # 報表 PageModel
│   ├── Error.cshtml                    # 錯誤頁面
│   ├── Error.cshtml.cs                 # 錯誤 PageModel
│   └── Shared/
│       ├── _Layout.cshtml              # 主版面（CDN 引入 + Sidebar 框架）
│       ├── _TopNav.cshtml              # 頂部導航列（Partial）
│       ├── _Sidebar.cshtml             # 側邊欄（Partial）
│       └── _KpiCard.cshtml             # KPI 卡片（Partial）
├── wwwroot/
│   ├── css/site.css                    # 自訂 CSS（目前空白）
│   └── js/site.js                      # 自訂 JS（目前空白）
└── docs/
    └── frontend-spec.md                # 本文件
```

---

## Lucide Icons 使用清單

| Icon 名稱 | 使用位置 |
|-----------|----------|
| `menu` | TopNav 漢堡選單 |
| `bar-chart-3` | Logo, 業務部 icon |
| `building-2` | 公司選擇器 |
| `search` | 搜尋框 |
| `chevron-down` | 下拉、Sidebar 展開 |
| `chevron-right` | Sidebar 收合 |
| `chevron-left` | Report 返回按鈕 |
| `home` | Sidebar 首頁連結 |
| `package` | 採購部 icon |
| `dollar-sign` | 財務部 icon |
| `users` | 人資部 icon |
| `monitor` | 資訊部 icon |
| `star` | 收藏 |
| `clock` | 最近瀏覽 |
| `filter` | 篩選 chip |
| `refresh-cw` | 重新整理 chip |
| `eye` | 檢視連結 |
| `grid-3x3` | 卡片檢視按鈕 |
| `list` | 表格檢視按鈕 |
| `arrow-up-right` | 正向趨勢 |
| `arrow-down-right` | 負向趨勢 |
| `file-spreadsheet` | Excel 匯出 |
| `file-text` | PDF 匯出 |
