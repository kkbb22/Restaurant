using Microsoft.EntityFrameworkCore;
using Restaurant.Data;
using Restaurant.Services;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// 1. ≈⁄œ«œ «·‹ Controllers
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<ReservationService>();

// 2. ≈⁄œ«œ «·‹ CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// 3. ≈⁄œ«œ ﬁ«⁄œ… «·»Ì«‰«  («· Ê«›ﬁ «· «„ „⁄ PostgreSQL)
var dbUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (!string.IsNullOrEmpty(dbUrl))
    {
        var uri = new Uri(dbUrl);
        var userInfo = uri.UserInfo.Split(':');
        var pgConn = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
        options.UseNpgsql(pgConn);
    }
    else
    {
        // «” Œœ„ SQL Server ›ﬁÿ ··„Õ·Ì ≈–« ﬂ‰   ›÷· –·ﬂ° 
        // ·ﬂ‰ «·√›÷·  ÊÕÌœÂ« ·‹ PostgreSQL · Ã‰» „‘«ﬂ· «·‹ nvarchar
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    }
});

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

var app = builder.Build();

// 4. »‰«¡ «·Ãœ«Ê· ›Ê—« (Õ· „‘ﬂ·… Relation does not exist)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try {
        // EnsureCreated ÂÌ «·Õ· «·√”—⁄ ·»Ì∆… Railway ·√‰Â«   Ã«Â·  Ê«›ﬁ «·‹ Migrations «·ﬁœÌ„… Ê »‰Ì «·Ãœ«Ê· ›Ê—«
        context.Database.EnsureCreated(); 
        Console.WriteLine("? Database Ready!");
    } catch (Exception ex) {
        Console.WriteLine($"? Error: {ex.Message}");
    }
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowAll");
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();
app.Run(); 
