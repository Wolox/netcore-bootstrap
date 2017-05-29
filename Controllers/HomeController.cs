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
        private readonly UserRepository _userRepository;

        [HttpGet("/TestUser")]
        public ActionResult TestUser()
        {
            var email = "test@user.com";
            var user = _userRepository.GetByEmail(email);
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
