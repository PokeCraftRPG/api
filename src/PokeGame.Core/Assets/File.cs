using FluentValidation;

namespace PokeGame.Core.Assets;

public interface IFile
{
  string Name { get; }
  string Extension { get; }
  string MimeType { get; }
  long Size { get; }
}

public sealed record AssetFile : IFile
{
  public const int ExtensionMaximumLength = 16;
  public const int MimeTypeMaximumLength = byte.MaxValue;
  public const int NameMaximumLength = byte.MaxValue;

  public string Name { get; }
  public string Extension { get; }
  public string MimeType { get; }
  public long Size { get; }

  public AssetFile(string name, string extension, string mimeType, long size)
  {
    Name = name.Trim();
    Extension = extension.Trim();
    MimeType = mimeType.Trim();
    Size = size;
    new FileValidator().ValidateAndThrow(this);
  }

  public static AssetFile From(IFile file) => new(file.Name, file.Extension, file.MimeType, file.Size);
}

internal class FileValidator : AbstractValidator<IFile>
{
  public FileValidator()
  {
    RuleFor(x => x.Name).FileName();
    RuleFor(x => x.Extension).NotEmpty().MaximumLength(AssetFile.ExtensionMaximumLength);
    RuleFor(x => x.MimeType).NotEmpty().MaximumLength(AssetFile.MimeTypeMaximumLength);
    RuleFor(x => x.Size).FileSize();
  }
}
