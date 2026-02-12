using System.Text.RegularExpressions;
using CommunityCar.Domain.Models.AI;
using Microsoft.Extensions.Logging;

namespace CommunityCar.Infrastructure.Services.AI
{
    public partial class AssistantService
    {
        private (string make, string model) ExtractVehicleFromMessage(string lowerMessage)
        {
            // Common car makes
            var makes = new[] { "toyota", "honda", "ford", "chevrolet", "chevy", "nissan", "mazda", "hyundai", "kia", "subaru", "volkswagen", "vw", "bmw", "mercedes", "audi", "lexus", "tesla", "jeep", "ram", "dodge" };
            
            string foundMake = null;
            foreach (var make in makes)
            {
                if (lowerMessage.Contains(make))
                {
                    foundMake = char.ToUpper(make[0]) + make.Substring(1);
                    if (make == "chevy") foundMake = "Chevrolet";
                    if (make == "vw") foundMake = "Volkswagen";
                    break;
                }
            }
            
            if (foundMake == null)
                return (null, null);
            
            // Common models
            var models = new Dictionary<string, string[]>
            {
                { "Toyota", new[] { "camry", "corolla", "rav4", "highlander", "tacoma", "tundra", "prius", "4runner", "sienna" } },
                { "Honda", new[] { "accord", "civic", "cr-v", "pilot", "odyssey", "ridgeline", "hr-v", "passport" } },
                { "Ford", new[] { "f-150", "f150", "mustang", "explorer", "escape", "edge", "ranger", "bronco", "expedition" } },
                { "Chevrolet", new[] { "silverado", "malibu", "equinox", "traverse", "tahoe", "suburban", "colorado", "blazer" } },
                { "Nissan", new[] { "altima", "sentra", "rogue", "pathfinder", "frontier", "titan", "murano", "kicks" } },
                { "Tesla", new[] { "model 3", "model s", "model x", "model y" } }
            };
            
            if (models.ContainsKey(foundMake))
            {
                foreach (var model in models[foundMake])
                {
                    if (lowerMessage.Contains(model))
                    {
                        // Capitalize first letter of each word
                        var modelWords = model.Split(' ');
                        var capitalizedModel = string.Join(" ", modelWords.Select(w => char.ToUpper(w[0]) + w.Substring(1)));
                        return (foundMake, capitalizedModel);
                    }
                }
            }
            
            return (foundMake, null);
        }
        
        private int ExtractYear(string message)
        {
            // Look for 4-digit year between 1990 and 2030
            var match = Regex.Match(message, @"\b(19[9]\d|20[0-3]\d)\b");
            if (match.Success && int.TryParse(match.Value, out var year))
            {
                return year;
            }
            return 0;
        }
        
