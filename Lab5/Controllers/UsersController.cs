using Microsoft.AspNetCore.Mvc;
using Kursach.Domain.Entities;
using Kursach.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using AutoMapper;
using Kursach.Application.Dtos;
using Kursach.Domain.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Kursach.Domain.Utilities;
using Kursach.Infrastructure.Repositories;
using Kursach.Domain.Filters;

namespace Lab5.Controllers
{
    public class UsersController(IUserRepository repository, IMapper mapper) : Controller
    {
        private readonly IUserRepository _repository = repository;
        private readonly IMapper _mapper = mapper;

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(UserFilter filter, int pageIndex = 1, int pageSize = 20)
        {
            var users = await _repository.GetFilteredPageAsync(pageIndex, pageSize, filter, true);
            //var usersDtoList = new List<UserDto>(); 
            //users.ForEach(x => usersDtoList.Add(_mapper.Map<UserDto>(x)));
            //var usersDtoPage = new PaginatedList<UserDto>(usersDtoList, pageIndex, usersDtoList.Count, pageSize);
            return View(users);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewBag.Roles = new List<string> { "User", "Admin"};

            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(UserForCreationDto userDto)
        {
            if (!ModelState.IsValid)
            {
                return View(userDto);
            }

            if (_repository.Get(false).Result.Any(u => u.Username == userDto.Username))
            {
                ModelState.AddModelError("Username", "Имя пользователя уже занято");
                return View(userDto);
            }

            var hashedPassword = PasswordHelper.HashPassword(userDto.Password);

            var newUser = new User()
            {
                Username = userDto.Username,
                PasswordHash = hashedPassword.hash,
                PasswordSalt = hashedPassword.salt,
                Role = userDto.Role,
                CreatedAt = DateTime.Now
            };

            await _repository.Create(newUser);
            await _repository.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            ViewBag.Roles = new List<string> { "User", "Admin" };

            var user = await _repository.GetById(id, true);
            if (user == null)
            {
                return NotFound();
            }


            return View(_mapper.Map<UserForUpdateDto>(user));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(UserForUpdateDto userDto)
        {
            bool updPass = false;

            if (userDto.Password == null && userDto.ConfirmPassword == null)
            {
                ModelState["Password"].ValidationState = Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid;
                ModelState["ConfirmPassword"].ValidationState = Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid;
            }
            else
            {
                updPass = true;
            }

            if (!ModelState.IsValid)
            {
                return View("Edit", userDto);
            }

            if (_repository.Get(false).Result.Any(u => u.Username == userDto.Username && u.Id != userDto.Id))
            {
                ModelState.AddModelError("Username", "Имя пользователя уже занято");
                return View(userDto.Id);
            }

            var user = await _repository.GetById(userDto.Id, false);
            user.Username = userDto.Username;
            user.Role = userDto.Role;
            if(updPass)
            {
                var passHash = PasswordHelper.HashPassword(userDto.Password);
                user.PasswordSalt = passHash.salt;
                user.PasswordHash = passHash.hash;
            }

            _repository.Update(user);
            await _repository.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _repository.GetById(id, false);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            _repository.Delete(id);
            await _repository.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
