using System.ComponentModel.DataAnnotations;

namespace Restaurant.Models
{
    public class RestaurantSettings
    {
        [Key]
        public int Id { get; set; }

        // ✅ ربط الإعدادات بمطعم محدد
        public int RestaurantId { get; set; } = 0;

        [Required]
        public string RestaurantName { get; set; } = string.Empty;

        public string? LogoUrl { get; set; }

        public string PrimaryColor { get; set; } = "#E63946";

        public string SecondaryColor { get; set; } = "#FF6B35";

        public string OpenTime { get; set; } = "10:00";

        public string CloseTime { get; set; } = "23:00";

        public string WorkingDays { get; set; } = "السبت,الأحد,الاثنين,الثلاثاء,الأربعاء,الخميس";

        public bool AcceptOrders { get; set; } = true;

        public string ClosedMessage { get; set; } = "المطعم مغلق حالياً";

        public bool PayCash { get; set; } = true;
        public bool PayCard { get; set; } = false;
        public bool PayOnline { get; set; } = false;

        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? WhatsApp { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
