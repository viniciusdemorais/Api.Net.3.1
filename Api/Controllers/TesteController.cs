using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize("Bearer")]
    public class TesteController : Controller
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Teste OK");
        }
    }
}