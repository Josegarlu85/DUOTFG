using Microsoft.AspNetCore.Mvc;

namespace DuoCareAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("API funcionando 🚀");
        }
    }
}