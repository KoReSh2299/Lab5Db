using AutoMapper;
using Kursach.Application.Dtos;
using Kursach.Domain.Abstractions;
using Kursach.Domain.Entities;
using Kursach.Domain.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Lab5.Controllers;

public class CarsController(ICarRepository carRepository, IMapper mapper, IClientRepository clientRepository) : Controller
{
    private readonly ICarRepository _repository = carRepository;
    private readonly IClientRepository _clientRepository = clientRepository;
    private readonly IMapper _mapper = mapper;

    [Authorize]
    public async Task<IActionResult> Index(CarFilter filter = default, int pageIndex = 1, int pageSize = 20)
    {
        var items = await _repository.GetFilteredPageAsync(pageIndex, pageSize, filter, false);
        return View(items);
    }

    // Создание нового автомобиля (GET)
    [Authorize]
    public async Task<IActionResult> Create()
    {
        var clients = await _clientRepository.Get(false);

        ViewBag.Clients = clients.Select(x => new SelectListItem
        {
            Value = x.Id.ToString(),
            Text = $"{x.Surname} {x.Name} {x.MiddleName}"
        }).ToList();
        
        return View();
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(CarForCreationDto car)
    {
        var entity = _mapper.Map<Car>(car);
        await _repository.Create(entity);
        await _repository.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // Редактирование автомобиля (GET)
    [Authorize]
    public async Task<IActionResult> Edit(int id)
    {
        var car = await _repository.GetById(id, true);
        if (car == null)
        {
            return NotFound();
        }

        var clients = await _clientRepository.Get(false);

        ViewBag.Clients = clients.Select(x => new SelectListItem
        {
            Value = x.Id.ToString(),
            Text = $"{x.Surname} {x.Name} {x.MiddleName}"
        }).ToList();

        return View(_mapper.Map<CarForUpdateDto>(car));
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Edit(CarForUpdateDto car)
    {
        var entity = _mapper.Map<Car>(car);
        _repository.Update(entity);
        await _repository.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // Удаление автомобиля (GET)
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var car = await _repository.GetById(id, false);
        if (car == null)
        {
            return NotFound();
        }
        return View(car);
    }

    // Удаление автомобиля (POST)
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        _repository.Delete(id);
        await _repository.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
