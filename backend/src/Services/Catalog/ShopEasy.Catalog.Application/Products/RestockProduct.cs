using FluentValidation;
using ShopEasy.BuildingBlocks.Cqrs;
using ShopEasy.BuildingBlocks.Persistence;
using ShopEasy.BuildingBlocks.Results;
using ShopEasy.Catalog.Domain.Products;

namespace ShopEasy.Catalog.Application.Products;

public sealed record RestockProductCommand(Guid ProductId, int Quantity) : ICommand;

internal sealed class RestockProductValidator : AbstractValidator<RestockProductCommand>
{
    public RestockProductValidator()
    {
        RuleFor(command => command.ProductId).NotEmpty();
        RuleFor(command => command.Quantity).GreaterThan(0);
    }
}

internal sealed class RestockProductHandler(IProductRepository products, IUnitOfWork unitOfWork)
    : ICommandHandler<RestockProductCommand>
{
    public async Task<Result> Handle(RestockProductCommand command, CancellationToken cancellationToken)
    {
        var product = await products.GetById(command.ProductId, cancellationToken);
        if (product is null)
        {
            return ProductErrors.NotFound(command.ProductId);
        }

        product.Restock(command.Quantity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
