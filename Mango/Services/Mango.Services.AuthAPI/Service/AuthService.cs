using Mango.Services.AuthAPI.Service.IService;
using Mango.Services.AuthAPI.Models.Dto;
using Mango.Services.AuthAPI.Data;
using Microsoft.AspNetCore.Identity;
using Mango.Services.AuthAPI.Models;

namespace Mango.Services.AuthAPI.Service
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AuthService(AppDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public Task<UserDto> Login(LoginRequestDto loginRequestDto)
        {
            throw new NotImplementedException();
        }

        public async Task<string> Register(RegistrationRequestDto registrationRequestDto)
        {
            ApplicationUser user = new()
            {
                UserName = registrationRequestDto.Name,
                Email = registrationRequestDto.Email,
                NormalizedEmail = registrationRequestDto.Email.ToLower(),
                Name = registrationRequestDto.Name,
                PhoneNumber = registrationRequestDto.PhoneNumber
            }; 

            try
            {
                var result = await _userManager.CreateAsync(user, registrationRequestDto.Password);
                if (result.Succeeded)
                {
                    var userToReturn = _db.ApplicationUsers.First(u => u.UserName == registrationRequestDto.Email);

                    UserDto userDto = new()
                    {
                        Email = userToReturn.Email,
                        ID = Convert.ToInt32(userToReturn.Id),
                        Name = userToReturn.Name,
                        PhoneNumber = userToReturn.PhoneNumber

                    };
                    return string.Empty;
                }
                else
                {
                    return result.Errors.FirstOrDefault().Description;
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "Error Encountered";
        }
    }
}
