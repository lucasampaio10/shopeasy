# SOLID no ShopEasy

Cada princípio abaixo aponta para um arquivo real deste repositório, não para um exemplo genérico de livro.

## S — Single Responsibility Principle

[`PlaceOrderHandler`](../backend/src/Services/Ordering/ShopEasy.Ordering.Application/Orders/PlaceOrder.cs) tem um único motivo para mudar: o caso de uso "colocar um pedido". Ele não sabe calcular preço, não sabe cobrar pagamento e não sabe reservar estoque — isso é responsabilidade de outros serviços, que reagem aos eventos do domínio (`OrderPlacedDomainEvent`). Se a regra de validação do pedido mudar, só esse handler (e seu validator) são tocados.

```csharp
internal sealed class PlaceOrderHandler(IOrderRepository orders, IUnitOfWork unitOfWork)
    : ICommandHandler<PlaceOrderCommand, Guid>
{
    public async Task<Result<Guid>> Handle(PlaceOrderCommand command, CancellationToken cancellationToken)
    {
        var order = Order.Place(command.CustomerId, address, command.PaymentToken, items);
        orders.Add(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return order.Id;
    }
}
```

## O — Open/Closed Principle

[`IPaymentGateway`](../backend/src/Services/Payment/ShopEasy.Payment.Application/Payments/IPaymentGateway.cs) é o ponto de extensão. Hoje só existe [`SimulatedPaymentGateway`](../backend/src/Services/Payment/ShopEasy.Payment.Infrastructure/Gateways/SimulatedPaymentGateway.cs), mas plugar um gateway real (Stripe, PagSeguro) significa criar uma nova classe que implementa a interface e registrar na DI — **nenhuma linha dos casos de uso que cobram pagamento muda**. O módulo está aberto para extensão e fechado para modificação.

## L — Liskov Substitution Principle

Qualquer implementação de `IPaymentGateway` tem que cumprir o mesmo contrato que `SimulatedPaymentGateway` cumpre: devolver `ChargeResult.Success` ou `ChargeResult.Decline` para uma recusa de negócio (cartão recusado, limite excedido), e nunca lançar exceção para esse caso — exceção fica reservada para falha de infraestrutura de verdade. Uma implementação que lançasse exceção para "cartão recusado" quebraria essa substituição: quem chama o gateway esperando um `ChargeResult` passaria a explodir onde antes só tratava um valor.

## I — Interface Segregation Principle

Em vez de uma interface única "faz tudo" para lidar com requisições, [`Requests.cs`](../backend/src/BuildingBlocks/ShopEasy.BuildingBlocks/Cqrs/Requests.cs) segrega `ICommandHandler<TCommand>` (escreve, devolve `Result`) de `IQueryHandler<TQuery, TResponse>` (só lê). Um handler de query nunca é forçado a conhecer o shape de um command, e vice-versa — cada um implementa só o que de fato usa.

## D — Dependency Inversion Principle

[`PlaceOrderHandler`](../backend/src/Services/Ordering/ShopEasy.Ordering.Application/Orders/PlaceOrder.cs) depende de [`IOrderRepository`](../backend/src/Services/Ordering/ShopEasy.Ordering.Domain/Orders/IOrderRepository.cs), uma interface definida no **domínio**, implementada depois na camada de infraestrutura com EF Core. A regra de negócio não sabe que existe um banco de dados por trás — a dependência de alto nível (o caso de uso) não depende do detalhe de baixo nível (Postgres); é o contrário: a infraestrutura é que depende da abstração definida pelo domínio.

## Bônus: a regra de negócio mora no agregado

[`Order.MarkAsPaid`](../backend/src/Services/Ordering/ShopEasy.Ordering.Domain/Orders/Order.cs) é o exemplo que costumo dar em entrevista: o próprio agregado sabe que não pode ser pago duas vezes.

```csharp
public void MarkAsPaid(Guid paymentId)
{
    if (Status == OrderStatus.Paid)
    {
        return; // idempotente: mensagem duplicada não tem efeito adicional
    }

    if (Status != OrderStatus.AwaitingPayment)
    {
        throw new DomainException("order.not_awaiting_payment", $"An order in status {Status} cannot be paid.");
    }

    PaymentId = paymentId;
    Status = OrderStatus.Paid;
}
```

O handler que chama esse método não precisa saber dessa regra — ele só chama `order.MarkAsPaid(paymentId)`. Isso é o "S" e o "D" trabalhando juntos: a regra fica onde o estado está, e quem usa o agregado não precisa reimplementá-la.
