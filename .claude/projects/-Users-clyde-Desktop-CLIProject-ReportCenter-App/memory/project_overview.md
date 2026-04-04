---
name: Project Overview
description: ReportCenter 企業報表中心專案概況與目前開發狀態
type: project
---

ReportCenter-App 是台灣寶工的企業報表中心，提供營運數據視覺化儀表板。

**Why:** 各部門需要統一的報表查閱平台，涵蓋採購、業務、財務、人資、資訊等部門。

**How to apply:** 目前為 MVP 階段，使用靜態 Mock Data（ReportModels.cs），尚未串接後端 API。前端從 React 重構為 .NET Razor Pages 架構（commit c3ce659）。開發時注意所有前端依賴走 CDN，不使用 npm/webpack 建置流程。
