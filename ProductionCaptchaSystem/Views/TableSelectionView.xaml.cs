using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace ProductionCaptchaSystem.Views;

public partial class TableSelectionView : Page
{
    private readonly DataTable _customersTable;
    private readonly DataTable _ordersTable;
    private readonly DataTable _productsTable;
    private readonly DataTable _pricesTable;
    private readonly DataTable _specificationTable;

    public TableSelectionView(
        DataTable customersTable,
        DataTable ordersTable,
        DataTable productsTable,
        DataTable pricesTable,
        DataTable specificationTable)
    {
        InitializeComponent();

        _customersTable = customersTable;
        _ordersTable = ordersTable;
        _productsTable = productsTable;
        _pricesTable = pricesTable;
        _specificationTable = specificationTable;
    }

    private void ShowCustomers_Click(object sender, RoutedEventArgs e)
    {
        NavigationService?.Navigate(new TableDetailView(_customersTable, "Заказчики"));
    }

    private void ShowOrders_Click(object sender, RoutedEventArgs e)
    {
        NavigationService?.Navigate(new TableDetailView(_ordersTable, "Заказы покупателей"));
    }

    private void ShowProducts_Click(object sender, RoutedEventArgs e)
    {
        NavigationService?.Navigate(new TableDetailView(_productsTable, "Продукция"));
    }

    private void ShowPrices_Click(object sender, RoutedEventArgs e)
    {
        NavigationService?.Navigate(new TableDetailView(_pricesTable, "Цены"));
    }

    private void ShowSpecification_Click(object sender, RoutedEventArgs e)
    {
        NavigationService?.Navigate(new TableDetailView(_specificationTable, "Спецификация"));
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        NavigationService?.GoBack();
    }
}
