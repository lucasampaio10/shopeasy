using ShopEasy.BuildingBlocks.Messaging;

namespace ShopEasy.Contracts;

public sealed record PaymentApprovedIntegrationEvent(Guid OrderId, Guid PaymentId) : IntegrationEvent;

public sealed record PaymentDeclinedIntegrationEvent(Guid OrderId, string Reason) : IntegrationEvent;
