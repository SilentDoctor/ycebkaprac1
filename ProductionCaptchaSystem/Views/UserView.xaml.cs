using Microsoft.Extensions.DependencyInjection;
using ProductionCaptchaSystem.Services;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ProductionCaptchaSystem.Views;

public partial class UserView : Page
{
    private readonly TransferSimulatorService _apiService;

    public UserView()
    {
        InitializeComponent();

        // Берём сервис из статического App.ServiceProvider
        _apiService = App.ServiceProvider!
            .GetRequiredService<TransferSimulatorService>();
    }

    private async void GetRandomNameButton_OnClick(object sender, RoutedEventArgs e)
    {
        ResultTextBlock.Text = "Загрузка...";

        try
        {
            var fullName = await _apiService.GetRandomFullNameAsync();
            ResultTextBlock.Text = $"Случайное имя: {fullName}";
        }
        catch (Exception ex)
        {
            ResultTextBlock.Text = $"Случайное имя: Ошибка сети: {ex.Message}";
        }
    }

    // Открытие страницы расчёта стоимости (модуль 3)
    private void OpenCostCalculationButton_OnClick(object sender, RoutedEventArgs e)
    {
        NavigationService?.Navigate(new CostCalculationView());
    }
}
