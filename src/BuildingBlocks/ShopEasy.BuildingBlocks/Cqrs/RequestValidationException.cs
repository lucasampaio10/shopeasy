namespace ShopEasy.BuildingBlocks.Cqrs;

/// <summary>The request is malformed and never reached its handler.</summary>
public sealed class RequestValidationException(IReadOnlyDictionary<string, string[]> errors)
    : Exception("One or more validation errors occurred.")
{
    public IReadOnlyDictionary<string, string[]> Errors { get; } = errors;
}
