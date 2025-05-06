using UserApi.Models;

namespace UserApi.Services {
    public class MemoryUserService : IUserService {

        private readonly Dictionary<Guid, User> _users = new Dictionary<Guid, User>();

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
        }
        public Task<User?> CreateUserAsync(string login, string password, string name, int gender, DateTime? birthday, bool isAdmin, string createdBy) {
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

            return Task.FromResult<User?>(newUser);
        }
    }
}