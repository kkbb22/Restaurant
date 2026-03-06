using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.Data;
using Restaurant.Models;

namespace Restaurant.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RestaurantTablesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RestaurantTablesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RestaurantTable>>> GetTables()
        {
            return await _context.RestaurantTables.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RestaurantTable>> GetTable(int id)
        {
            var table = await _context.RestaurantTables.FindAsync(id);
            if (table == null) return NotFound();
            return table;
        }

        [HttpPost]
        public async Task<ActionResult<RestaurantTable>> PostTable(RestaurantTable table)
        {
            _context.RestaurantTables.Add(table);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetTable", new { id = table.TableId }, table);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutTable(int id, RestaurantTable table)
        {
            if (id != table.TableId) return BadRequest();
            _context.Entry(table).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTable(int id)
        {
            var table = await _context.RestaurantTables.FindAsync(id);
            if (table == null) return NotFound();
            _context.RestaurantTables.Remove(table);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}