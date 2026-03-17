using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.Data;
using Restaurant.Models;
using BCrypt.Net;

namespace Restaurant.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public async Task<ActionResult<object>> Login([FromBody] LoginRequest req)
        {
            if (req.Username == "admin" && req.Password == "admin123")
            {
                return Ok(new { success = true, role = "superadmin", restaurantId = 0, name = "Super Admin" });
            }

            var restaurant = await _context.Restaurants
                .FirstOrDefaultAsync(r => r.Username == req.Username && r.IsActive);

            if (restaurant == null || !BCrypt.Net.BCrypt.Verify(req.Password, restaurant.Password))
            {
                return Unauthorized(new { success = false, message = "اسم المستخدم أو كلمة المرور غلط" });
            }

            return Ok(new {
                success = true,
                role = "restaurant",
                restaurantId = restaurant.RestaurantId,
                name = restaurant.Name,
                primaryColor = restaurant.PrimaryColor,
                secondaryColor = restaurant.SecondaryColor,
                openTime = restaurant.OpenTime,
                closeTime = restaurant.CloseTime,
                acceptOrders = restaurant.AcceptOrders,
                logoUrl = restaurant.LogoUrl,
                phone = restaurant.Phone,
                address = restaurant.Address
            });
        }

        [HttpPost("add-restaurant")]
        public async Task<ActionResult<object>> AddRestaurant([FromBody] AddRestaurantRequest req)
        {
            if (req.AdminPassword != "admin123")
                return Unauthorized(new { message = "غير مصرح" });

            var exists = await _context.Restaurants.AnyAsync(r => r.Username == req.Username);
            if (exists) return BadRequest(new { message = "اسم المستخدم موجود مسبقاً" });

            var restaurant = new RestaurantProfile 
            {
                Name = req.Name,
                Username = req.Username,
                Password = BCrypt.Net.BCrypt.HashPassword(req.Password),
                PrimaryColor = req.PrimaryColor ?? "#E63946",
                SecondaryColor = req.SecondaryColor ?? "#FF6B35",
                OpenTime = req.OpenTime ?? "10:00",
                CloseTime = req.CloseTime ?? "23:00",
                Phone = req.Phone,
                Address = req.Address,
                IsActive = true,
                AcceptOrders = true,
                CreatedAt = DateTime.Now
            };

            _context.Restaurants.Add(restaurant);
            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "تم إضافة المطعم بنجاح", restaurantId = restaurant.RestaurantId });
        }

        [HttpGet("restaurants")]
        public async Task<ActionResult<object>> GetRestaurants([FromQuery] string adminPassword)
        {
            if (adminPassword != "admin123") return Unauthorized();
            return await _context.Restaurants.Select(r => new {
                r.RestaurantId, r.Name, r.Username, r.IsActive, r.CreatedAt, r.Phone, r.Address
            }).ToListAsync();
        }

        [HttpPut("toggle/{id}")]
        public async Task<ActionResult> ToggleRestaurant(int id, [FromQuery] string adminPassword)
        {
            if (adminPassword != "admin123") return Unauthorized();
            var restaurant = await _context.Restaurants.FindAsync(id);
            if (restaurant == null) return NotFound();
            restaurant.IsActive = !restaurant.IsActive;
            await _context.SaveChangesAsync();
            return Ok(new { success = true, isActive = restaurant.IsActive });
        }
    }

    // ─── هذه هي الـ Classes التي كانت ناقصة وتسببت بالخطأ ───
    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class AddRestaurantRequest
    {
        public string AdminPassword { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? PrimaryColor { get; set; }
        public string? SecondaryColor { get; set; }
        public string? OpenTime { get; set; }
        public string? CloseTime { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
    }
}