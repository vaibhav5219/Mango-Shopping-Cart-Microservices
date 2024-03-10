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
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public AuthService(AppDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IJwtTokenGenerator jwtTokenGenerator)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<bool> AssignRole(string email, string roleName)
        {
            var user = _db.ApplicationUsers.FirstOrDefault(u => u.Email.ToLower() == email.ToLower());
            if(user != null)
            {
                if (!_roleManager.RoleExistsAsync(roleName).GetAwaiter().GetResult()) // GetAwaiter().GetResult() makes asynchronous
                {
                    // Create role if it does not exists
                    _roleManager.CreateAsync(new IdentityRole(roleName)).GetAwaiter().GetResult();
                }
                await _userManager.AddToRoleAsync(user, roleName);
                return true;
            }
            return false;
        }

        public async Task<LoginResponseDto> Login(LoginRequestDto loginRequestDto)
        {
            var user = _db.ApplicationUsers.FirstOrDefault(u => u.Email.ToLower() == loginRequestDto.Email.ToLower());

            bool isValid = await _userManager.CheckPasswordAsync(user, loginRequestDto.Password);
            if (user==null || isValid == false)
            {
                return new LoginResponseDto(){ User = null, Token = "" };
            }

            // Get Roles
            var roles = await _userManager.GetRolesAsync(user);

            // Generate token
            var token = _jwtTokenGenerator.GenerateToken(user, roles);

            UserDto userDto = new()
            {
                Email = user.Email,
                ID = user.Id,
                Name = user.Name,
                PhoneNumber = user.PhoneNumber
            };
            LoginResponseDto loginResponseDto = new()
            {
                User = userDto,
                Token = token
            };
            return loginResponseDto;
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
                    var userToReturn = _db.ApplicationUsers.First(u => u.Email == registrationRequestDto.Email);

                    UserDto userDto = new()
                    {
                        Email = userToReturn.Email,
                        ID = userToReturn.Id,
                        Name = userToReturn.Name,
                        PhoneNumber = userToReturn.PhoneNumber

                    };
                    return string.Empty; // return userDto;
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
