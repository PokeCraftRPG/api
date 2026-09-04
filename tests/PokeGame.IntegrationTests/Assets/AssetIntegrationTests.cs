using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using PokeGame.Builders;
using PokeGame.Core.Assets;
using PokeGame.Core.Assets.Models;
using PokeGame.Core.Permissions;

namespace PokeGame.Assets;

[Trait(Traits.Category, Categories.Integration)]
public class AssetIntegrationTests : IntegrationTests
{
  private readonly IAssetService _assetService;

  public AssetIntegrationTests()
  {
    _assetService = ServiceProvider.GetRequiredService<IAssetService>();
  }

  [Fact(DisplayName = "It should throw ValidationException when the stream is null.")]
  public async Task Given_NullStream_When_Upload_Then_ValidationException()
  {
    UploadAssetPayload payload = new("photo.jpg", fileSize: 1, stream: null);

    var exception = await Assert.ThrowsAsync<ValidationException>(async () => await _assetService.UploadAsync(payload));
    Assert.Contains(exception.Errors, error => error.PropertyName == nameof(UploadAssetPayload.Stream));
  }

  [Fact(DisplayName = "It should throw ValidationException when the stream is not readable.")]
  public async Task Given_UnreadableStream_When_Upload_Then_ValidationException()
  {
    await using NonReadableStream stream = new();
    UploadAssetPayload payload = new("photo.jpg", fileSize: 1, stream);

    var exception = await Assert.ThrowsAsync<ValidationException>(async () => await _assetService.UploadAsync(payload));
    Assert.Contains(exception.Errors, error => error.PropertyName == nameof(UploadAssetPayload.Stream) && error.ErrorCode == "StreamValidator");
  }

  [Fact(DisplayName = "It should throw ValidationException when the payload is invalid.")]
  public async Task Given_InvalidPayload_When_Upload_Then_ValidationException()
  {
    await using MemoryStream stream = new([0x00]);
    UploadAssetPayload payload = new(string.Empty, fileSize: 0, stream);

    await Assert.ThrowsAsync<ValidationException>(async () => await _assetService.UploadAsync(payload));
  }

  [Fact(DisplayName = "It should throw PermissionDeniedException when uploading an asset.")]
  public async Task Given_NotAllowed_When_Upload_Then_PermissionDeniedException()
  {
    Context.User = new UserBuilder(Faker).Build();

    await using MemoryStream stream = new(CreateBmp());
    UploadAssetPayload payload = new("denied.bmp", stream.Length, stream);

    PermissionDeniedException exception = await Assert.ThrowsAsync<PermissionDeniedException>(async () => await _assetService.UploadAsync(payload));
    Assert.Equal(Context.ActorId?.Value, exception.Principal);
    Assert.Equal("Upload", exception.Action);
    Assert.Null(exception.Resource);
    Assert.Equal(Context.WorldId.EntityId, exception.WorldId);
  }

  [Fact(DisplayName = "It should throw MediaTypeNotSupportedException when the media type is not supported.")]
  public async Task Given_UnsupportedMediaType_When_Upload_Then_MediaTypeNotSupportedException()
  {
    await using MemoryStream stream = new(CreateBmp());
    UploadAssetPayload payload = new("unsupported.bmp", stream.Length, stream);

    var exception = await Assert.ThrowsAsync<MediaTypeNotSupportedException>(async () => await _assetService.UploadAsync(payload));
    Assert.Equal("image/bmp", exception.MediaType);
  }

  [Fact(DisplayName = "It should upload an image asset.")]
  public async Task Given_ImageFile_When_Upload_Then_Uploaded()
  {
    string path = GetResourcePath("sample.jpg");
    Assert.True(File.Exists(path), $"Add a JPEG file at '{path}'.");

    await using FileStream stream = File.OpenRead(path);
    UploadAssetPayload payload = new(Path.GetFileName(path), stream.Length, stream);

    AssetDto? asset = await _assetService.UploadAsync(payload);
    Assert.NotNull(asset);

    Assert.NotEqual(Guid.Empty, asset.Id);
    Assert.Equal(1, asset.Version);
    Assert.Equal(Actor, asset.CreatedBy);
    Assert.Equal(DateTime.UtcNow, asset.CreatedOn, TimeSpan.FromSeconds(10));
    Assert.Equal(asset.CreatedBy, asset.UpdatedBy);

    Assert.Equal(AssetKind.Image, asset.Kind);
    Assert.Equal("sample", asset.File.Name);
    Assert.Equal("jpg", asset.File.Extension);
    Assert.Equal("image/jpeg", asset.File.MimeType);
    Assert.Equal(stream.Length, asset.File.Size);
    Assert.NotNull(asset.Dimensions);
    Assert.True(asset.Dimensions.Width > 0);
    Assert.True(asset.Dimensions.Height > 0);
    Assert.Null(asset.Duration);

    string storedPath = GetStoredPath(asset);
    Assert.True(File.Exists(storedPath));
    Assert.Equal(stream.Length, new FileInfo(storedPath).Length);
  }

