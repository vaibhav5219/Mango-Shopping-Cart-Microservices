using Mango.Services.ShoppingCartAPI.Models.Dto;

namespace Mango.Services.ShoppingCartAPI.Service.IService
{
    public interface ICouponService
    {
        public Task<CouponDto> GetCoupon(string couponCode);
    }
}
