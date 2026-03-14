using Newtonsoft.Json;
using OfficeOpenXml;
using ProductionCaptchaSystem.Entities;
using ProductionCaptchaSystem.Infrastructure.Interfaces;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace ProductionCaptchaSystem.Services;

public class CounterpartyJsonModel
{
    [JsonProperty("id")]
    public string? JsonId { get; set; }

    [JsonProperty("name")]
    public string? Name { get; set; }

    [JsonProperty("inn")]
    public string? Inn { get; set; }

    [JsonProperty("addres")]
    public string? Address { get; set; }

    [JsonProperty("phone")]
    public string? Phone { get; set; }

    [JsonProperty("salesman")]
    public bool Salesman { get; set; }

    [JsonProperty("buyer")]
    public bool Buyer { get; set; }
}

public class ImportService
{
    private readonly IUnitOfWork _unitOfWork;

    public ImportService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    public async Task<string> ImportAllDataAsync(string dataFolder)
    {
        var log = new StringBuilder();
        log.AppendLine("=== НАЧАЛО ИМПОРТА ===\n");

        try
        {
            await ImportItemTypes(log);

            var jsonFile = Path.Combine(dataFolder, "Zakazchiki-6.json");
            if (File.Exists(jsonFile))
                await ImportCounterpartiesFromJson(jsonFile, log);
            else
                log.AppendLine($"⚠️ Файл {jsonFile} не найден\n");

            var pricesFile = Path.Combine(dataFolder, "Tseny-4.xlsx");
            if (File.Exists(pricesFile))
                await ImportItemsAndPricesFromExcel(pricesFile, log);
            else
                log.AppendLine($"⚠️ Файл {pricesFile} не найден\n");

            var specsFile = Path.Combine(dataFolder, "Spetsifikatsiia-3.xlsx");
            if (File.Exists(specsFile))
                await ImportSpecificationsFromExcel(specsFile, log);
            else
                log.AppendLine($"⚠️ Файл {specsFile} не найден\n");

            var ordersFile = Path.Combine(dataFolder, "Zakaz-pokupatelia-5.xlsx");
            if (File.Exists(ordersFile))
                await ImportCustomerOrdersFromExcel(ordersFile, log);
            else
                log.AppendLine($"⚠️ Файл {ordersFile} не найден\n");

            var productionFile = Path.Combine(dataFolder, "Proizvodstvo.xlsx");
            if (File.Exists(productionFile))
                await ImportProductionOrdersFromExcel(productionFile, log);
            else
                log.AppendLine($"⚠️ Файл {productionFile} не найден\n");

            log.AppendLine("\n=== ИМПОРТ ЗАВЕРШЕН УСПЕШНО ===");
        }
        catch (Exception ex)
        {
            log.AppendLine($"\n❌ КРИТИЧЕСКАЯ ОШИБКА: {ex.Message}");
            if (ex.InnerException != null)
                log.AppendLine($"InnerException: {ex.InnerException.Message}");
            log.AppendLine($"StackTrace: {ex.StackTrace}");
        }

        return log.ToString();
    }

    private async Task ImportItemTypes(StringBuilder log)
    {
        log.AppendLine("📦 Импорт типов изделий...");

        var existingTypes = _unitOfWork.ItemTypes.GetAll().ToList();
        if (existingTypes.Any())
        {
            log.AppendLine($"✅ Типы изделий уже существуют ({existingTypes.Count} шт.)\n");
            return;
        }

        var types = new[]
        {
            new ItemType { Name = "Продукт" },
            new ItemType { Name = "Материал" }
        };

        _unitOfWork.ItemTypes.AddRange(types);
        await _unitOfWork.CompleteAsync();
        log.AppendLine($"✅ Добавлено типов изделий: {types.Length}\n");
    }

