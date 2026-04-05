namespace ReportCenter.Web.Services;

using ReportCenter.Web.Models;

public class MockReportService : IReportService
{
    // ─── 使用者與公司 ───

    public UserInfo GetCurrentUser() => new()
    {
        Id = "U001",
        Name = "陳彥廷",
        Initials = "CY",
        DeptId = "it",
        DeptName = "資訊部",
        Title = "系統工程師",
        CompanyId = "tw"
    };

    public List<Company> GetCompanies() =>
    [
        new() { Id = "tw", Name = "台灣寶工實業股份有限公司", ShortName = "台灣寶工", Region = "TW" },
        new() { Id = "sh", Name = "上海寶工工具有限公司", ShortName = "上海寶工", Region = "CN" },
    ];

    // ─── 部門 ───

    private static readonly List<Department> _departments =
    [
        new() { Id = "procurement", Label = "採購部", Icon = "package", Count = 24, Subs = ["成本分析", "供應商管理", "訂單追蹤", "績效報告"] },
        new() { Id = "sales", Label = "業務部", Icon = "bar-chart-3", Count = 31, Subs = ["客戶分析", "業績排名", "區域統計", "產品銷售"] },
        new() { Id = "finance", Label = "財務部", Icon = "dollar-sign", Count = 18, Subs = ["收支分析", "預算管理", "帳齡分析", "資金流向"] },
        new() { Id = "hr", Label = "人資部", Icon = "users", Count = 12, Subs = ["出勤管理", "薪資統計", "人力配置", "招募進度"] },
        new() { Id = "it", Label = "資訊部", Icon = "monitor", Count = 22, Subs = ["系統監控", "資安報告", "設備管理", "服務台統計"] },
    ];

    public List<Department> GetDepartments() => _departments;
    public Department? GetDepartment(string deptId) => _departments.Find(d => d.Id == deptId);

    // ─── 報表 ───

