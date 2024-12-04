using Microsoft.AspNetCore.Mvc;

namespace Lab5.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using Microsoft.IdentityModel.Tokens;

    public class AuthentificationController : Controller
    {
        private static readonly List<Person> People = new()
        {
            new Person { Username = "admin", Password = "admin" },
            new Person { Username = "user2@example.com", Password = "password2" }
        };

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
            var person = People.FirstOrDefault(p => p.Username == username && p.Password == password);
            if (person is null)
            {
                ViewData["ErrorMessage"] = "Invalid email or password";
                return View("Login");
            }

            // Создаем список утверждений (claims)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, person.Username),
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

            ViewData["Username"] = username;

            return RedirectToAction("Welcome");
        }

        public IActionResult Welcome()
        {
            return View();
        }

    }

    public class Person
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

}
