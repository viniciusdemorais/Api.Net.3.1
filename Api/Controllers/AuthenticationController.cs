using Core.Contract.Bll;
using Core.Model.DTO.Request;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthenticationController : Controller
    {
        private readonly IAuthenticationBll _authenticationBll;
        public AuthenticationController(IAuthenticationBll authenticationBll)
        {
			_authenticationBll = authenticationBll;
        }
		[HttpPost]
		public IActionResult GenerateToken([FromHeader(Name = "client-id")][Required] string clientId,
										   [FromHeader(Name = "client-secret")][Required] string clientSecret)
		{
			var received = _authenticationBll.GenerateBearerToken(new BearerRequestDTO
			{
				ClientId = clientId,
				ClientSecret = clientSecret
			});
			if (received.Success)
			{
				return Ok(received.Data);
			}
			else
			{
				return BadRequest(received.Message);
			}
		}
	}
}