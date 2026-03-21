namespace PokeGame.Extensions;

internal static class HttpContextExtensions
{
  public static Uri GetBaseUri(this HttpContext context) => new($"{context.Request.Scheme}://{context.Request.Host}", UriKind.Absolute);
}
