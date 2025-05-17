using Microsoft.AspNetCore.Mvc;
using UserApi.Models;
using UserApi.Services;
using UserApi.DTOs;
using UserApi.Exceptions;
using Microsoft.AspNetCore.Authorization;

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
        [Authorize(Roles = "Admin")] // Изменено с [Authorize] на [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();

            if (users == null)
            {
                return Ok(Enumerable.Empty<UserResponseDto>());
            }

            var userResponseDtos = users.Select(user => MapUserToResponseDto(user));
            return Ok(userResponseDtos);
        }

        // GET: api/users/{login}
        [HttpGet("{login}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserResponseDto>> GetUserByLogin([FromRoute] string login) {
            var user = await _userService.GetUserByLoginAsync(login);
            
            if (user == null) {
                return NotFound();
            }

            return Ok(MapUserToResponseDto(user));
        }

        // POST: api/users
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequestDto createUserDto) {
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            try {
                var newCreatedUser = await _userService.CreateUserAsync(createUserDto, "Admin"); // TODO: возможно CreatedBy нужен другой

                var userResponseDto = MapUserToResponseDto(newCreatedUser);

                // 201 Created и ссылочку на созданный ресурс и сам ресурс
                return CreatedAtAction(nameof(GetUserByLogin), new { login = userResponseDto.Login }, userResponseDto);
            }
            catch (DuplicateLoginException ex) {
                return Conflict(new { message = ex.Message });
            }
        }

        // PUT: api/users/{login} 
        [HttpPut("{login}")]
        [Authorize]
        public async Task<IActionResult> UpdateUserInfo([FromRoute] string login,
            [FromBody] UpdateUserInfoRequestDto updateUserDto) {

            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            var currentLoggedInUser = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(currentLoggedInUser))
            {
                return Unauthorized(new { message = "Cannot identify current user from token" });
            }

            if (!User.IsInRole("Admin") && !string.Equals(currentLoggedInUser, login, StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }

            try {
                // var userExisting = await _userService.GetUserByLoginAsync(login);

                var userUpdated = await _userService.UpdateUserInfoAsync(login, updateUserDto, currentLoggedInUser);

                return NoContent();
            }
            catch (UserNotFoundException ex) {
                return NotFound(new { message = ex.Message });
            }
        }

        // PUT: api/users/{login}/password
        [HttpPut("{login}/password")]
        [Authorize]
        public async Task<IActionResult> UpdateUserPassword([FromRoute] string login,
            [FromBody] UpdateUserPasswordRequestDto updatePasswordDto) {
            
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }
            
            var currentLoggedInUser = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(currentLoggedInUser))
            {
                return Unauthorized(new { message = "Cannot identify current user from token" });
            }

            if (!User.IsInRole("Admin") && !string.Equals(currentLoggedInUser, login, StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }

            try
            {
                var userUpdated = await _userService.UpdateUserPasswordAsync(login, updatePasswordDto.NewPassword, currentLoggedInUser);
                return NoContent();
            }
            catch (UserNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // PUT: api/users/{login}/login
        [HttpPut("{login}/login")]
        [Authorize]
        public async Task<IActionResult> UpdateUserLogin([FromRoute] string login,
            [FromBody] UpdateUserLoginRequestDto updateLoginDto) {
            
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }
            
            var currentLoggedInUser = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(currentLoggedInUser))
            {
                return Unauthorized(new { message = "Cannot identify current user from token" });
            }

            if (!User.IsInRole("Admin") && !string.Equals(currentLoggedInUser, login, StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }

            try
            {
                var userUpdated = await _userService.UpdateUserLoginAsync(login, updateLoginDto.NewLogin, currentLoggedInUser);
                return NoContent();
            }
            catch (UserNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (DuplicateLoginException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        // DELETE: api/users/{login}?type={soft|hard} 
        [HttpDelete("{login}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser([FromRoute] string login, [FromQuery] string type = "soft") {
            try {
                string deletedByLogin = "Admin"; // TODO: изменить на логин аутентифицированного админа
                if ("hard".Equals(type, StringComparison.OrdinalIgnoreCase)) {
                    await _userService.HardDeleteUserAsync(login, deletedByLogin);
                }
                else {
                    await _userService.SoftDeleteUserAsync(login, deletedByLogin);
                }
                return NoContent();
            }
            catch (UserNotFoundException ex) {
                return NotFound(new { message = ex.Message });
            }
        }

        // PUT: api/users/{login}/restore
        [HttpPut("{login}/restore")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RestoreUser([FromRoute] string login) {
            try {
                string modifiedByLogin = "Admin"; // TODO: изменить на логин аутентифицированного админа
                var restoredUser = await _userService.RestoreUserAsync(login, modifiedByLogin);

                return Ok(MapUserToResponseDto(restoredUser));
            }
            catch (UserNotFoundException ex) {
                return NotFound(new {message = ex.Message});
            }
        }

        // GET: api/users/older-than/{age}
        [HttpGet("older-than/{age}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetUsersOlderThan([FromRoute] int age) {
            if (age <= 0) {
                return BadRequest(new { message = "Age can't be a negative" });
            }

            string requestedByLogin = "Admin"; // TODO: заменить после аутентификации
            var users = await _userService.GetUsersOlderThanAsync(age, requestedByLogin);

            if (!users.Any()) {
                return Ok(Enumerable.Empty<UserResponseDto>());
            }

            var userResponseDtos = users.Select(u => MapUserToResponseDto(u));
            return Ok(userResponseDtos);
        }

        // POST: api/users/authenticate-details
        [HttpPost("authenticate-details")]
        public async Task<ActionResult<UserResponseDto>> GetSelftUserDetails([FromBody] UserCredentialsRequestDto credentialsDto) {
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            var user = await _userService.GetActiveUserByCredentialsAsync(credentialsDto.Login, credentialsDto.Password);
            if (user == null) {
                return Unauthorized(new { message = "Invalid login or password, or user is inactive" });
            }

            return Ok(MapUserToResponseDto(user));
        }

        private UserResponseDto MapUserToResponseDto(User user) {
            return new UserResponseDto {
                Guid = user.Guid,
                Login = user.Login,
                Name = user.Name,
                Gender = user.Gender,
                Birthday = user.Birthday,
                Admin = user.Admin,
                CreatedOn = user.CreatedOn,
                IsActive = user.RevokedOn == null
            };
        }
    }
}