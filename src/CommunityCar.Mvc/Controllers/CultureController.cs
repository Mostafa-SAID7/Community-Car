using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Mvc.Controllers;

public class CultureController : Controller
{
    [HttpPost]
    [HttpGet]
    public IActionResult SetLanguage(string culture, string returnUrl)
    {
        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
        );

        // Prepend culture to returnUrl if it's not already there
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            // Remove any trailing slashes to normalize
            returnUrl = returnUrl.TrimEnd('/');
            if (string.IsNullOrEmpty(returnUrl)) returnUrl = "/";

            var segments = returnUrl.Split('/', StringSplitOptions.RemoveEmptyEntries);
            
            // Handle root "/" case
            if (segments.Length == 0)
            {
                returnUrl = $"/{culture}";
            }
            // Handle existing culture prefix
            else if (segments[0] == "en" || segments[0] == "ar")
            {
                segments[0] = culture;
                returnUrl = "/" + string.Join("/", segments);
            }
            // Prepend new culture
            else
            {
                returnUrl = $"/{culture}/{string.Join("/", segments)}";
            }
        }
        else
        {
            returnUrl = $"/{culture}";
        }

        return LocalRedirect(returnUrl);
    }
}
