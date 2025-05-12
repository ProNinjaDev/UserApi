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

            if (users == null) {
                return Ok(Enumerable.Empty<UserResponseDto>());
            }

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
            return Ok(userResponseDtos);
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

        // PUT: api/users/{login} 
        [HttpPut("{login}")]
        public async Task<IActionResult> UpdateUserInfo([FromRoute] string login,
            [FromBody] UpdateUserInfoRequestDto updateUserDto) {

            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            var currentUserLogin = "Admin"; // TODO: заменить при аутентификации
            var userExisting = await _userService.GetUserByLoginAsync(login);
            if(userExisting == null) {
                return NotFound("User not found");
            }

            // todo: добавить проверки при аутентификации
            var userUpdated = await _userService.UpdateUserInfoAsync(login, updateUserDto, currentUserLogin);

            return NoContent();
        }

        // PUT: api/users/{login}/password
        [HttpPut("{login}/password")]
        public async Task<IActionResult> UpdateUserPassword([FromRoute] string login,
            [FromBody] UpdateUserPasswordRequestDto updatePasswordDto) {
            
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            var currentUserLogin = "Admin"; // TODO: заменить после аутентификации

            var userExisting = await _userService.GetUserByLoginAsync(login);
            if(userExisting == null) {
                return NotFound("User not found");
            }

            var userUpdated = await _userService.UpdateUserPasswordAsync(login, updatePasswordDto.NewPassword, currentUserLogin);
            return NoContent();
        }

        // PUT: api/users/{login}/login
        [HttpPut("{login}/login")]
        public async Task<IActionResult> UpdateUserLogin([FromRoute] string login,
            [FromBody] UpdateUserLoginRequestDto updateLoginDto) {
            
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            var currentUserLogin = "Admin"; // TODO: заменить после аутентификации
            var userExisting = await _userService.GetUserByLoginAsync(login);
            if(userExisting == null) {
                return NotFound("User not found");
            }

            var userUpdated = await _userService.UpdateUserLoginAsync(login, updateLoginDto.NewLogin, currentUserLogin);
            if(userUpdated == null) {
                var checkNewLogin = await _userService.GetUserByLoginAsync(updateLoginDto.NewLogin);
                if(checkNewLogin != null && !string.Equals(login, updateLoginDto.NewLogin, StringComparison.OrdinalIgnoreCase)) {
                    return Conflict($"The new login '{updateLoginDto.NewLogin}' is already taken"); // 409
                }
                
                return BadRequest($"Failed to update login for {login}");
            }

            return NoContent();
        }
    }
}