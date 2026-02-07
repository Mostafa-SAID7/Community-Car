using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Web.Controllers;

[Route("[controller]")]
public class InfoController : Controller
{
    // GET: Info/Terms
    [HttpGet("Terms")]
    public IActionResult Terms()
    {
        return View();
    }

    // GET: Info/Privacy
    [HttpGet("Privacy")]
    public IActionResult Privacy()
    {
        return View();
    }

    // GET: Info/Support
    [HttpGet("Support")]
    public IActionResult Support()
    {
        return View();
    }

    // GET: Info/About
    [HttpGet("About")]
    public IActionResult About()
    {
        return View();
    }

    // GET: Info/Contact
    [HttpGet("Contact")]
    public IActionResult Contact()
    {
        return View();
    }
}
