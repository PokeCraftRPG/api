using MetadataExtractor;
using MetadataExtractor.Formats.FileType;
using MetadataExtractor.Formats.Gif;
using MetadataExtractor.Formats.Jpeg;
using MetadataExtractor.Formats.Png;
using MetadataExtractor.Formats.QuickTime;
using MetadataExtractor.Formats.WebP;
using PokeGame.Core.Assets;
using IODirectory = System.IO.Directory;
using MetadataExtractorDirectory = MetadataExtractor.Directory;

namespace PokeGame.Infrastructure.Assets;

public class AssetManager : IAssetManager
{
  private readonly StorageSettings _storage;

  public AssetManager(StorageSettings storage)
  {
    _storage = storage;
  }

  public async Task<AssetMetadata> ExtractMetadataAsync(Stream stream, CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested();

    long? position = stream.CanSeek ? stream.Position : null;

    try
    {
      Stream source;

      if (stream.CanSeek)
      {
        stream.Seek(0, SeekOrigin.Begin);
        source = stream;
      }
      else
      {
        using MemoryStream buffer = new();
        await stream.CopyToAsync(buffer, cancellationToken);
        buffer.Position = 0;
        source = buffer;
      }

      AssetMetadata? svg = await SvgHelper.ExtractMetadataAsync(source, cancellationToken);
      if (svg is not null)
      {
        return svg;
      }

      source.Seek(0, SeekOrigin.Begin);

      IReadOnlyList<MetadataExtractorDirectory> directories = ImageMetadataReader.ReadMetadata(stream);

      cancellationToken.ThrowIfCancellationRequested();

      string mimeType = ExtractMimeType(directories);
      Dimensions? dimensions = ExtractImageDimensions(directories) ?? ExtractVideoDimensions(directories);
      TimeSpan? duration = ExtractDuration(directories);

      return new AssetMetadata(mimeType, dimensions, duration);
    }
    finally
    {
      if (position.HasValue)
      {
        stream.Seek(position.Value, SeekOrigin.Begin);
      }
    }
  }
  private static string ExtractMimeType(IEnumerable<MetadataExtractorDirectory> directories)
  {
    FileTypeDirectory fileType = directories.OfType<FileTypeDirectory>().Single();
    return fileType.GetString(FileTypeDirectory.TagDetectedFileMimeType) ?? throw new InvalidDataException("The MIME type could not be determined.");
  }
  private static Dimensions? ExtractImageDimensions(IEnumerable<MetadataExtractorDirectory> directories)
  {
    foreach (MetadataExtractorDirectory directory in directories)
    {
      Dimensions? dimensions = directory switch
      {
        GifHeaderDirectory gif => TryGetDimensions(gif, GifHeaderDirectory.TagImageWidth, GifHeaderDirectory.TagImageHeight),
        JpegDirectory jpeg => TryGetDimensions(jpeg, JpegDirectory.TagImageWidth, JpegDirectory.TagImageHeight),
        PngDirectory png => TryGetDimensions(png, PngDirectory.TagImageWidth, PngDirectory.TagImageHeight),
        WebPDirectory webp => TryGetDimensions(webp, WebPDirectory.TagImageWidth, WebPDirectory.TagImageHeight),
        _ => null
      };
      if (dimensions is not null)
      {
        return dimensions;
      }
    }
    return null;
  }
  private static Dimensions? ExtractVideoDimensions(IEnumerable<MetadataExtractorDirectory> directories)
  {
    Dimensions? selected = null;
    long selectedArea = 0;

    foreach (QuickTimeTrackHeaderDirectory track in directories.OfType<QuickTimeTrackHeaderDirectory>())
    {
      Dimensions? dimensions = TryGetDimensions(track, QuickTimeTrackHeaderDirectory.TagWidth, QuickTimeTrackHeaderDirectory.TagHeight);
      if (dimensions is null)
      {
        continue;
      }

      if (track.TryGetDouble(QuickTimeTrackHeaderDirectory.TagRotation, out double rotation) && SwapsDimensions(rotation))
      {
        dimensions = new Dimensions(dimensions.Height, dimensions.Width);
      }

      long area = (long)dimensions.Width * dimensions.Height;
      if (area > selectedArea)
      {
        selected = dimensions;
        selectedArea = area;
      }
    }

    return selected;
  }
  private static Dimensions? TryGetDimensions(MetadataExtractorDirectory directory, int widthTag, int heightTag)
  {
    return directory.TryGetInt32(widthTag, out int width) && directory.TryGetInt32(heightTag, out int height) && width > 0 && height > 0
      ? new Dimensions(width, height)
      : null;
  }
  private static TimeSpan? ExtractDuration(IEnumerable<MetadataExtractorDirectory> directories)
  {
    foreach (QuickTimeMovieHeaderDirectory movie in directories.OfType<QuickTimeMovieHeaderDirectory>())
    {
      if (movie.GetObject(QuickTimeMovieHeaderDirectory.TagDuration) is TimeSpan duration)
      {
        return duration;
      }
    }
    return null;
  }
  private static bool SwapsDimensions(double rotation)
  {
    double normalized = ((rotation % 360) + 360) % 360;
    const double tolerance = 0.5;
    return Math.Abs(normalized - 90) < tolerance || Math.Abs(normalized - 270) < tolerance;
  }

  public async Task StoreAsync(Asset asset, Stream stream, CancellationToken cancellationToken)
  {
    string directory = CreateDirectory(asset);
    string file = $"{asset.EntityId:N}.{asset.File.Extension}";
    string path = Path.Combine(directory, file);

    FileStreamOptions options = new()
    {
      Mode = FileMode.CreateNew,
      Access = FileAccess.Write,
      Share = FileShare.None,
      Options = FileOptions.Asynchronous | FileOptions.SequentialScan
    };
    await using FileStream output = new(path, options);
    await stream.CopyToAsync(output, cancellationToken);
  }
  private string CreateDirectory(Asset asset)
  {
    string path = Path.Combine(_storage.RootPath, asset.WorldId.EntityId.ToString("N"), asset.Kind.ToString()).ToLowerInvariant();
    IODirectory.CreateDirectory(path);
    return path;
  }
}