    private static readonly Dictionary<string, List<Report>> _reports = new()
    {
        ["procurement"] =
        [
            new() { Name = "月度採購成本分析", Desc = "各類物料採購金額與趨勢分析", Cat = "成本分析", Updated = "04/04", Fav = true },
            new() { Name = "供應商交貨準時率", Desc = "各供應商交期達成率統計", Cat = "供應商管理", Updated = "04/03", Fav = false },
            new() { Name = "採購訂單狀態追蹤", Desc = "進行中、已完成、逾期訂單一覽", Cat = "訂單追蹤", Updated = "04/04", Fav = true },
            new() { Name = "物料價格波動分析", Desc = "主要原料歷史價格走勢與預測", Cat = "成本分析", Updated = "04/01", Fav = false },
            new() { Name = "供應商績效評比", Desc = "品質、交期、配合度綜合評分", Cat = "供應商管理", Updated = "03/28", Fav = false },
            new() { Name = "採購預算執行率", Desc = "各部門採購預算使用進度", Cat = "績效報告", Updated = "03/25", Fav = true },
            new() { Name = "進口關稅成本報表", Desc = "進口物料之關稅與運費分析", Cat = "成本分析", Updated = "03/20", Fav = false },
            new() { Name = "合約到期提醒清單", Desc = "即將到期之供應商合約一覽", Cat = "供應商管理", Updated = "03/15", Fav = false },
            new() { Name = "緊急採購統計表", Desc = "非計畫性採購次數與金額統計", Cat = "訂單追蹤", Updated = "03/10", Fav = false },
        ],
        ["sales"] =
        [
            new() { Name = "客戶銷售排名", Desc = "依營收排序之客戶清單", Cat = "客戶分析", Updated = "04/04", Fav = true },
            new() { Name = "區域營收分佈", Desc = "各區域銷售金額與占比", Cat = "區域統計", Updated = "04/03", Fav = true },
            new() { Name = "產品銷售趨勢", Desc = "各產品線月銷售走勢", Cat = "產品銷售", Updated = "04/02", Fav = false },
            new() { Name = "業務員績效報表", Desc = "各業務員目標達成率", Cat = "業績排名", Updated = "04/01", Fav = false },
            new() { Name = "新客戶開發統計", Desc = "新客戶數量與首單分析", Cat = "客戶分析", Updated = "03/28", Fav = true },
            new() { Name = "報價成交轉換率", Desc = "報價單轉成交之比率追蹤", Cat = "業績排名", Updated = "03/21", Fav = false },
            new() { Name = "客戶帳款回收分析", Desc = "應收帳款回收天數與逾期比率", Cat = "客戶分析", Updated = "03/15", Fav = false },
            new() { Name = "產品退貨率統計", Desc = "各產品線退貨率與原因分析", Cat = "產品銷售", Updated = "03/10", Fav = false },
        ],
        ["finance"] =
        [
            new() { Name = "應收帳款帳齡表", Desc = "客戶應收帳款帳齡分佈", Cat = "帳齡分析", Updated = "04/04", Fav = true },
            new() { Name = "預算執行率追蹤", Desc = "年度預算各科目執行進度", Cat = "預算管理", Updated = "04/03", Fav = true },
            new() { Name = "資金日報表", Desc = "每日資金餘額與異動明細", Cat = "資金流向", Updated = "04/04", Fav = false },
            new() { Name = "損益月報", Desc = "本月收入、成本、費用彙總", Cat = "收支分析", Updated = "04/01", Fav = false },
            new() { Name = "應付帳款帳齡表", Desc = "供應商應付帳款帳齡分佈", Cat = "帳齡分析", Updated = "03/25", Fav = false },
            new() { Name = "費用報銷統計", Desc = "各部門費用報銷金額與趨勢", Cat = "收支分析", Updated = "03/18", Fav = false },
        ],
        ["hr"] =
        [
            new() { Name = "人員出勤統計", Desc = "各部門出勤率與異常統計", Cat = "出勤管理", Updated = "04/04", Fav = true },
            new() { Name = "加班時數分析", Desc = "各部門加班時數趨勢", Cat = "出勤管理", Updated = "04/02", Fav = false },
            new() { Name = "人力結構分析", Desc = "年齡、學歷、年資分佈", Cat = "人力配置", Updated = "03/26", Fav = false },
            new() { Name = "離職率趨勢分析", Desc = "月度離職率與離職原因統計", Cat = "人力配置", Updated = "03/20", Fav = false },
            new() { Name = "招募進度追蹤", Desc = "各部門招募需求與面試進度", Cat = "招募進度", Updated = "03/14", Fav = true },
            new() { Name = "薪資結構分析", Desc = "各職等薪資分佈與市場比較", Cat = "薪資統計", Updated = "03/08", Fav = false },
        ],
        ["it"] =
        [
            new() { Name = "系統可用性報告", Desc = "各系統 SLA 達成率", Cat = "系統監控", Updated = "04/04", Fav = true },
            new() { Name = "資安事件統計", Desc = "安全事件次數與處理時效", Cat = "資安報告", Updated = "04/02", Fav = false },
            new() { Name = "IT 服務台統計", Desc = "工單數量、處理時效、滿意度", Cat = "服務台統計", Updated = "04/01", Fav = false },
            new() { Name = "設備資產盤點", Desc = "IT 設備清冊與折舊狀態", Cat = "設備管理", Updated = "03/25", Fav = false },
            new() { Name = "網路流量監控", Desc = "內外部網路流量與異常偵測", Cat = "系統監控", Updated = "03/18", Fav = true },
        ],
    };

    public List<Report> GetReports(string deptId) =>
        _reports.GetValueOrDefault(deptId) ?? [];

    public Report? GetReport(string deptId, string reportName) =>
        GetReports(deptId).Find(r => r.Name == reportName);

    // ─── 快速存取 ───

