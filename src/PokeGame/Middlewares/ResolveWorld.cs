using Krakenar.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Primitives;
using PokeGame.Core;
using PokeGame.Core.Worlds;
using PokeGame.Core.Worlds.Models;
using PokeGame.Extensions;

namespace PokeGame.Middlewares;

internal class ResolveWorld
{
  public const string Header = "X-World";

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
    if (context.Request.Headers.TryGetValue(Header, out StringValues values))
    {
      IReadOnlyCollection<string> sanitized = values.Sanitize();
      if (sanitized.Count > 1)
      {
        Error error = new("InvalidWorldHeader", "At most one world header value is expected, but multiple were provided.");
        error.Data["Header"] = Header;
        error.Data["SanitizedCount"] = sanitized.Count;
        error.Data["TotalCount"] = values.Count;
        await WriteResponseAsync(context, StatusCodes.Status400BadRequest, error);
        return;
      }
      else if (sanitized.Count == 1)
      {
        string key = Slug.Normalize(sanitized.Single());
        bool parsed = Guid.TryParse(key, out Guid id);
        WorldModel? world = await worldService.ReadAsync(parsed ? id : null, key);
        if (world is null)
        {
          Error error = new("WorldNotFound", "The specified world was not found.");
          error.Data["Header"] = Header;
          error.Data["World"] = sanitized.Single();
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
