---
name: ReportCenter-App
description: 台灣寶工企業報表中心的精準營運工作台
colors:
  primary: "#005758"
  primary-hover: "#006d6e"
  primary-light: "#e8f4f4"
  accent: "#00b4b6"
  surface: "#f5f7f8"
  surface-hover: "#fafcfc"
  white: "#ffffff"
  star: "#f5a623"
  text: "#1a2e2f"
  text-secondary: "#4a6465"
  text-tertiary: "#5c7576"
  success: "#087a55"
  danger: "#b93838"
  border: "#e2e8ea"
  border-light: "#eef2f3"
  chart-tw: "#c27600"
  chart-cn: "#6366f1"
typography:
  display:
    fontFamily: "Noto Sans TC, -apple-system, BlinkMacSystemFont, sans-serif"
    fontSize: "40px"
    fontWeight: 800
    lineHeight: 1
    letterSpacing: "-0.025em"
  headline:
    fontFamily: "Noto Sans TC, -apple-system, BlinkMacSystemFont, sans-serif"
    fontSize: "26px"
    fontWeight: 700
    lineHeight: 1.25
  title:
    fontFamily: "Noto Sans TC, -apple-system, BlinkMacSystemFont, sans-serif"
    fontSize: "20px"
    fontWeight: 700
    lineHeight: 1.4
  body:
    fontFamily: "Noto Sans TC, -apple-system, BlinkMacSystemFont, sans-serif"
    fontSize: "13px"
    fontWeight: 400
    lineHeight: 1.5
  label:
    fontFamily: "Noto Sans TC, -apple-system, BlinkMacSystemFont, sans-serif"
    fontSize: "11px"
    fontWeight: 600
    lineHeight: 1.4
rounded:
  focus: "4px"
  sm: "6px"
  md: "8px"
  compact-card: "10px"
  lg: "12px"
  full: "9999px"
spacing:
  xs: "4px"
  sm: "8px"
  md: "12px"
  lg: "16px"
  xl: "20px"
  2xl: "24px"
components:
  button-primary:
    backgroundColor: "{colors.primary}"
    textColor: "{colors.white}"
    typography: "{typography.body}"
    rounded: "{rounded.sm}"
    padding: "6px 16px"
    height: "44px"
  button-primary-hover:
    backgroundColor: "{colors.primary-hover}"
    textColor: "{colors.white}"
    typography: "{typography.body}"
    rounded: "{rounded.sm}"
    padding: "6px 16px"
    height: "44px"
  button-secondary:
    backgroundColor: "{colors.white}"
    textColor: "{colors.primary}"
    typography: "{typography.body}"
    rounded: "{rounded.sm}"
    padding: "6px 16px"
    height: "44px"
  input:
    backgroundColor: "{colors.white}"
    textColor: "{colors.text}"
    typography: "{typography.body}"
    rounded: "{rounded.md}"
    padding: "8px 12px"
    height: "44px"
  card:
    backgroundColor: "{colors.white}"
    textColor: "{colors.text}"
    rounded: "{rounded.lg}"
    padding: "20px"
  chip:
    backgroundColor: "{colors.white}"
    textColor: "{colors.text-secondary}"
    typography: "{typography.label}"
    rounded: "{rounded.sm}"
    padding: "5px 12px"
  chip-active:
    backgroundColor: "{colors.primary-light}"
    textColor: "{colors.primary}"
    typography: "{typography.label}"
    rounded: "{rounded.sm}"
    padding: "5px 12px"
  nav-top:
    backgroundColor: "{colors.primary}"
    textColor: "{colors.white}"
    height: "56px"
    padding: "0 20px"
---

# Design System: ReportCenter-App

## Overview

**Creative North Star: "精準營運工作台"**

這套設計系統像一張整理完善的企業工作桌：資訊密度足以支援日常判讀與管理，所有控制項都放在預期位置，視線能快速從年度目標、趨勢與狀態移到下一個操作。寶工深青建立穩定識別，霧白介面與清楚分隔讓大量資料維持秩序。

整體氣質是專業、精準、高效率。設計服務於任務，不以大型標語、花俏動畫或展示型 Dashboard 的視覺噱頭搶走注意力，也拒絕老舊 ERP 的擁擠邊框與模糊層級。桌面版允許高資訊密度；行動版則重組導覽、控制列與表格，而不是縮小文字硬塞內容。

**Key Characteristics:**