    public List<QuickAccess> GetQuickAccessItems() =>
    [
        new() { Dept = "採購部", DeptId = "procurement", Name = "月度採購成本分析", Tag = "成本" },
        new() { Dept = "業務部", DeptId = "sales", Name = "客戶銷售排名", Tag = "銷售" },
        new() { Dept = "財務部", DeptId = "finance", Name = "應收帳款帳齡表", Tag = "財務" },
        new() { Dept = "業務部", DeptId = "sales", Name = "區域營收分佈", Tag = "銷售" },
        new() { Dept = "採購部", DeptId = "procurement", Name = "供應商績效評比", Tag = "供應商" },
        new() { Dept = "人資部", DeptId = "hr", Name = "人員出勤統計", Tag = "人資" },
        new() { Dept = "資訊部", DeptId = "it", Name = "系統可用性報告", Tag = "IT" },
        new() { Dept = "財務部", DeptId = "finance", Name = "預算執行率追蹤", Tag = "財務" },
    ];

    // ─── 明細資料 ───

    private static readonly List<MaterialRow> _allMaterialRows =
    [
        new() { Material = "碳鋼板 SUS304", Supplier = "台灣鋼鐵", Qty = "5,200 KG", UnitPrice = "$85", Amount = "$442,000", Change = -2.3, Category = "原料", Period = "2026/03" },
        new() { Material = "PE 包裝膜", Supplier = "永豐塑膠", Qty = "12,000 M", UnitPrice = "$12", Amount = "$144,000", Change = 1.1, Category = "包材", Period = "2026/03" },
        new() { Material = "電子控制模組", Supplier = "矽達科技", Qty = "800 PCS", UnitPrice = "$320", Amount = "$256,000", Change = -5.2, Category = "設備", Period = "2026/03" },
        new() { Material = "潤滑油 ISO VG68", Supplier = "中油化學", Qty = "2,400 L", UnitPrice = "$45", Amount = "$108,000", Change = 0.8, Category = "耗材", Period = "2026/03" },
        new() { Material = "銅線 Ø1.2mm", Supplier = "嘉義銅業", Qty = "3,600 KG", UnitPrice = "$210", Amount = "$756,000", Change = -1.7, Category = "原料", Period = "2026/02" },
        new() { Material = "不鏽鋼螺栓 M10", Supplier = "正達五金", Qty = "20,000 PCS", UnitPrice = "$3.5", Amount = "$70,000", Change = 2.4, Category = "原料", Period = "2026/02" },
        new() { Material = "矽膠密封圈", Supplier = "聯合橡膠", Qty = "8,500 PCS", UnitPrice = "$8", Amount = "$68,000", Change = -0.9, Category = "耗材", Period = "2026/02" },
        new() { Material = "鋁合金管 6061", Supplier = "建大鋁業", Qty = "1,800 KG", UnitPrice = "$165", Amount = "$297,000", Change = 3.2, Category = "原料", Period = "2026/01" },
        new() { Material = "工業用膠帶", Supplier = "三和黏著", Qty = "5,000 捲", UnitPrice = "$25", Amount = "$125,000", Change = -0.5, Category = "包材", Period = "2026/01" },
        new() { Material = "精密軸承 6205", Supplier = "東培工業", Qty = "3,200 PCS", UnitPrice = "$48", Amount = "$153,600", Change = -1.8, Category = "設備", Period = "2026/01" },
        new() { Material = "氟橡膠 O-ring", Supplier = "聯合橡膠", Qty = "15,000 PCS", UnitPrice = "$5.5", Amount = "$82,500", Change = 0.3, Category = "耗材", Period = "2025/12" },
        new() { Material = "鍍鋅鋼板 SGCC", Supplier = "中鋼構造", Qty = "4,800 KG", UnitPrice = "$72", Amount = "$345,600", Change = -3.1, Category = "原料", Period = "2025/12" },
        new() { Material = "PP 射出料", Supplier = "台塑石化", Qty = "6,500 KG", UnitPrice = "$38", Amount = "$247,000", Change = 1.5, Category = "原料", Period = "2025/12" },
        new() { Material = "伺服馬達 750W", Supplier = "台達電子", Qty = "120 PCS", UnitPrice = "$4,800", Amount = "$576,000", Change = -0.6, Category = "設備", Period = "2025/11" },
        new() { Material = "不鏽鋼焊條 E308", Supplier = "大同特殊鋼", Qty = "2,000 KG", UnitPrice = "$95", Amount = "$190,000", Change = 2.1, Category = "原料", Period = "2025/11" },
        new() { Material = "液壓油 HM46", Supplier = "中油化學", Qty = "3,000 L", UnitPrice = "$52", Amount = "$156,000", Change = -1.2, Category = "耗材", Period = "2025/11" },
        new() { Material = "碳纖維布 3K", Supplier = "台塑碳纖", Qty = "800 M²", UnitPrice = "$380", Amount = "$304,000", Change = 4.5, Category = "原料", Period = "2025/10" },
        new() { Material = "LED 驅動模組", Supplier = "億光電子", Qty = "2,500 PCS", UnitPrice = "$65", Amount = "$162,500", Change = -2.8, Category = "設備", Period = "2025/10" },
        new() { Material = "彈簧鋼線 SWC", Supplier = "嘉義銅業", Qty = "1,500 KG", UnitPrice = "$125", Amount = "$187,500", Change = 0.9, Category = "原料", Period = "2025/10" },
        new() { Material = "耐熱矽膠管", Supplier = "聯合橡膠", Qty = "4,000 M", UnitPrice = "$18", Amount = "$72,000", Change = -0.4, Category = "耗材", Period = "2025/10" },
    ];

