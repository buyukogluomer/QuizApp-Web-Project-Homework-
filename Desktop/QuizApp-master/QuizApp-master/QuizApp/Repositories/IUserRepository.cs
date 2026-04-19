using QuizApp.Models;

namespace QuizApp.Repositories;

// Başındaki "I" harfine dikkat!
public interface IUserRepository
{
    Task<Users?> GetUserByIdAsync(int id);
    Task<Users?> GetUserByEmailAsync(string email);
    Task AddUserAsync(Users user);
    Task UpdateUserAsync(Users user);
}