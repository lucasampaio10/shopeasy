using Microsoft.AspNetCore.Http;
using ShopEasy.BuildingBlocks.Results;

namespace ShopEasy.BuildingBlocks.Infrastructure.Web;

public static class ResultExtensions
{
    /// <summary>Maps a use case outcome to an HTTP response.</summary>
    public static IResult ToHttpResult<TValue>(this Result<TValue> result, Func<TValue, IResult> onSuccess) =>
        result.IsSuccess ? onSuccess(result.Value) : result.Error!.ToProblem();

    public static IResult ToHttpResult(this Result result, Func<IResult> onSuccess) =>
        result.IsSuccess ? onSuccess() : result.Error!.ToProblem();

    private static IResult ToProblem(this Error error) => Microsoft.AspNetCore.Http.Results.Problem(
        statusCode: error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError,
        },
        title: error.Message,
        extensions: new Dictionary<string, object?> { ["code"] = error.Code });
}
