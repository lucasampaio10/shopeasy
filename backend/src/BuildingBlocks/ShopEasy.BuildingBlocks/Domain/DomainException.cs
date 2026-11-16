namespace ShopEasy.BuildingBlocks.Domain;

/// <summary>Raised when an operation would break a business invariant.</summary>
public sealed class DomainException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}
