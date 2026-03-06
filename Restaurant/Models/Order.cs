using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Restaurant.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        public int? ReservationId { get; set; }

        [Required]
        public int CustomerId { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;

        public string Status { get; set; } = "Placed";

        public decimal TotalAmount { get; set; } = 0;

        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }

        [ForeignKey("ReservationId")]
        public virtual Reservation? Reservation { get; set; }

        public virtual ICollection<OrderItem>? OrderItems { get; set; }
    }
}