using Logitar;
using Microsoft.Extensions.Configuration;

namespace PokeGame.Infrastructure.Messaging;

internal record RabbitMQSettings
{
  private const string SectionKey = "RabbitMQ";

  public string Host { get; set; } = string.Empty;
  public string VirtualHost { get; set; } = string.Empty;
  public string Username { get; set; } = string.Empty;
  public string Password { get; set; } = string.Empty;

  public static RabbitMQSettings Initialize(IConfiguration configuration)
  {
    RabbitMQSettings settings = configuration.GetSection(SectionKey).Get<RabbitMQSettings>() ?? new();

    settings.Host = EnvironmentHelper.GetString("RABBITMQ_HOST", settings.Host);
    settings.VirtualHost = EnvironmentHelper.GetString("RABBITMQ_VIRTUAL_HOST", settings.VirtualHost);
    settings.Username = EnvironmentHelper.GetString("RABBITMQ_USERNAME", settings.Username);
    settings.Password = EnvironmentHelper.GetString("RABBITMQ_PASSWORD", settings.Password);

    return settings;
  }
}
