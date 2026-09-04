using Krakenar.Contracts;
using Logitar;

namespace PokeGame.Core.Assets;

public sealed class MediaTypeNotSupportedException : ErrorException
{
  private const string ErrorMessage = "The specified media type is not supported.";

  public string MediaType
  {
    get => (string)Data[nameof(MediaType)]!;
    private set => Data[nameof(MediaType)] = value;
  }

  public override Error Error
  {
    get
    {
      Error error = new(this.GetErrorCode(), ErrorMessage);
      error.Data[nameof(MediaType)] = MediaType;
      return error;
    }
  }

  public MediaTypeNotSupportedException(string mediaType)
    : base(BuildMessage(mediaType))
  {
    MediaType = mediaType;
  }

  private static string BuildMessage(string contentType) => new ErrorMessageBuilder(ErrorMessage)
    .AddData(nameof(MediaType), contentType)
    .Build();
}
