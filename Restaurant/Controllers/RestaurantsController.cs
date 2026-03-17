using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.Data;
using Restaurant.Models; // سيقرأ RestaurantProfile من هنا
using BCrypt.Net;

namespace Restaurant.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RestaurantsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RestaurantsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public async Task<ActionResult<object>> Register([FromBody] RestaurantProfile data) 
        {
            // فحص اسم المستخدم (Username)
            if (await _context.Restaurants.AnyAsync(r => r.Username == data.Username))
            {
                return BadRequest("اسم المستخدم مسجل مسبقاً");
            }

            // تشفير كلمة المرور
            data.Password = BCrypt.Net.BCrypt.HashPassword(data.Password);

            _context.Restaurants.Add(data);
            await _context.SaveChangesAsync();

            return Ok(new { message = "تم التسجيل بنجاح", id = data.RestaurantId });
        }

        [HttpPost("login")]
        public async Task<ActionResult<object>> Login([FromBody] RestaurantLoginDTO login)
        {
            var r = await _context.Restaurants
                .FirstOrDefaultAsync(x => x.Username == login.Username);

            if (r == null || !BCrypt.Net.BCrypt.Verify(login.Password, r.Password))
            {
                return Unauthorized("خطأ في اسم المستخدم أو كلمة المرور");
            }

            return Ok(new { 
                restaurantId = r.RestaurantId, 
                name = r.Name 
            });
        }

        [HttpGet]
        public async Task<ActionResult<object>> GetRestaurants()
        {
            return await _context.Restaurants.ToListAsync();
        }
    }

    public class RestaurantLoginDTO
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}