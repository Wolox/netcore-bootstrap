using Microsoft.AspNetCore.Mvc;

namespace NetCoreBootstrap.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseApiController : ControllerBase
    {
        internal JsonResult Json(object value) => new JsonResult(value);
    }
}
