-- ============================================================
--  ReportCenter 資料庫 DDL — 完整結構定義
--  最後更新：2026-07-15
--
--  本檔案包含 ReportCenter 資料庫的所有資料表、索引、
--  條件約束定義。執行順序已考慮 FK 相依性。
--
--  ※ PKSYS 資料庫 (User_Dept / User_Profile) 為外部系統，
--    僅於末段以 Reference 方式列出欄位定義，不納入本 DDL。
-- ============================================================

USE [ReportCenter];
GO

-- ────────────────────────────────────────────────────────────
--  1. ReportCategory — 報表分類
-- ────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ReportCategory')
BEGIN
    CREATE TABLE ReportCategory (
        CategoryID   INT            IDENTITY(1,1) PRIMARY KEY,
        CategoryName NVARCHAR(100)  NOT NULL,
        SortOrder    INT            NOT NULL DEFAULT 0,
        IsActive     BIT            NOT NULL DEFAULT 1
    );
END
GO

-- ────────────────────────────────────────────────────────────
--  2. ReportCatalog — 報表目錄主表
-- ────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ReportCatalog')
BEGIN
    CREATE TABLE ReportCatalog (
        ReportID        INT            IDENTITY(1,1) PRIMARY KEY,
        ReportName      NVARCHAR(200)  NOT NULL,
        ReportTool      NVARCHAR(50)   NOT NULL DEFAULT 'Internal',   -- Internal / SmartQuery / SSRS
        ReportPath      NVARCHAR(500)  NOT NULL DEFAULT '',            -- 報表資料夾路徑
        ReportCode      NVARCHAR(500)  NOT NULL DEFAULT '',            -- URL path 或內部路由
        SourceName      NVARCHAR(200)  NOT NULL DEFAULT '',            -- SP / 函數名稱
        CreateDate      DATETIME       NOT NULL DEFAULT GETDATE(),
        ModifyDate      DATETIME       NULL,
        IsActive        BIT            NOT NULL DEFAULT 1,
        Remarks         NVARCHAR(MAX)  NOT NULL DEFAULT '',            -- 報表描述
        UseCompanyParam BIT            NOT NULL DEFAULT 0,             -- 是否帶公司別參數 (dbs)
        CategoryID      INT            NULL,

        CONSTRAINT FK_Catalog_Category FOREIGN KEY (CategoryID)
            REFERENCES ReportCategory(CategoryID) ON DELETE SET NULL
    );

    CREATE NONCLUSTERED INDEX IX_Catalog_Category ON ReportCatalog (CategoryID);
    CREATE NONCLUSTERED INDEX IX_Catalog_Tool     ON ReportCatalog (ReportTool);
    CREATE NONCLUSTERED INDEX IX_Catalog_Active   ON ReportCatalog (IsActive);
END
GO

-- ────────────────────────────────────────────────────────────
--  3. ReportDepartment — 報表部門指派 (多對多)
-- ────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ReportDepartment')
BEGIN
    CREATE TABLE ReportDepartment (
        ReportID  INT            NOT NULL,
        DeptID    INT            NOT NULL,
        DeptName  NVARCHAR(100)  NOT NULL DEFAULT '',

        CONSTRAINT PK_ReportDepartment PRIMARY KEY (ReportID, DeptID),
        CONSTRAINT FK_RptDept_Catalog  FOREIGN KEY (ReportID)
            REFERENCES ReportCatalog(ReportID) ON DELETE CASCADE
    );

    CREATE NONCLUSTERED INDEX IX_RptDept_Dept ON ReportDepartment (DeptID);
END
GO

-- ────────────────────────────────────────────────────────────
--  4. ReportDependency — 報表相依物件
-- ────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ReportDependency')
BEGIN
    CREATE TABLE ReportDependency (
        ReportID   INT            NOT NULL,
        DependsOn  NVARCHAR(200)  NOT NULL,       -- SP / View / Table 名稱

        CONSTRAINT PK_ReportDependency PRIMARY KEY (ReportID, DependsOn),
        CONSTRAINT FK_RptDep_Catalog   FOREIGN KEY (ReportID)
            REFERENCES ReportCatalog(ReportID) ON DELETE CASCADE
    );
