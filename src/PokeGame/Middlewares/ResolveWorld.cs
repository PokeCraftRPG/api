using Krakenar.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Primitives;
using PokeGame.Constants;
using PokeGame.Core.Worlds;
using PokeGame.Core.Worlds.Models;
using PokeGame.Extensions;

namespace PokeGame.Middlewares;

internal class ResolveWorld
{
  private readonly RequestDelegate _next;
  private readonly ProblemDetailsFactory _problemDetailsFactory;
  private readonly IProblemDetailsService _problemDetailsService;

  public ResolveWorld(RequestDelegate next, ProblemDetailsFactory problemDetailsFactory, IProblemDetailsService problemDetailsService)
  {
    _next = next;
    _problemDetailsFactory = problemDetailsFactory;
    _problemDetailsService = problemDetailsService;
  }

  public async Task InvokeAsync(HttpContext context, IWorldService worldService)
  {
    if (context.User.Identity is not null
        && context.User.Identity.IsAuthenticated
        && context.Request.Headers.TryGetValue(Headers.World, out StringValues values))
    {
      IReadOnlyCollection<string> sanitized = values.Sanitize();
      if (sanitized.Count > 1)
      {
        Error error = new(code: "InvalidWorldHeader", message: "Only one world header value is expected, but multiple were specified.");
        error.Data["Header"] = Headers.World;
        error.Data["SanitizedCount"] = sanitized.Count;
        error.Data["TotalCount"] = values.Count;
        await WriteResponseAsync(context, StatusCodes.Status400BadRequest, error);
        return;
      }
      else if (sanitized.Count == 1)
      {
        string value = sanitized.Single();
        bool parsed = Guid.TryParse(value, out Guid id);
        WorldModel? world = parsed ? await worldService.ReadAsync(id) : null;
        if (world is null)
        {
          Error error = new(code: "WorldNotFound", message: "The specified world could not be found.");
          error.Data["World"] = value;
          error.Data["Header"] = Headers.World;
          await WriteResponseAsync(context, StatusCodes.Status404NotFound, error);
          return;
        }

        context.SetWorld(world);
      }
    }

    await _next(context);
  }

  private async Task WriteResponseAsync(HttpContext httpContext, int statusCode, Error error)
  {
    ProblemDetails problemDetails = _problemDetailsFactory.CreateProblemDetails(httpContext, statusCode, error);

    httpContext.Response.StatusCode = statusCode;
    ProblemDetailsContext context = new()
    {
      HttpContext = httpContext,
      ProblemDetails = problemDetails
    };
    _ = await _problemDetailsService.TryWriteAsync(context);
  }
}
