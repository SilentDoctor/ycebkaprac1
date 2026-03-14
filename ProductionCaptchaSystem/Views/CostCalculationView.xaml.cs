using Microsoft.Extensions.DependencyInjection;
using ProductionCaptchaSystem.Entities;
using ProductionCaptchaSystem.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ProductionCaptchaSystem.Views;

public partial class CostCalculationView : Page
{
    private readonly IUnitOfWork _unitOfWork;

    public CostCalculationView()
    {
        InitializeComponent();

        _unitOfWork = App.ServiceProvider!
            .GetRequiredService<IUnitOfWork>();

        LoadProducts();
    }

    private void LoadProducts()
    {
        // TypeId = 1 – Продукт
        var products = _unitOfWork.Items.Find(i => i.TypeId == 1).ToList();
        ProductComboBox.ItemsSource = products;
    }

    private void ProductComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        CalculateCost();
    }

    private void CalculateButton_Click(object sender, RoutedEventArgs e)
    {
        CalculateCost();
    }

    private void CalculateCost()
    {
        if (ProductComboBox.SelectedItem is not Item product)
        {
            MaterialsDataGrid.ItemsSource = null;
            TotalCostText.Text = "0.00 руб.";
            return;
        }

        var spec = _unitOfWork.Specifications
            .Find(s => s.ProductId == product.Id)
            .FirstOrDefault();

        if (spec == null)
        {
            MaterialsDataGrid.ItemsSource = null;
            TotalCostText.Text = "Спецификация не найдена";
            return;
        }

        var lines = _unitOfWork.SpecificationLines
            .Find(sl => sl.SpecificationId == spec.Id)
            .ToList();

        var rows = new List<CostRow>();
        decimal total = 0;

        foreach (var line in lines)
        {
            var material = _unitOfWork.Items
                .Find(i => i.Id == line.MaterialId)
                .FirstOrDefault();

            if (material == null)
                continue;

            var cost = line.Quantity * material.Price;
            total += cost;

            rows.Add(new CostRow
            {
                MaterialName = material.Name,
                Unit = material.Unit,
                Quantity = line.Quantity,
                Price = material.Price,
                Cost = cost
            });
        }

        MaterialsDataGrid.ItemsSource = rows;
        TotalCostText.Text = $"{total:N2} руб.";
    }
}

public class CostRow
{
    public string MaterialName { get; set; } = "";
    public string Unit { get; set; } = "";
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Cost { get; set; }
}
