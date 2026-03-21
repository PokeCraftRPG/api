namespace PokeGame.Infrastructure;

internal class EventBus : Logitar.EventSourcing.Infrastructure.EventBus
{
  public EventBus(IServiceProvider serviceProvider) : base(serviceProvider)
  {
  }
}
