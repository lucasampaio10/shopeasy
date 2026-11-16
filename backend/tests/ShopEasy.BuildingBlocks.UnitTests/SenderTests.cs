using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ShopEasy.BuildingBlocks.Cqrs;
using ShopEasy.BuildingBlocks.Results;

namespace ShopEasy.BuildingBlocks.UnitTests;

public class SenderTests
{
    private readonly ISender _sender = new ServiceCollection()
        .AddApplicationCore(typeof(SenderTests).Assembly)
        .BuildServiceProvider()
        .GetRequiredService<ISender>();

    [Fact]
    public async Task A_valid_request_reaches_its_handler()
    {
        var result = await _sender.Send(new Greet("Lucas"));

        Assert.True(result.IsSuccess);
        Assert.Equal("Hello, Lucas!", result.Value);
    }

    [Fact]
    public async Task An_invalid_request_is_stopped_before_the_handler()
    {
        var exception = await Assert.ThrowsAsync<RequestValidationException>(() => _sender.Send(new Greet(string.Empty)));

        Assert.Contains(nameof(Greet.Name), exception.Errors.Keys);
    }

    public sealed record Greet(string Name) : IQuery<string>;

    public sealed class GreetValidator : AbstractValidator<Greet>
    {
        public GreetValidator() => RuleFor(query => query.Name).NotEmpty();
    }

    public sealed class GreetHandler : IQueryHandler<Greet, string>
    {
        public Task<Result<string>> Handle(Greet request, CancellationToken cancellationToken) =>
            Task.FromResult<Result<string>>($"Hello, {request.Name}!");
    }
}
