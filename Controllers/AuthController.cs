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

        // ── إعادة تعيين كلمة المرور (من السوبر أدمن) ──────────────
        [HttpPut("reset-password")]
        public async Task<ActionResult> ResetPassword([FromQuery] string adminPassword, [FromQuery] string username, [FromQuery] string newPassword)
        {
            if (adminPassword != "admin123")
                return Unauthorized(new { message = "غير مصرح" });

            var restaurant = await _context.Restaurants.FirstOrDefaultAsync(r => r.Username == username);
            if (restaurant == null)
                return NotFound(new { message = "المطعم غير موجود" });

            restaurant.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = $"تم تغيير كلمة مرور {restaurant.Name} بنجاح" });
        }

        // ── تغيير كلمة المرور (من صاحب المطعم) ────────────────────
        [HttpPut("change-password")]
        public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequest req)
        {
            var restaurant = await _context.Restaurants.FirstOrDefaultAsync(r => r.Username == req.Username);
            if (restaurant == null)
                return NotFound(new { message = "المستخدم غير موجود" });

            if (!BCrypt.Net.BCrypt.Verify(req.CurrentPassword, restaurant.Password))
                return Unauthorized(new { message = "كلمة المرور الحالية غلط" });

            if (req.NewPassword.Length < 6)
                return BadRequest(new { message = "كلمة المرور يجب أن تكون 6 أحرف على الأقل" });

            restaurant.Password = BCrypt.Net.BCrypt.HashPassword(req.NewPassword);
            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "تم تغيير كلمة المرور بنجاح" });
        }

        // ── تغيير اسم المستخدم (من صاحب المطعم) ───────────────────
        [HttpPut("change-username")]
        public async Task<ActionResult> ChangeUsername([FromBody] ChangeUsernameRequest req)
        {
            var restaurant = await _context.Restaurants.FirstOrDefaultAsync(r => r.Username == req.CurrentUsername);
            if (restaurant == null)
                return NotFound(new { message = "المستخدم غير موجود" });

            if (!BCrypt.Net.BCrypt.Verify(req.Password, restaurant.Password))
                return Unauthorized(new { message = "كلمة المرور غلط" });

            var exists = await _context.Restaurants.AnyAsync(r => r.Username == req.NewUsername && r.RestaurantId != restaurant.RestaurantId);
            if (exists)
                return BadRequest(new { message = "اسم المستخدم موجود مسبقاً" });

            restaurant.Username = req.NewUsername;
            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "تم تغيير اسم المستخدم بنجاح" });
        }
    }

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

    public class ChangePasswordRequest
    {
        public string Username { get; set; } = string.Empty;
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public int RestaurantId { get; set; }
    }

    public class ChangeUsernameRequest
    {
        public string CurrentUsername { get; set; } = string.Empty;
        public string NewUsername { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int RestaurantId { get; set; }
    }
}
