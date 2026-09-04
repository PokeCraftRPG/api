using FluentValidation;

namespace PokeGame.Core.Assets.Models;

public record UploadAssetPayload
{
  public string FileName { get; set; }
  public long FileSize { get; set; }

  [JsonIgnore]
  public Stream? Stream { get; set; }

  public UploadAssetPayload() : this(string.Empty)
  {
  }

  public UploadAssetPayload(string fileName, long fileSize = 0, Stream? stream = null)
  {
    FileName = fileName;
    FileSize = fileSize;

    Stream = stream;
  }

  public void Validate() => new Validator().ValidateAndThrow(this);

  private class Validator : AbstractValidator<UploadAssetPayload>
  {
    public Validator()
    {
      RuleFor(x => x.FileName).FileName();
      RuleFor(x => x.FileSize).FileSize();

      RuleFor(x => x.Stream).NotNull()
        .Must(stream => stream is null || stream.CanRead).WithErrorCode("StreamValidator").WithMessage("'{PropertyName}' must be readable.");
    }
  }
}