        private async Task<string> SearchVehicleInDataset(string content, string make, string query, string extension)
        {
            try
            {
                if (extension == ".json")
                {
                    using (var doc = System.Text.Json.JsonDocument.Parse(content))
                    {
                        var root = doc.RootElement;
                        
                        if (root.TryGetProperty("vehicles", out var vehicles))
                        {
                            foreach (var vehicle in vehicles.EnumerateArray())
                            {
                                if (vehicle.TryGetProperty("make", out var vMake) && 
                                    vMake.GetString().Equals(make, StringComparison.OrdinalIgnoreCase))
                                {
                                    if (vehicle.TryGetProperty("model", out var vModel))
                                    {
                                        var model = vModel.GetString().ToLower();
                                        if (query.Contains(model))
                                        {
                                            var metadata = root.TryGetProperty("metadata", out var meta) ? meta : default;
                                            return FormatEnhancedVehicleDetails(vehicle, metadata);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching vehicle in dataset");
            }
            
            return null;
        }
        
        private async Task<string> SearchSpecificVehicle(string content, string make, string model, int year, string extension)
        {
            try
            {
                if (extension == ".json")
                {
                    using (var doc = System.Text.Json.JsonDocument.Parse(content))
                    {
                        var root = doc.RootElement;
                        
                        if (root.TryGetProperty("vehicles", out var vehicles))
                        {
                            foreach (var vehicle in vehicles.EnumerateArray())
                            {
                                var vMake = vehicle.TryGetProperty("make", out var mk) ? mk.GetString() : "";
                                var vModel = vehicle.TryGetProperty("model", out var md) ? md.GetString() : "";
                                var vYear = vehicle.TryGetProperty("year", out var yr) ? yr.GetInt32() : 0;
                                
                                if (vMake.Equals(make, StringComparison.OrdinalIgnoreCase) &&
                                    vModel.Equals(model, StringComparison.OrdinalIgnoreCase) &&
                                    vYear == year)
                                {
                                    var metadata = root.TryGetProperty("metadata", out var meta) ? meta : default;
                                    return FormatEnhancedVehicleDetails(vehicle, metadata);
                                }
                            }
                        }
                    }
                }
                else if (extension == ".csv")
                {
                    var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                    if (lines.Length < 2) return null;
                    
                    var headers = lines[0].Split(',').Select(h => h.Trim().Trim('"')).ToArray();
                    var makeIdx = Array.FindIndex(headers, h => h.Equals("Make", StringComparison.OrdinalIgnoreCase));
                    var modelIdx = Array.FindIndex(headers, h => h.Equals("Model", StringComparison.OrdinalIgnoreCase));
                    var yearIdx = Array.FindIndex(headers, h => h.Equals("Year", StringComparison.OrdinalIgnoreCase));
                    
                    if (makeIdx < 0 || modelIdx < 0 || yearIdx < 0) return null;
                    
                    foreach (var line in lines.Skip(1))
                    {
                        var values = line.Split(',').Select(v => v.Trim().Trim('"')).ToArray();
                        if (values.Length > Math.Max(makeIdx, Math.Max(modelIdx, yearIdx)))
                        {
                            if (values[makeIdx].Equals(make, StringComparison.OrdinalIgnoreCase) &&
                                values[modelIdx].Equals(model, StringComparison.OrdinalIgnoreCase) &&
                                int.TryParse(values[yearIdx], out var csvYear) && csvYear == year)
                            {
                                return FormatCsvVehicleDetails(headers, values);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching specific vehicle");
            }
            
            return null;
        }
        
        private string FormatCsvVehicleDetails(string[] headers, string[] values)
        {
            var result = new System.Text.StringBuilder();
            result.AppendLine("🚗 Vehicle Details (from your uploaded data):");
            result.AppendLine();
            
            for (int i = 0; i < Math.Min(headers.Length, values.Length); i++)
            {
                if (!string.IsNullOrEmpty(values[i]))
                {
                    var icon = headers[i].ToLower() switch
                    {
                        var h when h.Contains("price") || h.Contains("cost") => "💰",
                        var h when h.Contains("make") || h.Contains("brand") => "🏢",
                        var h when h.Contains("model") => "📛",
                        var h when h.Contains("year") => "📅",
                        var h when h.Contains("mileage") => "🛣️",
                        var h when h.Contains("condition") => "✅",
                        var h when h.Contains("fuel") => "⛽",
                        var h when h.Contains("transmission") => "⚙️",
                        var h when h.Contains("color") => "🎨",
                        _ => "•"
                    };
                    result.AppendLine($"{icon} {headers[i]}: {values[i]}");
                }
            }
            
            result.AppendLine();
            result.AppendLine("💡 This information is from your uploaded dataset!");
            
            return result.ToString();
        }
        
        private async Task<string> GetVehiclePricing(string make, string model, int year, string userId)
        {
            var datasets = GetUserDatasetFiles(userId);
            if (!datasets.Any())
            {
                return "I don't have pricing data uploaded yet. Please upload a price dataset to get specific pricing information.";
            }
            
            var latestDataset = datasets.First();
            var content = await File.ReadAllTextAsync(latestDataset.FullName);
            
            var vehicleInfo = await SearchSpecificVehicle(content, make, model, year, latestDataset.Extension);
            if (vehicleInfo != null)
            {
                return vehicleInfo;
            }
            
            return $"I couldn't find pricing for the {year} {make} {model} in your datasets. Try uploading a dataset with this vehicle's information.";
        }
        
        private async Task<string> GetVehicleMaintenance(string make, string model, int year, string userId)
        {
            var datasets = GetUserDatasetFiles(userId);
            var maintenanceDataset = datasets.FirstOrDefault(f => f.Name.ToLower().Contains("maintenance"));
            
            if (maintenanceDataset == null)
            {
                return $"I don't have maintenance data for the {year} {make} {model}. Upload a maintenance dataset to get specific service history and costs.";
            }
            
            var content = await File.ReadAllTextAsync(maintenanceDataset.FullName);
            var vehicleName = $"{make} {model}";
            
            // Search for maintenance records
            var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length < 2) return "No maintenance data found.";
            
            var headers = lines[0].Split(',').Select(h => h.Trim().Trim('"')).ToArray();
            var vehicleIdx = Array.FindIndex(headers, h => h.Equals("Vehicle", StringComparison.OrdinalIgnoreCase));
            
            if (vehicleIdx < 0) return "Maintenance dataset doesn't have vehicle information.";
            
            var matchingRecords = lines.Skip(1)
                .Select(line => line.Split(',').Select(v => v.Trim().Trim('"')).ToArray())
                .Where(values => values.Length > vehicleIdx && 
                                values[vehicleIdx].Contains(make, StringComparison.OrdinalIgnoreCase))
                .ToList();
            
            if (!matchingRecords.Any())
            {
                return $"No maintenance records found for {make} vehicles in your dataset.";
            }
            
            var result = new System.Text.StringBuilder();
            result.AppendLine($"🔧 Maintenance Records for {make} vehicles:");
            result.AppendLine();
            result.AppendLine($"📊 Total Services: {matchingRecords.Count}");
            result.AppendLine();
            
            foreach (var record in matchingRecords.Take(5))
            {
                var service = record.Length > 0 ? record[0] : "Unknown";
                var price = FindColumnValue(record, headers, "price");
                var date = FindColumnValue(record, headers, "date");
                result.AppendLine($"• {service} - ${price} ({date})");
            }
            
            if (matchingRecords.Count > 5)
            {
                result.AppendLine($"... and {matchingRecords.Count - 5} more services");
            }
            
            result.AppendLine();
            result.AppendLine("💡 This is from your uploaded maintenance dataset!");
            
            return result.ToString();
        }
        
        private async Task<string> CompareVehicles(string make1, string model1, int year1, string make2, string model2, string userId)
        {
            var result = new System.Text.StringBuilder();
            result.AppendLine($"🔄 Comparing: {year1} {make1} {model1} vs {make2} {model2}");
            result.AppendLine();
            
            var datasets = GetUserDatasetFiles(userId);
            if (!datasets.Any())
            {
                return "I need dataset information to compare vehicles. Please upload a price or comparison dataset.";
            }
            
            result.AppendLine("Based on your uploaded data, here's what I can tell you:");
            result.AppendLine();
            result.AppendLine("💡 Upload more datasets for detailed comparisons including:");
            result.AppendLine("• Price differences");
            result.AppendLine("• Fuel economy");
            result.AppendLine("• Maintenance costs");
            result.AppendLine("• Safety ratings");
            result.AppendLine("• Features and specifications");
            
            return result.ToString();
        }
    }
}
