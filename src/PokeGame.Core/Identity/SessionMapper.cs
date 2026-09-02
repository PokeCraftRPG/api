using Krakenar.Contracts;
using Krakenar.Contracts.Constants;
using Krakenar.Contracts.Sessions;
using PokeGame.Core.Identity.Models;

namespace PokeGame.Core.Identity;

internal partial class SessionMapper
{
  private readonly Guid? _currentId = null;

  public SessionMapper(Guid? currentId = null)
  {
    _currentId = currentId;
  }

  public SessionModel Map(Session source)
  {
    SessionModel destination = new()
    {
      Id = source.Id,
      CreatedOn = source.CreatedOn,
      UpdatedOn = source.UpdatedOn,
      IsCurrent = source.Id == _currentId
    };

    Dictionary<string, string>? headers = null;
    foreach (CustomAttribute customAttribute in source.CustomAttributes)
    {
      switch (customAttribute.Key)
      {
        case "AdditionalInformation":
          headers = JsonSerializer.Deserialize<Dictionary<string, string>>(customAttribute.Value);
          break;
        case "IpAddress":
          destination.IpAddress = NormalizeIpAddress(customAttribute.Value);
          break;
      }
    }
    if (headers is not null)
    {
      string userAgent = TryGetHeader(headers, Headers.UserAgent)?.Trim() ?? string.Empty;
      string secureClientHintUserAgent = TryGetHeader(headers, Headers.SecureClientHintUserAgent)?.Trim() ?? string.Empty;
      string secureClientHintUserAgentMobile = TryGetHeader(headers, Headers.SecureClientHintUserAgentMobile)?.Trim() ?? string.Empty;
      string secureClientHintUserAgentPlatform = TryGetHeader(headers, Headers.SecureClientHintUserAgentPlatform)?.Trim() ?? string.Empty;
      destination.Browser = ParseBrowser(secureClientHintUserAgent, userAgent);
      destination.OperatingSystem = ParseOperatingSystem(secureClientHintUserAgentPlatform, userAgent);
      destination.DeviceType = ParseDeviceType(secureClientHintUserAgentMobile, secureClientHintUserAgentPlatform, userAgent);
    }

    return destination;
  }

  private static string NormalizeIpAddress(string value)
  {
    if (IPAddress.TryParse(value, out IPAddress? ipAddress))
    {
      return ipAddress.IsIPv4MappedToIPv6 ? ipAddress.MapToIPv4().ToString() : ipAddress.ToString();
    }

    return value;
  }

  private static string? TryGetHeader(IReadOnlyDictionary<string, string> headers, string key) => headers.TryGetValue(key, out string? value) ? value : null;

  private static string? ParseBrowser(string secureClientHintUserAgent, string userAgent)
  {
    if (string.IsNullOrEmpty(secureClientHintUserAgent) && string.IsNullOrEmpty(userAgent))
    {
      return null;
    }

    IReadOnlyList<string> brands = ParseBrands(secureClientHintUserAgent);

    if (ContainsBrand(brands, "Microsoft Edge"))
    {
      return "Microsoft Edge";
    }
    if (ContainsBrand(brands, "Google Chrome"))
    {
      return "Google Chrome";
    }
    if (ContainsBrand(brands, "Opera"))
    {
      return "Opera";
    }
    if (ContainsBrand(brands, "Brave"))
    {
      return "Brave";
    }

    if (Contains(userAgent, "Edg/") || Contains(userAgent, "EdgA/") || Contains(userAgent, "EdgiOS/"))
    {
      return "Microsoft Edge";
    }
    if (Contains(userAgent, "OPR/") || Contains(userAgent, "Opera/"))
    {
      return "Opera";
    }
    if (Contains(userAgent, "SamsungBrowser/"))
    {
      return "Samsung Internet";
    }
    if (Contains(userAgent, "Firefox/") || Contains(userAgent, "FxiOS/"))
    {
      return "Mozilla Firefox";
    }
    if (Contains(userAgent, "Chrome/") || Contains(userAgent, "CriOS/"))
    {
      return "Google Chrome";
    }
    if (Contains(userAgent, "Safari/") && Contains(userAgent, "Version/"))
    {
      return "Safari";
    }

    if (ContainsBrand(brands, "Chromium"))
    {
      return "Chromium";
    }

    return null;
  }

