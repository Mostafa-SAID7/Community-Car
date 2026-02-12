using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CommunityCar.Mvc.Controllers.Base;
using CommunityCar.Mvc.ViewModels.Support;

namespace CommunityCar.Mvc.Controllers.Common;

public class SupportController : BaseController
{
    private readonly ILogger<SupportController> _logger;

    public SupportController(ILogger<SupportController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public IActionResult FAQ()
    {
        var faqs = new List<FAQItemViewModel>
        {
            new FAQItemViewModel
            {
                Category = "Getting Started",
                Question = "How do I create an account?",
                Answer = "Click on the 'Register' button in the top right corner and fill out the registration form with your details."
            },
            new FAQItemViewModel
            {
                Category = "Getting Started",
                Question = "How do I reset my password?",
                Answer = "Click on 'Forgot Password' on the login page and follow the instructions sent to your email."
            },
            new FAQItemViewModel
            {
                Category = "Community",
                Question = "How do I create a post?",
                Answer = "Navigate to the Posts section and click 'Create New Post'. Fill in the title, content, and any tags."
            },
            new FAQItemViewModel
            {
                Category = "Community",
                Question = "How do I join a group?",
                Answer = "Browse groups, find one you're interested in, and click the 'Join' button on the group page."
            },
            new FAQItemViewModel
            {
                Category = "Questions & Answers",
                Question = "How do I ask a question?",
                Answer = "Go to the Questions section and click 'Ask Question'. Provide a clear title and detailed description."
            },
            new FAQItemViewModel
            {
                Category = "Questions & Answers",
                Question = "How does voting work?",
                Answer = "You can upvote helpful questions and answers, or downvote those that aren't helpful. Your votes help surface quality content."
            },
            new FAQItemViewModel
            {
                Category = "Reviews",
                Question = "How do I write a review?",
                Answer = "Navigate to the item you want to review and click 'Write a Review'. Provide a rating and detailed feedback."
            },
            new FAQItemViewModel
            {
                Category = "Events",
                Question = "How do I create an event?",
                Answer = "Go to the Events section and click 'Create Event'. Fill in the event details including date, time, and location."
            },
            new FAQItemViewModel
            {
                Category = "Privacy & Security",
                Question = "How is my data protected?",
                Answer = "We use industry-standard encryption and security measures to protect your personal information."
            },
            new FAQItemViewModel
            {
                Category = "Privacy & Security",
                Question = "Can I delete my account?",
                Answer = "Yes, you can delete your account from your profile settings. Note that this action is permanent."
            }
        };

        return View(faqs);
    }

    [HttpGet]
    public IActionResult ContactUs()
    {
        return View(new ContactUsViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ContactUs(ContactUsViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            // TODO: Implement email sending logic
            _logger.LogInformation(
                "Contact form submitted - Name: {Name}, Email: {Email}, Subject: {Subject}",
                model.Name,
                model.Email,
                model.Subject);

            TempData["Success"] = "Thank you for contacting us! We'll get back to you soon.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing contact form");
            ModelState.AddModelError("", "An error occurred while sending your message. Please try again.");
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult Guidelines()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Terms()
    {
        return View();
    }

    [HttpGet]
    public IActionResult PrivacyPolicy()
    {
        return View();
    }

    [HttpGet]
    [Authorize]
    public IActionResult ReportIssue()
    {
        return View(new ReportIssueViewModel());
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public IActionResult ReportIssue(ReportIssueViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var userId = GetCurrentUserId();
            
            // TODO: Implement issue reporting logic
            _logger.LogInformation(
                "Issue reported by user {UserId} - Type: {Type}, Description: {Description}",
                userId,
                model.IssueType,
                model.Description);

            TempData["Success"] = "Thank you for reporting this issue. Our team will investigate it.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing issue report");
            ModelState.AddModelError("", "An error occurred while submitting your report. Please try again.");
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult Documentation()
    {
        return View();
    }

    [HttpGet]
    public IActionResult APIDocumentation()
    {
        return View();
    }
}
