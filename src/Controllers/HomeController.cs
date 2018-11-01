using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetCoreBootstrap.Models.Database;
using NetCoreBootstrap.Repositories;
using NetCoreBootstrap.Repositories.Database;

namespace NetCoreBootstrap.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHtmlLocalizer<HomeController> _localizer;
        private readonly ILogger<HomeController> _log;

        public HomeController(DbContextOptions<DataBaseContext> options, IHtmlLocalizer<HomeController> localizer, ILogger<HomeController> log)
        {
            this._localizer = localizer;
            this._log = log;
        }

        public IHtmlLocalizer<HomeController> Localizer { get => this._localizer; }

        public ILogger<HomeController> Log { get => this._log; }

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
    }
}
