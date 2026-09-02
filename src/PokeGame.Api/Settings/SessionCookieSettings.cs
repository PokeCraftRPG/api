namespace PokeGame.Api.Settings;

internal record SessionCookieSettings
{
  public SameSiteMode SameSite { get; set; }
}
