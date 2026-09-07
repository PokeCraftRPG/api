using Krakenar.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Primitives;
using PokeGame.Api.Constants;
using PokeGame.Api.Extensions;
using PokeGame.Core.Worlds;
using PokeGame.Core.Worlds.Models;

namespace PokeGame.Api.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
internal sealed class RequireWorld : Attribute, IAsyncResourceFilter
{
  public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
  {
    HttpContext httpContext = context.HttpContext;
    if (!httpContext.Request.Headers.TryGetValue(Headers.World, out StringValues values))
    {
      await WriteErrorAsync(httpContext, StatusCodes.Status400BadRequest, CreateMissingWorldError());
      return;
    }

    IReadOnlyCollection<string> sanitized = values.Sanitize();
    if (sanitized.Count < 1)
    {
      await WriteErrorAsync(httpContext, StatusCodes.Status400BadRequest, CreateMissingWorldError());
      return;
    }

    if (sanitized.Count > 1)
    {
      Error error = new(code: "InvalidWorldHeader", message: "Only one world header value is expected, but multiple were specified.");
      error.Data["Header"] = Headers.World;
      error.Data["SanitizedCount"] = sanitized.Count;
      error.Data["TotalCount"] = values.Count;
      await WriteErrorAsync(httpContext, StatusCodes.Status400BadRequest, error);
      return;
    }

    string value = sanitized.Single();
    bool parsed = Guid.TryParse(value, out Guid id);
    IWorldService worldService = httpContext.RequestServices.GetRequiredService<IWorldService>();
    WorldDto? world = await worldService.ReadAsync(parsed ? id : null, parsed ? null : value, cancellationToken: httpContext.RequestAborted);
    if (world is null)
    {
      Error error = new(code: "WorldNotFound", message: "The specified world could not be found.");
      error.Data["World"] = value;
      error.Data["Header"] = Headers.World;
      await WriteErrorAsync(httpContext, StatusCodes.Status404NotFound, error);
      return;
    }

    httpContext.SetWorld(world);

    await next();
  }

  private static Error CreateMissingWorldError()
  {
    Error error = new(code: "MissingWorld", message: "A world is required.");
    error.Data["Header"] = Headers.World;
    return error;
  }

  private static async Task WriteErrorAsync(HttpContext httpContext, int statusCode, Error error)
  {
    ProblemDetailsFactory problemDetailsFactory = httpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
    ProblemDetails problemDetails = problemDetailsFactory.CreateProblemDetails(httpContext, statusCode, error);
    httpContext.Response.StatusCode = statusCode;

    IProblemDetailsService problemDetailsService = httpContext.RequestServices.GetRequiredService<IProblemDetailsService>();
    ProblemDetailsContext problemDetailsContext = new()
    {
      HttpContext = httpContext,
      ProblemDetails = problemDetails
    };

    await problemDetailsService.WriteAsync(problemDetailsContext);
  }
}
