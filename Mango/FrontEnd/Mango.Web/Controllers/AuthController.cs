using Mango.Web.Models.Dto;
using Mango.Web.Service.IService;
using Mango.Web.Utility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace Mango.Web.Controllers
{
    public class AuthController : Controller
    {

        private readonly IAuthService _authService;
        private readonly ITokenProvider _tokenProvider;
        protected ResponseDto _response;
        public AuthController(IAuthService authService, ITokenProvider tokenProvider)
        {
            _authService = authService;
            _tokenProvider = tokenProvider;
            _response = new();
        }
        [HttpGet]
        public IActionResult Login()
        {
            LoginRequestDto loginRequestDto = new();
            return View(loginRequestDto);
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginRequestDto obj)
        {
            ResponseDto responseDto = await _authService.LoginAsync(obj);

            if (responseDto != null && responseDto.IsSuccess)
            {
                LoginResponseDto loginResponseDto = 
                    JsonConvert.DeserializeObject<LoginResponseDto>(Convert.ToString(responseDto.Result));

                await SignInUser(loginResponseDto);
                _tokenProvider.SetToken(loginResponseDto.Token);
                // TempData["success"] = "Successfully Logged In !";
                return RedirectToAction("Index", "Home");
            }
            //else
            //{
            //    ModelState.AddModelError("CustomerError", responseDto.Message);
            //    return View(obj);
            //}
            else
            {
                TempData["error"] = responseDto.Message;
                return View(obj);
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            var roleList = new List<SelectListItem>()
            {
                new SelectListItem{Text=SD.RoleAdmin, Value=SD.RoleAdmin },
                new SelectListItem{Text=SD.RoleCustomer, Value=SD.RoleCustomer },
            };
            ViewBag.RoleList = roleList;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegistrationRequestDto obj)
        {
            ResponseDto result = await _authService.RegisterAsync(obj);
            ResponseDto assignRole;

            if (result!=null && result.IsSuccess)
            {
                if (string.IsNullOrEmpty(obj.Role))
                {
                    obj.Role = SD.RoleCustomer;
                }
                assignRole = await _authService.AssignRoleAsync(obj);
                if (assignRole!=null && assignRole.IsSuccess)
                {
                    TempData["success"] = "Successfully Registered !";
                    return RedirectToAction(nameof(Login));
                }
            }
            else
            {
                TempData["error"] = result.Message;
            }

            var roleList = new List<SelectListItem>()
            {
                new SelectListItem{Text=SD.RoleAdmin, Value=SD.RoleAdmin },
                new SelectListItem{Text=SD.RoleCustomer, Value=SD.RoleCustomer },
            };
            ViewBag.RoleList = roleList;
            return View(obj);
        }

        // [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            _tokenProvider.ClearToken();
            return RedirectToAction("Index", "Home");
        }

        private async Task SignInUser(LoginResponseDto model)
        {
            var handler = new JwtSecurityTokenHandler();

            var jwt = handler.ReadJwtToken(model.Token);

            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
            identity.AddClaim(new Claim(JwtRegisteredClaimNames.Email,
                jwt.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Email).Value));
            identity.AddClaim(new Claim(JwtRegisteredClaimNames.Sub,
                jwt.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Sub).Value));
            identity.AddClaim(new Claim(JwtRegisteredClaimNames.Name,
                jwt.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Name).Value));

            identity.AddClaim(new Claim(ClaimTypes.Email,
                jwt.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Email).Value));
            identity.AddClaim(new Claim(ClaimTypes.Role,                      //  after use of ClaimTypes.Role we can use role attribute on a controller
                jwt.Claims.FirstOrDefault(u => u.Type == "role").Value));     //  Extract role of user

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,principal);

        }
    }
}
