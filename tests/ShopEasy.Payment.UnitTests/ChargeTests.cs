using NSubstitute;
using ShopEasy.BuildingBlocks.Messaging;
using ShopEasy.BuildingBlocks.Persistence;
using ShopEasy.Contracts;
using ShopEasy.Payment.Application.Payments;
using ShopEasy.Payment.Domain.Payments;
using ShopEasy.Payment.Infrastructure.Gateways;
using PaymentRecord = ShopEasy.Payment.Domain.Payments.Payment;

namespace ShopEasy.Payment.UnitTests;

public class ChargeTests
{
    private readonly IPaymentRepository _payments = Substitute.For<IPaymentRepository>();
    private readonly IIntegrationEventOutbox _outbox = Substitute.For<IIntegrationEventOutbox>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task A_valid_card_is_charged_and_the_approval_is_announced()
    {
        var order = OrderAwaitingPayment(amount: 700m, paymentToken: "tok_visa");

        await Handler().Handle(order, CancellationToken.None);

        _payments.Received(1).Add(Arg.Is<PaymentRecord>(payment =>
            payment.IsApproved && payment.OrderId == order.OrderId && payment.Amount.Amount == 700m));
        _outbox.Received(1).Add(Arg.Is<PaymentApprovedIntegrationEvent>(approved => approved.OrderId == order.OrderId));
    }

    [Theory]
    [InlineData("tok_declined_card", 700)]
    [InlineData("tok_visa", 10_000.01)]
    public async Task A_refused_charge_is_recorded_and_announced(string paymentToken, decimal amount)
    {
        var order = OrderAwaitingPayment(amount, paymentToken);

        await Handler().Handle(order, CancellationToken.None);

        _payments.Received(1).Add(Arg.Is<PaymentRecord>(payment => !payment.IsApproved && payment.DeclineReason != null));
        _outbox.Received(1).Add(Arg.Is<PaymentDeclinedIntegrationEvent>(declined => declined.OrderId == order.OrderId));
    }

    [Fact]
    public async Task An_order_that_was_already_processed_is_not_charged_again()
    {
        var gateway = Substitute.For<IPaymentGateway>();
        var order = OrderAwaitingPayment(amount: 700m, paymentToken: "tok_visa");
        _payments.ExistsForOrder(order.OrderId, Arg.Any<CancellationToken>()).Returns(true);

        await new ChargeWhenOrderAwaitingPayment(_payments, gateway, _outbox, _unitOfWork)
            .Handle(order, CancellationToken.None);

        await gateway.DidNotReceiveWithAnyArgs().Charge(default!, default!, default);
        _outbox.DidNotReceiveWithAnyArgs().Add(default!);
    }

    private ChargeWhenOrderAwaitingPayment Handler() =>
        new(_payments, new SimulatedPaymentGateway(), _outbox, _unitOfWork);

    private static OrderAwaitingPaymentIntegrationEvent OrderAwaitingPayment(decimal amount, string paymentToken) =>
        new(Guid.NewGuid(), Guid.NewGuid(), amount, "BRL", paymentToken);
}
