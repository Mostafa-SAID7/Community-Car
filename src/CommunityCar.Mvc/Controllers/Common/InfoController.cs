using Microsoft.AspNetCore.Mvc;
using CommunityCar.Mvc.Controllers.Base;

namespace CommunityCar.Mvc.Controllers.Common;

public class InfoController : BaseController
{
    private readonly ILogger<InfoController> _logger;

    public InfoController(ILogger<InfoController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult About()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Features()
    {
        return View();
    }

    [HttpGet]
    public IActionResult HowItWorks()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Team()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Careers()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Press()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Blog()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Roadmap()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Statistics()
    {
        // TODO: Implement real statistics from database
        ViewBag.TotalUsers = 10000;
        ViewBag.TotalPosts = 50000;
        ViewBag.TotalQuestions = 15000;
        ViewBag.TotalGroups = 500;
        ViewBag.TotalEvents = 1200;
        ViewBag.TotalReviews = 8000;

        return View();
    }

    [HttpGet]
    public IActionResult Pricing()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Partners()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Testimonials()
    {
        return View();
    }
}
