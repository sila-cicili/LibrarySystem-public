using LibrarySystem.Services;
using Microsoft.EntityFrameworkCore;
using LibrarySystem.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Npgsql; // 🛠️ Bunu ekledik (Adres dönüştürmek için)

var builder = WebApplication.CreateBuilder(args);

// 1. SERVİS AYARLARI
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

builder.Services.AddControllersWithViews();

// 👇 SWAGGER EKLENTİSİ
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// GİRİŞ SİSTEMİ (Authentication)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
    });

// ---------------------------------------------------------
// 🔥 VERİTABANI BAĞLANTISI (GÜÇLENDİRİLMİŞ KOD) 🔥
// ---------------------------------------------------------

var connectionString = "";

// 1. Railway'den gelen otomatik adresi al (DATABASE_URL)
var railwayDatabaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

if (!string.IsNullOrEmpty(railwayDatabaseUrl))
{
    // Railway adresi genellikle 'postgres://' ile başlar, bunu C#'ın anlayacağı formata çeviriyoruz:
    try 
    {
        var databaseUri = new Uri(railwayDatabaseUrl);
        var userInfo = databaseUri.UserInfo.Split(':');
        
        var builderDb = new NpgsqlConnectionStringBuilder
        {
            Host = databaseUri.Host,
            Port = databaseUri.Port,
            Username = userInfo[0],
            Password = userInfo[1],
            Database = databaseUri.LocalPath.TrimStart('/'),
            SslMode = SslMode.Require,
            TrustServerCertificate = true // Railway sertifikasını kabul et
        };
        connectionString = builderDb.ToString();
    }
    catch
    {
        // Çeviremezse olduğu gibi kullanmayı dener (Yedek plan)
        connectionString = railwayDatabaseUrl;
    }
}
else 
{
    // Railway yoksa, senin elle eklediğin veya Localhost ayarını kullan
    var manualStr = Environment.GetEnvironmentVariable("ConnectionStrings__LibraryContext");
    connectionString = !string.IsNullOrEmpty(manualStr) 
        ? manualStr 
        : builder.Configuration.GetConnectionString("LibraryContext");
}

// Bulunan adres ile veritabanına bağlan
builder.Services.AddDbContext<KütüphaneeContext>(options =>
    options.UseNpgsql(connectionString, 
        o => o.UseNetTopologySuite())); // Harita (PostGIS) desteği

// ---------------------------------------------------------

// Kendi Servislerin
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ILoanService, LoanService>();
builder.Services.AddScoped<IRequestService, RequestService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

var app = builder.Build();

// --- 🔥 OTOMATİK KURULUM VE HARİTA AÇMA (SİHİRLİ KISIM) ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<KütüphaneeContext>();
        
        // 👇 İŞTE BU SATIR TERMİNALLE UĞRAŞMANI ENGELLER!
        // Veritabanına bağlanır ve "PostGIS eklentisini aç" der.
        context.Database.ExecuteSqlRaw("CREATE EXTENSION IF NOT EXISTS postgis;");
        
        // Tabloları oluşturur
        context.Database.Migrate(); 
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Veritabanı kurulurken hata oluştu.");
    }
}
// ------------------------------------------------

// 2. MIDDLEWARE AYARLARI
if (app.Environment.IsDevelopment() || Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();