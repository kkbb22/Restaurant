using System.ComponentModel.DataAnnotations;

namespace Restaurant.Models
{
    public class RestaurantProfile 
    {
        [Key]
        public int RestaurantId { get; set; } // تأكدنا من الاسم هنا

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Username { get; set; } = string.Empty; // أنت تستخدم يوزر نيم

        [Required]
        public string Password { get; set; } = string.Empty;

        public string? LogoUrl { get; set; }
        public string PrimaryColor { get; set; } = "#E63946";
        public string SecondaryColor { get; set; } = "#FF6B35";
        public string OpenTime { get; set; } = "10:00";
        public string CloseTime { get; set; } = "23:00";
        public bool AcceptOrders { get; set; } = true;
        public bool IsActive { get; set; } = true;
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}