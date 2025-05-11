using UserApi.Models;
using UserApi.DTOs;

namespace UserApi.Services {
    public class MemoryUserService : IUserService {

        private readonly Dictionary<Guid, User> _users = new Dictionary<Guid, User>();
        private readonly Dictionary<string, Guid> _logins = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);

        public MemoryUserService() {
            var adminGuid = Guid.NewGuid();
            var adminUser = new User {
                Guid = adminGuid,
                Login = "Admin",
                Password = "Adminpassword555", // todo: хешировать парольчики
                Name = "Administrator",
                Gender = 2,
                Birthday = null,
                Admin = true,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "Developer",
                ModifiedOn = DateTime.UtcNow,
                ModifiedBy = "Developer",
                RevokedOn = null,
                RevokedBy = null
            };
            _users.Add(adminGuid, adminUser);
            _logins.Add(adminUser.Login, adminGuid);
        }
        public Task<User?> CreateUserAsync(CreateUserRequestDto createUserDto, string createdBy) {
            if (_logins.ContainsKey(createUserDto.Login)) {
                Console.WriteLine("This login is already taken");
                return Task.FromResult<User?>(null);
            }
            
            var newUserGuid = Guid.NewGuid();
            var newUser = new User {
                Guid = newUserGuid,
                Login = createUserDto.Login,
                Password = createUserDto.Password, // todo: хешировать пароль
                Name = createUserDto.Name,
                Gender = createUserDto.Gender,
                Birthday = createUserDto.Birthday,
                Admin = createUserDto.Admin,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = createdBy,
                ModifiedOn = DateTime.UtcNow,
                ModifiedBy = createdBy, // todo: возможно стоит изменить
                RevokedOn = null,
                RevokedBy = null
            };

            _users.Add(newUserGuid, newUser);
            _logins.Add(newUser.Login, newUserGuid);

            return Task.FromResult<User?>(newUser);
        }

        public Task<IEnumerable<User>> GetAllUsersAsync() {
            return Task.FromResult(_users.Values.Where(u => u.RevokedOn == null).OrderBy(u => u.CreatedOn).AsEnumerable());
        }

        public Task<User?> GetUserByLoginAsync(string login) {
            if (_logins.ContainsKey(login)) {
                Guid userId = _logins[login]; // todo: заменить на TryGetValue (а может и не надо)
                if(_users.TryGetValue(userId, out User? user)) {
                    return Task.FromResult<User?>(user);
                }
            }
            return Task.FromResult<User?>(null);
        }

        public Task<User?> UpdateUserInfoAsync(string login, UpdateUserInfoRequestDto updateUserDto, string modifiedBy) {
            if (!_logins.TryGetValue(login, out Guid userId)) {
                return Task.FromResult<User?>(null);
            }

            if (!_users.TryGetValue(userId, out User? user)) {
                return Task.FromResult<User?>(null);
            }

            bool isModified = false; // для null отслеживания

            if (updateUserDto.Name != null) {
                user.Name = updateUserDto.Name;
                isModified = true;
            }
            if (updateUserDto.Gender != null) {
                user.Gender = updateUserDto.Gender.Value;
                isModified = true;
            }
            if (updateUserDto.Birthday != null) { // todo: возможно, лучше создать новый эндпоинт для обработки др
                if (user.Birthday != updateUserDto.Birthday.Value) { // др не очищается
                    user.Birthday = updateUserDto.Birthday.Value;
                    isModified = true;
                }
            }

            if (isModified) {
                user.ModifiedBy = modifiedBy;
                user.ModifiedOn = DateTime.UtcNow;
            }

            return Task.FromResult<User?>(user);
        }
    }
}