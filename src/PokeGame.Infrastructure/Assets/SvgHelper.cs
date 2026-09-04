using PokeGame.Core.Assets;

namespace PokeGame.Infrastructure.Assets;

internal static class SvgHelper
{
  public static async Task<AssetMetadata?> ExtractMetadataAsync(Stream stream, CancellationToken cancellationToken)
  {
    try
    {
      XmlReaderSettings settings = new()
      {
        Async = true,
        DtdProcessing = DtdProcessing.Prohibit,
        XmlResolver = null,
        IgnoreComments = true,
        IgnoreProcessingInstructions = true
      };
      using XmlReader reader = XmlReader.Create(stream, settings);

      while (await reader.ReadAsync())
      {
        cancellationToken.ThrowIfCancellationRequested();

        if (reader.NodeType != XmlNodeType.Element)
        {
          continue;
        }

        if (!string.Equals(reader.LocalName, "svg", StringComparison.OrdinalIgnoreCase))
        {
          return null;
        }

        string? width = reader.GetAttribute("width");
        string? height = reader.GetAttribute("height");
        string? viewBox = reader.GetAttribute("viewBox");

        Dimensions? dimensions = TryGetSvgDimensions(width, height) ?? TryGetSvgViewBoxDimensions(viewBox);

        return new AssetMetadata(MediaTypeNames.Image.Svg, dimensions, Duration: null);
      }
    }
    catch (XmlException)
    {
    }

    return null;
  }

  private static Dimensions? TryGetSvgDimensions(string? width, string? height)
  {
    return TryParseSvgLength(width, out double parsedWidth) && TryParseSvgLength(height, out double parsedHeight)
      ? CreateDimensions(parsedWidth, parsedHeight)
      : null;
  }
  private static bool TryParseSvgLength(string? value, out double length)
  {
    length = 0;

    if (string.IsNullOrWhiteSpace(value))
    {
      return false;
    }
    string formatted = value.Trim();

    // NOTE(fpion): unitless SVG lengths are effectively user units. "px" is also safe. Don't invent a DPI conversion for cm/in/em/%.
    if (formatted.EndsWith("px", StringComparison.OrdinalIgnoreCase))
    {
      formatted = formatted[..^2].Trim();
    }
    else if (formatted.Any(char.IsLetter) || formatted.Contains('%'))
    {
      return false;
    }

    return double.TryParse(formatted, NumberStyles.Float, CultureInfo.InvariantCulture, out length) && length > 0;
  }

  private static Dimensions? TryGetSvgViewBoxDimensions(string? viewBox)
  {
    if (string.IsNullOrWhiteSpace(viewBox))
    {
      return null;
    }

    string[] values = viewBox.Split([' ', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    if (values.Length != 4
      || !double.TryParse(values[2], NumberStyles.Float, CultureInfo.InvariantCulture, out double width)
      || !double.TryParse(values[3], NumberStyles.Float, CultureInfo.InvariantCulture, out double height))
    {
      return null;
    }

    return CreateDimensions(width, height);
  }
  private static Dimensions? CreateDimensions(double width, double height)
  {
    if (width <= 0 || height <= 0 || width > int.MaxValue || height > int.MaxValue)
    {
      return null;
    }
    return new Dimensions((int)Math.Ceiling(width), (int)Math.Ceiling(height));
  }
}
