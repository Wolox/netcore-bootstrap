using Microsoft.AspNetCore.Mvc;

namespace NetcoreBootstrap.Controllers.api.v1
{
    [Route("api/v1/[controller]")]
    public class TestController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return Json("Hello World!");
        }
    }
}