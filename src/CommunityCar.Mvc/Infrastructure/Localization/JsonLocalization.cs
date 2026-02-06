using System.Text.Json;
using Microsoft.Extensions.Localization;

namespace CommunityCar.Web.Infrastructure.Localization;

public class JsonStringLocalizer : IStringLocalizer
{
    private readonly Dictionary<string, string> _localizationData;

    public JsonStringLocalizer(string filePath)
    {
        if (File.Exists(filePath))
        {
            var jsonContent = File.ReadAllText(filePath);
            _localizationData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonContent)
                ?.ToDictionary(x => x.Key, x => x.Value.GetProperty("Dashboard").ToString()) // Simplified for now
                ?? new Dictionary<string, string>();
            
            // Re-flattening if needed, but let's assume a simpler structure or handle nesting
            _localizationData = FlattenJson(jsonContent);
        }
        else
        {
            _localizationData = new Dictionary<string, string>();
        }
    }

    private Dictionary<string, string> FlattenJson(string json)
    {
        var result = new Dictionary<string, string>();
        using var doc = JsonDocument.Parse(json);
        Traverse(doc.RootElement, string.Empty, result);
        return result;
    }

    private void Traverse(JsonElement element, string prefix, Dictionary<string, string> result)
    {
        foreach (var prop in element.EnumerateObject())
        {
            var key = string.IsNullOrEmpty(prefix) ? prop.Name : $"{prefix}.{prop.Name}";
            if (prop.Value.ValueKind == JsonValueKind.Object)
            {
                Traverse(prop.Value, key, result);
            }
            else
            {
                result[key] = prop.Value.ToString();
            }
        }
    }

    public LocalizedString this[string name]
    {
        get
        {
            var value = _localizationData.TryGetValue(name, out var val) ? val : name;
            return new LocalizedString(name, value, !(_localizationData.ContainsKey(name)));
        }
    }

    public LocalizedString this[string name, params object[] arguments]
    {
        get
        {
            var format = _localizationData.TryGetValue(name, out var val) ? val : name;
            return new LocalizedString(name, string.Format(format, arguments), !(_localizationData.ContainsKey(name)));
        }
    }

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        return _localizationData.Select(x => new LocalizedString(x.Key, x.Value));
    }
}

public class JsonStringLocalizerFactory : IStringLocalizerFactory
{
    private readonly string _basePath;

    public JsonStringLocalizerFactory(string basePath)
    {
        _basePath = basePath;
    }

    public IStringLocalizer Create(Type resourceSource)
    {
        var culture = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        var filePath = Path.Combine(_basePath, $"{culture}.json");
        return new JsonStringLocalizer(filePath);
    }

    public IStringLocalizer Create(string baseName, string location)
    {
        var culture = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        var filePath = Path.Combine(_basePath, $"{culture}.json");
        return new JsonStringLocalizer(filePath);
    }
}
