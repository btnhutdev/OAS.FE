using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Utilities.ViewModel;
using Utilities.Constant;

namespace Admin.Controllers
{
    [AllowAnonymous]
    public class AuthenController : Controller
    {
        #region global variables
        private readonly HttpClient _httpAuthenClient;
        private readonly IConfiguration _configuration;
        #endregion

        #region Constructor
        public AuthenController(IConfiguration configuration)
        {
            _configuration = configuration;
            _httpAuthenClient = new HttpClient();
            _httpAuthenClient.BaseAddress = new Uri(_configuration["ApiGetwayBaseAddress"]);
        }
        #endregion

        #region Index
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
        #endregion

        #region Index - Post
        [HttpPost]
        public async Task<IActionResult> Index(LoginViewModel user)
        {
            var response = await _httpAuthenClient.PostAsJsonAsync("Authen/Admin", user);
            if (response.IsSuccessStatusCode)
            {
                var token = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (string.IsNullOrEmpty(token))
                {
                    TempData[TempDataNameConst.ErrorMessage] = "Username or Password incorrect";
                    return View(user);
                }

                ClaimsPrincipal userPrincipal = this.ValidateToken(token);
                var authProperties = new AuthenticationProperties
                {
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
                };

                await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            userPrincipal,
                            authProperties).ConfigureAwait(false);

                return RedirectToAction("Index", "Product");
            }
            else
            {
                TempData[TempDataNameConst.ErrorMessage] = "Username or Password incorrect";
                return View(user);
            }
        }
        #endregion

        #region LogOut
        [HttpGet]
        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).ConfigureAwait(false);
            return RedirectToAction("Index", "Authen");
        }
        #endregion

        #region ValidateToken
        private ClaimsPrincipal ValidateToken(string jwtToken)
        {
            SecurityToken validatedToken;
            TokenValidationParameters validationParameters = new TokenValidationParameters();

            validationParameters.ValidateLifetime = true;

            validationParameters.ValidAudience = _configuration["JwtToken:Audience"];
            validationParameters.ValidIssuer = _configuration["JwtToken:Issuer"];
            validationParameters.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtToken:Key"]));

            ClaimsPrincipal principal = new JwtSecurityTokenHandler().ValidateToken(jwtToken, validationParameters, out validatedToken);

            return principal;
        }
        #endregion
    }
}
