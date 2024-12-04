using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Kursach.Infrastructure;
using Kursach.Infrastructure.Repositories;
using Kursach.Domain.Abstractions;
using Microsoft.AspNetCore.Authorization;

namespace Lab5.Controllers
{
    public class ClientsController : Controller
    {
        private readonly IClientRepository _repository;

        public ClientsController(IClientRepository clientRepository)
        {
            _repository = clientRepository;
        }

        // GET: ClientsController
        public ActionResult Index()
        {
            return View();
        }
    }
}
