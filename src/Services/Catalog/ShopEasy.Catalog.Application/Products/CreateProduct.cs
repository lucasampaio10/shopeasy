using FluentValidation;
using ShopEasy.BuildingBlocks.Cqrs;
using ShopEasy.BuildingBlocks.Domain;
using ShopEasy.BuildingBlocks.Persistence;
using ShopEasy.BuildingBlocks.Results;
using ShopEasy.Catalog.Domain.Products;

namespace ShopEasy.Catalog.Application.Products;

public sealed record CreateProductCommand(string Name, string Sku, string Description, decimal Price, int InitialStock)
    : ICommand<Guid>;

internal sealed class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(command => command.Name).NotEmpty().MaximumLength(200);
        RuleFor(command => command.Sku).NotEmpty().MaximumLength(50);
        RuleFor(command => command.Description).NotNull().MaximumLength(2000);
        RuleFor(command => command.Price).GreaterThan(0);
        RuleFor(command => command.InitialStock).GreaterThanOrEqualTo(0);
    }
}

internal sealed class CreateProductHandler(IProductRepository products, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateProductCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateProductCommand command, CancellationToken cancellationToken)
    {
        var sku = command.Sku.Trim().ToUpperInvariant();
        if (await products.ExistsWithSku(sku, cancellationToken))
        {
            return ProductErrors.DuplicateSku(sku);
        }

        var product = Product.Create(
            command.Name,
            sku,
            command.Description,
            new Money(command.Price),
            command.InitialStock);

        products.Add(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return product.Id;
    }
}
