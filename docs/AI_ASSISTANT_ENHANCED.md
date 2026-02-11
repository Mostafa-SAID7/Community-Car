# AI Assistant - Enhanced Intelligence System

## Overview
The AI Assistant now features advanced dataset analysis capabilities, providing intelligent, context-aware responses based on uploaded data files.

## Key Features

### 1. Intelligent Dataset Analysis
The assistant can now:
- Read and analyze CSV, JSON, and TXT files
- Extract meaningful insights from your data
- Answer questions about uploaded datasets
- Provide statistical analysis automatically

### 2. Predictive Query Understanding
The system detects what you're asking about:
- **Price queries**: "What's the average price?", "Show me expensive items"
- **Statistics**: "How many records?", "Count by category"
- **Patterns**: "What trends do you see?", "Analyze the data"
- **Listings**: "Show me the data", "List all items"

### 3. Automatic Data Extraction
For CSV files, the assistant automatically:
- Identifies column headers
- Finds price/cost columns
- Detects category columns
- Calculates statistics (average, min, max, total)
- Groups data by categories
- Shows sample records

## Usage Examples

### Upload a Dataset
1. Click the upload button in the AI Assistant
2. Select a CSV, JSON, TXT, or XLSX file (max 10MB)
3. The file is saved to `wwwroot/uploads/datasets/`
4. Assistant provides immediate analysis

### Ask Questions About Your Data

**Price Analysis:**
```
User: "What's the average price?"
AI: Analyzes price column and shows:
  • Average: $XX.XX
  • Minimum: $XX.XX
  • Maximum: $XX.XX
  • Total: $XX.XX
```

**Statistics:**
```
User: "How many items do I have?"
AI: Shows total records and breakdown by category
```

**Pattern Detection:**
```
User: "What patterns do you see?"
AI: Identifies most common values and trends
```

**Data Listing:**
```
User: "Show me the data"
AI: Displays sample records with key columns
```

## Sample Datasets Included

### 1. Car Maintenance Costs (`sample_car_maintenance_*.csv`)
- Service types (Oil Change, Tire Rotation, Brake Pads, etc.)
- Prices for each service
- Mileage intervals
- Vehicle information
- Service dates

**Example Questions:**
- "What's the average maintenance cost?"
- "Show me the most expensive repairs"
- "How many oil changes are recorded?"
- "What's the total spent on maintenance?"

### 2. Car Prices (`sample_car_prices_*.csv`)
- Vehicle make and model
- Year, mileage, condition
- Prices and fuel types
- Market values

**Example Questions:**
- "What's the average car price?"
- "Show me the most expensive vehicles"
- "How many hybrid cars are there?"
- "What's the price range?"

### 3. Fuel Economy (`sample_fuel_economy_*.csv`)
- City, Highway, and Combined MPG
- Engine size and transmission type
- Fuel type (Gasoline, Hybrid, Electric)
- Vehicle categories

**Example Questions:**
- "What's the average fuel economy?"
- "Which vehicles have the best MPG?"
- "Compare hybrid vs gasoline efficiency"
- "Show me electric vehicle ranges"

### 4. Insurance Rates (`sample_insurance_rates_*.csv`)
- Annual and monthly premiums
- Coverage types and deductibles
- Driver age and location
- Safety ratings

**Example Questions:**
- "What's the average insurance cost?"
- "Which vehicles have the lowest premiums?"
- "How does age affect insurance rates?"
- "Compare insurance by state"

### 5. Repair Costs (`sample_repair_costs_*.csv`)
- Common repair types
- Average costs and labor hours
- Parts prices
- Difficulty levels and frequency

**Example Questions:**
- "What are the most expensive repairs?"
- "Show me common maintenance items"
- "What's the average repair cost?"
- "Which repairs are most frequent?"

### 6. Tire Data (`sample_tire_data_*.csv`)
- Brand and model information
- Prices and warranty coverage
- Tread life and ratings
- Season types

**Example Questions:**
- "What's the average tire price?"
- "Which tires have the longest warranty?"
- "Compare all-season vs all-terrain tires"
- "Show me the best rated tires"

### 7. Depreciation (`sample_depreciation_*.csv`)
- Original vs current values
- Age and mileage
- Depreciation percentages
- Resale ratings

**Example Questions:**
- "Which cars hold their value best?"
- "What's the average depreciation rate?"
- "Show me vehicles with lowest depreciation"
- "How does mileage affect value?"

### 8. Safety Ratings (`sample_safety_ratings_*.csv`)
- Overall and crash test ratings
- ADAS features availability
- Advanced safety systems
- Rollover ratings

**Example Questions:**
- "Which vehicles have 5-star ratings?"
- "Show me cars with adaptive cruise control"
- "What safety features are standard?"
- "Compare safety ratings by make"

