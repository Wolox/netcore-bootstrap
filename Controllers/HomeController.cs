using Microsoft.AspNetCore.Mvc;
using NetCoreBootstrap.Models.Database;
using NetCoreBootstrap.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Localization;

namespace NetCoreBootstrap.Controllers
{
    public class HomeController : Controller
    {
        private readonly DbContextOptions<DataBaseContext> _options;
        private readonly IHtmlLocalizer<HomeController> _localizer;

        public HomeController(DbContextOptions<DataBaseContext> options, IHtmlLocalizer<HomeController> localizer)
        {
            this._options = options;
            this._localizer = localizer;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("About")]
        public IActionResult About()
        {
            ViewData["Message"] = Localizer["DescriptionPage"];
            return View();
        }

        [HttpGet("Contact")]
        public IActionResult Contact()
        {
            ViewData["Message"] = Localizer["ContactPage"];
            return View();
        }

        [HttpGet("Error")]
        public IActionResult Error()
        {
            return View();
        }

        public IHtmlLocalizer<HomeController> Localizer
        {
            get { return this._localizer; }
        }
    }
}
