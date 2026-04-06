namespace ReportCenter.Web.Services;

using ReportCenter.Web.Models;

public class MockReportService : IReportService
{
    // ─── 使用者與公司 ───

    public UserInfo GetCurrentUser() => new()
    {
        Id = "U001",
        Name = "陳彥廷",
        DeptId = "it",
        DeptName = "資訊部",
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

    // ─── 收藏/釘選 (Mock 空實作) ───

    public List<int> GetUserFavorites() => [];
    public void ToggleFavorite(int reportId) { }
    public List<Report> GetUserPins() => [];
    public void TogglePin(int reportId) { }

}
