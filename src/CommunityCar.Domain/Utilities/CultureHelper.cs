using System.Globalization;

namespace CommunityCar.Domain.Utilities;

public static class CultureHelper
{
    public static readonly string DefaultCulture = "en";
    public static readonly string ArabicCulture = "ar";

    public static readonly string[] SupportedCultures = { "en", "ar" };

    public static bool IsRtl => CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft;

    public static string CurrentCultureName => CultureInfo.CurrentUICulture.Name;

    public static string GetDirection() => IsRtl ? "rtl" : "ltr";

    public static string GetTextAlign() => IsRtl ? "right" : "left";
    
    public static string GetFloatAlign() => IsRtl ? "end" : "start";
}
