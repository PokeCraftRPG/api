namespace PokeGame.Core.Assets;

public sealed class MediaTypeNotSupportedException : Exception
{
  public MediaTypeNotSupportedException(string mediaType) : base("The specified media type is not supported.")
  {
    Data["MediaType"] = mediaType;
  }
}
