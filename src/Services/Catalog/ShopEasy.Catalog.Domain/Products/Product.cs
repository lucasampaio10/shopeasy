using ShopEasy.BuildingBlocks.Domain;

namespace ShopEasy.Catalog.Domain.Products;

public sealed class Product : AggregateRoot<Guid>
{
    private Product()
    {
    }

    public string Name { get; private set; } = string.Empty;

    public string Sku { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public Money Price { get; private set; } = Money.Zero();

    public int AvailableStock { get; private set; }

    /// <summary>Optimistic concurrency token: two orders cannot take the same last unit.</summary>
    public uint Version { get; private set; }

    public static Product Create(string name, string sku, string description, Money price, int initialStock)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("product.name_required", "A product needs a name.");
        }

        if (string.IsNullOrWhiteSpace(sku))
        {
            throw new DomainException("product.sku_required", "A product needs a SKU.");
        }

        if (initialStock < 0)
        {
            throw new DomainException("product.negative_stock", "Initial stock cannot be negative.");
        }

        return new Product
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Sku = sku.Trim().ToUpperInvariant(),
            Description = description.Trim(),
            Price = price,
            AvailableStock = initialStock,
        };
    }

    public void Restock(int quantity)
    {
        EnsurePositive(quantity);
        AvailableStock += quantity;
    }

    public bool CanReserve(int quantity) => quantity > 0 && quantity <= AvailableStock;

    public void Reserve(int quantity)
    {
        EnsurePositive(quantity);
        if (!CanReserve(quantity))
        {
            throw new DomainException(
                "product.insufficient_stock",
                $"Only {AvailableStock} unit(s) of '{Name}' are available.");
        }

        AvailableStock -= quantity;
    }

    public void Release(int quantity)
    {
        EnsurePositive(quantity);
        AvailableStock += quantity;
    }

    private static void EnsurePositive(int quantity)
    {
        if (quantity <= 0)
        {
            throw new DomainException("product.invalid_quantity", "Quantity must be greater than zero.");
        }
    }
}
