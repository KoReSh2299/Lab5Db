using Microsoft.AspNetCore.Mvc;
using Kursach.Domain.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Kursach.Domain.Entities;
using Kursach.Application.Dtos;
using AutoMapper;
using Kursach.Domain.Filters;

namespace Lab5.Controllers
{
    public class ClientsController(IClientRepository clientRepository, IMapper mapper) : Controller
    {
        private readonly IClientRepository _repository = clientRepository;
        private readonly IMapper _mapper = mapper;

        [Authorize]
        public async Task<IActionResult> Index(ClientFilter filter = default, int pageIndex = 1, int pageSize = 20)
        {
            var items = await _repository.GetFilteredPageAsync(pageIndex, pageSize, filter, false);
            return View((items, filter));
        }

        // Создание нового клиента (GET)
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create(ClientForCreationDto client)
        {
            var entity = _mapper.Map<Client>(client);
            await _repository.Create(entity);
            await _repository.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Редактирование клиента (GET)
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var client = await _repository.GetById(id, true);
            if (client == null)
            {
                return NotFound();
            }
            return View(_mapper.Map<ClientForUpdateDto>(client));
        }

        // Редактирование клиента (POST)
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Edit(ClientForUpdateDto client)
        {
            var entity = _mapper.Map<Client>(client);
            _repository.Update(entity);
            await _repository.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Удаление клиента (GET)
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var client = await _repository.GetById(id, false);
            if (client == null)
            {
                return NotFound();
            }
            return View(client);
        }

        // Удаление клиента (POST)
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            _repository.Delete(id);
            await _repository.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