### 9. Electric Vehicles (`sample_electric_vehicles_*.csv`)
- Range and battery size
- Charging times
- Performance specs (acceleration, top speed)
- Pricing and efficiency (MPGe)

**Example Questions:**
- "What's the average EV range?"
- "Which electric car is fastest?"
- "Compare charging times"
- "Show me the most affordable EVs"

### 10. Warranty Coverage (`sample_warranty_coverage_*.csv`)
- Basic and powertrain warranty terms
- Corrosion and roadside assistance
- Hybrid battery coverage
- Years and mileage limits

**Example Questions:**
- "Which brands have the best warranty?"
- "Compare powertrain warranties"
- "Show me hybrid battery coverage"
- "What's the average warranty length?"

### 11. Common Problems (`sample_common_problems_*.csv`)
- Known issues by make/model
- Severity and affected mileage
- Repair costs
- Recall and TSB information

**Example Questions:**
- "What are common Honda Accord problems?"
- "Show me the most expensive issues"
- "Which vehicles have recalls?"
- "What problems occur at high mileage?"

## Technical Implementation

### File Storage
- Location: `wwwroot/uploads/datasets/`
- Naming: `{filename}_{timestamp}_{userid}.{ext}`
- Supported formats: CSV, JSON, TXT, XLSX

### Analysis Methods

#### CSV Analysis
```csharp
- Parses headers and data rows
- Identifies numeric columns (price, cost, amount)
- Identifies category columns (type, name, model)
- Calculates statistics
- Groups by categories
- Shows sample data
```

#### JSON Analysis
```csharp
- Counts objects and arrays
- Shows structure overview
- Displays preview
```

#### Text Analysis
```csharp
- Counts lines, words, characters
- Finds most common words
- Identifies patterns
```

### Query Detection
The system uses keyword matching to understand intent:
- `what`, `how many`, `show`, `tell` → General query
- `price`, `cost`, `expensive` → Price analysis
- `average`, `total`, `count` → Statistics
- `pattern`, `trend`, `insight` → Pattern detection

## Car-Related Intelligence

The assistant also provides expert knowledge about:

### Prices
- Oil changes: $30-$125
- Tires: $50-$500 per tire
- Brakes: $150-$800
- Batteries: $50-$250
- Insurance: $1,500-$2,500/year

### Maintenance
- Oil change intervals
- Tire rotation schedules
- Brake inspection guidelines
- Maintenance checklists

### Parts
- Where to buy (AutoZone, RockAuto, etc.)
- OEM vs Aftermarket comparison
- Brand recommendations

## Future Enhancements

### Planned Features
1. **Advanced ML Analysis**
   - Predictive maintenance recommendations
   - Cost forecasting
   - Anomaly detection

2. **Multi-Dataset Correlation**
   - Compare multiple datasets
   - Cross-reference data
   - Historical trend analysis

3. **Export Capabilities**
   - Generate reports
   - Export analysis results
   - Create visualizations

4. **Natural Language Processing**
   - Better query understanding
   - Context retention across conversations
   - Multi-turn dialogue support

## API Endpoints

### Upload Dataset
```
POST /{culture}/AI/Assistant/UploadDataset
- Accepts: multipart/form-data
- Max size: 10MB
- Returns: Analysis and file info
```

### List Datasets
```
GET /{culture}/AI/Assistant/ListDatasets
- Returns: Array of uploaded datasets (20 most recent)
```

### Send Message
```
POST /{culture}/AI/Assistant/SendMessage
- Body: { "message": "your question" }
- Returns: AI response with data analysis if applicable
```

## Best Practices

1. **Dataset Format**
   - Use clear column headers
   - Include numeric values for price/cost columns
   - Add category columns for grouping

2. **Questions**
   - Be specific about what you want to know
   - Use keywords like "average", "total", "show"
   - Reference specific columns if needed

3. **File Management**
   - Keep datasets under 10MB
   - Use descriptive filenames
   - Upload updated data regularly

## Troubleshooting

### Dataset Not Analyzed
- Ensure file format is supported (CSV, JSON, TXT, XLSX)
- Check file size (max 10MB)
- Verify CSV has proper headers

### Incorrect Analysis
- Check column names match expected patterns
- Ensure numeric values don't have special characters
- Verify data format is consistent

### No Response
- Try rephrasing your question
- Use specific keywords
- Check if dataset was uploaded successfully

## Security

- Files are stored with unique names including user ID
- Only user's own datasets are analyzed
- Files are not shared between users
- Uploaded files are excluded from git repository

## Performance

- Analysis is performed in real-time
- Large files may take longer to process
- Results are cached for repeated queries
- Maximum 20 datasets shown in list
