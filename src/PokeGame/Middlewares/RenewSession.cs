using Krakenar.Contracts.Sessions;
using PokeGame.Constants;
using PokeGame.Extensions;

namespace PokeGame.Middlewares;

internal class RenewSession
{
  private readonly RequestDelegate _next;

  public RenewSession(RequestDelegate next)
  {
    _next = next;
  }

  public virtual async Task InvokeAsync(HttpContext context, ISessionService sessionService)
  {
    if (!context.IsSignedIn())
    {
      if (context.Request.Cookies.TryGetValue(Cookies.RefreshToken, out string? refreshToken) && refreshToken is not null)
      {
        try
        {
          RenewSessionPayload payload = new(refreshToken, context.GetSessionCustomAttributes());
          Session session = await sessionService.RenewAsync(payload);
          context.SignIn(session);
        }
        catch (Exception)
        {
          context.Response.Cookies.Delete(Cookies.RefreshToken);
        }
      }
    }

    await _next(context);
  }
}
