using CommunityCar.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;

namespace CommunityCar.Mvc.Areas.AI.Controllers
{
    [Area("AI")]
    [Route("{culture:alpha}/[area]/[controller]")]
    [Authorize]
    public class AssistantController : Controller
    {
        private readonly IAssistantService _assistantService;
        private readonly IStringLocalizer<AssistantController> _localizer;
        private readonly ILogger<AssistantController> _logger;

        public AssistantController(
            IAssistantService assistantService,
            IStringLocalizer<AssistantController> localizer,
            ILogger<AssistantController> logger)
        {
            _assistantService = assistantService;
            _localizer = localizer;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("SendMessage")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage([FromBody] ChatMessageRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Message))
            {
                return BadRequest("Message cannot be empty.");
            }

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var response = await _assistantService.GetChatResponseAsync(userId, request.Message);
                
                return Json(new { success = true, response });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SendMessage");
                return Json(new { success = false, message = "Failed to get AI response." });
            }
        }
    }

    public class ChatMessageRequest
    {
        public string Message { get; set; }
    }
}
