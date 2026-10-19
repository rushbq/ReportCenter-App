USE [ReportCenter]
GO

/****** Object:  StoredProcedure [dbo].[usp_Rpt_Home_YtdTarget]    Script Date: 2026-10-19 下午 03:01:37 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


/*
================================================================
功能名稱：YTD 年度目標戰情首頁（外銷/台灣/中國三區塊彙總）
負責人　：Clyde
建立日期：2026-07-23
相依模組：usp_Rpt_Sales_YtdTargetByGroup（銷售模式）
          usp_Rpt_Order_YtdTargetByGroup（接單模式）
----------------------------------------------------------------
功能說明：
    報表入口首頁「年度目標戰情」的後端 SP。依規格書
    「YTD 年度目標戰情首頁（後端欄位與公式對照）」實作。

    首頁彙總粒度：
        報表模式（銷售/接單）+ 報表年度 + 累計區間 + 業務區塊
        （外銷/台灣/中國）

    設計核心：不重造底層邏輯。以 INSERT...EXEC 直接消費既有兩支
    YTD SP「已算好的累計欄位」，首頁只做第二層 RuleCode → 業務區塊
    彙總與百分比計算。

    三層架構：
    ┌────────────────────────────────────────────────┐
    │ 報表呈現層：前端（3 張區塊卡片 + 達成率走勢圖）  │
    ├────────────────────────────────────────────────┤
    │ 報表組裝層：本 SP                              │
    │   - INSERT EXEC 擷取既有 YTD SP 寬表輸出        │
    │   - 依 RuleCode 對應業務區塊、選取對應幣別欄     │
    │   - 第二層彙總：年度目標 / 累計金額 / 去年累計   │
    │   - 計算達成率、YoY、逐月走勢率                 │
    ├────────────────────────────────────────────────┤
    │ 資料模組層：                                   │
    │   - usp_Rpt_Sales_YtdTargetByGroup             │
    │   - usp_Rpt_Order_YtdTargetByGroup             │
    └────────────────────────────────────────────────┘
----------------------------------------------------------------
參數說明：
    @Mode            - 報表模式（必填）
                       'S' = 銷售、'O' = 接單
    @CumulativeType  - 累計區間（必填）
                       'M' = 月結累計（截止月 M = 系統當月 - 1）
                       'R' = 即時累計（截止月 M = 系統當月）
    @ReportYear      - 報表年度；傳 NULL 則預設系統當年度
                       （首頁固定查當年度，不開放歷史/未來年度）
----------------------------------------------------------------
設計說明：
    - 公司別固定 'TW,SH'（規格書第 2.1 節），於內部寫死傳入底層 SP。
    - 截止月 M 由系統日期推算；月結累計 M = 當月 - 1、即時累計 M = 當月。
      → 邊界：若系統為 1 月且選月結累計，M = 0，代表上月尚未結算，
        三區塊累計金額與走勢皆回 0（符合「上月未結算」語意）。
      → 指定非當年度時：過去年度 M = 12（全年）、未來年度 M = 0。
    - 業務區塊與幣別（規格書第 6.1 / 7.2 節）：
        外銷 = SH_EXPORT-A + TW_EXPORT-A          → _USD
        台灣 = TW_DOMESTIC-A + TW_DOMESTIC-B      → _NTD
        中國 = SH_SALES-A + SH_SALES-B
             + SH_EC-A + SH_EC-B                  → _RMB
    - 年度目標 T(S)：取 ReportMonth = 12 的 CumulativeMonthTarget_C，
      跨區塊內 RuleCode 加總（累計欄僅取第 12 月一次，不重複累加）。
    - 本年累計 A(S)：取 ReportMonth = M 的累計金額
      （銷售 CumulativeSaleAmount_C / 接單 CumulativeOrderAmount_C）。
    - 去年累計 L(S)：取 ReportMonth = M 的去年累計金額。
    - 接單模式只取 CumulativeOrderAmount_*（已確認），
      不使用 CumulativeOrderEstimate_*（不含待確認）。
    - 各區塊只加總單一幣別欄，不同幣別不直接相加；跨區塊僅比百分比。
    - 三區塊即使無資料仍保留一列，金額補 0（由 #BlockList 驅動）。
----------------------------------------------------------------
輸出結果集：
    【結果集 1】三區塊 KPI（每區塊一列，固定 3 列）
        Mode, ReportYear, EndMonth,
        BlockCode, BlockName, BlockSort, Currency,
        AnnualTarget    - 年度目標 T(S)
        CumAmount       - 本年累計金額 A(S)
        LastYearAmount  - 去年同期累計 L(S)
        AchievementRate - 累計達成率  = A/T（T=0 回 0，保留小數）
        YoYRate         - 去年同期成長率 = (A-L)/ABS(L)（L=0 回 0）

    【結果集 2】三區塊 × 逐月（1~M）累計達成率走勢
        Mode, ReportYear,
        BlockCode, BlockName, BlockSort, Currency,
        ReportMonth,
        CumAmount       - 該月累計金額
        AnnualTarget    - 全年目標 T(S)（走勢分母固定用全年目標）
        TrendRate       - 該月累計達成率 = 當月累計 / T(S)（T=0 回 0）

    比率均保留小數（例 0.199507）；畫面顯示自行 ×100 四捨五入。
----------------------------------------------------------------
異動紀錄：
  2026-07-23, Clyde
    初版建立（依「YTD 年度目標戰情首頁」規格書）
  2026-10-19, Clyde
    截止月 M 依 @ReportYear 判斷：過去年度取 12、未來年度取 0
    （原本一律以系統當月推算，查詢過去年度會少算月份；首頁固定傳 NULL 不受影響）
================================================================
*/
CREATE PROCEDURE [dbo].[usp_Rpt_Home_YtdTarget]
    @Mode           CHAR(1),
    @CumulativeType CHAR(1),
    @ReportYear     INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- ============================================================
    -- 1. 參數驗證與預設
    -- ============================================================
    SET @Mode           = UPPER(LTRIM(RTRIM(@Mode)));
    SET @CumulativeType = UPPER(LTRIM(RTRIM(@CumulativeType)));

    IF @Mode IS NULL OR @Mode NOT IN ('S', 'O')
    BEGIN
        RAISERROR(N'報表模式 @Mode 僅接受 ''S''(銷售) 或 ''O''(接單)', 16, 1);
        RETURN -1;
    END

    IF @CumulativeType IS NULL OR @CumulativeType NOT IN ('M', 'R')
    BEGIN
        RAISERROR(N'累計區間 @CumulativeType 僅接受 ''M''(月結) 或 ''R''(即時)', 16, 1);
        RETURN -1;
    END

    -- 年度預設系統當年度（首頁固定查當年度）
    IF @ReportYear IS NULL
        SET @ReportYear = YEAR(GETDATE());

    -- 截止月 M：
    --   過去年度 → 12（全年已結束，月結 / 即時相同）
    --   未來年度 → 0（尚無實績）
    --   當年度   → 月結 = 當月 - 1、即時 = 當月
    DECLARE @EndMonth INT;
    SET @EndMonth = CASE WHEN @ReportYear < YEAR(GETDATE()) THEN 12
                         WHEN @ReportYear > YEAR(GETDATE()) THEN 0
                         WHEN @CumulativeType = 'M'          THEN MONTH(GETDATE()) - 1
                         ELSE MONTH(GETDATE())
                    END;

    -- 公司別固定清單（規格書第 2.1 節）
    DECLARE @CompanyCodeList VARCHAR(20) = 'TW,SH';

    -- ============================================================
    -- 2. 業務區塊定義（區塊清單 + RuleCode 對應）
    -- ============================================================
    CREATE TABLE #BlockList
    (
        BlockCode VARCHAR(10)  NOT NULL,
        BlockName NVARCHAR(20) NOT NULL,
        BlockSort INT          NOT NULL,
        Currency  CHAR(3)      NOT NULL
    );
    INSERT INTO #BlockList (BlockCode, BlockName, BlockSort, Currency)
    SELECT 'EXPORT', N'外銷', 1, 'USD'
    UNION ALL SELECT 'TW', N'台灣', 2, 'NTD'
    UNION ALL SELECT 'CN', N'中國', 3, 'RMB';

    CREATE TABLE #BlockMap
    (
        RuleCode  NVARCHAR(100) NOT NULL,
        BlockCode VARCHAR(10)   NOT NULL
    );
    INSERT INTO #BlockMap (RuleCode, BlockCode)
    SELECT 'SH_EXPORT-A',   'EXPORT'
    UNION ALL SELECT 'TW_EXPORT-A',   'EXPORT'
    UNION ALL SELECT 'TW_DOMESTIC-A', 'TW'
    UNION ALL SELECT 'TW_DOMESTIC-B', 'TW'
    UNION ALL SELECT 'SH_SALES-A',    'CN'
    UNION ALL SELECT 'SH_SALES-B',    'CN'
    UNION ALL SELECT 'SH_EC-A',       'CN'
    UNION ALL SELECT 'SH_EC-B',       'CN';

    -- ============================================================
    -- 3. 擷取既有 YTD SP 寬表輸出 → 正規化為 #Src
    --    #Src 僅保留首頁所需：RuleCode + 月份 + 三幣別累計欄
    --    （本年累計金額 / 累計目標 / 去年累計金額）
    -- ============================================================
    CREATE TABLE #Src
    (
        RuleCode      NVARCHAR(100) NOT NULL,
        ReportMonth   INT           NOT NULL,
        CumAmt_NTD    DECIMAL(38, 6) NULL,
        CumAmt_RMB    DECIMAL(38, 6) NULL,
        CumAmt_USD    DECIMAL(38, 6) NULL,
        CumTarget_NTD DECIMAL(38, 6) NULL,
        CumTarget_RMB DECIMAL(38, 6) NULL,
        CumTarget_USD DECIMAL(38, 6) NULL,
        LYCumAmt_NTD  DECIMAL(38, 6) NULL,
        LYCumAmt_RMB  DECIMAL(38, 6) NULL,
        LYCumAmt_USD  DECIMAL(38, 6) NULL
    );

    IF @Mode = 'O'
    BEGIN
        -- 接單模式：擷取 usp_Rpt_Order_YtdTargetByGroup 全欄（INSERT EXEC 需位置對齊）
        CREATE TABLE #OrderRaw
        (
            CompanyCode  NVARCHAR(10),
            RuleCode     NVARCHAR(100),
            DisplayName  NVARCHAR(200),
            ProductType  NVARCHAR(50),
            ReportYear   INT,
            ReportMonth  INT,
            MonthTarget_NTD DECIMAL(38, 6),
            MonthTarget_RMB DECIMAL(38, 6),
            MonthTarget_USD DECIMAL(38, 6),
            OrderAmount_NTD DECIMAL(38, 6),
            OrderAmount_RMB DECIMAL(38, 6),
            OrderAmount_USD DECIMAL(38, 6),
            PendingAmount_NTD DECIMAL(38, 6),
            PendingAmount_RMB DECIMAL(38, 6),
            PendingAmount_USD DECIMAL(38, 6),
            UnshippedAmount_NTD DECIMAL(38, 6),
            UnshippedAmount_RMB DECIMAL(38, 6),
            UnshippedAmount_USD DECIMAL(38, 6),
            LastYearOrderAmount_NTD DECIMAL(38, 6),
            LastYearOrderAmount_RMB DECIMAL(38, 6),
            LastYearOrderAmount_USD DECIMAL(38, 6),
            CumulativeOrderAmount_NTD DECIMAL(38, 6),
            CumulativeOrderAmount_RMB DECIMAL(38, 6),
            CumulativeOrderAmount_USD DECIMAL(38, 6),
            CumulativeOrderEstimate_NTD DECIMAL(38, 6),
            CumulativeOrderEstimate_RMB DECIMAL(38, 6),
            CumulativeOrderEstimate_USD DECIMAL(38, 6),
            CumulativeMonthTarget_NTD DECIMAL(38, 6),
            CumulativeMonthTarget_RMB DECIMAL(38, 6),
            CumulativeMonthTarget_USD DECIMAL(38, 6),
            LastYearCumulativeOrderAmount_NTD DECIMAL(38, 6),
            LastYearCumulativeOrderAmount_RMB DECIMAL(38, 6),
            LastYearCumulativeOrderAmount_USD DECIMAL(38, 6)
        );

        INSERT INTO #OrderRaw
        EXEC dbo.usp_Rpt_Order_YtdTargetByGroup
            @ReportYear      = @ReportYear,
            @CompanyCodeList = @CompanyCodeList;

        INSERT INTO #Src
        (
            RuleCode, ReportMonth,
            CumAmt_NTD, CumAmt_RMB, CumAmt_USD,
            CumTarget_NTD, CumTarget_RMB, CumTarget_USD,
            LYCumAmt_NTD, LYCumAmt_RMB, LYCumAmt_USD
        )
        SELECT
            RuleCode, ReportMonth,
            CumulativeOrderAmount_NTD, CumulativeOrderAmount_RMB, CumulativeOrderAmount_USD,
            CumulativeMonthTarget_NTD, CumulativeMonthTarget_RMB, CumulativeMonthTarget_USD,
            LastYearCumulativeOrderAmount_NTD, LastYearCumulativeOrderAmount_RMB, LastYearCumulativeOrderAmount_USD
        FROM #OrderRaw;
    END
    ELSE
    BEGIN
        -- 銷售模式：擷取 usp_Rpt_Sales_YtdTargetByGroup 全欄（INSERT EXEC 需位置對齊）
        CREATE TABLE #SalesRaw
        (
            CompanyCode    NVARCHAR(10),
            GroupCode      NVARCHAR(50),
            RuleCode       NVARCHAR(100),
            DisplayName    NVARCHAR(200),
            ProductType    NVARCHAR(50),
            GroupSortOrder INT,
            RuleSortOrder  INT,
            ReportYear     INT,
            ReportMonth    INT,
            MonthTarget_NTD DECIMAL(38, 6),
            MonthTarget_RMB DECIMAL(38, 6),
            MonthTarget_USD DECIMAL(38, 6),
            SaleAmount_NTD DECIMAL(38, 6),
            SaleAmount_RMB DECIMAL(38, 6),
            SaleAmount_USD DECIMAL(38, 6),
            PaperCost_NTD DECIMAL(38, 6),
            PaperCost_RMB DECIMAL(38, 6),
            PaperCost_USD DECIMAL(38, 6),
            CSHPaperCost_USD DECIMAL(38, 6),
            CSHPaperCost_NTD DECIMAL(38, 6),
            SaleGross_NTD DECIMAL(38, 6),
            SaleGross_RMB DECIMAL(38, 6),
            SaleGross_USD DECIMAL(38, 6),
            GrossRate_NTD DECIMAL(38, 6),
            GrossRate_RMB DECIMAL(38, 6),
            GrossRate_USD DECIMAL(38, 6),
            UnshippedAmount_NTD DECIMAL(38, 6),
            UnshippedAmount_RMB DECIMAL(38, 6),
            UnshippedAmount_USD DECIMAL(38, 6),
            LastYearSaleAmount_NTD DECIMAL(38, 6),
            LastYearSaleAmount_RMB DECIMAL(38, 6),
            LastYearSaleAmount_USD DECIMAL(38, 6),
            CumulativeSaleAmount_NTD DECIMAL(38, 6),
            CumulativeSaleAmount_RMB DECIMAL(38, 6),
            CumulativeSaleAmount_USD DECIMAL(38, 6),
            CumulativeMonthTarget_NTD DECIMAL(38, 6),
            CumulativeMonthTarget_RMB DECIMAL(38, 6),
            CumulativeMonthTarget_USD DECIMAL(38, 6),
            LastYearCumulativeSaleAmount_NTD DECIMAL(38, 6),
            LastYearCumulativeSaleAmount_RMB DECIMAL(38, 6),
            LastYearCumulativeSaleAmount_USD DECIMAL(38, 6)
        );

        INSERT INTO #SalesRaw
        EXEC dbo.usp_Rpt_Sales_YtdTargetByGroup
            @ReportYear      = @ReportYear,
            @CompanyCodeList = @CompanyCodeList;

        INSERT INTO #Src
        (
            RuleCode, ReportMonth,
            CumAmt_NTD, CumAmt_RMB, CumAmt_USD,
            CumTarget_NTD, CumTarget_RMB, CumTarget_USD,
            LYCumAmt_NTD, LYCumAmt_RMB, LYCumAmt_USD
        )
        SELECT
            RuleCode, ReportMonth,
            CumulativeSaleAmount_NTD, CumulativeSaleAmount_RMB, CumulativeSaleAmount_USD,
            CumulativeMonthTarget_NTD, CumulativeMonthTarget_RMB, CumulativeMonthTarget_USD,
            LastYearCumulativeSaleAmount_NTD, LastYearCumulativeSaleAmount_RMB, LastYearCumulativeSaleAmount_USD
        FROM #SalesRaw;
    END

    -- ============================================================
    -- 4. 依區塊選取對應幣別欄（RuleCode → 區塊 → 挑幣別）
    --    picked* 已是「單一幣別」的純量，後續彙總不再跨幣
    -- ============================================================
    SELECT
        bl.BlockCode,
        bl.BlockName,
        bl.BlockSort,
        bl.Currency,
        s.ReportMonth,
        CASE bl.Currency WHEN 'USD' THEN s.CumAmt_USD
                         WHEN 'NTD' THEN s.CumAmt_NTD
                         ELSE s.CumAmt_RMB END AS PickedAmt,
        CASE bl.Currency WHEN 'USD' THEN s.CumTarget_USD
                         WHEN 'NTD' THEN s.CumTarget_NTD
                         ELSE s.CumTarget_RMB END AS PickedTarget,
        CASE bl.Currency WHEN 'USD' THEN s.LYCumAmt_USD
                         WHEN 'NTD' THEN s.LYCumAmt_NTD
                         ELSE s.LYCumAmt_RMB END AS PickedLY
    INTO #SrcPicked
    FROM #Src s
    INNER JOIN #BlockMap bm ON bm.RuleCode = s.RuleCode
    INNER JOIN #BlockList bl ON bl.BlockCode = bm.BlockCode;

    -- ============================================================
    -- 5. 區塊 KPI 彙總（年度目標 / 本年累計 / 去年累計）
    --    - 年度目標 T(S)：ReportMonth = 12
    --    - 本年/去年累計：ReportMonth = M（M=0 時無列 → 補 0）
    --    三區塊由 #BlockList 驅動，無資料仍保留並補 0
    -- ============================================================
    SELECT
        bl.BlockCode,
        bl.BlockName,
        bl.BlockSort,
        bl.Currency,
        ISNULL(t.AnnualTarget, 0)   AS AnnualTarget,
        ISNULL(a.CumAmount, 0)      AS CumAmount,
        ISNULL(a.LastYearAmount, 0) AS LastYearAmount
    INTO #BlockKPI
    FROM #BlockList bl
    LEFT JOIN (
        SELECT BlockCode, SUM(PickedTarget) AS AnnualTarget
        FROM #SrcPicked
        WHERE ReportMonth = 12
        GROUP BY BlockCode
    ) t ON t.BlockCode = bl.BlockCode
    LEFT JOIN (
        SELECT
            BlockCode,
            SUM(PickedAmt) AS CumAmount,
            SUM(PickedLY)  AS LastYearAmount
        FROM #SrcPicked
        WHERE ReportMonth = @EndMonth
        GROUP BY BlockCode
    ) a ON a.BlockCode = bl.BlockCode;

    -- ============================================================
    -- 6. 【結果集 1】三區塊 KPI
    --    達成率 = A/T（T=0 回 0）；YoY = (A-L)/ABS(L)（L=0 回 0）
    -- ============================================================
    SELECT
        @Mode        AS Mode,
        @ReportYear  AS ReportYear,
        @EndMonth    AS EndMonth,
        k.BlockCode,
        k.BlockName,
        k.BlockSort,
        k.Currency,
        k.AnnualTarget,
        k.CumAmount,
        k.LastYearAmount,
        CASE WHEN k.AnnualTarget = 0 THEN CAST(0 AS DECIMAL(18, 6))
             ELSE CAST(k.CumAmount AS DECIMAL(38, 6)) / k.AnnualTarget
        END AS AchievementRate,
        CASE WHEN k.LastYearAmount = 0 THEN CAST(0 AS DECIMAL(18, 6))
             ELSE CAST(k.CumAmount - k.LastYearAmount AS DECIMAL(38, 6)) / ABS(k.LastYearAmount)
        END AS YoYRate
    FROM #BlockKPI k
    ORDER BY k.BlockSort;

    -- ============================================================
    -- 7. 【結果集 2】三區塊 × 逐月（1~M）累計達成率走勢
    --    走勢分母固定用全年目標 T(S)，非當月累計目標
    -- ============================================================
    SELECT number AS ReportMonth
    INTO #TrendMonths
    FROM master..spt_values
    WHERE type = 'P'
      AND number BETWEEN 1 AND @EndMonth;

    SELECT
        BlockCode,
        ReportMonth,
        SUM(PickedAmt) AS CumAmount
    INTO #TrendAmt
    FROM #SrcPicked
    WHERE ReportMonth BETWEEN 1 AND @EndMonth
    GROUP BY BlockCode, ReportMonth;

    SELECT
        @Mode       AS Mode,
        @ReportYear AS ReportYear,
        k.BlockCode,
        k.BlockName,
        k.BlockSort,
        k.Currency,
        mn.ReportMonth,
        ISNULL(tr.CumAmount, 0) AS CumAmount,
        k.AnnualTarget,
        CASE WHEN k.AnnualTarget = 0 THEN CAST(0 AS DECIMAL(18, 6))
             ELSE CAST(ISNULL(tr.CumAmount, 0) AS DECIMAL(38, 6)) / k.AnnualTarget
        END AS TrendRate
    FROM #BlockKPI k
    CROSS JOIN #TrendMonths mn
    LEFT JOIN #TrendAmt tr
        ON tr.BlockCode = k.BlockCode
       AND tr.ReportMonth = mn.ReportMonth
    ORDER BY k.BlockSort, mn.ReportMonth;

    -- ============================================================
    -- 8. 清理暫存表
    -- ============================================================
    DROP TABLE #TrendAmt;
    DROP TABLE #TrendMonths;
    DROP TABLE #BlockKPI;
    DROP TABLE #SrcPicked;
    DROP TABLE #Src;
    DROP TABLE #BlockMap;
    DROP TABLE #BlockList;
    IF OBJECT_ID('tempdb..#OrderRaw') IS NOT NULL DROP TABLE #OrderRaw;
    IF OBJECT_ID('tempdb..#SalesRaw') IS NOT NULL DROP TABLE #SalesRaw;
END
GO

