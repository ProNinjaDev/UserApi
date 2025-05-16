using UserApi.Models;
using UserApi.DTOs;

namespace UserApi.Services 
{
    public interface IUserService 
    {
        Task<User> CreateUserAsync(CreateUserRequestDto createUserDto, string createdBy);

        Task<IEnumerable<User>> GetAllUsersAsync();

        Task<User?> GetUserByLoginAsync(string login);

        Task<User> UpdateUserInfoAsync(string login, UpdateUserInfoRequestDto updateUserDto, string modifiedBy);

        Task<User> UpdateUserPasswordAsync(string login, string newPassword, string modifiedBy);

        Task<User> UpdateUserLoginAsync(string oldLogin, string newLogin, string modifiedBy);

        Task<User> SoftDeleteUserAsync(string login, string revokedByLogin);

        Task HardDeleteUserAsync(string login, string deletedByLogin);

        Task<User> RestoreUserAsync(string login, string modifiedByLogin);

        Task<IEnumerable<User>> GetUsersOlderThanAsync(int age, string requestedByLogin);

        Task<User?> GetActiveUserByCredentialsAsync(string login, string password);
    }
}