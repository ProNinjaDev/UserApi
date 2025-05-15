using Microsoft.AspNetCore.Mvc;
using UserApi.Models;
using UserApi.Services;
using UserApi.DTOs;
using UserApi.Exceptions;

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

            try {
                var newCreatedUser = await _userService.CreateUserAsync(createUserDto, "Admin"); // TODO: возможно CreatedBy нужен другой

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
            catch (DuplicateLoginException ex) {
                return Conflict(new { message = ex.Message });
            }
        }

        // PUT: api/users/{login} 
        [HttpPut("{login}")]
        public async Task<IActionResult> UpdateUserInfo([FromRoute] string login,
            [FromBody] UpdateUserInfoRequestDto updateUserDto) {

            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            try {
                var currentUserLogin = "Admin"; // TODO: заменить при аутентификации
                var userExisting = await _userService.GetUserByLoginAsync(login);

                // TODO: добавить проверки при аутентификации
                var userUpdated = await _userService.UpdateUserInfoAsync(login, updateUserDto, currentUserLogin);

                return NoContent();
            }
            catch (UserNotFoundException ex) {
                return NotFound(new { message = ex.Message });
            }
        }

        // PUT: api/users/{login}/password
        [HttpPut("{login}/password")]
        public async Task<IActionResult> UpdateUserPassword([FromRoute] string login,
            [FromBody] UpdateUserPasswordRequestDto updatePasswordDto) {
            
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            try {
                var currentUserLogin = "Admin"; // TODO: заменить после аутентификации

                var userExisting = await _userService.GetUserByLoginAsync(login);

                var userUpdated = await _userService.UpdateUserPasswordAsync(login, updatePasswordDto.NewPassword, currentUserLogin);
                return NoContent();
            }
            catch (UserNotFoundException ex) {
                return NotFound(new { message = ex.Message });
            }
        }

        // PUT: api/users/{login}/login
        [HttpPut("{login}/login")]
        public async Task<IActionResult> UpdateUserLogin([FromRoute] string login,
            [FromBody] UpdateUserLoginRequestDto updateLoginDto) {
            
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            try {
                var currentUserLogin = "Admin"; // TODO: заменить после аутентификации
                var userExisting = await _userService.GetUserByLoginAsync(login);

                var userUpdated = await _userService.UpdateUserLoginAsync(login, updateLoginDto.NewLogin, currentUserLogin);
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

        // DELETE: api/users/{login}
        [HttpDelete("{login}")]
        public async Task<IActionResult> SoftDeleteUser([FromRoute] string login) {
            try {
                string revokedByLogin = "Admin"; // TODO: изменить на логин аутентифицированного админа
                await _userService.SoftDeleteUserAsync(login, revokedByLogin);
                return NoContent();
            }
            catch (UserNotFoundException ex) {
                return NotFound(new { message = ex.Message });
            }

        }

        // PUT: api/users/{login}/restore
        [HttpPut("{login}/restore")]
        public async Task<IActionResult> RestoreUser([FromRoute] string login) {
            try {
                string modifiedByLogin = "Admin"; // TODO: изменить на логин аутентифицированного админа
                var restoredUser = await _userService.RestoreUserAsync(login, modifiedByLogin);

                var userResponseDto = new UserResponseDto {
                    Guid = restoredUser.Guid,
                    Login = restoredUser.Login,
                    Name = restoredUser.Name,
                    Gender = restoredUser.Gender,
                    Birthday = restoredUser.Birthday,
                    Admin = restoredUser.Admin,
                    CreatedOn = restoredUser.CreatedOn,
                    IsActive = restoredUser.RevokedOn == null
                };
                
                return Ok(userResponseDto);
            }
            catch (UserNotFoundException ex) {
                return NotFound(new {message = ex.Message});
            }
        }
    }
}