using Microsoft.AspNetCore.Mvc;

namespace CommunityCar.Mvc.Controllers.Support;

[Route("Support")]
public class SupportController : Controller
{
    [HttpGet("")]
    public IActionResult Index()
    {
        return View();
    }
    
    [HttpGet("FAQ")]
    public IActionResult FAQ()
    {
        return View();
    }
    
    [HttpGet("Contact")]
    public IActionResult Contact()
    {
        return View();
    }
    
    [HttpPost("Contact")]
    [ValidateAntiForgeryToken]
    public IActionResult Contact(ContactFormModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        
        // TODO: Send email or save to database
        TempData["Success"] = "Your message has been sent successfully. We'll get back to you soon!";
        return RedirectToAction(nameof(Index));
    }
}

public class ContactFormModel
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