END
GO

-- ────────────────────────────────────────────────────────────
--  5. UserReportPermission — 使用者報表權限
-- ────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'UserReportPermission')
BEGIN
    CREATE TABLE UserReportPermission (
        PermissionID  INT           IDENTITY(1,1) PRIMARY KEY,
        EmployeeId    NVARCHAR(20)  NOT NULL,     -- 對應 PKSYS.User_Profile.Account_Name
        ReportID      INT           NOT NULL,     -- 對應 ReportCatalog.ReportID
        GrantedBy     NVARCHAR(20)  NOT NULL,     -- 授權者 Account_Name
        GrantedDate   DATETIME      NOT NULL DEFAULT GETDATE(),

        CONSTRAINT UQ_UserReport       UNIQUE (EmployeeId, ReportID),
        CONSTRAINT FK_Permission_Report FOREIGN KEY (ReportID)
            REFERENCES ReportCatalog(ReportID) ON DELETE CASCADE
    );

    CREATE NONCLUSTERED INDEX IX_Perm_Employee ON UserReportPermission (EmployeeId);
    CREATE NONCLUSTERED INDEX IX_Perm_Report   ON UserReportPermission (ReportID);
END
GO

-- ────────────────────────────────────────────────────────────
--  6. UserFavorite — 使用者收藏
-- ────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'UserFavorite')
BEGIN
    CREATE TABLE UserFavorite (
        UserID    NVARCHAR(20)  NOT NULL,         -- 員工編號
        ReportID  INT           NOT NULL,

        CONSTRAINT PK_UserFavorite  PRIMARY KEY (UserID, ReportID),
        CONSTRAINT FK_Fav_Catalog   FOREIGN KEY (ReportID)
            REFERENCES ReportCatalog(ReportID) ON DELETE CASCADE
    );
END
GO

-- ────────────────────────────────────────────────────────────
--  7. UserPin — 使用者釘選 (快速存取)
-- ────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'UserPin')
BEGIN
    CREATE TABLE UserPin (
        UserID      NVARCHAR(20)  NOT NULL,       -- 員工編號
        ReportID    INT           NOT NULL,
        SortOrder   INT           NOT NULL DEFAULT 0,
        CreatedDate DATETIME      NOT NULL DEFAULT GETDATE(),

        CONSTRAINT PK_UserPin     PRIMARY KEY (UserID, ReportID),
        CONSTRAINT FK_Pin_Catalog FOREIGN KEY (ReportID)
            REFERENCES ReportCatalog(ReportID) ON DELETE CASCADE
    );
END
GO


-- ────────────────────────────────────────────────────────────
--  8. ReportUsageLog — 報表使用記錄
-- ────────────────────────────────────────────────────────────
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

    CREATE NONCLUSTERED INDEX IX_UsageLog_Report   ON ReportUsageLog (ReportID, ClickedAt);
    CREATE NONCLUSTERED INDEX IX_UsageLog_Employee ON ReportUsageLog (EmployeeId, ClickedAt);
END
GO


-- ============================================================
--  PKSYS 資料庫 — 外部系統參考 (僅供對照，請勿在此資料庫執行)
-- ============================================================
/*
  ── User_Dept (部門主檔) ──
  Area        NVARCHAR(10)   -- 區域 (TW / SH)
  DeptID      NVARCHAR(10)   -- 部門代碼
  DeptName    NVARCHAR(100)  -- 部門名稱
  Display     CHAR(1)        -- 是否顯示 ('Y'/'N')
  Area_Sort   INT            -- 區域排序
  Sort        INT            -- 部門排序

  ── User_Profile (人員主檔) ──
  Account_Name  NVARCHAR(20)   -- 員工編號 (登入帳號數字部分)
  Display_Name  NVARCHAR(50)   -- 姓名
  DeptID        NVARCHAR(10)   -- 所屬部門代碼 (FK → User_Dept.DeptID)
  Display       CHAR(1)        -- 是否在職 ('Y'/'N')
*/
