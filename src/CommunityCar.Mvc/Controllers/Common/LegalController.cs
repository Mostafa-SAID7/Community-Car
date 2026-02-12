using Microsoft.AspNetCore.Mvc;
using CommunityCar.Mvc.Controllers.Base;

namespace CommunityCar.Mvc.Controllers.Common;

public class LegalController : BaseController
{
    private readonly ILogger<LegalController> _logger;

    public LegalController(ILogger<LegalController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Terms()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Privacy()
    {
        return View();
    }

    [HttpGet]
    public IActionResult CookiePolicy()
    {
        return View();
    }

    [HttpGet]
    public IActionResult AcceptableUse()
    {
        return View();
    }

    [HttpGet]
    public IActionResult CommunityGuidelines()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Copyright()
    {
        return View();
    }

    [HttpGet]
    public IActionResult DMCA()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Disclaimer()
    {
        return View();
    }

    [HttpGet]
    public IActionResult DataProtection()
    {
        return View();
    }

    [HttpGet]
    public IActionResult GDPR()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Accessibility()
    {
        return View();
    }
}
