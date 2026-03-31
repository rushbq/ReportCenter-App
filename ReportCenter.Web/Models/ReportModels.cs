namespace ReportCenter.Web.Models;

public class Department
{
    public string Id { get; set; } = "";
    public string Label { get; set; } = "";
    public string Icon { get; set; } = ""; // Lucide icon name
    public int Count { get; set; }
    public List<string> Subs { get; set; } = [];
}

public class Report
{
    public string Name { get; set; } = "";
    public string Desc { get; set; } = "";
    public string Cat { get; set; } = "";
    public string Updated { get; set; } = "";
    public bool Fav { get; set; }
}

public class QuickAccess
{
    public string Dept { get; set; } = "";
    public string DeptId { get; set; } = "";
    public string Name { get; set; } = "";
    public string Tag { get; set; } = "";
}

public class MaterialRow
{
    public string Material { get; set; } = "";
    public string Supplier { get; set; } = "";
    public string Qty { get; set; } = "";
    public string UnitPrice { get; set; } = "";
    public string Amount { get; set; } = "";
    public double Change { get; set; }
}

public static class ReportData
{
    public static readonly List<Department> Departments =
    [
        new() { Id = "procurement", Label = "採購部", Icon = "package", Count = 24, Subs = ["成本分析", "供應商管理", "訂單追蹤", "績效報告"] },
        new() { Id = "sales", Label = "業務部", Icon = "bar-chart-3", Count = 31, Subs = ["客戶分析", "業績排名", "區域統計", "產品銷售"] },
        new() { Id = "finance", Label = "財務部", Icon = "dollar-sign", Count = 18, Subs = ["收支分析", "預算管理", "帳齡分析", "資金流向"] },
        new() { Id = "hr", Label = "人資部", Icon = "users", Count = 12, Subs = ["出勤管理", "薪資統計", "人力配置", "招募進度"] },
        new() { Id = "it", Label = "資訊部", Icon = "monitor", Count = 22, Subs = ["系統監控", "資安報告", "設備管理", "服務台統計"] },
    ];

    public static readonly Dictionary<string, List<Report>> Reports = new()
    {
        ["procurement"] =
        [
            new() { Name = "月度採購成本分析", Desc = "各類物料採購金額與趨勢分析", Cat = "成本分析", Updated = "03/20", Fav = true },
            new() { Name = "供應商交貨準時率", Desc = "各供應商交期達成率統計", Cat = "供應商管理", Updated = "03/18", Fav = false },
            new() { Name = "採購訂單狀態追蹤", Desc = "進行中、已完成、逾期訂單一覽", Cat = "訂單追蹤", Updated = "03/20", Fav = true },
            new() { Name = "物料價格波動分析", Desc = "主要原料歷史價格走勢與預測", Cat = "成本分析", Updated = "03/17", Fav = false },
            new() { Name = "供應商績效評比", Desc = "品質、交期、配合度綜合評分", Cat = "供應商管理", Updated = "03/15", Fav = false },
            new() { Name = "採購預算執行率", Desc = "各部門採購預算使用進度", Cat = "績效報告", Updated = "03/16", Fav = true },
            new() { Name = "進口關稅成本報表", Desc = "進口物料之關稅與運費分析", Cat = "成本分析", Updated = "03/14", Fav = false },
            new() { Name = "合約到期提醒清單", Desc = "即將到期之供應商合約一覽", Cat = "供應商管理", Updated = "03/13", Fav = false },
            new() { Name = "緊急採購統計表", Desc = "非計畫性採購次數與金額統計", Cat = "訂單追蹤", Updated = "03/12", Fav = false },
        ],
        ["sales"] =
        [
            new() { Name = "客戶銷售排名", Desc = "依營收排序之客戶清單", Cat = "客戶分析", Updated = "03/20", Fav = true },
            new() { Name = "區域營收分佈", Desc = "各區域銷售金額與占比", Cat = "區域統計", Updated = "03/19", Fav = true },
            new() { Name = "產品銷售趨勢", Desc = "各產品線月銷售走勢", Cat = "產品銷售", Updated = "03/18", Fav = false },
            new() { Name = "業務員績效報表", Desc = "各業務員目標達成率", Cat = "業績排名", Updated = "03/17", Fav = false },
            new() { Name = "新客戶開發統計", Desc = "新客戶數量與首單分析", Cat = "客戶分析", Updated = "03/16", Fav = true },
            new() { Name = "報價成交轉換率", Desc = "報價單轉成交之比率追蹤", Cat = "業績排名", Updated = "03/15", Fav = false },
        ],
        ["finance"] =
        [
            new() { Name = "應收帳款帳齡表", Desc = "客戶應收帳款帳齡分佈", Cat = "帳齡分析", Updated = "03/20", Fav = true },
            new() { Name = "預算執行率追蹤", Desc = "年度預算各科目執行進度", Cat = "預算管理", Updated = "03/19", Fav = true },
            new() { Name = "資金日報表", Desc = "每日資金餘額與異動明細", Cat = "資金流向", Updated = "03/20", Fav = false },
            new() { Name = "損益月報", Desc = "本月收入、成本、費用彙總", Cat = "收支分析", Updated = "03/18", Fav = false },
        ],
        ["hr"] =
        [
            new() { Name = "人員出勤統計", Desc = "各部門出勤率與異常統計", Cat = "出勤管理", Updated = "03/20", Fav = true },
            new() { Name = "加班時數分析", Desc = "各部門加班時數趨勢", Cat = "出勤管理", Updated = "03/19", Fav = false },
            new() { Name = "人力結構分析", Desc = "年齡、學歷、年資分佈", Cat = "人力配置", Updated = "03/15", Fav = false },
        ],
        ["it"] =
        [
            new() { Name = "系統可用性報告", Desc = "各系統 SLA 達成率", Cat = "系統監控", Updated = "03/20", Fav = true },
            new() { Name = "資安事件統計", Desc = "安全事件次數與處理時效", Cat = "資安報告", Updated = "03/18", Fav = false },
            new() { Name = "IT 服務台統計", Desc = "工單數量、處理時效、滿意度", Cat = "服務台統計", Updated = "03/17", Fav = false },
        ],
    };

    public static readonly List<QuickAccess> QuickAccessItems =
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

    public static readonly List<MaterialRow> MaterialRows =
    [
        new() { Material = "碳鋼板 SUS304", Supplier = "台灣鋼鐵", Qty = "5,200 KG", UnitPrice = "$85", Amount = "$442,000", Change = -2.3 },
        new() { Material = "PE 包裝膜", Supplier = "永豐塑膠", Qty = "12,000 M", UnitPrice = "$12", Amount = "$144,000", Change = 1.1 },
        new() { Material = "電子控制模組", Supplier = "矽達科技", Qty = "800 PCS", UnitPrice = "$320", Amount = "$256,000", Change = -5.2 },
        new() { Material = "潤滑油 ISO VG68", Supplier = "中油化學", Qty = "2,400 L", UnitPrice = "$45", Amount = "$108,000", Change = 0.8 },
        new() { Material = "銅線 Ø1.2mm", Supplier = "嘉義銅業", Qty = "3,600 KG", UnitPrice = "$210", Amount = "$756,000", Change = -1.7 },
        new() { Material = "不鏽鋼螺栓 M10", Supplier = "正達五金", Qty = "20,000 PCS", UnitPrice = "$3.5", Amount = "$70,000", Change = 2.4 },
        new() { Material = "矽膠密封圈", Supplier = "聯合橡膠", Qty = "8,500 PCS", UnitPrice = "$8", Amount = "$68,000", Change = -0.9 },
    ];
}
