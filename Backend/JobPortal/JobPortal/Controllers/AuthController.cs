using Microsoft.AspNetCore.Mvc;
using Service.DTO;
using Service.Interface;

namespace JobPortal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService authService;

        public AuthController(IAuthService authService)
        {
            this.authService = authService;
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Register([FromBody] UserDTO userDTO)
        {
            var response = await authService.CreateUser(userDTO);
            return Ok(response);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
        {
            var response = await authService.Login(loginDTO);
            return Ok(response);
        }
    }
}
