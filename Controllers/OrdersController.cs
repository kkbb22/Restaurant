using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.Data;
using Restaurant.Models;

namespace Restaurant.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;
        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetOrders([FromQuery] int restaurantId = 0)
        {
            var query = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems!) // علامة التعجب لحل الـ Warning
                    .ThenInclude(oi => oi.MenuItem)
                .AsQueryable();

            if (restaurantId > 0)
                query = query.Where(o => o.RestaurantId == restaurantId);

            var orders = await query.ToListAsync();

            var result = orders.Select(o => new {
                o.OrderId,
                o.CustomerId,
                o.RestaurantId,
                o.OrderDate,
                o.Status,
                o.TotalAmount,
                o.ReservationId,
                CustomerName  = o.Customer != null ? o.Customer.FullName : "زبون خارجي",
                CustomerPhone = o.Customer != null ? o.Customer.PhoneNumber : "",
                Items = o.OrderItems?.Select(i => new {
                    i.OrderItemId,
                    i.MenuItemId,
                    i.Quantity,
                    i.UnitPrice,
                    // هنا السحر: جلب اسم الأكلة
                    MenuName = i.MenuItem != null ? i.MenuItem.Name : "صنف غير معروف",
                    Price = i.UnitPrice
                })
            });

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetOrder(int id)
        {
            var o = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems!)
                    .ThenInclude(oi => oi.MenuItem)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (o == null) return NotFound();

            return Ok(new {
                o.OrderId, o.CustomerId, o.RestaurantId,
                o.OrderDate, o.Status, o.TotalAmount,
                CustomerName  = o.Customer?.FullName ?? "زبون خارجي",
                orderItems = o.OrderItems?.Select(i => new {
                    i.OrderItemId, i.MenuItemId, i.Quantity, i.UnitPrice,
                    MenuName = i.MenuItem?.Name ?? "صنف"
                })
            });
        }

        [HttpPost]
        public async Task<ActionResult<Order>> PostOrder(Order order)
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetOrder", new { id = order.OrderId }, order);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrder(int id, Order order)
        {
            if (id != order.OrderId) return BadRequest();
            _context.Entry(order).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}