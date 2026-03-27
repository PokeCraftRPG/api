using Krakenar.Contracts.Sessions;

namespace PokeGame.Core.Accounts.Models;

public record SignInAccountResult
{
  public bool IsPasswordRequired { get; set; }
  public string? ProfileCompletionToken { get; set; }
  public Session? Session { get; set; }

  public static SignInAccountResult CompleteProfile(string token) => new()
  {
    ProfileCompletionToken = token
  };

  public static SignInAccountResult RequirePassword() => new()
  {
    IsPasswordRequired = true
  };

  public static SignInAccountResult Success(Session session) => new()
  {
    Session = session
  };
}