  [Fact(DisplayName = "It should upload a video asset.")]
  public async Task Given_VideoFile_When_Upload_Then_Uploaded()
  {
    string path = GetResourcePath("sample.mp4");
    Assert.True(File.Exists(path), $"Add an MP4 file at '{path}'.");

    await using FileStream stream = File.OpenRead(path);
    UploadAssetPayload payload = new(Path.GetFileName(path), stream.Length, stream);

    AssetDto? asset = await _assetService.UploadAsync(payload);
    Assert.NotNull(asset);

    Assert.NotEqual(Guid.Empty, asset.Id);
    Assert.Equal(1, asset.Version);
    Assert.Equal(Actor, asset.CreatedBy);
    Assert.Equal(DateTime.UtcNow, asset.CreatedOn, TimeSpan.FromSeconds(10));
    Assert.Equal(asset.CreatedBy, asset.UpdatedBy);

    Assert.Equal(AssetKind.Video, asset.Kind);
    Assert.Equal("sample", asset.File.Name);
    Assert.Equal("mp4", asset.File.Extension);
    Assert.Equal("video/mp4", asset.File.MimeType);
    Assert.Equal(stream.Length, asset.File.Size);
    Assert.NotNull(asset.Dimensions);
    Assert.True(asset.Dimensions.Width > 0);
    Assert.True(asset.Dimensions.Height > 0);
    Assert.NotNull(asset.Duration);
    Assert.True(asset.Duration > TimeSpan.Zero);

    string storedPath = GetStoredPath(asset);
    Assert.True(File.Exists(storedPath));
    Assert.Equal(stream.Length, new FileInfo(storedPath).Length);
  }

  [Fact(DisplayName = "It should read an asset by ID.")]
  public async Task Given_Id_When_Read_Then_Read()
  {
    string path = GetResourcePath("sample.jpg");
    await using FileStream stream = File.OpenRead(path);
    UploadAssetPayload payload = new(Path.GetFileName(path), stream.Length, stream);

    AssetDto uploaded = (await _assetService.UploadAsync(payload))!;

    AssetDto? asset = await _assetService.ReadAsync(uploaded.Id);
    Assert.NotNull(asset);
    Assert.Equal(uploaded.Id, asset.Id);
    Assert.Equal(AssetKind.Image, asset.Kind);
    Assert.Equal(uploaded.File.Name, asset.File.Name);
    Assert.Equal(uploaded.File.Extension, asset.File.Extension);
    Assert.Equal(uploaded.File.MimeType, asset.File.MimeType);
    Assert.Equal(uploaded.File.Size, asset.File.Size);
  }

  [Fact(DisplayName = "It should return null when the asset was not found.")]
  public async Task Given_Missing_When_Read_Then_NullReturned()
  {
    Assert.Null(await _assetService.ReadAsync(Guid.NewGuid()));
  }

  private string GetStoredPath(AssetDto asset)
  {
    string directory = Path.Combine(StorageRootPath, "assets", Context.WorldId.EntityId.ToString("N"), asset.Kind.ToString()).ToLowerInvariant();
    return Path.Combine(directory, $"{asset.Id:N}.{asset.File.Extension}");
  }

  private static string GetResourcePath(string fileName) => Path.Combine(AppContext.BaseDirectory, "Assets", fileName);

  private static byte[] CreateBmp()
  {
    const int width = 1;
    const int height = 1;
    const int rowSize = 4;
    const int pixelDataOffset = 54;
    int fileSize = pixelDataOffset + rowSize;

    byte[] bytes = new byte[fileSize];
    bytes[0] = (byte)'B';
    bytes[1] = (byte)'M';
    BitConverter.GetBytes(fileSize).CopyTo(bytes, 2);
    BitConverter.GetBytes(pixelDataOffset).CopyTo(bytes, 10);
    BitConverter.GetBytes(40).CopyTo(bytes, 14);
    BitConverter.GetBytes(width).CopyTo(bytes, 18);
    BitConverter.GetBytes(height).CopyTo(bytes, 22);
    BitConverter.GetBytes((short)1).CopyTo(bytes, 26);
    BitConverter.GetBytes((short)24).CopyTo(bytes, 28);
    return bytes;
  }

  private sealed class NonReadableStream : Stream
  {
    public override bool CanRead => false;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => throw new NotSupportedException();
    public override long Position
    {
      get => throw new NotSupportedException();
      set => throw new NotSupportedException();
    }

    public override void Flush() => throw new NotSupportedException();
    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
  }
}
