using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.Data;
using Restaurant.Models;
using Restaurant.Services;

namespace Restaurant.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ReservationService _reservationService;

        public ReservationsController(AppDbContext context, ReservationService reservationService)
        {
            _context = context;
            _reservationService = reservationService;
        }

        // GET: api/Reservations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Reservation>>> GetReservations()
        {
            return await _context.Reservations
                .Include(r => r.Customer)
                .Include(r => r.Table)
                .OrderByDescending(r => r.ReservationDate)
                .ThenBy(r => r.ReservationTime)
                .ToListAsync();
        }

        // GET: api/Reservations/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Reservation>> GetReservation(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Customer)
                .Include(r => r.Table)
                .FirstOrDefaultAsync(r => r.ReservationId == id);

            if (reservation == null)
            {
                return NotFound();
            }

            return reservation;
        }

        // GET: api/Reservations/AvailableTables
        [HttpGet("AvailableTables")]
        public async Task<ActionResult<IEnumerable<RestaurantTable>>> GetAvailableTables(
            [FromQuery] DateTime date,
            [FromQuery] TimeSpan time,
            [FromQuery] int guests)
        {
            var tables = await _context.RestaurantTables
                .Where(t => t.Capacity >= guests && t.IsAvailable)
                .ToListAsync();

            var availableTables = new List<RestaurantTable>();

            foreach (var table in tables)
            {
                var isAvailable = await _reservationService.IsTableAvailable(
                    table.TableId, date, time, guests);

                if (isAvailable)
                    availableTables.Add(table);
            }

            return availableTables;
        }

        // POST: api/Reservations
        [HttpPost]
        public async Task<ActionResult<Reservation>> PostReservation(Reservation reservation)
        {
            try
            {
                // التحقق من توفر الطاولة
                var available = await _reservationService.IsTableAvailable(
                    reservation.TableId,
                    reservation.ReservationDate,
                    reservation.ReservationTime,
                    reservation.NumberOfGuests
                );

                if (!available)
                {
                    return BadRequest(new { message = "الطاولة غير متوفرة في هذا الوقت" });
                }

                reservation.CreatedAt = DateTime.Now;
                _context.Reservations.Add(reservation);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetReservation", new { id = reservation.ReservationId }, reservation);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/Reservations/MakeReservation
        [HttpPost("MakeReservation")]
        public async Task<ActionResult<Reservation>> MakeReservation(Reservation reservation)
        {
            try
            {
                var newReservation = await _reservationService.CreateReservation(reservation);
                return Ok(new
                {
                    message = "تم الحجز بنجاح",
                    reservation = newReservation
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/Reservations/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutReservation(int id, Reservation reservation)
        {
            if (id != reservation.ReservationId)
            {
                return BadRequest();
            }

            _context.Entry(reservation).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReservationExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // PUT: api/Reservations/Cancel/5
        [HttpPut("Cancel/{id}")]
        public async Task<IActionResult> CancelReservation(int id)
        {
            var result = await _reservationService.CancelReservation(id);
            if (!result)
                return NotFound();

            return Ok(new { message = "تم إلغاء الحجز بنجاح" });
        }

        // DELETE: api/Reservations/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReservation(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }

            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Reservations/Active
        [HttpGet("Active")]
        public async Task<ActionResult<IEnumerable<Reservation>>> GetActiveReservations()
        {
            var reservations = await _reservationService.GetActiveReservations();
            return Ok(reservations);
        }

        // GET: api/Reservations/MyReservations/5
        [HttpGet("MyReservations/{customerId}")]
        public async Task<ActionResult<IEnumerable<Reservation>>> GetMyReservations(int customerId)
        {
            var reservations = await _reservationService.GetCustomerReservations(customerId);
            return Ok(reservations);
        }

        // POST: api/Reservations/CheckAvailability
        [HttpPost("CheckAvailability")]
        public async Task<ActionResult<bool>> CheckAvailability(
            [FromBody] AvailabilityRequest request)
        {
            var available = await _reservationService.IsTableAvailable(
                request.TableId,
                request.Date,
                request.Time,
                request.Guests
            );

            return Ok(new { available, message = available ? "متوفرة" : "غير متوفرة" });
        }

        private bool ReservationExists(int id)
        {
            return _context.Reservations.Any(e => e.ReservationId == id);
        }
    }

    public class AvailabilityRequest
    {
        public int TableId { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public int Guests { get; set; }
    }
}