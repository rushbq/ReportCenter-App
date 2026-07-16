-- ============================================================
--  Migration 002 — 報表使用記錄 (ReportUsageLog)
--  建立日期：2026-07-15
--
--  用途：記錄使用者每次點擊報表的行為，供使用分析
--  (熱門/冷門報表、廢除評估) 使用。
--  防重複寫入 (30 秒) 由應用層 UsageService 控制。
-- ============================================================

USE [ReportCenter];
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ReportUsageLog')
BEGIN
    CREATE TABLE ReportUsageLog (
        LogID       BIGINT        IDENTITY(1,1) PRIMARY KEY,
        ReportID    INT           NOT NULL,                     -- 對應 ReportCatalog.ReportID
        EmployeeId  NVARCHAR(20)  NOT NULL,                     -- 對應 PKSYS.User_Profile.Account_Name
        CompanyId   NVARCHAR(10)  NOT NULL DEFAULT '',          -- 點擊當下的公司別 (companyId Cookie)
        Source      NVARCHAR(20)  NOT NULL DEFAULT '',          -- 入口：department / pin / favorite / search
        ClickedAt   DATETIME2(0)  NOT NULL DEFAULT SYSDATETIME(),

        CONSTRAINT FK_UsageLog_Catalog FOREIGN KEY (ReportID)
            REFERENCES ReportCatalog(ReportID) ON DELETE CASCADE
    );

    -- 分析查詢：依報表統計期間點擊
    CREATE NONCLUSTERED INDEX IX_UsageLog_Report   ON ReportUsageLog (ReportID, ClickedAt);
    -- 防重複檢查 + 依使用者查明細
    CREATE NONCLUSTERED INDEX IX_UsageLog_Employee ON ReportUsageLog (EmployeeId, ClickedAt);
END
GO
