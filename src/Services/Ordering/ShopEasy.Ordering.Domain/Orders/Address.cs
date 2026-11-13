using ShopEasy.BuildingBlocks.Domain;

namespace ShopEasy.Ordering.Domain.Orders;

public sealed record Address
{
    public Address(string street, string city, string state, string zipCode)
    {
        Street = Required(street, nameof(street));
        City = Required(city, nameof(city));
        State = Required(state, nameof(state));
        ZipCode = Required(zipCode, nameof(zipCode));
    }

    private Address()
    {
    }

    public string Street { get; private init; } = string.Empty;

    public string City { get; private init; } = string.Empty;

    public string State { get; private init; } = string.Empty;

    public string ZipCode { get; private init; } = string.Empty;

    private static string Required(string value, string field) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new DomainException("address.incomplete", $"The address is missing its {field}.")
            : value.Trim();
}
