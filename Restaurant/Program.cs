using Microsoft.EntityFrameworkCore;
using Restaurant.Data;
using Restaurant.Services;

var builder = WebApplication.CreateBuilder(args);

// إعداد JSON لمنع حلقات التكرار (Circular References)
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = 
        System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<ReservationService>();

// ── إعداد قاعدة البيانات (PostgreSQL للإنتاج و SQL Server للمحلي) ──
var dbUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

if (!string.IsNullOrEmpty(dbUrl))
{
    // تحويل رابط PostgreSQL من Railway إلى صيغة تفهمها Npgsql
    var uri = new Uri(dbUrl);
    var userInfo = uri.UserInfo.Split(':');
    var pgConn = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
    
    builder.Services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(pgConn));
}
else
{
    // تشغيل SQL Server محلياً إذا لم يوجد DATABASE_URL
    builder.Services.AddDbContext<AppDbContext>(opt =>
        opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
}

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// إعداد المنفذ (Port) الخاص بـ Railway
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

var app = builder.Build();

// تنفيذ الـ Migrations تلقائياً عند تشغيل السيرفر
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try {
        db.Database.Migrate();
    } catch (Exception ex) {
        Console.WriteLine($"Migration Error: {ex.Message}");
    }
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();
app.UseAuthorization();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();
app.Run();