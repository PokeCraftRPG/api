using FluentValidation;
using Krakenar.Contracts;
using Logitar;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using PokeGame.Core;
using PokeGame.Core.Identity;
using PokeGame.Core.Permissions;

namespace PokeGame.Api.Extensions;

internal static class ErrorExtensions
{
  public static ProblemDetails CreateProblemDetails(this ProblemDetailsFactory factory, HttpContext httpContext, int statusCode, Error error)
  {
    ProblemDetails problemDetails = factory.CreateProblemDetails(
      httpContext,
      statusCode,
      title: error.Code.Humanize(),
      type: null,
      detail: error.Message,
      instance: httpContext.Request.GetDisplayUrl());

    problemDetails.Extensions["error"] = error;

    return problemDetails;
  }

  public static int GetStatusCode(this Exception exception)
  {
    if (exception is IdentityException || exception is ValidationException)
    {
      return StatusCodes.Status400BadRequest;
    }
    if (exception is PermissionDeniedException)
    {
      return StatusCodes.Status403Forbidden;
    }
    // TODO(fpion): if (exception is NotFoundException)
    //{
    //  return StatusCodes.Status404NotFound;
    //}
    if (exception is ConflictException /*|| exception is ImmutablePropertyException*/)
    {
      return StatusCodes.Status409Conflict;
    }
    // TODO(fpion): if (exception is MediaTypeNotSupportedException)
    //{
    //  return StatusCodes.Status415UnsupportedMediaType;
    //}
    return StatusCodes.Status500InternalServerError;
  }

  public static Error ToError(this Exception exception)
  {
    if (exception is IdentityException)
    {
      return new InvalidCredentialsError();
    }
    if (exception is ErrorException errorException)
    {
      return errorException.Error;
    }
    if (exception is ValidationException validation)
    {
      Error error = new(exception.GetErrorCode(), "Validation failed.");
      error.Data["Failures"] = validation.Errors;
      return error;
    }
    return new Error(exception);
  }
}
