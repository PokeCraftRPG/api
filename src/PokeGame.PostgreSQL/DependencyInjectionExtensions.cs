using Logitar;
using Logitar.EventSourcing.EntityFrameworkCore.PostgreSQL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Infrastructure;

namespace PokeGame.PostgreSQL;

public static class DependencyInjectionExtensions
{
  public static IServiceCollection AddPokeGamePostgreSQL(this IServiceCollection services, IConfiguration configuration)
  {
    string? connectionString = EnvironmentHelper.TryGetString("POSTGRESQLCONNSTR_Pokemon") ?? configuration.GetConnectionString("PostgreSQL");
    if (string.IsNullOrWhiteSpace(connectionString))
    {
      throw new InvalidOperationException("The PostgreSQL connection string was not found.");
    }
    return services.AddPokeGamePostgreSQL(connectionString);
  }
  public static IServiceCollection AddPokeGamePostgreSQL(this IServiceCollection services, string connectionString)
  {
    return services
      .AddLogitarEventSourcingWithEntityFrameworkCorePostgreSQL(connectionString)
      .AddDbContext<PokemonContext>(options => options.UseNpgsql(connectionString, options => options.MigrationsAssembly("PokeGame.PostgreSQL")));
  }
}
