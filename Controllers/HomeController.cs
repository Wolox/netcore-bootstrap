using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyMVCProject.Models.Database;
using MyMVCProject.Respositories;
using MyMVCProject.Mappers;

namespace MyMVCProject.Controllers
{
    public class HomeController : Controller
    {
        private DataBaseContext context;
        private UserRepository userRepository;

        public HomeController(DataBaseContext context)
        {
            this.context = context;
            this.userRepository = new UserRepository(this.context);
        }

        [HttpGet("/TestGetUser")]
        public ActionResult TestGetUser()
        {
            var email = "test@test.com";
            var user = userRepository.GetByEmail(email);
            var userViewModel = UserViewModelMapper.MapFrom(user);
            return View(userViewModel);
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("About")]
        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        [HttpGet("Contact")]
        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        [HttpGet("Error")]
        public IActionResult Error()
        {
            return View();
        }
    }
}
