namespace PokeGame.Infrastructure.Converters;

internal static class JsonSerializerOptionsExtensions
{
  public static void RegisterConverters(this JsonSerializerOptions options)
  {
    options.Converters.Add(new AbilityIdConverter());
    options.Converters.Add(new AccuracyConverter());
    options.Converters.Add(new AssetIdConverter());
    options.Converters.Add(new CatchRateConverter());
    options.Converters.Add(new ContentConverter());
    options.Converters.Add(new EmailAddressConverter());
    options.Converters.Add(new FormIdConverter());
    options.Converters.Add(new FriendshipConverter());
    options.Converters.Add(new GenderRatioConverter());
    options.Converters.Add(new GenusConverter());
    options.Converters.Add(new ItemIdConverter());
    options.Converters.Add(new KeyConverter());
    options.Converters.Add(new LevelConverter());
    options.Converters.Add(new LicenseConverter());
    options.Converters.Add(new MemberInvitationIdConverter());
    options.Converters.Add(new MoneyConverter());
    options.Converters.Add(new MoveIdConverter());
    options.Converters.Add(new NameConverter());
    options.Converters.Add(new NumberConverter());
    options.Converters.Add(new PowerConverter());
    options.Converters.Add(new PowerPointsConverter());
    options.Converters.Add(new PriceConverter());
    options.Converters.Add(new RegionIdConverter());
    options.Converters.Add(new SpeciesIdConverter());
    options.Converters.Add(new SummaryConverter());
    options.Converters.Add(new TrainerIdConverter());
    options.Converters.Add(new UserIdConverter());
    options.Converters.Add(new VarietyIdConverter());
    options.Converters.Add(new WorldIdConverter());
  }
}
