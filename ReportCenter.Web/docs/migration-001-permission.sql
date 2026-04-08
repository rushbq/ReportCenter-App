-- ============================================================
--  權限管理資料表 — ReportCenter 資料庫
--  建立日期：2026-04-08
-- ============================================================

-- 使用者報表權限 (一人對一報表一筆)
CREATE TABLE UserReportPermission (
    PermissionID  INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId    NVARCHAR(20) NOT NULL,   -- 對應 User_Profile.Account_Name
    ReportID      INT          NOT NULL,   -- 對應 ReportCatalog.ReportID
    GrantedBy     NVARCHAR(20) NOT NULL,   -- 授權者 Account_Name
    GrantedDate   DATETIME     NOT NULL DEFAULT GETDATE(),
    CONSTRAINT UQ_UserReport UNIQUE (EmployeeId, ReportID),
    CONSTRAINT FK_Permission_Report FOREIGN KEY (ReportID)
        REFERENCES ReportCatalog(ReportID) ON DELETE CASCADE
);

CREATE NONCLUSTERED INDEX IX_Perm_Employee ON UserReportPermission (EmployeeId);
CREATE NONCLUSTERED INDEX IX_Perm_Report   ON UserReportPermission (ReportID);
