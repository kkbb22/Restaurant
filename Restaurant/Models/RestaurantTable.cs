using System.ComponentModel.DataAnnotations;

namespace Restaurant.Models
{
    public class RestaurantTable
    {
        [Key]
        public int TableId { get; set; }

        [Required]
        public int TableNumber { get; set; }

        [Required]
        public int Capacity { get; set; }

        public bool IsAvailable { get; set; } = true;
    }
}