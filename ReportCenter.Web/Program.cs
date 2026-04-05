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

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
