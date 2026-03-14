using ProductionCaptchaSystem.Services;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ProductionCaptchaSystem.Views;

public partial class AuthenticationView : Page
{
    private readonly AuthenticationService _authService;

    // Капча
    private readonly double[] _angles = new double[4];
    private readonly Image[] _pieces;
    private readonly Random _random = new();

    public AuthenticationView(AuthenticationService authService)
    {
        InitializeComponent();
        _authService = authService;

        _pieces = new[] { Part1, Part2, Part3, Part4 };
        InitializeCaptchaChallenge();
    }

    private async void LoginButton_OnClick(object sender, RoutedEventArgs e)
    {
        var username = UsernameTextBox.Text;
        var password = PasswordBox.Password;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            MessageBox.Show("Введите логин и пароль", "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // Проверяем логин/пароль через сервис
        var (result, message, user) = await _authService.TryLoginAsync(username, password);

        if (!result)
        {
            // Неверный логин/пароль или уже заблокирован – сервис сам всё учёл
            MessageBox.Show(message, "Ошибка входа",
                MessageBoxButton.OK, MessageBoxImage.Error);

            InitializeCaptchaChallenge();
            return;
        }

        // Логин/пароль верны – теперь проверяем капчу
        if (!ValidateCaptchaChallenge())
        {
            MessageBox.Show("Капча решена неверно. Попробуйте ещё раз.", "Ошибка входа",
                MessageBoxButton.OK, MessageBoxImage.Error);

            InitializeCaptchaChallenge();
            return;
        }

        // Всё успешно
        MessageBox.Show($"Добро пожаловать, {user?.Username}!", "Успех",
            MessageBoxButton.OK, MessageBoxImage.Information);

        if (user?.IsAdmin == true)
            NavigationService?.Navigate(new AdminView());
        else
            NavigationService?.Navigate(new UserView());
    }

    // Инициализация капчи: случайные углы 0/90/180/270
    private void InitializeCaptchaChallenge()
    {
        for (int i = 0; i < _pieces.Length; i++)
        {
            var angle = 90 * _random.Next(0, 4);
            _angles[i] = angle;

            var rt = _pieces[i].RenderTransform as RotateTransform;
            if (rt == null)
            {
                rt = new RotateTransform();
                _pieces[i].RenderTransform = rt;
            }

            rt.Angle = angle;
        }
    }

    // Проверка: все углы должны быть ровно 0
    private bool ValidateCaptchaChallenge()
    {
        return _angles.All(a => Math.Abs(a % 360) < 0.1);
    }

    // Поворот кусочка на 90 градусов по клику
    private void CaptchaPiece_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Image img)
            return;

        var index = Array.IndexOf(_pieces, img);
        if (index < 0)
            return;

        var rt = img.RenderTransform as RotateTransform;
        if (rt == null)
        {
            rt = new RotateTransform();
            img.RenderTransform = rt;
        }

        var newAngle = (_angles[index] + 90) % 360;
        _angles[index] = newAngle;
        rt.Angle = newAngle;
    }
}
