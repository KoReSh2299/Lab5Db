using Microsoft.AspNetCore.Mvc;

namespace Lab5.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using Microsoft.IdentityModel.Tokens;
    using Kursach.Domain.Abstractions;
    using Kursach.Domain.Utilities;

    public class AuthentificationController : Controller
    {
        private IUserRepository _userRepository;

        public AuthentificationController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Response.Cookies.Delete("jwtToken");
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public IActionResult Authenticate(string username, string password)
        {
            // Проверяем пользователя
            var user = _userRepository.GetByUsername(username, true).Result;
            if (user is null || !PasswordHelper.VerifyPassword(password, user.PasswordHash, user.PasswordSalt))
            {
                ViewData["ErrorMessage"] = "Неверное имя пользователя или пароль";
                return View("Login");
            }

            // Создаем список утверждений (claims)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
            };

            // Создаем JWT токен
            var jwt = new JwtSecurityToken(
                issuer: AuthOptions.ISSUER,
                audience: AuthOptions.AUDIENCE,
                claims: claims,
                expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(30)),
                signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            HttpContext.Response.Cookies.Append("jwtToken", encodedJwt, new CookieOptions()
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Strict,
                Secure = true
            });

            return RedirectToAction("Welcome");
        }

        public IActionResult Welcome()
        {
            return View();
        }

    }
}
