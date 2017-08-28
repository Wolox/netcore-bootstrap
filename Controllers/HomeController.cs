using Microsoft.AspNetCore.Mvc;
using NetCoreBootstrap.Models.Database;
using NetCoreBootstrap.Repositories;
using Microsoft.EntityFrameworkCore;

namespace NetCoreBootstrap.Controllers
{
    public class HomeController : Controller
    {
        private readonly DbContextOptions<DataBaseContext> _options;

        public HomeController(DbContextOptions<DataBaseContext> options)
        {
            this._options = options;
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