  private static string? ParseOperatingSystem(string secureClientHintUserAgentPlatform, string userAgent)
  {
    if (string.IsNullOrEmpty(secureClientHintUserAgentPlatform) && string.IsNullOrEmpty(userAgent))
    {
      return null;
    }

    string platform = Unquote(secureClientHintUserAgentPlatform);

    if (!string.IsNullOrWhiteSpace(platform))
    {
      if (platform.Equals("Windows", StringComparison.OrdinalIgnoreCase))
      {
        return "Windows";
      }
      if (platform.Equals("macOS", StringComparison.OrdinalIgnoreCase) || platform.Equals("Mac OS", StringComparison.OrdinalIgnoreCase))
      {
        return "macOS";
      }
      if (platform.Equals("Android", StringComparison.OrdinalIgnoreCase))
      {
        return "Android";
      }
      if (platform.Equals("iOS", StringComparison.OrdinalIgnoreCase))
      {
        return "iOS";
      }
      if (platform.Equals("Chrome OS", StringComparison.OrdinalIgnoreCase))
      {
        return "ChromeOS";
      }
      if (platform.Equals("Linux", StringComparison.OrdinalIgnoreCase))
      {
        return "Linux";
      }
      return platform;
    }

    if (Contains(userAgent, "Windows NT"))
    {
      return "Windows";
    }
    if (Contains(userAgent, "Android"))
    {
      return "Android";
    }
    if (Contains(userAgent, "iPhone") || Contains(userAgent, "iPad") || Contains(userAgent, "iPod"))
    {
      return "iOS";
    }
    if (Contains(userAgent, "CrOS"))
    {
      return "ChromeOS";
    }
    if (Contains(userAgent, "Macintosh") || Contains(userAgent, "Mac OS X"))
    {
      return "macOS";
    }
    if (Contains(userAgent, "Linux"))
    {
      return "Linux";
    }

    return null;
  }

  private static DeviceType? ParseDeviceType(string secureClientHintUserAgentMobile, string secureClientHintUserAgentPlatform, string userAgent)
  {
    if (string.IsNullOrEmpty(secureClientHintUserAgentMobile) && string.IsNullOrEmpty(secureClientHintUserAgentPlatform) && string.IsNullOrEmpty(userAgent))
    {
      return null;
    }

    if (Contains(userAgent, "iPad") || Contains(userAgent, "Tablet") || Contains(userAgent, "Kindle") || Contains(userAgent, "Silk/")
      || (Contains(userAgent, "Android") && !Contains(userAgent, "Mobile")))
    {
      return DeviceType.Tablet;
    }

    if (secureClientHintUserAgentMobile.Trim().Equals("?1", StringComparison.OrdinalIgnoreCase))
    {
      return DeviceType.Mobile;
    }

    if (Contains(userAgent, "iPhone") || Contains(userAgent, "iPod") || Contains(userAgent, "Windows Phone") || Contains(userAgent, "Mobile"))
    {
      return DeviceType.Mobile;
    }

    if (secureClientHintUserAgentMobile.Trim().Equals("?0", StringComparison.OrdinalIgnoreCase))
    {
      return DeviceType.Desktop;
    }

    string platform = Unquote(secureClientHintUserAgentPlatform);

    if (platform.Equals("Windows", StringComparison.OrdinalIgnoreCase) || platform.Equals("macOS", StringComparison.OrdinalIgnoreCase) ||
      platform.Equals("Linux", StringComparison.OrdinalIgnoreCase) || platform.Equals("Chrome OS", StringComparison.OrdinalIgnoreCase))
    {
      return DeviceType.Desktop;
    }

    if (Contains(userAgent, "Windows NT") || Contains(userAgent, "Macintosh") || Contains(userAgent, "CrOS") || Contains(userAgent, "X11"))
    {
      return DeviceType.Desktop;
    }

    return null;
  }

  private static bool Contains(string value, string expected) => value.Contains(expected, StringComparison.OrdinalIgnoreCase);
  private static bool ContainsBrand(IEnumerable<string> brands, string expected) => brands.Any(brand => brand.Contains(expected, StringComparison.OrdinalIgnoreCase));

  private static string Unquote(string value) => value.Trim().Trim('"');

  private static IReadOnlyList<string> ParseBrands(string secChUa)
  {
    if (string.IsNullOrWhiteSpace(secChUa))
    {
      return [];
    }

    return BrandRegex()
      .Matches(secChUa)
      .Select(match => match.Groups["brand"].Value)
      .Where(brand => !IsGreaseBrand(brand))
      .Distinct(StringComparer.OrdinalIgnoreCase)
      .ToArray();
  }
  [GeneratedRegex("\"(?<brand>[^\"]+)\"\\s*;\\s*v=\"(?<version>[^\"]+)\"", RegexOptions.CultureInvariant)]
  private static partial Regex BrandRegex();
  private static bool IsGreaseBrand(string brand) => brand.Contains("Not", StringComparison.OrdinalIgnoreCase) && brand.Contains("Brand", StringComparison.OrdinalIgnoreCase);
}
