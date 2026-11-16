namespace ShopEasy.Catalog.Domain.Products;

public interface IProductRepository
{
    Task<Product?> GetById(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<Guid, Product>> GetByIds(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken);

    Task<bool> ExistsWithSku(string sku, CancellationToken cancellationToken);

    void Add(Product product);
}
