using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Interface;
using Shared.DTO;

namespace JobPortal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService userService;

        public UserController(IUserService userService)
        {
            this.userService = userService;
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetUserProfile()
        {
            var response = await userService.GetUserDetails();
            return Ok(response);
        }

        [HttpGet("[action]")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsersProfile()
        {
            var response = await userService.GetAllUserDetails();
            return Ok(response);
        }

        [HttpDelete("[action]")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser([FromBody] int userId)
        {
            var response = await userService.DeleteUser(userId);
            return Ok(response);
        }

        [HttpPut("[action]")]
        public async Task<IActionResult> UpdateUserProfile([FromBody]UpdateUserProfile userProfile)
        {
            var response = await userService.UpdateUserProfile(userProfile);
            return Ok(response);
        }
    }
}
