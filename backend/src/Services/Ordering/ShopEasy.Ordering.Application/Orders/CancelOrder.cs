using ShopEasy.BuildingBlocks.Cqrs;
using ShopEasy.BuildingBlocks.Persistence;
using ShopEasy.BuildingBlocks.Results;
using ShopEasy.Ordering.Domain.Orders;

namespace ShopEasy.Ordering.Application.Orders;

public sealed record CancelOrderCommand(Guid OrderId) : ICommand;

internal sealed class CancelOrderHandler(IOrderRepository orders, IUnitOfWork unitOfWork)
    : ICommandHandler<CancelOrderCommand>
{
    // Shown as-is in cancellationReason by clients (see OrderStatusScreen in the mobile app).
    private const string CancelledByCustomer = "Cancelado pelo cliente.";

    public async Task<Result> Handle(CancelOrderCommand command, CancellationToken cancellationToken)
    {
        var order = await orders.GetById(command.OrderId, cancellationToken);
        if (order is null)
        {
            return OrderErrors.NotFound(command.OrderId);
        }

        if (order.Status == OrderStatus.Paid)
        {
            return Error.Conflict("order.already_paid", "A paid order cannot be cancelled.");
        }

        order.Cancel(CancelledByCustomer);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
