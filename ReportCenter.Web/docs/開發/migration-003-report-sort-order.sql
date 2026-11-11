-- ============================================================
--  Migration 003 — 報表自訂排序 (ReportCatalog.SortOrder)
--  建立日期：2026-11-11
--
--  用途：管理者可指定報表在部門頁、部門使用矩陣、收藏頁的顯示順序。
--  前端排序規則：SortOrder ASC → ReportName ASC。
--  預設 999：未設定的報表排在後面，數字設小即可往前提。
--  相容性：SQL Server 2008 R2 (compatibility level 100)
-- ============================================================

USE [ReportCenter];
GO

IF COL_LENGTH('dbo.ReportCatalog', 'SortOrder') IS NULL
BEGIN
    ALTER TABLE dbo.ReportCatalog
        ADD SortOrder INT NOT NULL
            CONSTRAINT DF_ReportCatalog_SortOrder DEFAULT 999;
END
GO
