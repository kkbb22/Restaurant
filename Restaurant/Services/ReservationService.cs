using Microsoft.EntityFrameworkCore;
using Restaurant.Data;
using Restaurant.Models;

namespace Restaurant.Services
{
    public class ReservationService
    {
        private readonly AppDbContext _context;

        public ReservationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsTableAvailable(int tableId, DateTime date, TimeSpan time, int numberOfGuests)
        {
            var table = await _context.RestaurantTables.FindAsync(tableId);
            if (table == null || table.Capacity < numberOfGuests)
                return false;

            var existingReservation = await _context.Reservations
                .AnyAsync(r => r.TableId == tableId &&
                               r.ReservationDate == date &&
                               r.ReservationTime == time &&
                               r.Status != "Cancelled");

            return !existingReservation;
        }

        public async Task<Reservation> CreateReservation(Reservation reservation)
        {
            var available = await IsTableAvailable(
                reservation.TableId,
                reservation.ReservationDate,
                reservation.ReservationTime,
                reservation.NumberOfGuests
            );

            if (!available)
                throw new Exception("الطاولة غير متوفرة في هذا الوقت");

            reservation.Status = "Confirmed";
            reservation.CreatedAt = DateTime.Now;

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            return reservation;
        }

        public async Task<bool> CancelReservation(int reservationId)
        {
            var reservation = await _context.Reservations.FindAsync(reservationId);
            if (reservation == null)
                return false;

            reservation.Status = "Cancelled";
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Reservation>> GetActiveReservations()
        {
            return await _context.Reservations
                .Include(r => r.Customer)
                .Include(r => r.Table)
                .Where(r => r.ReservationDate >= DateTime.Today && r.Status != "Cancelled")
                .OrderBy(r => r.ReservationDate)
                .ThenBy(r => r.ReservationTime)
                .ToListAsync();
        }

        public async Task<List<Reservation>> GetCustomerReservations(int customerId)
        {
            return await _context.Reservations
                .Where(r => r.CustomerId == customerId)
                .OrderByDescending(r => r.ReservationDate)
                .ToListAsync();
        }
    }
}