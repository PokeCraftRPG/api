namespace PokeGame.Settings;

internal record SessionCookieSettings
{
  public SameSiteMode SameSite { get; set; }
}
