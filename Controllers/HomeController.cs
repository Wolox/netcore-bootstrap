using Microsoft.AspNetCore.Mvc;
using MyMVCProject.Models.Database;
using MyMVCProject.Respositories;
using MyMVCProject.Mappers;

namespace MyMVCProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly DataBaseContext _context;
        private readonly UserRepository _userRepository;

        public HomeController(DataBaseContext context)
        {
            this._context = context;
            this._userRepository = new UserRepository(this._context);
        }

        [HttpGet("/TestGetUser")]
        public ActionResult TestGetUser()
        {
            return View(UserViewModelMapper.MapFrom(userRepository.GetByEmail("test@test.com")));
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
