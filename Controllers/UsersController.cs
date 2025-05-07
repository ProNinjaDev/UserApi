using Microsoft.AspNetCore.Mvc;
using UserApi.Models;
using UserApi.Services;

namespace UserApi.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase {
        private readonly IUserService _userService;

        public UsersController(IUserService userService) {
            _userService = userService;
        }

        // GET: api/users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsers() {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        // GET: api/users/{login}
        [HttpGet("{login}")] 
        public async Task<ActionResult<User>> GetUserByLogin([FromRoute] string login) {
            var user = await _userService.GetUserByLoginAsync(login);
            
            if (user == null) {
                return NotFound();
            }
            return Ok(user);
        }
    }
}