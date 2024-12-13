using Kursach.Domain.Abstractions;
using Kursach.Application;
using Microsoft.AspNetCore.Mvc;
using Kursach.Application.Dtos;
using Kursach.Domain.Utilities;
using Kursach.Domain.Entities;

namespace Lab5.Controllers
{
    public class RegistrationController : Controller
    {
        private IUserRepository _userRepository;

        public RegistrationController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(UserForCreationDto user)
        {
            if(!ModelState.IsValid)
            {
                return View(user);
            }

            if(_userRepository.Get(false).Result.Any(u => u.Username == user.Username))
            {
                ModelState.AddModelError("Username", "Имя пользователя уже занято");
                return View(user);
            }

            var hashedPassword = PasswordHelper.HashPassword(user.Password);

            var newUser = new User()
            {
                Username = user.Username,
                PasswordHash = hashedPassword.hash,
                PasswordSalt = hashedPassword.salt,
                Role = "User",
                CreatedAt = DateTime.Now
            };

            await _userRepository.Create(newUser);
            await _userRepository.SaveChangesAsync();

            return RedirectToAction("SuccessRegistration");
        }

        public IActionResult SuccessRegistration()
        {
            return View();
        }
    }
}
