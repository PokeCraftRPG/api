using FluentValidation;
using Krakenar.Contracts;
using Logitar;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using PokeGame.Api.Models.Errors;
using PokeGame.Core;
using PokeGame.Core.Assets;
using PokeGame.Core.Identity;
using PokeGame.Core.Membership;
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
    if (exception is ValidationException)
    {
      return StatusCodes.Status400BadRequest;
    }
    if (exception is InvalidOneTimePasswordException || exception is OneTimePasswordNotFoundException)
    {
      return StatusCodes.Status401Unauthorized;
    }
    if (exception is AuthenticationFlowNotAllowedException || exception is PermissionDeniedException)
    {
      return StatusCodes.Status403Forbidden;
    }
    if (exception is NotFoundException)
    {
      return StatusCodes.Status404NotFound;
    }
    if (exception is ConflictException)
    {
      return StatusCodes.Status409Conflict;
    }
    if (exception is MemberInvitationExpiredException)
    {
      return StatusCodes.Status410Gone;
    }
    if (exception is MediaTypeNotSupportedException)
    {
      return StatusCodes.Status415UnsupportedMediaType;
    }
    if (exception is DomainException)
    {
      return StatusCodes.Status422UnprocessableEntity;
    }
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
      return new ValidationError(validation.Errors);
    }

    Error error = new(exception.GetErrorCode(), exception.Message);
    foreach (DictionaryEntry data in exception.Data)
    {
      string? key = data.Key.ToString();
      if (key is not null)
      {
        error.Data[key] = data.Value;
      }
    }
    return error;
  }
}

/* TODO(fpion): ErrorException
 * MemberInvitationExpiredException → 410 Gone (0)
 * PermissionDeniedException (0)
 * NotFoundException (2) → RegionsNotFoundException?
 * IdentityException (3)
 * ConflictException (11)
 * DomainException (13)
 *
 * ValidationException
 * - if command: 422 Unprocessable Entity
 * - if query: 400 Bad Request
 */
