using ProductionCaptchaSystem.Entities;
using ProductionCaptchaSystem.Infrastructure.Interfaces;
using ProductionCaptchaSystem.Infrastructure.Persistence;

namespace ProductionCaptchaSystem.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly CaptchaDbContext _context;

    public IUserRepository Users { get; }
    public IRepository<Counterparty> Counterparties { get; }
    public IRepository<Item> Items { get; }
    public IRepository<ItemType> ItemTypes { get; }
    public IRepository<Specification> Specifications { get; }
    public IRepository<SpecificationLine> SpecificationLines { get; }
    public IRepository<CustomerOrder> CustomerOrders { get; }
    public IRepository<OrderItem> OrderItems { get; }
    public IRepository<ProductionOrder> ProductionOrders { get; }
    public IRepository<ProductionOrderProduct> ProductionOrderProducts { get; }
    public IRepository<ProductionOrderMaterial> ProductionOrderMaterials { get; }

    public UnitOfWork(CaptchaDbContext context)
    {
        _context = context;

        Users = new UserRepository(_context);
        Counterparties = new GenericRepository<Counterparty>(_context);
        Items = new GenericRepository<Item>(_context);
        ItemTypes = new GenericRepository<ItemType>(_context);
        Specifications = new GenericRepository<Specification>(_context);
        SpecificationLines = new GenericRepository<SpecificationLine>(_context);
        CustomerOrders = new GenericRepository<CustomerOrder>(_context);
        OrderItems = new GenericRepository<OrderItem>(_context);
        ProductionOrders = new GenericRepository<ProductionOrder>(_context);
        ProductionOrderProducts = new GenericRepository<ProductionOrderProduct>(_context);
        ProductionOrderMaterials = new GenericRepository<ProductionOrderMaterial>(_context);
    }

    public int Complete()
    {
        return _context.SaveChanges();
    }

    public async Task<int> CompleteAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}