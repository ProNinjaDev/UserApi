using UserApi.Models;

namespace UserApi.Services 
{
    public interface IUserService 
    {
        Task<User?> CreateUserAsync(string login, string password, string name, int gender, DateTime? birthday, bool isAdmin, string createdBy);

        Task<IEnumerable<User>> GetAllUsersAsync();

        Task<User?> GetUserByLoginAsync(string login);
    }
}