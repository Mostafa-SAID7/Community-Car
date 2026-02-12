using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CommunityCar.Domain.Interfaces.Services;
using CommunityCar.Infrastructure.Interfaces.ML;
using CommunityCar.Infrastructure.Services.ML;
using System.IO;
using CommunityCar.Domain.Models.AI;
using System.Collections.Concurrent;

namespace CommunityCar.Infrastructure.Services.AI
{
    public partial class AssistantService : CommunityCar.Domain.Interfaces.Services.IAssistantService
    {
        private readonly ILogger<AssistantService> _logger;
        private readonly IPredictionService _predictionService;
        private readonly ISentimentAnalysisService _sentimentService;
        private readonly string _datasetsPath;
        
        // Store conversation contexts per user (in-memory for now, could be moved to Redis/Database)
        private static readonly ConcurrentDictionary<string, ConversationContext> _conversations = new();

        public AssistantService(
            ILogger<AssistantService> logger,
            IPredictionService predictionService,
            ISentimentAnalysisService sentimentService)
        {
            _logger = logger;
            _predictionService = predictionService;
            _sentimentService = sentimentService;
            _datasetsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "datasets");
        }

        public async Task<string> GetChatResponseAsync(string userId, string message)
        {
            try
            {
                // Simple deliberation delay to feel more "AI"
                await Task.Delay(800);

                var sentiment = _sentimentService.AnalyzeSentiment(message);
                var intent = _predictionService.PredictIntent(message);

                _logger.LogInformation("ML Assistant: Intent={Intent}, Sentiment={Sentiment}", intent.PredictedIntent, sentiment.Prediction);

                // Normalize message for better matching
                var lowerMessage = message.ToLower();

                // Handle numbered selections (1-5)
                var numberedResponse = HandleNumberedSelection(message.Trim(), lowerMessage);
                if (!string.IsNullOrEmpty(numberedResponse))
                {
                    return numberedResponse;
                }

                // Advanced dataset analysis - check if user is asking about data
                var datasetResponse = await AnalyzeUserQueryWithDatasets(userId, message, lowerMessage);
                if (!string.IsNullOrEmpty(datasetResponse))
                {
                    return datasetResponse;
                }

                // Check if user is asking about uploaded datasets
                if (ContainsAny(lowerMessage, "dataset", "uploaded", "file", "my data", "show data"))
                {
                    var datasetInfo = GetAvailableDatasets(userId);
                    if (!string.IsNullOrEmpty(datasetInfo))
                    {
                        return $"📊 Available Datasets:\n\n{datasetInfo}\n\nYou can ask me questions like:\n• 'What's the average price?'\n• 'Show me the most expensive items'\n• 'Analyze the maintenance costs'\n• 'What patterns do you see?'";
                    }
                }

                // Handle negative sentiment
                if (!sentiment.Prediction && sentiment.Probability > 0.7)
                {
                    return "I'm sorry you're feeling frustrated. How can I better assist you with the Community Car platform?";
                }

                // Car Price Queries
                if (ContainsAny(lowerMessage, "price", "cost", "expensive", "cheap", "budget", "afford", "how much"))
                {
                    // First check if we have dataset information
                    var priceDatasetResponse = await AnalyzeUserQueryWithDatasets(userId, message, lowerMessage);
                    if (!string.IsNullOrEmpty(priceDatasetResponse))
                    {
                        return priceDatasetResponse;
                    }
                    
                    // Fall back to generic information
                    if (ContainsAny(lowerMessage, "oil", "oil change"))
                        return "💰 Oil change costs typically range from $30-$75 for conventional oil, and $65-$125 for synthetic oil. Prices vary by location and vehicle type. Check our Reviews section to see what community members paid at local shops!";
                    
                    if (ContainsAny(lowerMessage, "tire", "tires"))
                        return "💰 Tire prices vary widely:\n• Budget tires: $50-$150 per tire\n• Mid-range: $100-$300 per tire\n• Premium/Performance: $150-$500+ per tire\n\nDon't forget installation ($15-$45 per tire) and alignment ($75-$200). Check our Reviews for local shop recommendations!";
                    
                    if (ContainsAny(lowerMessage, "brake", "brakes"))
                        return "💰 Brake service costs:\n• Brake pads replacement: $150-$300 per axle\n• Rotors replacement: $200-$400 per axle\n• Complete brake job: $300-$800\n\nPrices depend on vehicle type and parts quality. Our community has shared many reviews about local mechanics!";
                    
                    if (ContainsAny(lowerMessage, "battery"))
                        return "💰 Car battery prices:\n• Standard battery: $50-$120\n• Premium/AGM battery: $120-$250\n• Installation: Usually free at auto parts stores\n\nBatteries typically last 3-5 years. Check our Guides section for battery maintenance tips!";
                    
                    if (ContainsAny(lowerMessage, "insurance"))
                        return "💰 Car insurance costs vary based on:\n• Your age and driving history\n• Vehicle make/model/year\n• Coverage level\n• Location\n\nAverage: $1,500-$2,500/year. Join our Groups to discuss insurance tips with other members!";
                    
                    return "💰 Car costs vary by make, model, and condition. For specific pricing:\n• Check our Reviews section for real user experiences\n• Join Groups for your car model to get community insights\n• Browse Guides for cost-saving maintenance tips\n\nWhat specific car or service are you asking about?";
                }

                // Maintenance Queries
                if (ContainsAny(lowerMessage, "maintenance", "service", "maintain", "check", "inspect"))
                {
                    if (ContainsAny(lowerMessage, "oil", "oil change"))
                        return "🔧 Oil Change Guide:\n• Frequency: Every 3,000-7,500 miles (conventional) or 7,500-15,000 miles (synthetic)\n• Check your owner's manual for specific intervals\n• DIY: $25-$50 | Shop: $30-$125\n\nCheck our Guides section for step-by-step DIY oil change tutorials!";
                    
                    if (ContainsAny(lowerMessage, "tire", "tires"))
                        return "🔧 Tire Maintenance:\n• Check pressure monthly (proper PSI in door jamb)\n• Rotate every 5,000-7,500 miles\n• Check tread depth (penny test)\n• Alignment check if pulling to one side\n\nOur Guides have detailed tire care articles with photos!";
                    
                    if (ContainsAny(lowerMessage, "brake", "brakes"))
                        return "🔧 Brake Maintenance:\n• Inspect pads every 12,000 miles\n• Replace pads at 3mm thickness\n• Check brake fluid level monthly\n• Listen for squealing or grinding\n\nWarning signs: Squealing, grinding, vibration, or soft pedal. Check our Guides for DIY brake inspection!";
                    
                    if (ContainsAny(lowerMessage, "schedule", "interval"))
                        return "🔧 Basic Maintenance Schedule:\n• Oil change: 3,000-7,500 miles\n• Tire rotation: 5,000-7,500 miles\n• Air filter: 15,000-30,000 miles\n• Brake inspection: 12,000 miles\n• Coolant flush: 30,000-50,000 miles\n\nAlways check your owner's manual! Browse our Guides for detailed maintenance checklists.";
                    
                    return "🔧 Regular maintenance keeps your car running smoothly!\n\nKey areas:\n• Engine (oil, filters, belts)\n• Brakes and tires\n• Fluids (coolant, transmission, brake)\n• Battery and electrical\n\nCheck our Guides section for detailed maintenance tutorials, or ask about a specific system!";
                }

                // Parts Queries
                if (ContainsAny(lowerMessage, "part", "parts", "replace", "replacement"))
                {
                    if (ContainsAny(lowerMessage, "where", "buy", "purchase", "shop"))
                        return "🛒 Where to buy car parts:\n• AutoZone, O'Reilly, Advance Auto (retail)\n• RockAuto.com (online, great prices)\n• Amazon (convenience)\n• Dealer (OEM parts, pricier)\n• Junkyards (budget option)\n\nOur community shares part recommendations in Reviews and Groups!";
                    
                    if (ContainsAny(lowerMessage, "oem", "aftermarket"))
                        return "🔩 OEM vs Aftermarket:\n\nOEM (Original Equipment):\n✅ Perfect fit\n✅ Warranty protection\n❌ More expensive\n\nAftermarket:\n✅ Cheaper\n✅ More options\n❌ Quality varies\n\nFor critical parts (brakes, suspension), many prefer OEM. Check our Reviews for brand recommendations!";
                    
                    return "🔩 Need parts? Our community can help!\n• Check Reviews for part brand recommendations\n• Join Groups for your car model for specific advice\n• Browse Guides for DIY installation tutorials\n\nWhat part are you looking for?";
                }

                // Intent-based responses
                return intent.PredictedIntent switch
                {
                    "Greeting" => "Hello! 👋 I'm your Community Car Assistant. I can help you with:\n\n" +
                                  "1️⃣ Car prices and costs analysis\n" +
                                  "2️⃣ Maintenance schedules and tips\n" +
                                  "3️⃣ Parts recommendations and comparisons\n" +
                                  "4️⃣ Analyze your uploaded datasets\n" +
                                  "5️⃣ Insurance and fuel economy info\n\n" +
                                  "💡 Type a number (1-5) or ask me anything!",
                    
                    "Cars" => "🚗 Community Car connects car enthusiasts!\n\n" +
                              "Choose what interests you:\n" +
                              "1️⃣ Browse Guides - DIY tutorials and tips\n" +
                              "2️⃣ Join Groups - Communities for your car model\n" +
                              "3️⃣ Read Reviews - Real experiences from members\n" +
                              "4️⃣ Find Events - Local car meets and shows\n" +
                              "5️⃣ Analyze Data - Upload and analyze car data\n\n" +
                              "💡 Type a number (1-5) to explore!",
                    
                    "General" => "I'm here to help! Choose an option:\n\n" +
                                 "1️⃣ Ask about car maintenance, prices, or parts\n" +
                                 "2️⃣ Learn how to use Community Car features\n" +
                                 "3️⃣ Get recommendations from our community\n" +
                                 "4️⃣ Analyze uploaded datasets\n" +
                                 "5️⃣ Compare vehicles and costs\n\n" +
                                 "💡 Type a number (1-5) or ask your question!",
                    
                    _ => "I'm here to help with car-related questions!\n\n" +
                         "Quick options:\n" +
                         "1️⃣ Prices (oil changes, tires, brakes, insurance)\n" +
                         "2️⃣ Maintenance (schedules, tips, DIY guides)\n" +
                         "3️⃣ Parts (where to buy, OEM vs aftermarket)\n" +
                         "4️⃣ Data Analysis (analyze your uploaded files)\n" +
                         "5️⃣ Vehicle Comparison (compare makes and models)\n\n" +
                         "💡 Type a number (1-5) or ask me anything!"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating AI response");
                return "I'm sorry, I encountered a bit of a glitch. Could you try asking that again?";
            }
        }

        private bool ContainsAny(string text, params string[] keywords)
        {
            return keywords.Any(keyword => text.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        private string HandleNumberedSelection(string message, string lowerMessage)
        {
            // Check if message is just a number or contains a number selection
            if (message == "1" || lowerMessage.Contains("option 1") || lowerMessage.Contains("choice 1"))
            {
                return "💰 Car Prices & Costs\n\n" +
                       "I can help you with:\n" +
                       "• Oil change costs: $30-$125\n" +
                       "• Tire prices: $50-$500 per tire\n" +
                       "• Brake service: $150-$800\n" +
                       "• Battery replacement: $50-$250\n" +
                       "• Insurance rates: $1,500-$2,500/year\n\n" +
                       "📊 I can also analyze your uploaded price datasets!\n\n" +
                       "What specific pricing would you like to know about?";
            }

            if (message == "2" || lowerMessage.Contains("option 2") || lowerMessage.Contains("choice 2"))
            {
                return "🔧 Maintenance Schedules & Tips\n\n" +
                       "Regular maintenance schedule:\n" +
                       "• Oil change: Every 3,000-7,500 miles\n" +
                       "• Tire rotation: Every 5,000-7,500 miles\n" +
                       "• Air filter: Every 15,000-30,000 miles\n" +
                       "• Brake inspection: Every 12,000 miles\n" +
                       "• Coolant flush: Every 30,000-50,000 miles\n\n" +
                       "📚 Check our Guides section for detailed DIY tutorials!\n" +
                       "📊 Upload your maintenance records for personalized analysis!\n\n" +
                       "What maintenance topic interests you?";
            }

            if (message == "3" || lowerMessage.Contains("option 3") || lowerMessage.Contains("choice 3"))
            {
                return "🔩 Parts Recommendations\n\n" +
                       "Where to buy parts:\n" +
                       "• AutoZone, O'Reilly, Advance Auto (retail stores)\n" +
                       "• RockAuto.com (online, great prices)\n" +
                       "• Amazon (convenience)\n" +
                       "• Dealer (OEM parts, pricier but guaranteed fit)\n" +
                       "• Local junkyards (budget option)\n\n" +
                       "💡 OEM vs Aftermarket:\n" +
                       "• OEM: Perfect fit, warranty protection, more expensive\n" +
                       "• Aftermarket: Cheaper, more options, quality varies\n\n" +
                       "🌟 Check our Reviews section for brand recommendations!\n\n" +
                       "What part are you looking for?";
            }

            if (message == "4" || lowerMessage.Contains("option 4") || lowerMessage.Contains("choice 4"))
            {
                return "📊 Dataset Analysis\n\n" +
                       "I can analyze your car-related data!\n\n" +
                       "Available sample datasets:\n" +
                       "• Car Prices & Values\n" +
                       "• Maintenance Costs\n" +
                       "• Fuel Economy\n" +
                       "• Insurance Rates\n" +
                       "• Repair Costs\n" +
                       "• Tire Data\n" +
                       "• Depreciation\n" +
                       "• Safety Ratings\n" +
                       "• Electric Vehicles\n" +
                       "• Warranty Coverage\n" +
                       "• Common Problems\n\n" +
                       "📤 Upload your own CSV, JSON, or Excel files!\n" +
                       "💬 Ask questions like:\n" +
                       "   • 'What's the average price?'\n" +
                       "   • 'Show me the most expensive items'\n" +
                       "   • 'Analyze maintenance costs'\n\n" +
                       "Type 'show datasets' to see your uploaded files!";
            }

            if (message == "5" || lowerMessage.Contains("option 5") || lowerMessage.Contains("choice 5"))
            {
                return "🚗 Vehicle Comparison\n\n" +
                       "I can help you compare:\n\n" +
                       "📊 By Category:\n" +
                       "• Prices and values\n" +
                       "• Fuel economy (MPG)\n" +
                       "• Insurance rates\n" +
                       "• Safety ratings\n" +
                       "• Maintenance costs\n" +
                       "• Depreciation rates\n" +
                       "• Warranty coverage\n\n" +
                       "💡 Example questions:\n" +
                       "   • 'Compare Honda Accord vs Toyota Camry'\n" +
                       "   • 'Which has better fuel economy?'\n" +
                       "   • 'Show me the cheapest insurance'\n" +
                       "   • 'Best resale value vehicles'\n\n" +
                       "📈 Upload your own comparison data for custom analysis!\n\n" +
                       "What would you like to compare?";
            }

            return null;
        }

        private string GetAvailableDatasets(string userId)
        {
            try
            {
                if (!Directory.Exists(_datasetsPath))
                    return string.Empty;

                var files = Directory.GetFiles(_datasetsPath)
                    .Select(f => new FileInfo(f))
                    .Where(f => userId == null || f.Name.Contains(userId.Substring(0, Math.Min(8, userId.Length))))
                    .OrderByDescending(f => f.CreationTime)
                    .Take(10)
                    .ToList();

                if (!files.Any())
                    return string.Empty;

                var result = new System.Text.StringBuilder();
                foreach (var file in files)
                {
                    var size = FormatFileSize(file.Length);
                    var date = file.CreationTime.ToString("yyyy-MM-dd HH:mm");
                    result.AppendLine($"• {file.Name} ({size}) - Uploaded: {date}");
                }

                return result.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available datasets");
                return string.Empty;
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

        private async Task<string> AnalyzeUserQueryWithDatasets(string userId, string originalMessage, string lowerMessage)
        {
            try
            {
                // Get user's datasets
                var datasets = GetUserDatasetFiles(userId);
                if (!datasets.Any())
                    return null;

                // Detect query intent
                var isAskingAbout = ContainsAny(lowerMessage, "what", "how many", "show", "tell", "find", "average", "total", "sum", "count", "list", "analyze", "compare");
                var isAskingPrice = ContainsAny(lowerMessage, "price", "cost", "expensive", "cheap", "budget");
                var isAskingMaintenance = ContainsAny(lowerMessage, "maintenance", "service", "repair", "fix");
                var isAskingStats = ContainsAny(lowerMessage, "average", "mean", "median", "total", "sum", "count", "statistics", "stats");
                var isAskingPattern = ContainsAny(lowerMessage, "pattern", "trend", "insight", "analysis", "summary");

                if (!isAskingAbout && !isAskingPrice && !isAskingMaintenance && !isAskingStats && !isAskingPattern)
                    return null;

                // Read and analyze the most recent dataset
                var latestDataset = datasets.First();
                var content = await File.ReadAllTextAsync(latestDataset.FullName);
                
                // Analyze based on file type
                var analysis = latestDataset.Extension.ToLower() switch
                {
                    ".csv" => AnalyzeCsvData(content, originalMessage, lowerMessage),
                    ".json" => AnalyzeJsonData(content, originalMessage, lowerMessage),
                    ".txt" => AnalyzeTextData(content, originalMessage, lowerMessage),
                    _ => null
                };

                if (!string.IsNullOrEmpty(analysis))
                {
                    return $"📊 Analysis from '{latestDataset.Name}':\n\n{analysis}\n\n💡 This analysis is based on your uploaded dataset. Upload more data for deeper insights!";
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing datasets");
                return null;
            }
        }

        private List<FileInfo> GetUserDatasetFiles(string userId)
        {
            try
            {
                if (!Directory.Exists(_datasetsPath))
                    return new List<FileInfo>();

                return Directory.GetFiles(_datasetsPath)
                    .Select(f => new FileInfo(f))
                    .Where(f => userId == null || f.Name.Contains(userId.Substring(0, Math.Min(8, userId.Length))))
                    .OrderByDescending(f => f.CreationTime)
                    .ToList();
            }
            catch
            {
                return new List<FileInfo>();
            }
        }

        private string AnalyzeCsvData(string content, string originalMessage, string lowerMessage)
        {
            try
            {
                var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length < 2) return "Dataset appears to be empty or invalid.";

                var headers = lines[0].Split(',').Select(h => h.Trim().Trim('"')).ToArray();
                var dataRows = lines.Skip(1).Select(line => line.Split(',').Select(v => v.Trim().Trim('"')).ToArray()).ToList();

                var result = new System.Text.StringBuilder();
                result.AppendLine($"📋 Dataset Overview (from your uploaded file):");
                result.AppendLine($"• Columns: {string.Join(", ", headers.Take(6))}...");
                result.AppendLine($"• Total Records: {dataRows.Count}");
                result.AppendLine();

                // Detect specific service or vehicle query
                var specificService = FindSpecificService(dataRows, headers, lowerMessage);
                if (!string.IsNullOrEmpty(specificService))
                {
                    return specificService;
                }

                // Urgency analysis
                if (ContainsAny(lowerMessage, "urgent", "critical", "priority", "important", "emergency"))
                {
                    var urgencyColumn = FindColumn(headers, "urgency", "priority");
                    if (urgencyColumn >= 0)
                    {
                        result.AppendLine("🚨 Urgency Analysis:");
                        var urgentServices = dataRows
                            .Where(r => r.Length > urgencyColumn && ContainsAny(r[urgencyColumn].ToLower(), "high", "critical"))
                            .ToList();
                        
                        if (urgentServices.Any())
                        {
                            result.AppendLine($"• {urgentServices.Count} urgent/critical services found");
                            result.AppendLine();
                            foreach (var service in urgentServices.Take(5))
                            {
                                var serviceName = service.Length > 0 ? service[0] : "Unknown";
                                var vehicle = FindColumnValue(service, headers, "vehicle");
                                var price = FindColumnValue(service, headers, "price");
                                var urgency = service[urgencyColumn];
                                result.AppendLine($"⚠️ {serviceName} - {vehicle}");
                                result.AppendLine($"   Priority: {urgency} | Cost: ${price}");
                            }
                        }
                        result.AppendLine();
                    }
                }

                // Warranty analysis
                if (ContainsAny(lowerMessage, "warranty", "guarantee", "covered"))
                {
                    var warrantyColumn = FindColumn(headers, "warranty");
                    if (warrantyColumn >= 0)
                    {
                        result.AppendLine("🛡️ Warranty Information:");
                        var withWarranty = dataRows
                            .Where(r => r.Length > warrantyColumn && !string.IsNullOrEmpty(r[warrantyColumn]) && r[warrantyColumn].ToLower() != "none")
                            .ToList();
                        
                        result.AppendLine($"• {withWarranty.Count} services with warranty");
                        result.AppendLine();
                        foreach (var service in withWarranty.Take(5))
                        {
                            var serviceName = service.Length > 0 ? service[0] : "Unknown";
                            var warranty = service[warrantyColumn];
                            result.AppendLine($"✓ {serviceName}: {warranty} warranty");
                        }
                        result.AppendLine();
                    }
                }

                // Service provider analysis
                if (ContainsAny(lowerMessage, "provider", "shop", "where", "mechanic", "dealer"))
                {
                    var providerColumn = FindColumn(headers, "serviceprovider", "provider", "shop");
                    if (providerColumn >= 0)
                    {
                        result.AppendLine("🏪 Service Providers:");
                        var providers = dataRows
                            .Where(r => r.Length > providerColumn)
                            .GroupBy(r => r[providerColumn])
                            .OrderByDescending(g => g.Count());
                        
                        foreach (var provider in providers)
                        {
                            var avgPrice = provider
                                .Select(r => FindColumnValue(r, headers, "price"))
                                .Where(p => decimal.TryParse(p, out _))
                                .Select(p => decimal.Parse(p))
                                .DefaultIfEmpty(0)
                                .Average();
                            
                            result.AppendLine($"• {provider.Key}: {provider.Count()} services (Avg: ${avgPrice:F2})");
                        }
                        result.AppendLine();
                    }
                }

                // Vehicle-specific analysis
                if (ContainsAny(lowerMessage, "toyota", "honda", "ford", "camry", "accord", "f-150", "vehicle"))
                {
                    var vehicleColumn = FindColumn(headers, "vehicle", "car");
                    if (vehicleColumn >= 0)
                    {
                        var vehicleServices = dataRows
                            .Where(r => r.Length > vehicleColumn && ContainsAny(r[vehicleColumn].ToLower(), lowerMessage.Split(' ')))
                            .ToList();
                        
                        if (vehicleServices.Any())
                        {
                            var vehicle = vehicleServices.First()[vehicleColumn];
                            result.AppendLine($"🚗 Services for {vehicle}:");
                            result.AppendLine($"• Total Services: {vehicleServices.Count}");
                            
                            var totalCost = vehicleServices
                                .Select(r => FindColumnValue(r, headers, "price"))
                                .Where(p => decimal.TryParse(p, out _))
                                .Select(p => decimal.Parse(p))
                                .Sum();
                            
                            result.AppendLine($"• Total Cost: ${totalCost:F2}");
                            result.AppendLine();
                            
                            result.AppendLine("Recent Services:");
                            foreach (var service in vehicleServices.Take(5))
                            {
                                var serviceName = service.Length > 0 ? service[0] : "Unknown";
                                var price = FindColumnValue(service, headers, "price");
                                var date = FindColumnValue(service, headers, "date");
                                var mileage = FindColumnValue(service, headers, "mileage");
                                result.AppendLine($"• {serviceName} - ${price} ({date}) at {mileage} miles");
                            }
                            result.AppendLine();
                        }
                    }
                }

                // Price analysis
                if (ContainsAny(lowerMessage, "price", "cost", "expensive", "cheap", "how much", "average"))
                {
                    var priceColumn = FindColumn(headers, "price", "cost", "amount", "value");
                    if (priceColumn >= 0)
                    {
                        var prices = ExtractNumericValues(dataRows, priceColumn);
                        if (prices.Any())
                        {
                            result.AppendLine("💰 Price Analysis:");
                            result.AppendLine($"• Average: ${prices.Average():F2}");
                            result.AppendLine($"• Minimum: ${prices.Min():F2}");
                            result.AppendLine($"• Maximum: ${prices.Max():F2}");
                            result.AppendLine($"• Total: ${prices.Sum():F2}");
                            result.AppendLine();
                            
                            // Show most expensive
                            var maxPrice = prices.Max();
                            var expensiveService = dataRows.FirstOrDefault(r => 
                                r.Length > priceColumn && 
                                decimal.TryParse(r[priceColumn].Replace("$", "").Replace(",", ""), out var p) && 
                                p == maxPrice);
                            
                            if (expensiveService != null)
                            {
                                var serviceName = expensiveService.Length > 0 ? expensiveService[0] : "Unknown";
                                result.AppendLine($"💎 Most Expensive: {serviceName} - ${maxPrice:F2}");
                            }
                            result.AppendLine();
                        }
                    }
                }

                // Category breakdown
                if (ContainsAny(lowerMessage, "category", "type", "breakdown", "maintenance", "repair"))
                {
                    var categoryColumn = FindColumn(headers, "category", "type");
                    if (categoryColumn >= 0)
                    {
                        result.AppendLine("📊 Service Categories:");
                        var categories = dataRows
                            .Where(r => r.Length > categoryColumn)
                            .GroupBy(r => r[categoryColumn])
                            .OrderByDescending(g => g.Count());
                        
                        foreach (var cat in categories)
                        {
                            var catTotal = cat
                                .Select(r => FindColumnValue(r, headers, "price"))
                                .Where(p => decimal.TryParse(p, out _))
                                .Select(p => decimal.Parse(p))
                                .Sum();
                            
                            result.AppendLine($"• {cat.Key}: {cat.Count()} services (${catTotal:F2} total)");
                        }
                        result.AppendLine();
                    }
                }

                // Next service due
                if (ContainsAny(lowerMessage, "next", "due", "upcoming", "schedule", "when"))
                {
                    var nextServiceColumn = FindColumn(headers, "nextservicedue", "nextservice", "due");
                    if (nextServiceColumn >= 0)
                    {
                        result.AppendLine("📅 Upcoming Services:");
                        var upcomingServices = dataRows
                            .Where(r => r.Length > nextServiceColumn && !string.IsNullOrEmpty(r[nextServiceColumn]) && r[nextServiceColumn].ToLower() != "n/a")
                            .Take(5);
                        
                        foreach (var service in upcomingServices)
                        {
                            var serviceName = service.Length > 0 ? service[0] : "Unknown";
                            var nextDue = service[nextServiceColumn];
                            var vehicle = FindColumnValue(service, headers, "vehicle");
                            result.AppendLine($"• {serviceName} for {vehicle}: Due at {nextDue} miles");
                        }
                        result.AppendLine();
                    }
                }

                // Show sample data if general query
                if (ContainsAny(lowerMessage, "show", "list", "display", "all", "what"))
                {
                    result.AppendLine("📄 Recent Services:");
                    foreach (var row in dataRows.Take(5))
                    {
                        var serviceName = row.Length > 0 ? row[0] : "Unknown";
                        var price = FindColumnValue(row, headers, "price");
                        var vehicle = FindColumnValue(row, headers, "vehicle");
                        var date = FindColumnValue(row, headers, "date");
                        result.AppendLine($"• {serviceName} - {vehicle} (${price}) on {date}");
                    }
                    if (dataRows.Count > 5)
                    {
                        result.AppendLine($"... and {dataRows.Count - 5} more services");
                    }
                }

                result.AppendLine();
                result.AppendLine("💡 All information is from your uploaded maintenance records!");

                return result.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing CSV");
                return "I had trouble analyzing the CSV data. Please ensure it's properly formatted.";
            }
        }

        private string FindSpecificService(List<string[]> rows, string[] headers, string query)
        {
            // Check if asking about a specific service
            foreach (var row in rows)
            {
                if (row.Length == 0) continue;
                
                var serviceName = row[0].ToLower();
                if (query.Contains(serviceName) || serviceName.Contains(query.Replace(" ", "")))
                {
                    var result = new System.Text.StringBuilder();
                    result.AppendLine($"🔧 Service Details (from your data):");
                    result.AppendLine();
                    
                    for (int i = 0; i < Math.Min(headers.Length, row.Length); i++)
                    {
                        if (!string.IsNullOrEmpty(row[i]))
                        {
                            var icon = headers[i].ToLower() switch
                            {
                                var h when h.Contains("price") || h.Contains("cost") => "💰",
                                var h when h.Contains("vehicle") || h.Contains("car") => "🚗",
                                var h when h.Contains("date") => "📅",
                                var h when h.Contains("mileage") => "🛣️",
                                var h when h.Contains("provider") || h.Contains("shop") => "🏪",
                                var h when h.Contains("warranty") => "🛡️",
                                var h when h.Contains("urgency") || h.Contains("priority") => "⚠️",
                                _ => "•"
                            };
                            result.AppendLine($"{icon} {headers[i]}: {row[i]}");
                        }
                    }
                    
                    result.AppendLine();
                    result.AppendLine("💡 This is actual data from your maintenance records!");
                    return result.ToString();
                }
            }
            
            return null;
        }

        private string FindColumnValue(string[] row, string[] headers, params string[] possibleNames)
        {
            var columnIndex = FindColumn(headers, possibleNames);
            if (columnIndex >= 0 && row.Length > columnIndex)
            {
                return row[columnIndex];
            }
            return "";
        }

        private string AnalyzeJsonData(string content, string originalMessage, string lowerMessage)
        {
            try
            {
                var result = new System.Text.StringBuilder();
                
                // Try to parse as structured JSON
                using (var doc = System.Text.Json.JsonDocument.Parse(content))
                {
                    var root = doc.RootElement;
                    
                    // Check for metadata
                    if (root.TryGetProperty("metadata", out var metadata))
                    {
                        result.AppendLine("📋 Dataset Information:");
                        if (metadata.TryGetProperty("datasetName", out var datasetName))
                            result.AppendLine($"• Dataset: {datasetName.GetString()}");
                        if (metadata.TryGetProperty("country", out var country))
                            result.AppendLine($"• Country: {country.GetString()}");
                        if (metadata.TryGetProperty("currency", out var currency))
                            result.AppendLine($"• Currency: {currency.GetString()}");
                        if (metadata.TryGetProperty("lastUpdated", out var updated))
                            result.AppendLine($"• Last Updated: {updated.GetString()}");
                        if (metadata.TryGetProperty("recordCount", out var recordCount))
                            result.AppendLine($"• Total Records: {recordCount.GetInt32()}");
                        result.AppendLine();
                    }
                    
                    // Check for vehicles array (new enhanced format)
                    if (root.TryGetProperty("vehicles", out var vehicles) && vehicles.ValueKind == System.Text.Json.JsonValueKind.Array)
                    {
                        var vehiclesList = vehicles.EnumerateArray().ToList();
                        result.AppendLine($"📊 Total Vehicles: {vehiclesList.Count}");
                        result.AppendLine();
                        
                        // Check if user is asking about a specific vehicle
                        var specificVehicle = FindSpecificVehicle(vehiclesList, lowerMessage);
                        if (specificVehicle.HasValue)
                        {
                            return FormatEnhancedVehicleDetails(specificVehicle.Value, metadata);
                        }
                        
                        // Price analysis
                        if (ContainsAny(lowerMessage, "price", "cost", "expensive", "cheap", "how much", "average"))
                        {
                            result.AppendLine("💰 Price Analysis:");
                            var prices = vehiclesList
                                .Where(v => v.TryGetProperty("price", out _))
                                .Select(v => v.GetProperty("price").GetDecimal())
                                .ToList();
                            
                            if (prices.Any())
                            {
                                var currencySymbol = metadata.TryGetProperty("currency", out var curr) ? curr.GetString() : "USD";
                                result.AppendLine($"• Average: ${prices.Average():N0} {currencySymbol}");
                                result.AppendLine($"• Minimum: ${prices.Min():N0} {currencySymbol}");
                                result.AppendLine($"• Maximum: ${prices.Max():N0} {currencySymbol}");
                                result.AppendLine();
                                
                                // Show cheapest and most expensive
                                var cheapest = vehiclesList.FirstOrDefault(v => v.GetProperty("price").GetDecimal() == prices.Min());
                                var expensive = vehiclesList.FirstOrDefault(v => v.GetProperty("price").GetDecimal() == prices.Max());
                                
                                if (cheapest.ValueKind != System.Text.Json.JsonValueKind.Undefined)
                                {
                                    result.AppendLine($"💵 Cheapest: {cheapest.GetProperty("make").GetString()} {cheapest.GetProperty("model").GetString()} - ${prices.Min():N0}");
                                }
                                if (expensive.ValueKind != System.Text.Json.JsonValueKind.Undefined)
                                {
                                    result.AppendLine($"💎 Most Expensive: {expensive.GetProperty("make").GetString()} {expensive.GetProperty("model").GetString()} - ${prices.Max():N0}");
                                }
                            }
                            result.AppendLine();
                        }
                        
                        // Fuel economy analysis
                        if (ContainsAny(lowerMessage, "fuel", "mpg", "economy", "efficient", "gas"))
                        {
                            result.AppendLine("⛽ Fuel Economy Analysis:");
                            var mpgData = vehiclesList
                                .Where(v => v.TryGetProperty("specifications", out var specs) && 
                                           specs.TryGetProperty("mpgCombined", out _))
                                .Select(v => new {
                                    Vehicle = $"{v.GetProperty("make").GetString()} {v.GetProperty("model").GetString()}",
                                    MPG = v.GetProperty("specifications").GetProperty("mpgCombined").GetInt32()
                                })
                                .OrderByDescending(x => x.MPG)
                                .ToList();
                            
                            if (mpgData.Any())
                            {
                                result.AppendLine($"• Average MPG: {mpgData.Average(x => x.MPG):F1}");
                                result.AppendLine();
                                result.AppendLine("🏆 Most Efficient:");
                                foreach (var item in mpgData.Take(3))
                                {
                                    result.AppendLine($"• {item.Vehicle}: {item.MPG} MPG");
                                }
                            }
                            result.AppendLine();
                        }
                        
                        // Safety analysis
                        if (ContainsAny(lowerMessage, "safety", "safe", "rating", "crash"))
                        {
                            result.AppendLine("🛡️ Safety Ratings:");
                            var safetyData = vehiclesList
                                .Where(v => v.TryGetProperty("safety", out var safety) && 
                                           safety.TryGetProperty("rating", out _))
                                .Select(v => new {
                                    Vehicle = $"{v.GetProperty("make").GetString()} {v.GetProperty("model").GetString()}",
                                    Rating = v.GetProperty("safety").GetProperty("rating").GetInt32()
                                })
                                .OrderByDescending(x => x.Rating)
                                .ToList();
                            
                            if (safetyData.Any())
                            {
                                foreach (var item in safetyData)
                                {
                                    var stars = new string('⭐', item.Rating);
                                    result.AppendLine($"• {item.Vehicle}: {stars} ({item.Rating}/5)");
                                }
                            }
                            result.AppendLine();
                        }
                        
                        // Warranty analysis
                        if (ContainsAny(lowerMessage, "warranty", "guarantee", "coverage"))
                        {
                            result.AppendLine("🛡️ Warranty Information:");
                            var warrantyData = vehiclesList
                                .Where(v => v.TryGetProperty("warranty", out var warranty) && 
                                           warranty.TryGetProperty("remaining", out _))
                                .Select(v => new {
                                    Vehicle = $"{v.GetProperty("make").GetString()} {v.GetProperty("model").GetString()}",
                                    Months = v.GetProperty("warranty").GetProperty("remaining").GetInt32(),
                                    Type = v.GetProperty("warranty").TryGetProperty("type", out var t) ? t.GetString() : "N/A"
                                })
                                .OrderByDescending(x => x.Months)
                                .ToList();
                            
                            if (warrantyData.Any())
                            {
                                foreach (var item in warrantyData)
                                {
                                    result.AppendLine($"• {item.Vehicle}: {item.Months} months ({item.Type})");
                                }
                            }
                            result.AppendLine();
                        }
                        
                        // Make/Brand analysis
                        if (ContainsAny(lowerMessage, "brand", "make", "manufacturer", "compare"))
                        {
                            result.AppendLine("🏢 Makes/Brands:");
                            var makes = vehiclesList
                                .Where(v => v.TryGetProperty("make", out _))
                                .GroupBy(v => v.GetProperty("make").GetString())
                                .OrderByDescending(g => g.Count());
                            
                            foreach (var make in makes)
                            {
                                var avgPrice = make
                                    .Where(v => v.TryGetProperty("price", out _))
                                    .Average(v => v.GetProperty("price").GetDecimal());
                                result.AppendLine($"• {make.Key}: {make.Count()} models (Avg: ${avgPrice:N0})");
                            }
                            result.AppendLine();
                        }
                        
                        // Market insights
                        if (root.TryGetProperty("marketInsights", out var insights))
                        {
                            if (ContainsAny(lowerMessage, "trend", "market", "insight", "recommend", "best"))
                            {
                                result.AppendLine("📈 Market Insights:");
                                
                                if (insights.TryGetProperty("trending", out var trending) && trending.ValueKind == System.Text.Json.JsonValueKind.Array)
                                {
                                    result.AppendLine("🔥 Trending:");
                                    foreach (var trend in trending.EnumerateArray())
                                    {
                                        result.AppendLine($"• {trend.GetString()}");
                                    }
                                    result.AppendLine();
                                }
                                
                                if (insights.TryGetProperty("recommendations", out var recommendations))
                                {
                                    result.AppendLine("💡 Recommendations:");
                                    if (recommendations.TryGetProperty("bestValue", out var bestValue))
                                        result.AppendLine($"• Best Value: {bestValue.GetString()}");
                                    if (recommendations.TryGetProperty("bestPerformance", out var bestPerf))
                                        result.AppendLine($"• Best Performance: {bestPerf.GetString()}");
                                    if (recommendations.TryGetProperty("bestTechnology", out var bestTech))
                                        result.AppendLine($"• Best Technology: {bestTech.GetString()}");
                                    result.AppendLine();
                                }
                            }
                        }
                        
                        // Show sample data
                        if (ContainsAny(lowerMessage, "show", "list", "display", "all"))
                        {
                            result.AppendLine("📄 Available Vehicles:");
                            foreach (var vehicle in vehiclesList.Take(5))
                            {
                                var make = vehicle.TryGetProperty("make", out var mk) ? mk.GetString() : "Unknown";
                                var model = vehicle.TryGetProperty("model", out var md) ? md.GetString() : "Unknown";
                                var year = vehicle.TryGetProperty("year", out var yr) ? yr.GetInt32().ToString() : "";
                                var price = vehicle.TryGetProperty("price", out var pr) ? pr.GetDecimal().ToString("N0") : "";
                                var condition = vehicle.TryGetProperty("condition", out var cond) ? cond.GetString() : "";
                                
                                result.AppendLine($"• {make} {model} {year} - ${price} ({condition})");
                            }
                            if (vehiclesList.Count > 5)
                            {
                                result.AppendLine($"... and {vehiclesList.Count - 5} more vehicles");
                            }
                        }
                    }
                    // Fallback to old "cars" array format
                    else if (root.TryGetProperty("cars", out var cars) && cars.ValueKind == System.Text.Json.JsonValueKind.Array)
                    {
                        var carsList = cars.EnumerateArray().ToList();
                        result.AppendLine($"📊 Total Vehicles: {carsList.Count}");
                        result.AppendLine();
                        
                        // Check if user is asking about a specific car
                        var specificCar = FindSpecificCar(carsList, lowerMessage);
                        if (specificCar.HasValue)
                        {
                            return FormatCarDetails(specificCar.Value, metadata);
                        }
                        
                        // Price analysis
                        if (ContainsAny(lowerMessage, "price", "cost", "expensive", "cheap", "how much"))
                        {
                            result.AppendLine("💰 Price Analysis:");
                            var prices = carsList
                                .Where(c => c.TryGetProperty("price", out _))
                                .Select(c => c.GetProperty("price").GetDecimal())
                                .ToList();
                            
                            if (prices.Any())
                            {
                                var currencySymbol = metadata.TryGetProperty("currency", out var curr) ? curr.GetString() : "";
                                result.AppendLine($"• Average: {prices.Average():N0} {currencySymbol}");
                                result.AppendLine($"• Minimum: {prices.Min():N0} {currencySymbol}");
                                result.AppendLine($"• Maximum: {prices.Max():N0} {currencySymbol}");
                                result.AppendLine();
                                
                                // Show cheapest and most expensive
                                var cheapest = carsList.FirstOrDefault(c => c.GetProperty("price").GetDecimal() == prices.Min());
                                var expensive = carsList.FirstOrDefault(c => c.GetProperty("price").GetDecimal() == prices.Max());
                                
                                if (cheapest.ValueKind != System.Text.Json.JsonValueKind.Undefined)
                                {
                                    result.AppendLine($"💵 Cheapest: {cheapest.GetProperty("brand").GetString()} {cheapest.GetProperty("model").GetString()} - {prices.Min():N0} {currencySymbol}");
                                }
                                if (expensive.ValueKind != System.Text.Json.JsonValueKind.Undefined)
                                {
                                    result.AppendLine($"💎 Most Expensive: {expensive.GetProperty("brand").GetString()} {expensive.GetProperty("model").GetString()} - {prices.Max():N0} {currencySymbol}");
                                }
                            }
                            result.AppendLine();
                        }
                        
                        // Brand analysis
                        if (ContainsAny(lowerMessage, "brand", "make", "manufacturer", "compare"))
                        {
                            result.AppendLine("🏢 Brands:");
                            var brands = carsList
                                .Where(c => c.TryGetProperty("brand", out _))
                                .GroupBy(c => c.GetProperty("brand").GetString())
                                .OrderByDescending(g => g.Count());
                            
                            foreach (var brand in brands)
                            {
                                result.AppendLine($"• {brand.Key}: {brand.Count()} models");
                            }
                            result.AppendLine();
                        }
                        
                        // Show sample data
                        if (ContainsAny(lowerMessage, "show", "list", "display", "all"))
                        {
                            result.AppendLine("📄 Available Vehicles:");
                            foreach (var car in carsList.Take(5))
                            {
                                var brand = car.TryGetProperty("brand", out var b) ? b.GetString() : "Unknown";
                                var model = car.TryGetProperty("model", out var m) ? m.GetString() : "Unknown";
                                var year = car.TryGetProperty("year", out var y) ? y.GetInt32().ToString() : "";
                                var price = car.TryGetProperty("price", out var p) ? p.GetDecimal().ToString("N0") : "";
                                var currency = metadata.TryGetProperty("currency", out var c) ? c.GetString() : "";
                                
                                result.AppendLine($"• {brand} {model} {year} - {price} {currency}");
                            }
                            if (carsList.Count > 5)
                            {
                                result.AppendLine($"... and {carsList.Count - 5} more vehicles");
                            }
                        }
                    }
                }
                
                return result.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing JSON");
                return "I had trouble analyzing the JSON data. Please ensure it's properly formatted.";
            }
        }

        private System.Text.Json.JsonElement? FindSpecificVehicle(List<System.Text.Json.JsonElement> vehicles, string query)
        {
            foreach (var vehicle in vehicles)
            {
                var make = vehicle.TryGetProperty("make", out var mk) ? mk.GetString()?.ToLower() : "";
                var model = vehicle.TryGetProperty("model", out var md) ? md.GetString()?.ToLower() : "";
                
                if (!string.IsNullOrEmpty(make) && query.Contains(make))
                {
                    if (string.IsNullOrEmpty(model) || query.Contains(model))
                    {
                        return vehicle;
                    }
                }
            }
            return null;
        }

        private System.Text.Json.JsonElement? FindSpecificCar(List<System.Text.Json.JsonElement> cars, string query)
        {
            foreach (var car in cars)
            {
                var brand = car.TryGetProperty("brand", out var b) ? b.GetString()?.ToLower() : "";
                var model = car.TryGetProperty("model", out var m) ? m.GetString()?.ToLower() : "";
                
                if (!string.IsNullOrEmpty(brand) && query.Contains(brand))
                {
                    if (string.IsNullOrEmpty(model) || query.Contains(model))
                    {
                        return car;
                    }
                }
            }
            return null;
        }

        private string FormatCarDetails(System.Text.Json.JsonElement car, System.Text.Json.JsonElement metadata)
        {
            var result = new System.Text.StringBuilder();
            var currency = metadata.TryGetProperty("currency", out var curr) ? curr.GetString() : "";
            var country = metadata.TryGetProperty("country", out var ctry) ? ctry.GetString() : "";
            
            result.AppendLine("🚗 Vehicle Details (from your uploaded data):");
            result.AppendLine();
            
            if (car.TryGetProperty("brand", out var brand))
                result.AppendLine($"🏢 Brand: {brand.GetString()}");
            if (car.TryGetProperty("model", out var model))
                result.AppendLine($"📛 Model: {model.GetString()}");
            if (car.TryGetProperty("year", out var year))
                result.AppendLine($"📅 Year: {year.GetInt32()}");
            if (car.TryGetProperty("price", out var price))
                result.AppendLine($"💰 Price: {price.GetDecimal():N0} {currency}");
            
            result.AppendLine();
            result.AppendLine("⚙️ Specifications:");
            
            if (car.TryGetProperty("bodyType", out var bodyType))
                result.AppendLine($"• Body Type: {bodyType.GetString()}");
            if (car.TryGetProperty("fuelType", out var fuelType))
                result.AppendLine($"• Fuel Type: {fuelType.GetString()}");
            if (car.TryGetProperty("transmission", out var transmission))
                result.AppendLine($"• Transmission: {transmission.GetString()}");
            if (car.TryGetProperty("engineCc", out var engineCc))
                result.AppendLine($"• Engine: {engineCc.GetInt32()} cc");
            if (car.TryGetProperty("horsePower", out var hp))
                result.AppendLine($"• Power: {hp.GetInt32()} HP");
            
            result.AppendLine();
            
            if (car.TryGetProperty("availability", out var availability))
                result.AppendLine($"📦 Availability: {availability.GetString()}");
            if (car.TryGetProperty("origin", out var origin))
                result.AppendLine($"🌍 Origin: {origin.GetString()}");
            
            if (!string.IsNullOrEmpty(country))
            {
                result.AppendLine();
                result.AppendLine($"📍 Market: {country}");
            }
            
            result.AppendLine();
            result.AppendLine("💡 This information is from your uploaded dataset!");
            
            return result.ToString();
        }

        private string FormatEnhancedVehicleDetails(System.Text.Json.JsonElement vehicle, System.Text.Json.JsonElement metadata)
        {
            var result = new System.Text.StringBuilder();
            var currency = metadata.TryGetProperty("currency", out var curr) ? curr.GetString() : "USD";
            
            result.AppendLine("🚗 Vehicle Details (from your uploaded data):");
            result.AppendLine();
            
            // Basic Information
            if (vehicle.TryGetProperty("make", out var make))
                result.AppendLine($"🏢 Make: {make.GetString()}");
            if (vehicle.TryGetProperty("model", out var model))
                result.AppendLine($"📛 Model: {model.GetString()}");
            if (vehicle.TryGetProperty("trim", out var trim))
                result.AppendLine($"✨ Trim: {trim.GetString()}");
            if (vehicle.TryGetProperty("year", out var year))
                result.AppendLine($"📅 Year: {year.GetInt32()}");
            if (vehicle.TryGetProperty("price", out var price))
                result.AppendLine($"💰 Price: ${price.GetDecimal():N0} {currency}");
            if (vehicle.TryGetProperty("originalMSRP", out var msrp))
                result.AppendLine($"💵 Original MSRP: ${msrp.GetDecimal():N0}");
            if (vehicle.TryGetProperty("depreciation", out var depreciation))
                result.AppendLine($"📉 Depreciation: {depreciation.GetDecimal():F1}%");
            if (vehicle.TryGetProperty("mileage", out var mileage))
                result.AppendLine($"🛣️ Mileage: {mileage.GetInt32():N0} miles");
            if (vehicle.TryGetProperty("condition", out var condition))
                result.AppendLine($"✅ Condition: {condition.GetString()}");
            
            result.AppendLine();
            
            // Specifications
            if (vehicle.TryGetProperty("specifications", out var specs))
            {
                result.AppendLine("⚙️ Specifications:");
                if (specs.TryGetProperty("engine", out var engine))
                    result.AppendLine($"• Engine: {engine.GetString()}");
                if (specs.TryGetProperty("horsepower", out var hp))
                    result.AppendLine($"• Horsepower: {hp.GetInt32()} HP");
                if (specs.TryGetProperty("torque", out var torque))
                    result.AppendLine($"• Torque: {torque.GetInt32()} lb-ft");
                if (specs.TryGetProperty("transmission", out var transmission))
                    result.AppendLine($"• Transmission: {transmission.GetString()}");
                if (specs.TryGetProperty("drivetrain", out var drivetrain))
                    result.AppendLine($"• Drivetrain: {drivetrain.GetString()}");
                if (specs.TryGetProperty("fuelType", out var fuelType))
                    result.AppendLine($"• Fuel Type: {fuelType.GetString()}");
                if (specs.TryGetProperty("mpgCity", out var mpgCity) && specs.TryGetProperty("mpgHighway", out var mpgHighway))
                    result.AppendLine($"• Fuel Economy: {mpgCity.GetInt32()} city / {mpgHighway.GetInt32()} highway MPG");
                if (specs.TryGetProperty("mpgCombined", out var mpgCombined))
                    result.AppendLine($"• Combined MPG: {mpgCombined.GetInt32()}");
                if (specs.TryGetProperty("range", out var range))
                    result.AppendLine($"• Range: {range.GetInt32()} miles");
                if (specs.TryGetProperty("bodyType", out var bodyType))
                    result.AppendLine($"• Body Type: {bodyType.GetString()}");
                if (specs.TryGetProperty("doors", out var doors))
                    result.AppendLine($"• Doors: {doors.GetInt32()}");
                if (specs.TryGetProperty("seats", out var seats))
                    result.AppendLine($"• Seats: {seats.GetInt32()}");
                if (specs.TryGetProperty("color", out var color))
                    result.AppendLine($"• Color: {color.GetString()}");
                if (specs.TryGetProperty("towingCapacity", out var towing))
                    result.AppendLine($"• Towing Capacity: {towing.GetInt32():N0} lbs");
                result.AppendLine();
            }
            
            // Features
            if (vehicle.TryGetProperty("features", out var features) && features.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                result.AppendLine("✨ Features:");
                var featuresList = features.EnumerateArray().Take(10).ToList();
                foreach (var feature in featuresList)
                {
                    result.AppendLine($"• {feature.GetString()}");
                }
                if (features.GetArrayLength() > 10)
                {
                    result.AppendLine($"... and {features.GetArrayLength() - 10} more features");
                }
                result.AppendLine();
            }
            
            // Safety
            if (vehicle.TryGetProperty("safety", out var safety))
            {
                result.AppendLine("🛡️ Safety:");
                if (safety.TryGetProperty("rating", out var rating))
                {
                    var stars = new string('⭐', rating.GetInt32());
                    result.AppendLine($"• Rating: {stars} ({rating.GetInt32()}/5)");
                }
                if (safety.TryGetProperty("airbags", out var airbags))
                    result.AppendLine($"• Airbags: {airbags.GetInt32()}");
                if (safety.TryGetProperty("crashTestScore", out var crashScore))
                    result.AppendLine($"• Crash Test Score: {crashScore.GetInt32()}/100");
                result.AppendLine();
            }
            
            // History
            if (vehicle.TryGetProperty("history", out var history))
            {
                result.AppendLine("📜 History:");
                if (history.TryGetProperty("accidents", out var accidents))
                    result.AppendLine($"• Accidents: {accidents.GetInt32()}");
                if (history.TryGetProperty("owners", out var owners))
                    result.AppendLine($"• Previous Owners: {owners.GetInt32()}");
                if (history.TryGetProperty("title", out var title))
                    result.AppendLine($"• Title: {title.GetString()}");
                if (history.TryGetProperty("serviceRecords", out var serviceRecords))
                    result.AppendLine($"• Service Records: {serviceRecords.GetInt32()}");
                if (history.TryGetProperty("carfaxScore", out var carfaxScore))
                    result.AppendLine($"• Carfax Score: {carfaxScore.GetInt32()}/100");
                result.AppendLine();
            }
            
            // Seller
            if (vehicle.TryGetProperty("seller", out var seller))
            {
                result.AppendLine("🏪 Seller:");
                if (seller.TryGetProperty("name", out var sellerName))
                    result.AppendLine($"• Name: {sellerName.GetString()}");
                if (seller.TryGetProperty("type", out var sellerType))
                    result.AppendLine($"• Type: {sellerType.GetString()}");
                if (seller.TryGetProperty("location", out var location))
                    result.AppendLine($"• Location: {location.GetString()}");
                if (seller.TryGetProperty("rating", out var sellerRating))
                    result.AppendLine($"• Rating: {sellerRating.GetDecimal():F1}/5.0");
                result.AppendLine();
            }
            
            // Warranty
            if (vehicle.TryGetProperty("warranty", out var warranty))
            {
                result.AppendLine("🛡️ Warranty:");
                if (warranty.TryGetProperty("remaining", out var remaining))
                    result.AppendLine($"• Remaining: {remaining.GetInt32()} months");
                if (warranty.TryGetProperty("type", out var warrantyType))
                    result.AppendLine($"• Type: {warrantyType.GetString()}");
                if (warranty.TryGetProperty("coverage", out var coverage))
                    result.AppendLine($"• Coverage: {coverage.GetString()}");
                if (warranty.TryGetProperty("transferable", out var transferable))
                    result.AppendLine($"• Transferable: {(transferable.GetBoolean() ? "Yes" : "No")}");
                result.AppendLine();
            }
            
            // Financing
            if (vehicle.TryGetProperty("financing", out var financing))
            {
                result.AppendLine("💳 Financing:");
                if (financing.TryGetProperty("available", out var available) && available.GetBoolean())
                {
                    if (financing.TryGetProperty("apr", out var apr))
                        result.AppendLine($"• APR: {apr.GetDecimal():F1}%");
                    if (financing.TryGetProperty("monthlyPayment", out var monthly))
                        result.AppendLine($"• Monthly Payment: ${monthly.GetInt32()}");
                    if (financing.TryGetProperty("term", out var term))
                        result.AppendLine($"• Term: {term.GetInt32()} months");
                }
                else
                {
                    result.AppendLine("• Not Available");
                }
                result.AppendLine();
            }
            
            // Listing
            if (vehicle.TryGetProperty("listing", out var listing))
            {
                if (listing.TryGetProperty("daysOnMarket", out var daysOnMarket))
                    result.AppendLine($"📅 Days on Market: {daysOnMarket.GetInt32()}");
                if (listing.TryGetProperty("views", out var views))
                    result.AppendLine($"👁️ Views: {views.GetInt32()}");
                if (listing.TryGetProperty("saves", out var saves))
                    result.AppendLine($"❤️ Saves: {saves.GetInt32()}");
                result.AppendLine();
            }
            
            result.AppendLine("💡 This detailed information is from your uploaded dataset!");
            
            return result.ToString();
        }

        private string AnalyzeTextData(string content, string originalMessage, string lowerMessage)
        {
            try
            {
                var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                var words = content.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                var result = new System.Text.StringBuilder();
                result.AppendLine("📋 Text Dataset Analysis:");
                result.AppendLine($"• Lines: {lines.Length}");
                result.AppendLine($"• Words: {words.Length}");
                result.AppendLine($"• Characters: {content.Length}");
                result.AppendLine();

                // Word frequency
                if (ContainsAny(lowerMessage, "common", "frequent", "pattern"))
                {
                    var wordFreq = words
                        .Where(w => w.Length > 3)
                        .GroupBy(w => w.ToLower())
                        .OrderByDescending(g => g.Count())
                        .Take(5);

                    result.AppendLine("🔤 Most Common Words:");
                    foreach (var word in wordFreq)
                    {
                        result.AppendLine($"• {word.Key}: {word.Count()} times");
                    }
                }

                return result.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing text");
                return "I had trouble analyzing the text data.";
            }
        }

        private int FindColumn(string[] headers, params string[] possibleNames)
        {
            for (int i = 0; i < headers.Length; i++)
            {
                var header = headers[i].ToLower();
                if (possibleNames.Any(name => header.Contains(name)))
                    return i;
            }
            return -1;
        }

        private List<decimal> ExtractNumericValues(List<string[]> rows, int columnIndex)
        {
            var values = new List<decimal>();
            foreach (var row in rows)
            {
                if (row.Length > columnIndex)
                {
                    var value = row[columnIndex].Replace("$", "").Replace(",", "").Trim();
                    if (decimal.TryParse(value, out var numValue))
                    {
                        values.Add(numValue);
                    }
                }
            }
            return values;
        }
    }
}