    public List<MaterialRow> GetMaterialRows(string deptId, string reportName, int page = 1, int pageSize = 20)
    {
        return _allMaterialRows
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    public int GetMaterialRowCount(string deptId, string reportName) => _allMaterialRows.Count;

    // ─── 儀表板 KPI ───

    public DashboardKpi GetDashboardKpi(string companyId)
    {
        if (companyId == "sh")
        {
            return new DashboardKpi
            {
                LastUpdated = "2026/03/20 10:15",
                Items =
                [
                    new() { Title = "本月營收", Value = "¥58.6M", Trend = 5.7, Note = "vs 上月" },
                    new() { Title = "採購成本", Value = "¥21.3M", Trend = -1.8, Note = "vs 上月" },
                    new() { Title = "毛利率", Value = "63.7%", Trend = 1.9, Note = "" },
                    new() { Title = "訂單數", Value = "2,156", Trend = 9.2, Note = "vs 上月" },
                ]
            };
        }
        return new DashboardKpi
        {
            LastUpdated = "2026/03/20 09:30",
            Items =
            [
                new() { Title = "本月營收", Value = "$12.8M", Trend = 8.3, Note = "vs 上月" },
                new() { Title = "採購成本", Value = "$4.2M", Trend = -3.1, Note = "vs 上月" },
                new() { Title = "毛利率", Value = "67.2%", Trend = 2.4, Note = "" },
                new() { Title = "訂單數", Value = "1,847", Trend = 12.6, Note = "vs 上月" },
            ]
        };
    }

    // ─── 圖表資料 ───

    public ChartData GetRevenueChartData(string companyId, string period = "month")
    {
        return new ChartData
        {
            Labels = ["10月", "11月", "12月", "1月", "2月", "3月"],
            Datasets =
            [
                new() { Label = "本年", Data = [10.2, 11.1, 12.5, 10.8, 11.6, 12.8], Color = "#005758", Fill = true },
                new() { Label = "去年", Data = [9.8, 10.3, 11.0, 9.5, 10.1, 10.9], Color = "#00b4b6", BorderDash = "4,3" },
                new() { Label = "預算", Data = [10.5, 10.8, 11.2, 11.0, 11.3, 11.5], Color = "#94aeb0", BorderDash = "2,2" },
            ]
        };
    }

    public ChartData GetDeptComparisonData(string companyId)
    {
        return new ChartData
        {
            Labels = ["採購部", "業務部", "財務部", "人資部", "資訊部"],
            Datasets =
            [
                new() { Label = "實際", Data = [4.2, 5.8, 1.2, 0.8, 1.6], Color = "#005758", Type = "bar" },
                new() { Label = "目標", Data = [4.5, 5.2, 1.3, 0.9, 1.8], Color = "#eef2f3", Type = "bar" },
            ]
        };
    }

    public ChartData GetReportChartData(string deptId, string reportName, string chartType)
    {
        if (chartType == "pie")
        {
            return new ChartData
            {
                Labels = ["原料", "包材", "設備", "其他"],
                Datasets =
                [
                    new() { Label = "佔比", Data = [62, 18, 12, 8], Color = "#005758", Type = "doughnut" },
                ]
            };
        }

        return new ChartData
        {
            Labels = ["10月", "11月", "12月", "1月", "2月", "3月"],
            Datasets =
            [
                new() { Label = "原料", Data = [2.5, 2.7, 2.9, 2.6, 2.8, 2.6], Color = "#005758", Fill = true },
                new() { Label = "包材", Data = [0.8, 0.7, 0.9, 0.8, 0.7, 0.8], Color = "#00b4b6", Fill = true },
                new() { Label = "設備", Data = [0.6, 0.5, 0.7, 0.4, 0.6, 0.5], Color = "#006d6e" },
                new() { Label = "其他", Data = [0.4, 0.3, 0.5, 0.4, 0.3, 0.3], Color = "#94aeb0", BorderDash = "3,3" },
            ]
        };
    }

    // ─── 報表目錄管理 ───

    private int _nextId = 15;
    private readonly List<ReportCatalogItem> _catalog =
    [
        // Internal 報表
        new() { ReportID = 1, ReportName = "月度採購成本分析", ReportTool = "Internal", ReportPath = "採購部",
                ReportCode = "/Report?dept=procurement&name=月度採購成本分析", SourceName = "usp_Rpt_ProcurementCost",
                CreateDate = new DateTime(2025, 6, 15), ModifyDate = new DateTime(2026, 4, 4), IsActive = true,
                Departments = [new() { DeptID = 1, DeptName = "採購部" }, new() { DeptID = 5, DeptName = "資訊部" }],
                Dependencies = ["fn_Cost_MonthlyByDept", "vw_PurchaseOrder", "usp_Rpt_ProcurementCost"] },
        new() { ReportID = 2, ReportName = "客戶銷售排名", ReportTool = "Internal", ReportPath = "業務部",
                ReportCode = "/Report?dept=sales&name=客戶銷售排名", SourceName = "usp_Rpt_SalesRanking",
                CreateDate = new DateTime(2025, 7, 20), ModifyDate = new DateTime(2026, 4, 3), IsActive = true,
                Departments = [new() { DeptID = 2, DeptName = "業務部" }],
                Dependencies = ["fn_Sales_MonthlyByDept", "vw_CustomerOrder", "usp_Rpt_SalesRanking"] },
        new() { ReportID = 3, ReportName = "應收帳款帳齡表", ReportTool = "Internal", ReportPath = "財務部",
                ReportCode = "/Report?dept=finance&name=應收帳款帳齡表", SourceName = "usp_Rpt_ARaging",
                CreateDate = new DateTime(2025, 8, 10), ModifyDate = new DateTime(2026, 4, 4), IsActive = true,
                Departments = [new() { DeptID = 3, DeptName = "財務部" }],
                Dependencies = ["vw_AccountReceivable", "usp_Rpt_ARaging"] },
        new() { ReportID = 4, ReportName = "人員出勤統計", ReportTool = "Internal", ReportPath = "人資部",
                ReportCode = "/Report?dept=hr&name=人員出勤統計", SourceName = "usp_Rpt_Attendance",
                CreateDate = new DateTime(2025, 9, 5), ModifyDate = new DateTime(2026, 4, 2), IsActive = true,
                Departments = [new() { DeptID = 4, DeptName = "人資部" }],
                Dependencies = ["vw_AttendanceDaily", "usp_Rpt_Attendance"] },
        new() { ReportID = 5, ReportName = "系統可用性報告", ReportTool = "Internal", ReportPath = "資訊部",
                ReportCode = "/Report?dept=it&name=系統可用性報告", SourceName = "usp_Rpt_SysAvailability",
                CreateDate = new DateTime(2025, 10, 1), ModifyDate = new DateTime(2026, 3, 28), IsActive = true,
                Departments = [new() { DeptID = 5, DeptName = "資訊部" }],
                Dependencies = ["vw_SystemHealth", "usp_Rpt_SysAvailability"] },

        // SmartQuery 報表
        new() { ReportID = 6, ReportName = "庫存即時查詢", ReportTool = "SmartQuery", ReportPath = "採購部",
                ReportCode = "https://sq.proskit.com/report/9f3ac9e2-51e9-9332-5312-df1553576d1a", SourceName = "vw_StockBalance",
                CreateDate = new DateTime(2026, 1, 27), ModifyDate = null, IsActive = true,
                Departments = [new() { DeptID = 1, DeptName = "採購部" }, new() { DeptID = 2, DeptName = "業務部" }],
                Dependencies = ["vw_StockBalance"] },
        new() { ReportID = 7, ReportName = "訂單進度查詢", ReportTool = "SmartQuery", ReportPath = "業務部",
                ReportCode = "https://sq.proskit.com/report/6a4c953a-6f1e-b65b-5071-cc2a35f72b7b", SourceName = "vw_OrderStatus",
                CreateDate = new DateTime(2026, 1, 27), ModifyDate = null, IsActive = true,
                Departments = [new() { DeptID = 2, DeptName = "業務部" }],
                Dependencies = ["vw_OrderStatus", "fn_Sales_MonthlyByDept"] },
        new() { ReportID = 8, ReportName = "供應商交貨追蹤", ReportTool = "SmartQuery", ReportPath = "採購部",
                ReportCode = "https://sq.proskit.com/report/7d414441-3642-4fb6-a6f7-0e0dc9e6c501", SourceName = "vw_DeliveryTracking",
                CreateDate = new DateTime(2026, 2, 15), ModifyDate = null, IsActive = true,
                Departments = [new() { DeptID = 1, DeptName = "採購部" }],
                Dependencies = ["vw_DeliveryTracking", "vw_PurchaseOrder"] },
        new() { ReportID = 9, ReportName = "產品 BOM 查詢", ReportTool = "SmartQuery", ReportPath = "採購部",
                ReportCode = "https://sq.proskit.com/report/a1b2c3d4-e5f6-7890-abcd-ef1234567890", SourceName = "TSQL",
                CreateDate = new DateTime(2026, 3, 1), ModifyDate = null, IsActive = true,
                Departments = [new() { DeptID = 1, DeptName = "採購部" }, new() { DeptID = 5, DeptName = "資訊部" }],
                Dependencies = [] },

        // SSRS 報表
        new() { ReportID = 10, ReportName = "月結損益表", ReportTool = "SSRS", ReportPath = "財務部",
                ReportCode = "https://ssrs.proskit.com/Reports/PnL", SourceName = "usp_RPT_PnL",
                CreateDate = new DateTime(2025, 3, 10), ModifyDate = new DateTime(2026, 3, 31), IsActive = true,
                Departments = [new() { DeptID = 3, DeptName = "財務部" }, new() { DeptID = 1, DeptName = "採購部" }],
                Dependencies = ["usp_RPT_PnL", "vw_AccountReceivable"] },
        new() { ReportID = 11, ReportName = "應收帳款明細", ReportTool = "SSRS", ReportPath = "財務部",
                ReportCode = "https://ssrs.proskit.com/Reports/ARDetail", SourceName = "usp_RPT_ARDetail",
                CreateDate = new DateTime(2025, 4, 20), ModifyDate = new DateTime(2026, 3, 15), IsActive = true,
                Departments = [new() { DeptID = 3, DeptName = "財務部" }],
                Dependencies = ["usp_RPT_ARDetail", "vw_AccountReceivable"] },
        new() { ReportID = 12, ReportName = "採購彙總報表", ReportTool = "SSRS", ReportPath = "採購部",
                ReportCode = "https://ssrs.proskit.com/Reports/ProcSummary", SourceName = "usp_RPT_ProcSummary",
                CreateDate = new DateTime(2025, 5, 5), ModifyDate = new DateTime(2026, 2, 28), IsActive = true,
                Departments = [new() { DeptID = 1, DeptName = "採購部" }],
                Dependencies = ["usp_RPT_ProcSummary", "fn_Cost_MonthlyByDept"] },
        new() { ReportID = 13, ReportName = "業績獎金計算表", ReportTool = "SSRS", ReportPath = "人資部",
                ReportCode = "https://ssrs.proskit.com/Reports/Bonus", SourceName = "usp_RPT_Bonus",
                CreateDate = new DateTime(2025, 6, 1), ModifyDate = new DateTime(2026, 1, 31), IsActive = true,
                Departments = [new() { DeptID = 4, DeptName = "人資部" }, new() { DeptID = 2, DeptName = "業務部" }],
                Dependencies = ["usp_RPT_Bonus", "fn_Sales_MonthlyByDept"] },

        // 停用 + 孤立報表
        new() { ReportID = 14, ReportName = "設備折舊明細", ReportTool = "SSRS", ReportPath = "財務部",
                ReportCode = "https://ssrs.proskit.com/Reports/Depreciation", SourceName = "usp_RPT_Depreciation",
                CreateDate = new DateTime(2025, 2, 1), ModifyDate = new DateTime(2025, 12, 15), IsActive = false,
                Departments = [],
                Dependencies = ["usp_RPT_Depreciation"] },
    ];

    public List<ReportCatalogItem> GetCatalogItems(string? toolFilter = null, bool? isActive = null, string? search = null)
    {
        IEnumerable<ReportCatalogItem> items = _catalog;

        if (!string.IsNullOrEmpty(toolFilter))
            items = items.Where(i => i.ReportTool == toolFilter);

        if (isActive.HasValue)
            items = items.Where(i => i.IsActive == isActive.Value);

        if (!string.IsNullOrEmpty(search))
            items = items.Where(i =>
                i.ReportName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                i.SourceName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                i.ReportPath.Contains(search, StringComparison.OrdinalIgnoreCase));

        return items.OrderByDescending(i => i.ModifyDate ?? i.CreateDate).ToList();
    }

    public ReportCatalogItem? GetCatalogItem(int reportId) =>
        _catalog.Find(i => i.ReportID == reportId);

    public ReportCatalogItem SaveCatalogItem(ReportCatalogItem item)
    {
        if (item.ReportID == 0)
        {
            item.ReportID = _nextId++;
            item.CreateDate = DateTime.Now;
            item.ModifyDate = DateTime.Now;
            _catalog.Add(item);
        }
        else
        {
            var existing = _catalog.Find(i => i.ReportID == item.ReportID);
            if (existing != null)
            {
                existing.ReportName = item.ReportName;
                existing.ReportTool = item.ReportTool;
                existing.ReportPath = item.ReportPath;
                existing.ReportCode = item.ReportCode;
                existing.SourceName = item.SourceName;
                existing.IsActive = item.IsActive;
                existing.Remarks = item.Remarks;
                existing.ModifyDate = DateTime.Now;
                existing.Departments = item.Departments;
                existing.Dependencies = item.Dependencies;
                return existing;
            }
        }
        return item;
    }

    public bool DeleteCatalogItem(int reportId)
    {
        var item = _catalog.Find(i => i.ReportID == reportId);
        if (item == null) return false;
        _catalog.Remove(item);
        return true;
    }

    public AdminStats GetAdminStats()
    {
        var stats = new AdminStats
        {
            TotalReports = _catalog.Count,
            ActiveReports = _catalog.Count(i => i.IsActive),
            InactiveReports = _catalog.Count(i => !i.IsActive),
            InternalCount = _catalog.Count(i => i.ReportTool == "Internal"),
            SmartQueryCount = _catalog.Count(i => i.ReportTool == "SmartQuery"),
            SsrsCount = _catalog.Count(i => i.ReportTool == "SSRS"),
            OrphanCount = _catalog.Count(i => i.Departments.Count == 0),
            RecentItems = _catalog.OrderByDescending(i => i.ModifyDate ?? i.CreateDate).Take(10).ToList(),
            OrphanItems = _catalog.Where(i => i.Departments.Count == 0).ToList(),
        };

        // 部門使用矩陣
        var deptMap = new Dictionary<string, List<ReportCatalogItem>>();
        foreach (var item in _catalog.Where(i => i.IsActive))
        {
            foreach (var dept in item.Departments)
            {
                if (!deptMap.ContainsKey(dept.DeptName))
                    deptMap[dept.DeptName] = [];
                deptMap[dept.DeptName].Add(item);
            }
        }
        stats.DeptUsages = deptMap.Select(kv => new DeptUsage { DeptName = kv.Key, Reports = kv.Value }).ToList();

        return stats;
    }

    public List<string> GetAllDependencyObjects() =>
        _catalog.SelectMany(i => i.Dependencies).Distinct().OrderBy(d => d).ToList();

    // ─── 報表分類管理 ───

    private int _nextCatId = 1;
    private readonly List<ReportCategory> _categories = [];

    public List<ReportCategory> GetCategories(int? deptId = null)
    {
        IEnumerable<ReportCategory> items = _categories;
        if (deptId.HasValue)
            items = items.Where(c => c.DeptID == deptId.Value);
        return items.OrderBy(c => c.SortOrder).ThenBy(c => c.CategoryName).ToList();
    }

    public ReportCategory? GetCategory(int categoryId) =>
        _categories.Find(c => c.CategoryID == categoryId);

    public ReportCategory SaveCategory(ReportCategory category)
    {
        if (category.CategoryID == 0)
        {
            category.CategoryID = _nextCatId++;
            _categories.Add(category);
        }
        else
        {
            var existing = _categories.Find(c => c.CategoryID == category.CategoryID);
            if (existing != null)
            {
                existing.CategoryName = category.CategoryName;
                existing.DeptID = category.DeptID;
                existing.SortOrder = category.SortOrder;
                existing.IsActive = category.IsActive;
                return existing;
            }
        }
        return category;
    }

    public bool DeleteCategory(int categoryId)
    {
        var item = _categories.Find(c => c.CategoryID == categoryId);
        if (item == null) return false;
        _categories.Remove(item);
        return true;
    }

    // ─── 部門 (User_Dept) ───

    public List<CatalogDept> GetCatalogDepartments(string? area = null) =>
    [
        new() { Area = "TW", DeptID = "100", DeptName = "總經理室" },
        new() { Area = "TW", DeptID = "109", DeptName = "資訊部" },
        new() { Area = "TW", DeptID = "110", DeptName = "管理部" },
        new() { Area = "TW", DeptID = "120", DeptName = "外銷業務部" },
        new() { Area = "TW", DeptID = "130", DeptName = "內銷業務部" },
        new() { Area = "TW", DeptID = "140", DeptName = "品保部" },
        new() { Area = "TW", DeptID = "150", DeptName = "生產部" },
        new() { Area = "TW", DeptID = "151", DeptName = "採購部" },
        new() { Area = "TW", DeptID = "180", DeptName = "行銷企劃部" },
        new() { Area = "TW", DeptID = "190", DeptName = "資材部" },
    ];
}
