namespace PokeGame.Core.Assets.Models;

public record FileDto : IFile
{
  public string Name { get; set; }
  public string Extension { get; set; }
  public string MimeType { get; set; }
  public long Size { get; set; }

  public FileDto() : this(string.Empty, string.Empty, string.Empty, 0)
  {
  }

  [JsonConstructor]
  public FileDto(string name, string extension, string mimeType, long size)
  {
    Name = name;
    Extension = extension;
    MimeType = mimeType;
    Size = size;
  }

  public FileDto(IFile file) : this(file.Name, file.Extension, file.MimeType, file.Size)
  {
  }
}