    private async Task ImportCounterpartiesFromJson(string filePath, StringBuilder log)
    {
        log.AppendLine("👥 Импорт контрагентов из JSON...");

        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            var jsonModels = JsonConvert.DeserializeObject<List<CounterpartyJsonModel>>(json);

            if (jsonModels == null || !jsonModels.Any())
            {
                log.AppendLine("⚠️ JSON файл пуст или неверный формат\n");
                return;
            }

            var existingNames = _unitOfWork.Counterparties.GetAll()
                .Select(c => c.Name).ToList();

            var addedCount = 0;

            foreach (var model in jsonModels)
            {
                if (string.IsNullOrWhiteSpace(model.Name) || existingNames.Contains(model.Name))
                {
                    continue;
                }

                var counterparty = new Counterparty
                {
                    Name = model.Name ?? "Без имени",
                    Inn = model.Inn ?? "",
                    Address = model.Address ?? "",
                    Phone = model.Phone ?? "",
                    CreatedAt = DateTime.UtcNow
                };

                _unitOfWork.Counterparties.Add(counterparty);
                addedCount++;
            }

            if (addedCount > 0)
                await _unitOfWork.CompleteAsync();

            log.AppendLine($"✅ Добавлено новых контрагентов: {addedCount}\n");
        }
        catch (Exception ex)
        {
            log.AppendLine($"❌ Ошибка импорта контрагентов: {ex.Message}");
            if (ex.InnerException != null)
                log.AppendLine($"   InnerException: {ex.InnerException.Message}");
            log.AppendLine("");
        }
    }

    private async Task ImportItemsAndPricesFromExcel(string filePath, StringBuilder log)
    {
        log.AppendLine("💰 Импорт изделий и цен из Excel...");

        try
        {
            using var package = new ExcelPackage(new FileInfo(filePath));
            var worksheet = package.Workbook.Worksheets[0];
            var rowCount = worksheet.Dimension?.Rows ?? 0;

            if (rowCount <= 1)
            {
                log.AppendLine("⚠️ Excel файл пуст\n");
                return;
            }

            var materials = new[] { "молоко нормализованное", "закваска сметанная" };
            var addedCount = 0;
            var updatedCount = 0;
            var articleIndex = 1;

            for (int row = 2; row <= rowCount; row++)
            {
                var name = worksheet.Cells[row, 1].Text?.Trim();
                var priceText = worksheet.Cells[row, 2].Text?.Trim();

                if (string.IsNullOrEmpty(name))
                    continue;

                if (!decimal.TryParse(priceText?.Replace('.', ','), out decimal price))
                    price = 0;

                var isMaterial = materials.Contains(name.ToLower());
                var typeId = isMaterial ? 2 : 1;
                var unit = isMaterial ? "кг" : "шт";

                var existingItem = _unitOfWork.Items.Find(i => i.Name == name).FirstOrDefault();

                if (existingItem == null)
                {
                    var article = $"НФ-{articleIndex:D8}";
                    var allArticles = _unitOfWork.Items.GetAll().Select(i => i.Article).ToList();
                    while (allArticles.Contains(article))
                    {
                        articleIndex++;
                        article = $"НФ-{articleIndex:D8}";
                    }

                    var newItem = new Item
                    {
                        Name = name,
                        Article = article,
                        Unit = unit,
                        Price = price,
                        TypeId = typeId,
                        CreatedAt = DateTime.UtcNow
                    };
                    _unitOfWork.Items.Add(newItem);
                    addedCount++;
                    articleIndex++;
                }
                else
                {
                    existingItem.Price = price;
                    _unitOfWork.Items.Update(existingItem);
                    updatedCount++;
                }
            }

            await _unitOfWork.CompleteAsync();
            log.AppendLine($"✅ Добавлено изделий: {addedCount}, обновлено: {updatedCount}\n");
        }
        catch (Exception ex)
        {
            log.AppendLine($"❌ Ошибка импорта изделий: {ex.Message}");
            if (ex.InnerException != null)
                log.AppendLine($"   InnerException: {ex.InnerException.Message}");
            log.AppendLine("");
        }
    }

    private async Task ImportSpecificationsFromExcel(string filePath, StringBuilder log)
    {
        log.AppendLine("📋 Импорт спецификаций из Excel...");

        try
        {
            using var package = new ExcelPackage(new FileInfo(filePath));
            var worksheet = package.Workbook.Worksheets[0];
            var rowCount = worksheet.Dimension?.Rows ?? 0;

            var productName = worksheet.Cells[4, 4].Text?.Trim();

            if (string.IsNullOrEmpty(productName))
            {
                log.AppendLine("⚠️ Не найдено название продукции в ячейке [4,4]\n");
                return;
            }

            var product = _unitOfWork.Items.Find(i => i.Name == productName).FirstOrDefault();
            if (product == null)
            {
                log.AppendLine($"⚠️ Продукт \"{productName}\" не найден в БД\n");
                return;
            }

            var spec = _unitOfWork.Specifications.Find(s => s.ProductId == product.Id).FirstOrDefault();
            var addedSpecs = 0;
            var addedLines = 0;

            if (spec == null)
            {
                var specName = worksheet.Cells[2, 2].Text?.Trim() ?? $"Спецификация для {product.Name}";
                spec = new Specification
                {
                    ProductId = product.Id,
                    Name = specName,
                    CreatedAt = DateTime.UtcNow
                };
                _unitOfWork.Specifications.Add(spec);
                await _unitOfWork.CompleteAsync();
                addedSpecs++;
            }

            for (int row = 11; row <= rowCount; row++)
            {
                var materialName = worksheet.Cells[row, 2].Text?.Trim();
                var quantityText = worksheet.Cells[row, 12].Text?.Trim();

                if (string.IsNullOrEmpty(materialName))
                    continue;

                var material = _unitOfWork.Items.Find(i => i.Name == materialName).FirstOrDefault();
                if (material == null)
                    continue;

                if (!decimal.TryParse(quantityText?.Replace('.', ','), out decimal quantity))
                    quantity = 1;

                var existingLine = _unitOfWork.SpecificationLines
                    .Find(sl => sl.SpecificationId == spec.Id && sl.MaterialId == material.Id)
                    .FirstOrDefault();

                if (existingLine == null)
                {
                    var line = new SpecificationLine
                    {
                        SpecificationId = spec.Id,
                        MaterialId = material.Id,
                        Quantity = quantity
                    };
                    _unitOfWork.SpecificationLines.Add(line);
                    addedLines++;
                }
            }

            await _unitOfWork.CompleteAsync();
            log.AppendLine($"✅ Добавлено спецификаций: {addedSpecs}, строк: {addedLines}\n");
        }
        catch (Exception ex)
        {
            log.AppendLine($"❌ Ошибка импорта спецификаций: {ex.Message}");
            if (ex.InnerException != null)
                log.AppendLine($"   InnerException: {ex.InnerException.Message}");
            log.AppendLine("");
        }
    }

    private async Task ImportCustomerOrdersFromExcel(string filePath, StringBuilder log)
    {
        log.AppendLine("🛒 Импорт заказов покупателя из Excel...");

        try
        {
            using var package = new ExcelPackage(new FileInfo(filePath));
            var worksheet = package.Workbook.Worksheets[0];
            var rowCount = worksheet.Dimension?.Rows ?? 0;

            var headerText = worksheet.Cells[3, 2].Text?.Trim() ?? "";
            var match = Regex.Match(headerText, @"№\s*(\d+)\s*от\s*(.+?)\s*г?\.");
            var orderNumber = match.Success ? match.Groups[1].Value : "1";
            var orderDateStr = match.Success ? match.Groups[2].Value.Trim() : "";

            DateTime orderDate;
            if (!DateTime.TryParse(orderDateStr, new System.Globalization.CultureInfo("ru-RU"), System.Globalization.DateTimeStyles.None, out orderDate))
                orderDate = DateTime.UtcNow;
            else
                orderDate = DateTime.SpecifyKind(orderDate, DateTimeKind.Utc);

            var counterpartyRaw = worksheet.Cells[7, 6].Text?.Trim().Trim('"', ' ') ?? "";
            var counterparty = _unitOfWork.Counterparties
                .Find(c => c.Name.Contains(counterpartyRaw) || counterpartyRaw.Contains(c.Name))
                .FirstOrDefault();

            if (counterparty == null)
            {
                counterparty = _unitOfWork.Counterparties.GetAll()
                    .FirstOrDefault(c => counterpartyRaw.ToLower().Contains(c.Name.ToLower().Replace("ооо ", "").Replace("\"", "").Trim()));
            }

            if (counterparty == null)
            {
                log.AppendLine($"⚠️ Контрагент \"{counterpartyRaw}\" не найден в БД\n");
                return;
            }

            var fullOrderNumber = $"ЗП-{orderNumber}";
            var order = _unitOfWork.CustomerOrders
                .Find(o => o.OrderNumber == fullOrderNumber).FirstOrDefault();

            var addedOrders = 0;
            var addedItems = 0;

            if (order == null)
            {
                order = new CustomerOrder
                {
                    OrderNumber = fullOrderNumber,
                    CounterpartyId = counterparty.Id,
                    OrderDate = orderDate,
                    Status = "Новый",
                    TotalAmount = 0
                };
                _unitOfWork.CustomerOrders.Add(order);
                await _unitOfWork.CompleteAsync();
                addedOrders++;
            }

            decimal totalAmount = 0;

            for (int row = 11; row <= rowCount; row++)
            {
                var numText = worksheet.Cells[row, 2].Text?.Trim();
                if (string.IsNullOrEmpty(numText) || !int.TryParse(numText, out _))
                    continue;

                var productName = worksheet.Cells[row, 4].Text?.Trim();
                var quantityText = worksheet.Cells[row, 18].Text?.Trim();
                var priceText = worksheet.Cells[row, 23].Text?.Trim();
                var amountText = worksheet.Cells[row, 27].Text?.Trim();

                if (string.IsNullOrEmpty(productName))
                    continue;

                var product = _unitOfWork.Items.Find(i => i.Name == productName).FirstOrDefault();
                if (product == null)
                    continue;

                if (!decimal.TryParse(quantityText?.Replace('.', ','), out decimal quantity))
                    continue;

                decimal.TryParse(priceText?.Replace('.', ','), out decimal price);
                decimal.TryParse(amountText?.Replace('.', ','), out decimal amount);

                if (price == 0) price = product.Price;
                if (amount == 0) amount = quantity * price;

                var existingOrderItem = _unitOfWork.OrderItems
                    .Find(oi => oi.CustomerOrderId == order.Id && oi.ProductId == product.Id)
                    .FirstOrDefault();

                if (existingOrderItem == null)
                {
                    var orderItem = new OrderItem
                    {
                        CustomerOrderId = order.Id,
                        ProductId = product.Id,
                        Quantity = quantity,
                        Price = price,
                        Amount = amount
                    };
                    _unitOfWork.OrderItems.Add(orderItem);
                    addedItems++;
                    totalAmount += amount;
                }
            }

            if (totalAmount > 0)
            {
                order.TotalAmount = totalAmount;
                _unitOfWork.CustomerOrders.Update(order);
            }

            await _unitOfWork.CompleteAsync();
            log.AppendLine($"✅ Добавлено заказов: {addedOrders}, позиций: {addedItems}\n");
        }
        catch (Exception ex)
        {
            log.AppendLine($"❌ Ошибка импорта заказов: {ex.Message}");
            if (ex.InnerException != null)
                log.AppendLine($"   InnerException: {ex.InnerException.Message}");
            log.AppendLine("");
        }
    }

    private async Task ImportProductionOrdersFromExcel(string filePath, StringBuilder log)
    {
        log.AppendLine("🏭 Импорт производственных заказов из Excel...");

        try
        {
            using var package = new ExcelPackage(new FileInfo(filePath));
            var worksheet = package.Workbook.Worksheets[0];
            var rowCount = worksheet.Dimension?.Rows ?? 0;

            var headerText = worksheet.Cells[3, 2].Text?.Trim() ?? "";
            var match = Regex.Match(headerText, @"№\s*(\d+)\s*от\s*(.+?)\s*г?\.");
            var orderNumber = match.Success ? match.Groups[1].Value : "1";
            var dateStr = match.Success ? match.Groups[2].Value.Trim() : "";

            DateTime startDate;
            if (!DateTime.TryParse(dateStr, new System.Globalization.CultureInfo("ru-RU"), System.Globalization.DateTimeStyles.None, out startDate))
                startDate = DateTime.UtcNow;
            else
                startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);

            var fullOrderNumber = $"ПР-{orderNumber}";
            var prodOrder = _unitOfWork.ProductionOrders
                .Find(o => o.OrderNumber == fullOrderNumber).FirstOrDefault();

            var addedOrders = 0;
            var addedProducts = 0;

            if (prodOrder == null)
            {
                prodOrder = new ProductionOrder
                {
                    OrderNumber = fullOrderNumber,
                    StartDate = startDate,
                    Status = "Новый"
                };
                _unitOfWork.ProductionOrders.Add(prodOrder);
                await _unitOfWork.CompleteAsync();
                addedOrders++;
            }

            for (int row = 10; row <= rowCount; row++)
            {
                var numText = worksheet.Cells[row, 2].Text?.Trim();
                if (string.IsNullOrEmpty(numText) || !int.TryParse(numText, out _))
                    continue;

                var sectionAbove = "";
                for (int r = row - 1; r >= 1; r--)
                {
                    var cellText = worksheet.Cells[r, 2].Text?.Trim().ToLower() ?? "";
                    if (cellText == "продукция" || cellText == "материалы")
                    {
                        sectionAbove = cellText;
                        break;
                    }
                }

                if (sectionAbove != "продукция")
                    continue;

                var productName = worksheet.Cells[row, 4].Text?.Trim();
                var quantityText = worksheet.Cells[row, 24].Text?.Trim();

                if (string.IsNullOrEmpty(productName))
                    continue;

                var product = _unitOfWork.Items.Find(i => i.Name == productName).FirstOrDefault();
                if (product == null)
                    continue;

                if (!decimal.TryParse(quantityText?.Replace('.', ','), out decimal quantity))
                    quantity = 1;

                var existingProd = _unitOfWork.ProductionOrderProducts
                    .Find(p => p.ProductionOrderId == prodOrder.Id && p.ProductId == product.Id)
                    .FirstOrDefault();

                if (existingProd == null)
                {
                    var prodOrderProduct = new ProductionOrderProduct
                    {
                        ProductionOrderId = prodOrder.Id,
                        ProductId = product.Id,
                        Quantity = quantity,
                        ProducedQuantity = 0
                    };
                    _unitOfWork.ProductionOrderProducts.Add(prodOrderProduct);
                    addedProducts++;
                }
            }

            await _unitOfWork.CompleteAsync();
            log.AppendLine($"✅ Добавлено производственных заказов: {addedOrders}, позиций: {addedProducts}\n");
        }
        catch (Exception ex)
        {
            log.AppendLine($"❌ Ошибка импорта производственных заказов: {ex.Message}");
            if (ex.InnerException != null)
                log.AppendLine($"   InnerException: {ex.InnerException.Message}");
            log.AppendLine("");
        }
    }
}
