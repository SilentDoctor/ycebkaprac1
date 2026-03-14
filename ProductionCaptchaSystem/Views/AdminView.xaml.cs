using Microsoft.Extensions.DependencyInjection;
using ProductionCaptchaSystem.Infrastructure.Interfaces;
using ProductionCaptchaSystem.Services;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ProductionCaptchaSystem.Views;

public partial class AdminView : Page
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ImportService _importService;

    public AdminView()
    {
        InitializeComponent();

        _unitOfWork = App.ServiceProvider!
            .GetRequiredService<IUnitOfWork>();

        _importService = App.ServiceProvider!
            .GetRequiredService<ImportService>();
    }

    private async void ImportButton_OnClick(object sender, RoutedEventArgs e)
    {
        ImportLogScroll.Visibility = Visibility.Visible;
        UsersDataGrid.Visibility = Visibility.Collapsed;

        ImportLogTextBlock.Text = "Начинаем импорт данных...\n";

        var dataFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");

        if (!Directory.Exists(dataFolder))
        {
            ImportLogTextBlock.Text =
                $"ОШИБКА: Папка Data не найдена!\nОжидается: {dataFolder}";
            return;
        }

        var result = await _importService.ImportAllDataAsync(dataFolder);
        ImportLogTextBlock.Text = result;

        MessageBox.Show("Импорт завершен! Смотрите лог для деталей.", "Импорт",
            MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void UsersButton_OnClick(object sender, RoutedEventArgs e)
    {
        ImportLogScroll.Visibility = Visibility.Collapsed;
        UsersDataGrid.Visibility = Visibility.Visible;

        var users = _unitOfWork.Users.GetAll();
        UsersDataGrid.ItemsSource = users;
    }

    private void ViewTablesButton_OnClick(object sender, RoutedEventArgs e)
    {
        // Загружаем таблицы из репозиториев
        var customersTable = ConvertToDataTable(_unitOfWork.Counterparties.GetAll(), "Заказчики");
        var ordersTable = ConvertToDataTable(_unitOfWork.CustomerOrders.GetAll(), "Заказы");
        var productsTable = ConvertToDataTable(_unitOfWork.Items.GetAll(), "Продукция");
        var pricesTable = ConvertToDataTable(_unitOfWork.Items.GetAll(), "Цены");
        var specificationTable = ConvertToDataTable(_unitOfWork.Specifications.GetAll(), "Спецификация");

        // Переходим на страницу выбора таблиц
        NavigationService?.Navigate(new TableSelectionView(
            customersTable,
            ordersTable,
            productsTable,
            pricesTable,
            specificationTable
        ));
    }

    private DataTable ConvertToDataTable<T>(IEnumerable<T> items, string tableName)
    {
        var dataTable = new DataTable(tableName);

        try
        {
            var properties = typeof(T).GetProperties();

            foreach (var prop in properties)
            {
                dataTable.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }

            foreach (var item in items)
            {
                var row = dataTable.NewRow();
                foreach (var prop in properties)
                {
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                }
                dataTable.Rows.Add(row);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка преобразования данных ({tableName}): {ex.Message}", "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }

        return dataTable;
    }
}
