using Microsoft.EntityFrameworkCore;
using Restaurant.Data;
using Restaurant.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. منع حلقات التكرار في JSON (مهم جداً للطلبات)
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = 
        System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<ReservationService>();

// 2. إعداد الـ CORS (للسماح للفرونت إند بالتحدث مع الباك إند)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// 3. إعداد قاعدة البيانات (Railway vs Local)
var dbUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (!string.IsNullOrEmpty(dbUrl))
{
    var uri = new Uri(dbUrl);
    var userInfo = uri.UserInfo.Split(':');
    var pgConn = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
    builder.Services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(pgConn));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(opt =>
        opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
}

// 4. ضبط المنفذ الخاص بـ Railway
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

var app = builder.Build();

// 5. تنفيذ الميغريشن تلقائياً عند التشغيل
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try {
        db.Database.Migrate();
        Console.WriteLine("Database Migration Executed Successfully!");
    } catch (Exception ex) {
        Console.WriteLine($"Migration Error: {ex.Message}");
    }
}

// 6. Middleware Pipeline (الترتيب مهم جداً)
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll"); // استخدام السياسة التي عرفناها

app.UseAuthorization();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();

app.Run();