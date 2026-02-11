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
                
                // Create uploads/datasets directory if it doesn't exist
                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "datasets");
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                // Generate unique filename with timestamp and user ID
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var safeFileName = Path.GetFileNameWithoutExtension(file.FileName)
                    .Replace(" ", "_")
                    .Replace("-", "_");
                var uniqueFileName = $"{safeFileName}_{timestamp}_{userId?.Substring(0, 8)}{extension}";
                var filePath = Path.Combine(uploadsPath, uniqueFileName);

                // Save file to disk
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Read file content for analysis
                string content;
                using (var reader = new StreamReader(file.OpenReadStream()))
                {
                    content = await reader.ReadToEndAsync();
                }

                // Process the dataset with AI
                var prompt = $"I've uploaded a dataset file named '{file.FileName}' (saved as '{uniqueFileName}'). Here's a preview of the content:\n\n{content.Substring(0, Math.Min(content.Length, 5000))}\n\nPlease analyze this data and provide insights. The file is now saved and can be referenced in future conversations.";
                var response = await _assistantService.GetChatResponseAsync(userId, prompt);

                // Add file path info to response
                var enhancedResponse = $"✅ Dataset saved successfully!\n\n📁 File: {uniqueFileName}\n📍 Location: /uploads/datasets/\n📊 Size: {FormatFileSize(file.Length)}\n\n{response}\n\n💡 You can reference this dataset in future questions!";

                return Json(new
                {
                    success = true,
                    fileName = file.FileName,
                    savedFileName = uniqueFileName,
                    fileSize = FormatFileSize(file.Length),
                    filePath = $"/uploads/datasets/{uniqueFileName}",
                    response = enhancedResponse
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading dataset");
                return Json(new { success = false, message = "Failed to process dataset. Please try again." });
            }
        }

        [HttpGet("ListDatasets")]
        public IActionResult ListDatasets()
        {
            try
            {
                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "datasets");
                
                if (!Directory.Exists(uploadsPath))
                {
                    return Json(new { success = true, datasets = new List<object>() });
                }

                var files = Directory.GetFiles(uploadsPath)
                    .Select(f => new FileInfo(f))
                    .OrderByDescending(f => f.CreationTime)
                    .Take(20) // Limit to 20 most recent files
                    .Select(f => new
                    {
                        fileName = f.Name,
                        size = FormatFileSize(f.Length),
                        uploadedDate = f.CreationTime.ToString("yyyy-MM-dd HH:mm"),
                        path = $"/uploads/datasets/{f.Name}"
                    })
                    .ToList();

                return Json(new { success = true, datasets = files });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing datasets");
                return Json(new { success = false, message = "Failed to list datasets." });
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
