using Mango.Services.EmailAPI.Models.Dto;

namespace Mango.Services.EmailAPI.Services.IService
{
    public interface IEmailService
    {
        Task EmailCartAndLog(CartDto cartDto);
    }
}
