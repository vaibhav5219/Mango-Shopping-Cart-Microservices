using System.ComponentModel.DataAnnotations;

namespace Mango.Services.CouponAPI.Models
{
    public class Coupon
    {
        [Required]
        public int CouponId {  get; set; }
        [Required]
        public string CouponCode { get; set; }
        [Required]
        public string DiscountAmount { get; set; }
        public int MinAmount { get; set; }
        
    }
}