- 克制的深青品牌識別，強調色只傳達狀態與互動。
- 緊湊但清楚的資訊層級，數字、標籤與操作容易掃讀。
- 熟悉且一致的企業工具元件，不重新發明標準操作。
- 鍵盤焦點、44px 觸控目標與 reduced motion 是系統基線。
- 內容區使用 16–24px 響應式內距，頁面依資料需求延展至 1200–1800px。

## Colors

色彩以深青建立可信賴的企業識別，近白冷調表面承載資料，語意色只在需要判斷狀態時出現。

### Primary

- **寶工深青** (`primary`, #005758)：頂部導覽、主要按鈕、目前選取與關鍵數值；它是主要品牌聲音。
- **深青互動色** (`primary-hover`, #006d6e)：只用於主要操作的 hover 或 active 回饋。
- **深青淡底** (`primary-light`, #e8f4f4)：目前選單、選取 Chip、輕量提示與品牌色背景。

### Secondary

- **行動青** (`accent`, #00b4b6)：鍵盤焦點環與需要立即辨識的互動訊號，不作大面積裝飾。
- **收藏琥珀** (`star`, #f5a623)：僅表示收藏狀態。
- **台灣數據琥珀** (`chart-tw`, #c27600) 與 **上海數據靛藍** (`chart-cn`, #6366f1)：圖表系列辨識；圖例與資料標籤必須同步提供文字。

### Tertiary

- **成功綠** (`success`, #087a55)：成功、正向趨勢與正常狀態。
- **風險紅** (`danger`, #b93838)：錯誤、負向趨勢及破壞性操作。

### Neutral

- **墨青文字** (`text`, #1a2e2f)：標題、正文與主要資料。
- **輔助墨青** (`text-secondary`, #4a6465)：次要說明與欄位內容。
- **中階墨青** (`text-tertiary`, #5c7576)：metadata、提示與非主要導覽文字。
- **霧白介面** (`surface`, #f5f7f8) 與 **霧白 Hover** (`surface-hover`, #fafcfc)：頁面背景、工具列與非選取 hover。
- **內容白** (`white`, #ffffff)：卡片、表格、輸入與浮動層。
- **結構線** (`border`, #e2e8ea) 與 **輕分隔線** (`border-light`, #eef2f3)：容器輪廓、表格分隔與低層級群組。

**The Signal Rarity Rule.** 行動青只用於焦點與即時互動訊號；主要操作和目前選取使用寶工深青。強調色的稀少正是辨識力來源。

**The Meaning Beyond Color Rule.** 成功、錯誤、趨勢與圖表系列必須同時使用文字、圖示、符號或圖例，禁止只靠顏色傳意。

## Typography

**Display Font:** Noto Sans TC（後備為 Apple system sans 與 BlinkMacSystemFont）
**Body Font:** Noto Sans TC（後備為 Apple system sans 與 BlinkMacSystemFont）

**Character:** 單一繁中文字族維持跨頁一致性；以字重、字級與 tabular numbers 建立層級，不用展示字體製造不必要的個性。

### Hierarchy

- **Display** (800, 40px, 1): 只用於首頁關鍵達成率或同級核心數字，不作一般頁面標題。
- **Headline** (700, 26px, 1.25): 首頁戰情與重要頁面標題；小螢幕降至 22px。
- **Title** (700, 20px, 1.4): 區段標題與頁面次標題。
- **Body** (400–600, 13px, 1.5): 主要操作、表格內容與說明；長篇說明限制在 65–75ch。
- **Label** (600, 11px, 1.4): metadata、徽章與欄標；10px 僅供極短且非核心的標籤。

**The Dense but Legible Rule.** 10–12px 只承載 metadata 與短標籤；主要操作與正文至少 13px，互動目標高度至少 44px。

**The One-Family Rule.** 按鈕、欄位、表格、導覽與資料數字一律使用 Noto Sans TC 字族；禁止在產品介面加入展示字體。

## Elevation

系統採「平面結構優先、浮動層才抬升」的混合策略。頁面與卡片主要靠白色表面、霧白背景及結構線分層；陰影用於輕量卡片層次、固定導覽、Dropdown、Tooltip、Dialog 與 Toast，不用來裝飾每一個容器。

### Shadow Vocabulary

- **卡片微層次** (`box-shadow: 0 1px 2px 0 rgb(0 0 0 / 0.05)`): 白色資料卡片在霧白背景上的最低層次。
- **固定導覽** (`box-shadow: 0 2px 8px rgba(0, 87, 88, 0.15)`): 讓深青頂部導覽與捲動內容分離。
- **浮動控制** (`box-shadow: 0 10px 15px -3px rgb(0 0 0 / 0.1), 0 4px 6px -4px rgb(0 0 0 / 0.1)`): Dropdown 與 Tooltip。
- **Dialog** (`box-shadow: 0 25px 50px -12px rgb(0 0 0 / 0.25)`): 只用於真正脫離頁面的搜尋或管理對話框。

**The Flat-by-Default Rule.** 靜態內容先用表面色與 1px 結構線分層；寬而柔的陰影只屬於浮動介面，禁止同時用厚重邊框與大陰影裝飾一般卡片。

## Components

元件整體感受是「克制、明確、可預期」。狀態變化通常為 150–200ms，使用 ease-out；`prefers-reduced-motion` 下縮短至近乎即時。

### Buttons

- **Shape:** 緊湊圓角（6px），常規高度至少 44px。
- **Primary:** 寶工深青底、白字，13px semibold，水平內距 16–20px。
- **Hover / Focus:** hover 轉深青互動色；focus-visible 使用 2px 行動青外框及 2px offset。
- **Secondary / Ghost:** 白底搭配結構線，或透明底搭配寶工深青文字；hover 只改邊框、文字或霧白背景。
- **Disabled / Loading:** 降低 opacity 並顯示 wait/not-allowed cursor；保留原標籤避免版面跳動。

### Chips

- **Style:** 6px 圓角、11–12px semibold、5px × 12px 內距；預設白底、結構線與輔助墨青文字。
- **State:** 選取時使用深青淡底、寶工深青文字及品牌色邊框；破折線只用於「更多篩選」等可新增條件。

### Cards / Containers

- **Corner Style:** 主要容器 12px；緊湊報表連結可用 10px。
- **Background:** 內容白置於霧白介面上。
- **Shadow Strategy:** 預設只使用卡片微層次；hover 可提高一級，但不可形成漂浮卡片牆。
- **Border:** 1px 結構線；需要更輕分組時使用輕分隔線。
- **Internal Padding:** 16–20px；大型頁面區段可用 24px。

### Inputs / Fields

- **Style:** 白底或霧白底、1px 結構線、8px 圓角，13px 墨青文字，最小高度 44px。
- **Focus:** 邊框轉寶工深青，並保留全域 2px 行動青 focus-visible 外框。
- **Error / Disabled:** 錯誤以風險紅文字、圖示和淡紅背景共同呈現；disabled 降低 opacity 並禁止互動。

### Navigation

- **Top navigation:** 56px 寶工深青固定列，白色品牌與操作；桌面顯示公司及搜尋，行動版保留 44px 圖示按鈕。
- **Sidebar:** 248px 內容白側欄；預設為墨青文字，hover 使用霧白背景，active 使用深青淡底與寶工深青 semibold。
- **Mobile:** 側欄轉為滑入式導覽並搭配遮罩；關閉後設為 inert，避免鍵盤焦點落入隱藏內容。

### Data Displays

- **KPI:** 標籤 12px，核心數值 26–40px bold/extrabold，趨勢同時呈現箭頭、文字與語意色。
- **Tables:** 表頭 11–12px semibold，內容 13px；寬資料表允許水平捲動，不縮小到難以閱讀。
- **Charts:** 圖例、Tooltip、資料標籤與空狀態必須可理解；所有圖表容器需有文字標題。

## Do's and Don'ts

### Do:

- **Do** 使用寶工深青作為主操作與目前選取，行動青只負責焦點與即時互動訊號。
- **Do** 使用 16–24px 內容間距、6–12px 元件圓角與至少 44px 的互動高度。
- **Do** 以表面、結構線、標題層級和留白整理高密度資料。
- **Do** 為 hover、focus、active、disabled、loading、error 與空狀態提供一致回饋。
- **Do** 讓行動版重組導覽與資料結構，並讓寬表格安全水平捲動。
- **Do** 讓所有顏色狀態同時具備文字、圖示、符號或圖例。

### Don't:

- **Don't** 採用以大型標語、過度動畫或裝飾效果為主的行銷官網風格。
- **Don't** 做只追求視覺衝擊、犧牲資料可讀性與操作效率的展示型 Dashboard。
- **Don't** 沿用老舊 ERP 常見的擁擠表格、模糊層級、密集邊框與難以辨識的操作狀態。
- **Don't** 以大量同質卡片、無意義漸層、玻璃擬態或過度圓角堆砌「現代感」。
- **Don't** 在一般卡片同時使用裝飾性 1px 邊框與寬廣柔陰影；選擇結構線或必要的浮動層次。
- **Don't** 使用彩色粗側邊線、漸層文字或對角線條紋作為裝飾。
- **Don't** 讓動畫阻擋內容顯示，或忽略 `prefers-reduced-motion`。
