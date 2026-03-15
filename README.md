# ProductionCaptchaSystem

Десктопное приложение для управления производством, написанное на C# (.NET 8.0, WPF) с базой данных PostgreSQL.

## О проекте

**ProductionCaptchaSystem** — система управления производственными процессами. Реализует полный цикл: от регистрации заказов клиентов до формирования производственных заданий и расчёта себестоимости.

Вход в систему защищён CAPTCHA (изображения с цифрами) и блокировкой после 3 неудачных попыток.

## Функциональность

### Аутентификация
- Ввод логина и пароля с CAPTCHA-верификацией (4 варианта изображений)
- Блокировка аккаунта на 10 минут после 3 неверных попыток входа
- Роли: **Администратор** и **Пользователь**

### Административный интерфейс
- Управление пользователями (создание, блокировка)
- Импорт данных из Excel (.xlsx) и JSON файлов
- Полный доступ ко всем таблицам БД (просмотр и редактирование)

### Пользовательский интерфейс
- Просмотр каталога изделий и материалов
- Управление заказами клиентов и производственными заказами
- Расчёт себестоимости продукции по спецификациям (BOM)
- Планирование материальных потребностей

## Структура проекта

```
ProductionCaptchaSystem/
├── Assets/               # CAPTCHA изображения (4 PNG файла)
├── Data/                 # Примеры файлов импорта (Excel, JSON)
├── Entities/             # 11 классов-сущностей (модели таблиц БД)
├── Infrastructure/
│   ├── Interfaces/       # IRepository, IUnitOfWork
│   ├── Persistence/      # EF Core DbContext (CaptchaDbContext)
│   └── Repositorie/      # Реализации репозиториев
├── Models/               # View-модели
├── Service/
│   ├── AuthenticationService.cs    # Логика входа и блокировки
│   ├── ImportService.cs            # Импорт из Excel/JSON
│   └── TransferSimulatorService.cs # Интеграция с внешним сервисом
├── Views/                # 6 WPF-экранов (XAML + code-behind)
│   ├── AuthenticationView          # Экран входа с CAPTCHA
│   ├── AdminView                   # Панель администратора
│   ├── UserView                    # Основной экран пользователя
│   ├── TableSelectionView          # Выбор таблицы БД
│   ├── TableDetailView             # Просмотр/редактирование записей
│   └── CostCalculationView         # Расчёт себестоимости
├── App.xaml / App.xaml.cs          # Точка входа, DI-контейнер
├── MainWindow.xaml                 # Главное окно с Frame-навигацией
└── ProductionCaptchaSystem.csproj
```

## База данных

Схема содержит 11 таблиц. SQL-скрипт для создания — файл `zaprosdb` в корне репозитория.

| Таблица | Назначение |
|---|---|
| `User` | Пользователи системы (логин, пароль, роль, статус блокировки) |
| `Counterparty` | Контрагенты — клиенты и поставщики |
| `ItemType` | Типы изделий (продукция / материал) |
| `Item` | Каталог изделий и материалов (артикул, цена, единица измерения) |
| `Specification` | Спецификации (BOM) для изделий |
| `SpecificationLine` | Строки спецификации — состав материалов |
| `CustomerOrder` | Заказы клиентов |
| `OrderItem` | Позиции заказов клиентов |
| `ProductionOrder` | Производственные заказы |
| `ProductionProduct` | Изделия в производственном заказе |
| `ProductionMaterial` | Материалы в производственном заказе |

## Технологии

| Компонент | Версия |
|---|---|
| .NET / WPF | 8.0 |
| Entity Framework Core | 8.0.20 |
| Npgsql.EFCore.PostgreSQL | 8.0.11 |
| EPPlus (Excel) | 7.0.5 |
| Newtonsoft.Json | 13.0.4 |
| PostgreSQL | 14+ |

## Установка и запуск

### Требования
- Windows 10/11
- [.NET 8.0 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)
- PostgreSQL 14 или новее
- Visual Studio 2022 (для сборки)

### Настройка базы данных

1. Создайте базу данных в PostgreSQL:
```sql
CREATE DATABASE productioncaptcha;
```

2. Выполните скрипт из файла `zaprosdb` для создания всех таблиц.

3. Создайте первого администратора:
```sql
INSERT INTO "User" ("Username", "Password", "IsAdmin")
VALUES ('admin', '<bcrypt-hash>', TRUE);
```

### Настройка подключения

Строка подключения задаётся в `Infrastructure/Persistence/CaptchaDbContext.cs`:
```
Host=localhost;Port=5432;Database=productioncaptcha;Username=postgres;Password=ваш_пароль
```

### Сборка

```bash
git clone https://github.com/SilentDoctor/ycebkaprac1.git
cd ycebkaprac1
dotnet restore
dotnet build
dotnet run --project ProductionCaptchaSystem
```

Или откройте `ProductionCaptchaSystem.slnx` в Visual Studio 2022 и нажмите F5.

## Архитектура

Приложение построено по многоуровневой архитектуре:

- **Entities** — POCO-классы, отражающие таблицы БД
- **Infrastructure** — Repository Pattern + Unit of Work для работы с данными
- **Service** — бизнес-логика (аутентификация, импорт, интеграции)
- **Views** — WPF-экраны, навигация через `Frame` в MainWindow
- **DI-контейнер** — `Microsoft.Extensions.DependencyInjection`, настройка в `App.xaml.cs`
