using ShopEasy.BuildingBlocks.Domain;

namespace ShopEasy.BuildingBlocks.UnitTests;

public class MoneyTests
{
    [Fact]
    public void Two_amounts_with_the_same_value_and_currency_are_equal()
    {
        Assert.Equal(new Money(10.50m, "brl"), new Money(10.50m, "BRL"));
    }

    [Fact]
    public void Amounts_are_rounded_to_cents()
    {
        Assert.Equal(10.13m, new Money(10.1349m).Amount);
    }

    [Fact]
    public void A_negative_amount_is_rejected()
    {
        var exception = Assert.Throws<DomainException>(() => new Money(-1m));

        Assert.Equal("money.negative_amount", exception.Code);
    }

    [Fact]
    public void Adding_different_currencies_is_rejected()
    {
        var exception = Assert.Throws<DomainException>(() => new Money(1m, "BRL").Add(new Money(1m, "USD")));

        Assert.Equal("money.currency_mismatch", exception.Code);
    }

    [Fact]
    public void Multiplying_scales_the_amount_and_keeps_the_currency()
    {
        Assert.Equal(new Money(59.70m, "USD"), new Money(19.90m, "USD").Multiply(3));
    }
}
