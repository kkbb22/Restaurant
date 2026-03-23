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

// 3. ≈⁄œ«œ ﬁ«⁄œ… «·»Ì«‰«  (—»ÿ „»«‘— ⁄‘«‰ ‰Œ·’ „‰ «·‹ 500)
// Â«œ «·—«»ÿ „‰ »Ì«‰«  «·‹ Postgres  »⁄ ﬂ ›Ì Railway
var pgConn = "Host=viaduct.proxy.rlwy.net;Port=25152;Database=railway;Username=postgres;Password=mndXisvYFvTfXmPNojYwNqOfVfGNoTte;SSL Mode=Require;Trust Server Certificate=true";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(pgConn));

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

var app = builder.Build();

// 4. »‰«¡ «·Ãœ«Ê· ›Ê—«
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try {
        context.Database.EnsureCreated(); 
        Console.WriteLine("?? Database Ready!");
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