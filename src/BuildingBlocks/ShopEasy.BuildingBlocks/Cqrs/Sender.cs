using System.Collections.Concurrent;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;

namespace ShopEasy.BuildingBlocks.Cqrs;

/// <summary>Routes a request to its single handler, validating it first.</summary>
internal sealed class Sender(IServiceProvider serviceProvider) : ISender
{
    private static readonly ConcurrentDictionary<Type, object> Pipelines = new();

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var pipeline = (RequestPipeline<TResponse>)Pipelines.GetOrAdd(
            request.GetType(),
            static (requestType, responseType) =>
                Activator.CreateInstance(typeof(RequestPipeline<,>).MakeGenericType(requestType, responseType))!,
            typeof(TResponse));

        return pipeline.Run(request, serviceProvider, cancellationToken);
    }

    private abstract class RequestPipeline<TResponse>
    {
        public abstract Task<TResponse> Run(
            IRequest<TResponse> request,
            IServiceProvider serviceProvider,
            CancellationToken cancellationToken);
    }

    private sealed class RequestPipeline<TRequest, TResponse> : RequestPipeline<TResponse>
        where TRequest : IRequest<TResponse>
    {
        public override async Task<TResponse> Run(
            IRequest<TResponse> request,
            IServiceProvider serviceProvider,
            CancellationToken cancellationToken)
        {
            var typedRequest = (TRequest)request;
            await Validate(typedRequest, serviceProvider, cancellationToken);

            var handler = serviceProvider.GetRequiredService<IRequestHandler<TRequest, TResponse>>();
            return await handler.Handle(typedRequest, cancellationToken);
        }

        private static async Task Validate(
            TRequest request,
            IServiceProvider serviceProvider,
            CancellationToken cancellationToken)
        {
            var failures = new List<ValidationFailure>();
            foreach (var validator in serviceProvider.GetServices<IValidator<TRequest>>())
            {
                var result = await validator.ValidateAsync(request, cancellationToken);
                failures.AddRange(result.Errors);
            }

            if (failures.Count == 0)
            {
                return;
            }

            var errors = failures
                .GroupBy(failure => failure.PropertyName)
                .ToDictionary(group => group.Key, group => group.Select(failure => failure.ErrorMessage).ToArray());

            throw new RequestValidationException(errors);
        }
    }
}
