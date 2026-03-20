using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.Data;
using Restaurant.Models;

namespace Restaurant.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RestaurantSettingsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RestaurantSettingsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/RestaurantSettings?restaurantId=1
        [HttpGet]
        public async Task<ActionResult<RestaurantSettings>> GetSettings([FromQuery] int restaurantId = 0)
        {
            RestaurantSettings? settings;

            if (restaurantId > 0)
                settings = await _context.RestaurantSettings
                    .FirstOrDefaultAsync(s => s.RestaurantId == restaurantId);
            else
                settings = await _context.RestaurantSettings.FirstOrDefaultAsync();

            if (settings == null)
                settings = new RestaurantSettings { RestaurantName = "مطعمي", RestaurantId = restaurantId };

            return settings;
        }

        // PUT: api/RestaurantSettings
        [HttpPut]
        public async Task<ActionResult<RestaurantSettings>> UpdateSettings(RestaurantSettings updated)
        {
            RestaurantSettings? settings;

            if (updated.RestaurantId > 0)
                settings = await _context.RestaurantSettings
                    .FirstOrDefaultAsync(s => s.RestaurantId == updated.RestaurantId);
            else
                settings = await _context.RestaurantSettings.FirstOrDefaultAsync();

            if (settings == null)
            {
                updated.UpdatedAt = DateTime.Now;
                _context.RestaurantSettings.Add(updated);
            }
            else
            {
                settings.RestaurantName = updated.RestaurantName;
                settings.LogoUrl        = updated.LogoUrl;
                settings.PrimaryColor   = updated.PrimaryColor;
                settings.SecondaryColor = updated.SecondaryColor;
                settings.OpenTime       = updated.OpenTime;
                settings.CloseTime      = updated.CloseTime;
                settings.WorkingDays    = updated.WorkingDays;
                settings.AcceptOrders   = updated.AcceptOrders;
                settings.ClosedMessage  = updated.ClosedMessage;
                settings.PayCash        = updated.PayCash;
                settings.PayCard        = updated.PayCard;
                settings.PayOnline      = updated.PayOnline;
                settings.Phone          = updated.Phone;
                settings.Address        = updated.Address;
                settings.WhatsApp       = updated.WhatsApp;
                settings.UpdatedAt      = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            if (updated.RestaurantId > 0)
                return await _context.RestaurantSettings
                    .FirstAsync(s => s.RestaurantId == updated.RestaurantId);
            else
                return await _context.RestaurantSettings.FirstAsync();
        }

        // GET: api/RestaurantSettings/isopen?restaurantId=1
        [HttpGet("isopen")]
        public async Task<ActionResult<object>> IsOpen([FromQuery] int restaurantId = 0)
        {
            RestaurantSettings? settings;

            if (restaurantId > 0)
                settings = await _context.RestaurantSettings
                    .FirstOrDefaultAsync(s => s.RestaurantId == restaurantId);
            else
                settings = await _context.RestaurantSettings.FirstOrDefaultAsync();

            if (settings == null)
                return Ok(new { isOpen = true, message = "" });

            if (!settings.AcceptOrders)
                return Ok(new { isOpen = false, message = settings.ClosedMessage });

            var now   = DateTime.Now.TimeOfDay;
            var open  = TimeSpan.Parse(settings.OpenTime ?? "10:00");
            var close = TimeSpan.Parse(settings.CloseTime ?? "23:00");
            bool isOpen = now >= open && now <= close;

            return Ok(new {
                isOpen,
                message   = isOpen ? "" : settings.ClosedMessage,
                openTime  = settings.OpenTime,
                closeTime = settings.CloseTime
            });
        }
    }
}
