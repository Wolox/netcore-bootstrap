using Microsoft.AspNetCore.Mvc;
using NetCoreBootstrap.Models.Database;
using NetCoreBootstrap.Respositories;
using NetCoreBootstrap.Mappers;
using Microsoft.EntityFrameworkCore;

namespace NetCoreBootstrap.Controllers
{
    public class HomeController : Controller
    {
        private readonly DbContextOptions<DataBaseContext> _options;
        private readonly UserRepository _userRepository;

        public HomeController(DbContextOptions<DataBaseContext> options)
        {
            this._options = options;
            this._userRepository = new UserRepository(_options);
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

        public UserRepository userRepository
        {
            get {return _userRepository;}
        }
    }
}
