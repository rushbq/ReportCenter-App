using Microsoft.AspNetCore.Authentication.Negotiate;
using ReportCenter.Web.Middleware;
using ReportCenter.Web.Models.Settings;
using ReportCenter.Web.Repositories;
using ReportCenter.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();

// 設定 (appsettings.json)
builder.Services.Configure<ReportBaseUrlSettings>(
    builder.Configuration.GetSection(ReportBaseUrlSettings.SectionName));
builder.Services.Configure<DepartmentDisplaySettings>(
    builder.Configuration.GetSection(DepartmentDisplaySettings.SectionName));

// ── Windows 驗證設定 ──────────────────────────────
if (builder.Environment.IsDevelopment())
{
    // 開發環境：模擬 Windows AD 身份（macOS 無法使用 Windows 驗證）
    builder.Services.Configure<MockWindowsAuthSettings>(
        builder.Configuration.GetSection(MockWindowsAuthSettings.SectionName));
}
else
{
    // 正式環境 (IIS)：啟用 Negotiate (Windows) 驗證
    builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
        .AddNegotiate();
    builder.Services.AddAuthorization();
}

// 三層式架構 DI 註冊
// DAL — 資料存取層
builder.Services.AddScoped<ICatalogRepository, SqlCatalogRepository>();

// BLL — 商業邏輯層
builder.Services.AddScoped<IReportService, SqlReportService>();
builder.Services.AddScoped<ICatalogService, CatalogService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseRouting();

// ── 驗證中介層 ──────────────────────────────────
if (app.Environment.IsDevelopment())
{
    // 開發環境：注入模擬 Windows AD 身份
    app.UseMiddleware<DevWindowsAuthMiddleware>();
}
else
{
    // 正式環境：由 IIS 處理 Windows 驗證
    app.UseAuthentication();
}

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
