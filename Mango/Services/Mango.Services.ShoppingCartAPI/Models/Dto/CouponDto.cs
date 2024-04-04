namespace Mango.Services.ShoppingCartAPI.Models.Dto
{
    public class CouponDto
    {
        public int CouponId { get; set; }
        public string CouponCode { get; set; }
        public string DiscountAmount { get; set; }
        public int MinAmount { get; set; }
    }
}
