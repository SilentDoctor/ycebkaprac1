using ProductionCaptchaSystem.Entities;
using ProductionCaptchaSystem.Infrastructure.Interfaces;

namespace ProductionCaptchaSystem.Services;

public class AuthenticationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly Dictionary<string, (int attempts, DateTime? lockUntil)> _loginAttempts = new();
    private const int MaxLoginAttempts = 3;
    private const int LockoutMinutes = 10;

    public AuthenticationService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<(bool success, string message, User? user)> TryLoginAsync(string username, string password)
    {
        return await Task.Run(() =>
        {
            // Проверка блокировки по попыткам входа
            if (_loginAttempts.ContainsKey(username))
            {
                var (attempts, lockUntil) = _loginAttempts[username];
                
                if (lockUntil.HasValue && DateTime.Now < lockUntil.Value)
                {
                    var remaining = (lockUntil.Value - DateTime.Now).Minutes + 1;
                    return (false, $"Аккаунт временно заблокирован. Попробуйте через {remaining} мин.", null);
                }

                if (lockUntil.HasValue && DateTime.Now >= lockUntil.Value)
                {
                    _loginAttempts.Remove(username);
                }
            }

            var user = _unitOfWork.Users.GetByUsername(username);

            if (user == null)
            {
                RecordFailedAttempt(username);
                return (false, "Неверный логин или пароль", null);
            }

            // Проверка блокировки в БД
            if (user.IsBlocked)
            {
                if (user.BlockedUntil.HasValue && DateTime.Now < user.BlockedUntil.Value)
                {
                    var remaining = (user.BlockedUntil.Value - DateTime.Now).Minutes + 1;
                    return (false, $"Пользователь заблокирован до {user.BlockedUntil.Value:dd.MM.yyyy HH:mm}", null);
                }

                if (user.BlockedUntil.HasValue && DateTime.Now >= user.BlockedUntil.Value)
                {
                    _unitOfWork.Users.UnblockUser(user.Id);
                    _unitOfWork.Complete();
                }
                else
                {
                    return (false, "Пользователь заблокирован администратором", null);
                }
            }

            if (user.Password != password)
            {
                RecordFailedAttempt(username);
                
                if (_loginAttempts.ContainsKey(username))
                {
                    var remainingAttempts = MaxLoginAttempts - _loginAttempts[username].attempts;
                    if (remainingAttempts > 0)
                    {
                        return (false, $"Неверный пароль. Осталось попыток: {remainingAttempts}", null);
                    }
                }
                
                return (false, "Неверный логин или пароль", null);
            }

            // Успешный вход - очищаем попытки
            if (_loginAttempts.ContainsKey(username))
            {
                _loginAttempts.Remove(username);
            }

            return (true, "Успешный вход", user);
        });
    }

    private void RecordFailedAttempt(string username)
    {
        if (!_loginAttempts.ContainsKey(username))
        {
            _loginAttempts[username] = (1, null);
        }
        else
        {
            var (attempts, _) = _loginAttempts[username];
            attempts++;

            if (attempts >= MaxLoginAttempts)
            {
                var lockUntil = DateTime.Now.AddMinutes(LockoutMinutes);
                _loginAttempts[username] = (attempts, lockUntil);
            }
            else
            {
                _loginAttempts[username] = (attempts, null);
            }
        }
    }
}
