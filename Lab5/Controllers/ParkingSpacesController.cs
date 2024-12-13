using AutoMapper;
using Kursach.Application.Dtos;
using Kursach.Domain.Abstractions;
using Kursach.Domain.Entities;
using Kursach.Domain.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Lab5.Controllers
{
    public class ParkingSpacesController(IParkingSpaceRepository repository, ICarRepository carRepository, IMapper mapper) : Controller
    {
        private readonly IParkingSpaceRepository _repository = repository;
        private readonly ICarRepository _carRepository = carRepository;
        private readonly IMapper _mapper = mapper;

        public async Task<IActionResult> Index(ParkingSpaceFilter filter = default, int pageIndex = 1, int pageSize = 20)
        {
            var items = await _repository.GetFilteredPageAsync(pageIndex, pageSize, filter, true);

            var cars = await _carRepository.Get(false);

            ViewBag.Cars = new List<SelectListItem>() { new SelectListItem("Свободно", null) };
            ViewBag.Cars.AddRange(cars.Select(car => new SelectListItem
            {
                Value = car.Id.ToString(),
                Text = $"{car.Brand} {car.Number}"
            }).ToList());

            return View((items, filter));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            var cars = await _carRepository.Get(false);

            ViewBag.Cars = new List<SelectListItem>() { new SelectListItem("Свободно", null) };
            ViewBag.Cars.AddRange(cars.Select(car => new SelectListItem
            {
                Value = car.Id.ToString(),
                Text = $"{car.Brand} {car.Number}"
            }).ToList());

            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(ParkingSpaceForCreationDto parkingSpacedto)
        {
            var entity = _mapper.Map<ParkingSpace>(parkingSpacedto);
            await _repository.Create(entity);
            await _repository.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var parkingSpace = await _repository.GetById(id, true);
            if (parkingSpace == null)
            {
                return NotFound();
            }

            var cars = await _carRepository.Get(false);

            ViewBag.Cars = new List<SelectListItem>() { new SelectListItem("Свободно", null, parkingSpace.Car == null) };
            ViewBag.Cars.AddRange(cars.Select(car => new SelectListItem
            {
                Value = car.Id.ToString(),
                Text = $"{car.Brand} {car.Number}",
                Selected = car.Id == parkingSpace.Id
            }).ToList());

            return View(_mapper.Map<ParkingSpaceForUpdateDto>(parkingSpace));
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Edit(ParkingSpaceForUpdateDto parkingSpacedto)
        {
            var entity = _mapper.Map<ParkingSpace>(parkingSpacedto);
            _repository.Update(entity);
            await _repository.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var parkingSpace = await _repository.GetById(id, false);
            if (parkingSpace == null)
            {
                return NotFound();
            }

            return View(parkingSpace);
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
