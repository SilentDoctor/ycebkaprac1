using ProductionCaptchaSystem.Entities;

namespace ProductionCaptchaSystem.Infrastructure.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IRepository<Counterparty> Counterparties { get; }
    IRepository<Item> Items { get; }
    IRepository<ItemType> ItemTypes { get; }
    IRepository<Specification> Specifications { get; }
    IRepository<SpecificationLine> SpecificationLines { get; }
    IRepository<CustomerOrder> CustomerOrders { get; }
    IRepository<OrderItem> OrderItems { get; }
    IRepository<ProductionOrder> ProductionOrders { get; }
    IRepository<ProductionOrderProduct> ProductionOrderProducts { get; }
    IRepository<ProductionOrderMaterial> ProductionOrderMaterials { get; }

    int Complete();
    Task<int> CompleteAsync();
}