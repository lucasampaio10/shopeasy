using FluentValidation;
using ShopEasy.BuildingBlocks.Cqrs;
using ShopEasy.BuildingBlocks.Persistence;
using ShopEasy.BuildingBlocks.Results;
using ShopEasy.Ordering.Domain.Orders;

namespace ShopEasy.Ordering.Application.Orders;

public sealed record PlaceOrderItem(Guid ProductId, int Quantity);

public sealed record ShippingAddressInput(string Street, string City, string State, string ZipCode);

public sealed record PlaceOrderCommand(
    Guid CustomerId,
    ShippingAddressInput ShippingAddress,
    string PaymentToken,
    IReadOnlyList<PlaceOrderItem> Items) : ICommand<Guid>;

internal sealed class PlaceOrderValidator : AbstractValidator<PlaceOrderCommand>
{
    private const int MaxQuantityPerItem = 100;

    public PlaceOrderValidator()
    {
        RuleFor(command => command.CustomerId).NotEmpty();
        RuleFor(command => command.PaymentToken).NotEmpty().MaximumLength(100);
        RuleFor(command => command.Items).NotEmpty();
        RuleForEach(command => command.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).NotEmpty();
            item.RuleFor(i => i.Quantity).InclusiveBetween(1, MaxQuantityPerItem);
        });
        RuleFor(command => command.ShippingAddress).NotNull().ChildRules(address =>
        {
            address.RuleFor(a => a.Street).NotEmpty().MaximumLength(200);
            address.RuleFor(a => a.City).NotEmpty().MaximumLength(100);
            address.RuleFor(a => a.State).NotEmpty().MaximumLength(50);
            address.RuleFor(a => a.ZipCode).NotEmpty().MaximumLength(20);
        });
    }
}

/// <summary>
/// Accepts the order and returns immediately. Stock and payment are settled afterwards by the
/// other services, which the client follows by reading the order status.
/// </summary>
internal sealed class PlaceOrderHandler(IOrderRepository orders, IUnitOfWork unitOfWork)
    : ICommandHandler<PlaceOrderCommand, Guid>
{
    public async Task<Result<Guid>> Handle(PlaceOrderCommand command, CancellationToken cancellationToken)
    {
        var address = new Address(
            command.ShippingAddress.Street,
            command.ShippingAddress.City,
            command.ShippingAddress.State,
            command.ShippingAddress.ZipCode);
        var items = command.Items.Select(item => new RequestedItem(item.ProductId, item.Quantity)).ToList();

        var order = Order.Place(command.CustomerId, address, command.PaymentToken, items);

        orders.Add(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return order.Id;
    }
}
