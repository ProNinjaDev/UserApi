using UserApi.Models;
using UserApi.DTOs;
using UserApi.Security;
using UserApi.Exceptions;

namespace UserApi.Services {
    public class MemoryUserService : IUserService {

        private readonly Dictionary<Guid, User> _users = new Dictionary<Guid, User>();
        private readonly Dictionary<string, Guid> _logins = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);

        public MemoryUserService() {
            var adminGuid = Guid.NewGuid();
            var adminPassword = "Adminpassword555";
            var hashedPassword = PasswordHasher.GenerateHashedPassword(adminPassword);
            var adminUser = new User {
                Guid = adminGuid,
                Login = "Admin",
                PasswordHash = hashedPassword.Hash,
                PasswordSalt = hashedPassword.Salt,
                PasswordIterations = hashedPassword.Iterations,
                PasswordHashAlgorithm = hashedPassword.Algorithm,
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

        private async Task<User> GetUserByLoginOrThrowAsync(string login) {
            var user = await GetUserByLoginAsync(login);
            if (user == null) {
                throw new UserNotFoundException(login);
            }
            return user;
        }

        private void TouchUser(User user, string modifiedBy) {
            user.ModifiedBy = modifiedBy;
            user.ModifiedOn = DateTime.UtcNow;
        }

        public async Task<User> CreateUserAsync(CreateUserRequestDto createUserDto, string createdBy) {
            if (_logins.ContainsKey(createUserDto.Login)) {
                throw new DuplicateLoginException(createUserDto.Login);
            }
            
            var hashedPassword = PasswordHasher.GenerateHashedPassword(createUserDto.Password);
            var newUserGuid = Guid.NewGuid();
            var newUser = new User {
                Guid = newUserGuid,
                Login = createUserDto.Login,
                PasswordHash = hashedPassword.Hash,
                PasswordSalt = hashedPassword.Salt,
                PasswordIterations = hashedPassword.Iterations,
                PasswordHashAlgorithm = hashedPassword.Algorithm,
                Name = createUserDto.Name,
                Gender = createUserDto.Gender,
                Birthday = createUserDto.Birthday,
                Admin = createUserDto.Admin,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = createdBy,
                ModifiedOn = DateTime.UtcNow,
                ModifiedBy = createdBy,
                RevokedOn = null,
                RevokedBy = null
            };

            _users.Add(newUserGuid, newUser);
            _logins.Add(newUser.Login, newUserGuid);

            return await Task.FromResult<User>(newUser);
        }

        public Task<IEnumerable<User>> GetAllUsersAsync() {
            return Task.FromResult(_users.Values.Where(u => u.RevokedOn == null).OrderBy(u => u.CreatedOn).AsEnumerable());
        }

        public async Task<User?> GetUserByLoginAsync(string login) {
            if (_logins.TryGetValue(login, out Guid userId)) {
                if (_users.TryGetValue(userId, out User? user)) {
                    return await Task.FromResult<User?>(user);
                }
            }
            return await Task.FromResult<User?>(null);
        }

        public async Task<User> UpdateUserInfoAsync(string login, UpdateUserInfoRequestDto updateUserDto, string modifiedBy) {
            User user = await GetUserByLoginOrThrowAsync(login);

            bool isModified = false; // для null отслеживания

            if (updateUserDto.Name != null) {
                user.Name = updateUserDto.Name;
                isModified = true;
            }
            if (updateUserDto.Gender != null) {
                user.Gender = updateUserDto.Gender.Value;
                isModified = true;
            }
            if (updateUserDto.Birthday != null) { // FIXME: возможно, лучше создать новый эндпоинт для обработки др
                if (user.Birthday != updateUserDto.Birthday.Value) { // др не очищается
                    user.Birthday = updateUserDto.Birthday.Value;
                    isModified = true;
                }
            }

            if (isModified) {
                TouchUser(user, modifiedBy);
            }

            return await Task.FromResult(user);
        }

        public async Task<User> UpdateUserPasswordAsync(string login, string newPassword, string modifiedBy) {
            User user = await GetUserByLoginOrThrowAsync(login);

            var hashedPassword = PasswordHasher.GenerateHashedPassword(newPassword);
            user.PasswordHash = hashedPassword.Hash;
            user.PasswordSalt = hashedPassword.Salt;
            user.PasswordIterations = hashedPassword.Iterations;
            user.PasswordHashAlgorithm = hashedPassword.Algorithm;
            TouchUser(user, modifiedBy);

            return await Task.FromResult(user);
        }

        public async Task<User> UpdateUserLoginAsync(string oldLogin, string newLogin, string modifiedBy) {
            // Из-за игнора регистра дополнительные проверки
            if (string.Equals(oldLogin, newLogin, StringComparison.OrdinalIgnoreCase)) {
                User userNoChange = await GetUserByLoginOrThrowAsync(oldLogin);

                TouchUser(userNoChange, modifiedBy);
                return await Task.FromResult(userNoChange);
            }
            
            if (_logins.ContainsKey(newLogin)) {
                throw new DuplicateLoginException(newLogin);
            }
            
            User user = await GetUserByLoginOrThrowAsync(oldLogin);
            
            _logins.Remove(oldLogin);
            user.Login = newLogin;
            _logins.Add(newLogin, user.Guid);

            TouchUser(user, modifiedBy);
            
            return await Task.FromResult(user);
        }
        public async Task<User> SoftDeleteUserAsync(string login, string revokedByLogin) {
            User user = await GetUserByLoginOrThrowAsync(login);
            
            user.RevokedBy = revokedByLogin;
            user.RevokedOn = DateTime.UtcNow;
            TouchUser(user, revokedByLogin);

            return await Task.FromResult(user);
        }

        public async Task<User> RestoreUserAsync(string login, string modifiedByLogin) {
            User user = await GetUserByLoginOrThrowAsync(login);

            if (user.RevokedOn != null) {
                user.RevokedOn = null;
                user.RevokedBy = null;
            }

            TouchUser(user, modifiedByLogin);

            return await Task.FromResult(user);
        }

        public async Task<IEnumerable<User>> GetUsersOlderThanAsync(int age, string requestedByLogin) {
            // TODO: использовать requestedByLogin при авторизации
            var today = DateTime.UtcNow.Date;
            var usersOlderThanAge = _users.Values.Where(u => u.RevokedOn == null && u.Birthday != null && u.Birthday.Value.AddYears(age) < today).OrderBy(u => u.CreatedOn).AsEnumerable();

            return await Task.FromResult(usersOlderThanAge);
        }

        public async Task<User?> GetActiveUserByCredentialsAsync(string login, string password) {
            var user = await GetUserByLoginAsync(login);

            if (user == null || user.RevokedOn != null) {
                return null;
            }

            bool isValidPassword = PasswordHasher.IsMatchPasswords(password, user.PasswordHash, user.PasswordSalt, user.PasswordIterations, user.PasswordHashAlgorithm);
            if (!isValidPassword) {
                return null;
            }
            return user; // TODO: возможно стоит обернуть в FromResult
        }
    }
}