using ProductionCaptchaSystem.Entities;
using ProductionCaptchaSystem.Infrastructure.Interfaces;
using ProductionCaptchaSystem.Infrastructure.Persistence;

namespace ProductionCaptchaSystem.Infrastructure.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(CaptchaDbContext context) : base(context)
    {
    }

    public User? GetByUsername(string username)
    {
        return Context.Users.FirstOrDefault(u => u.Username == username);
    }

    public void BlockUser(int userId, DateTime? blockedUntil)
    {
        var user = GetById(userId);
        if (user != null)
        {
            user.IsBlocked = true;
            user.BlockedUntil = blockedUntil;
            Update(user);
        }
    }

    public void UnblockUser(int userId)
    {
        var user = GetById(userId);
        if (user != null)
        {
            user.IsBlocked = false;
            user.BlockedUntil = null;
            Update(user);
        }
    }
}