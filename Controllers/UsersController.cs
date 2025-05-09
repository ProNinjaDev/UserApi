using Microsoft.AspNetCore.Mvc;
using UserApi.Models;
using UserApi.Services;
using UserApi.DTOs;

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
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetAllUsers() {
            var users = await _userService.GetAllUsersAsync();

            var userResponseDtos = users.Select(user => new UserResponseDto {
                Guid = user.Guid,
                Login = user.Login,
                Name = user.Name,
                Gender = user.Gender,
                Birthday = user.Birthday,
                Admin = user.Admin,
                CreatedOn = user.CreatedOn,
                IsActive = user.RevokedOn == null
            });
            return Ok(users);
        }

        // GET: api/users/{login}
        [HttpGet("{login}")] 
        public async Task<ActionResult<UserResponseDto>> GetUserByLogin([FromRoute] string login) {
            var user = await _userService.GetUserByLoginAsync(login);
            
            if (user == null) {
                return NotFound();
            }

            var userResponseDto = new UserResponseDto {
                Guid = user.Guid,
                Login = user.Login,
                Name = user.Name,
                Gender = user.Gender,
                Birthday = user.Birthday,
                Admin = user.Admin,
                CreatedOn = user.CreatedOn,
                IsActive = user.RevokedOn == null
            };
            return Ok(userResponseDto);
        }

        // POST: api/users
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequestDto createUserDto) {
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            var newCreatedUser = await _userService.CreateUserAsync(createUserDto, "Admin"); // todo: возможно CreatedBy нужен другой

            if (newCreatedUser == null) {
                return BadRequest("User with this login already exists");
            }

            var userResponseDto = new UserResponseDto {
                Guid = newCreatedUser.Guid,
                Login = newCreatedUser.Login,
                Name = newCreatedUser.Name,
                Gender = newCreatedUser.Gender,
                Birthday = newCreatedUser.Birthday,
                Admin = newCreatedUser.Admin,
                CreatedOn = newCreatedUser.CreatedOn,
                IsActive = newCreatedUser.RevokedOn == null
            };

            // 201 Created и ссылочку на созданный ресурс и сам ресурс
            return CreatedAtAction(nameof(GetUserByLogin), new { login = userResponseDto.Login }, userResponseDto); 
        }
    }
}