using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using ProductionCaptchaSystem.Infrastructure.Interfaces;
using ProductionCaptchaSystem.Infrastructure.Persistence;
using ProductionCaptchaSystem.Infrastructure.Repositories;
using ProductionCaptchaSystem.Services;
using ProductionCaptchaSystem.Views;

namespace ProductionCaptchaSystem;

public partial class App : Application
{
    public static ServiceProvider? ServiceProvider { get; private set; }

    // Статический конструктор — выполняется один раз при запуске приложения
    static App()
    {
        // Разрешить работу с DateTime (Local/Unspecified) для timestamptz в PostgreSQL
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();

        // DbContext
        services.AddScoped<CaptchaDbContext>();

        // UnitOfWork и репозитории
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Сервисы
        services.AddScoped<AuthenticationService>();
        services.AddScoped<TransferSimulatorService>();
        services.AddScoped<ImportService>();

        ServiceProvider = services.BuildServiceProvider();

        var authService = ServiceProvider.GetRequiredService<AuthenticationService>();
        var mainWindow = new MainWindow(authService);
        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        ServiceProvider?.Dispose();
        base.OnExit(e);
    }
}
