using Krakenar.Contracts;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using PokeGame.Api.Extensions;
using PokeGame.Api.Settings;

namespace PokeGame.Api;

internal class ExceptionHandler : IExceptionHandler
{
  private readonly ApiSettings _apiSettings;
  private readonly ProblemDetailsFactory _problemDetailsFactory;
  private readonly IProblemDetailsService _problemDetailsService;

  public ExceptionHandler(ApiSettings apiSettings, ProblemDetailsFactory problemDetailsFactory, IProblemDetailsService problemDetailsService)
  {
    _apiSettings = apiSettings;
    _problemDetailsFactory = problemDetailsFactory;
    _problemDetailsService = problemDetailsService;
  }

  public virtual async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
  {
    int statusCode = exception.GetStatusCode();
    if (statusCode == StatusCodes.Status500InternalServerError && !_apiSettings.ExposeErrorDetail)
    {
      return false;
    }

    Error error = exception.ToError();
    ProblemDetails problemDetails = _problemDetailsFactory.CreateProblemDetails(httpContext, statusCode, error);

    httpContext.Response.StatusCode = statusCode;
    ProblemDetailsContext context = new()
    {
      HttpContext = httpContext,
      ProblemDetails = problemDetails,
      Exception = exception
    };
    return await _problemDetailsService.TryWriteAsync(context);
  }
}
