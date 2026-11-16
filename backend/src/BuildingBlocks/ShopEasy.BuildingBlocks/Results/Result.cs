namespace ShopEasy.BuildingBlocks.Results;

public class Result
{
    protected Result(Error? error) => Error = error;

    public Error? Error { get; }

    public bool IsSuccess => Error is null;

    public bool IsFailure => !IsSuccess;

    public static Result Success() => new(null);

    public static Result Failure(Error error) => new(error);

    public static implicit operator Result(Error error) => Failure(error);
}

public sealed class Result<TValue> : Result
{
    private readonly TValue? _value;

    private Result(TValue value)
        : base(null) => _value = value;

    private Result(Error error)
        : base(error)
    {
    }

    public TValue Value =>
        IsSuccess ? _value! : throw new InvalidOperationException("A failed result has no value.");

    /// <summary>Explicit factory for interface-typed values, which cannot use the implicit conversion.</summary>
    public static Result<TValue> From(TValue value) => new(value);

    public static implicit operator Result<TValue>(TValue value) => new(value);

    public static implicit operator Result<TValue>(Error error) => new(error);
}
