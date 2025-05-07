using UserApi.Models;

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
        public Task<User?> CreateUserAsync(string login, string password, string name, int gender, DateTime? birthday, bool isAdmin, string createdBy) {
            if (_logins.ContainsKey(login)) {
                Console.WriteLine("This login is already taken");
                return Task.FromResult<User?>(null);
            }
            
            var newUserGuid = Guid.NewGuid();
            var newUser = new User {
                Guid = newUserGuid,
                Login = login,
                Password = password, // todo: хешировать пароль
                Name = name,
                Gender = gender,
                Birthday = birthday,
                Admin = isAdmin,
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
            return Task.FromResult(_users.Values.ToList() as IEnumerable<User>);
        }

        public Task<User?> GetUserByLoginAsync(string login) {
            if (_logins.ContainsKey(login)) {
                Guid userId = _logins[login];
                if(_users.TryGetValue(userId, out User? user)) {
                    return Task.FromResult<User?>(user);
                }
            }
            return Task.FromResult<User?>(null);
        }
    }
}