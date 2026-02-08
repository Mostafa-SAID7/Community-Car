using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Mvc.Controllers.Legal;

[Route("{culture:alpha}/Legal")]
public class LegalController : Controller
{
    [HttpGet("Terms")]
    public IActionResult Terms()
    {
        return View();
    }
    
    [HttpGet("Privacy")]
    public IActionResult Privacy()
    {
        return View();
    }
}
