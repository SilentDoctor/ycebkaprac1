using ProductionCaptchaSystem.Entities;

namespace ProductionCaptchaSystem.Infrastructure.Interfaces;

public interface IUserRepository : IRepository<User>
{
    User? GetByUsername(string username);
    void BlockUser(int userId, DateTime? blockedUntil);
    void UnblockUser(int userId);
}