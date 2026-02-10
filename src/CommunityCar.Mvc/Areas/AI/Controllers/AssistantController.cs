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
        public async Task<IActionResult> SendMessage([FromBody] ChatMessageRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Message))
            {
                return Json(new { success = false, message = "Message cannot be empty." });
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
                return Json(new { success = false, message = "Failed to get AI response. Please try again." });
            }
        }

        [HttpPost("UploadDataset")]
        public async Task<IActionResult> UploadDataset(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return Json(new { success = false, message = "No file uploaded." });
            }

            // Validate file size (max 10MB)
            if (file.Length > 10 * 1024 * 1024)
            {
                return Json(new { success = false, message = "File size exceeds 10MB limit." });
            }

            // Validate file extension
            var allowedExtensions = new[] { ".csv", ".json", ".txt", ".xlsx" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                return Json(new { success = false, message = "Invalid file format. Supported: CSV, JSON, TXT, XLSX" });
            }

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                // Read file content
                using var reader = new StreamReader(file.OpenReadStream());
                var content = await reader.ReadToEndAsync();

                // Process the dataset with AI
                var prompt = $"I've uploaded a dataset file named '{file.FileName}'. Here's the content:\n\n{content.Substring(0, Math.Min(content.Length, 5000))}\n\nPlease analyze this data and provide insights.";
                var response = await _assistantService.GetChatResponseAsync(userId, prompt);

                return Json(new
                {
                    success = true,
                    fileName = file.FileName,
                    fileSize = FormatFileSize(file.Length),
                    response = response
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading dataset");
                return Json(new { success = false, message = "Failed to process dataset. Please try again." });
            }
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }

    public class ChatMessageRequest
    {
        public string Message { get; set; }
    }
}
