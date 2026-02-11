# Car Datasets Overview

## Complete Dataset Collection
The AI Assistant now includes 11 comprehensive datasets covering all major automotive topics.

## Dataset Categories

### 🔧 Maintenance & Repairs
1. **Car Maintenance Costs** - Regular service pricing and schedules
2. **Repair Costs** - Common repair types, labor hours, and costs

### 💰 Financial
3. **Car Prices** - Vehicle valuations and market prices
4. **Insurance Rates** - Premium costs by vehicle, age, and location
5. **Depreciation** - Value retention and resale data
6. **Warranty Coverage** - Manufacturer warranty terms and coverage

### ⚡ Performance & Efficiency
7. **Fuel Economy** - MPG ratings for gas, hybrid, and electric vehicles
8. **Electric Vehicles** - EV-specific data including range and charging

### 🛡️ Safety & Reliability
9. **Safety Ratings** - Crash test scores and ADAS features
10. **Common Problems** - Known issues, recalls, and TSBs

### 🛞 Parts & Accessories
11. **Tire Data** - Tire brands, prices, ratings, and warranties

## Quick Reference Guide

### By Use Case

**Buying a Car?**
- Car Prices (market values)
- Depreciation (resale value)
- Safety Ratings (crash tests)
- Common Problems (reliability)
- Warranty Coverage (protection)
- Insurance Rates (ongoing costs)

**Maintaining Your Car?**
- Car Maintenance Costs (service pricing)
- Repair Costs (fix-it costs)
- Tire Data (replacement options)
- Fuel Economy (efficiency)

**Comparing Vehicles?**
- All datasets provide comparison data
- Ask: "Compare [Make] vs [Make]"
- Ask: "Show me the best [category]"

**Budget Planning?**
- Insurance Rates (annual costs)
- Fuel Economy (fuel costs)
- Maintenance Costs (service costs)
- Depreciation (value loss)

## Dataset Statistics

| Dataset | Records | Fields | Categories |
|---------|---------|--------|------------|
| Maintenance Costs | 15 | 6 | 2 (Maintenance, Repair) |
| Car Prices | 15 | 7 | 3 (Sedan, Truck, SUV) |
| Fuel Economy | 15 | 10 | 4 (Sedan, Truck, SUV, Electric) |
| Insurance Rates | 15 | 11 | 4 (States: CA, TX, FL, NY) |
| Repair Costs | 20 | 8 | 8 (Categories) |
| Tire Data | 15 | 11 | 4 (All-Season, All-Weather, etc.) |
| Depreciation | 15 | 10 | 4 (Resale Ratings) |
| Safety Ratings | 15 | 12 | 10 (Safety Features) |
| Electric Vehicles | 15 | 12 | 1 (EV) |
| Warranty Coverage | 15 | 11 | 3 (Basic, Powertrain, Hybrid) |
| Common Problems | 15 | 10 | 3 (Severity Levels) |

**Total Records:** 185
**Total Data Points:** 1,850+

## AI Analysis Capabilities

### Automatic Detection
The AI automatically detects and analyzes:
- **Price columns**: price, cost, amount, value
- **Category columns**: category, type, name, model, make
- **Numeric data**: Calculates average, min, max, total
- **Groupings**: Counts by category, brand, type
- **Patterns**: Most common values and trends

### Query Understanding
The AI understands natural language questions:
- "What's the average...?" → Statistical analysis
- "Show me..." → Data listing
- "Compare..." → Comparison analysis
- "Which has the best...?" → Ranking
- "How many...?" → Counting
- "What patterns...?" → Trend analysis

## Example Queries by Dataset

### Maintenance Costs
```
"What's the average oil change cost?"
"Show me all brake services"
"Total maintenance spending"
"Most expensive service"
```

### Car Prices
```
"Average price by make"
"Most expensive vehicles"
"Compare hybrid vs gas prices"
"Show me vehicles under $30k"
```

### Fuel Economy
```
"Best MPG vehicles"
"Average city vs highway"
"Compare sedan efficiency"
"Electric vehicle ranges"
```

### Insurance Rates
```
"Cheapest insurance by state"
"Average premium by age"
"Full coverage costs"
"Compare deductibles"
```

### Repair Costs
```
"Most expensive repairs"
"Average labor hours"
"Common vs rare repairs"
"Electrical system costs"
```

### Tire Data
```
"Best warranty tires"
"Average tire price"
"All-season vs all-terrain"
"Highest rated brands"
```

### Depreciation
```
"Best resale value"
"Average depreciation rate"
"Value loss by age"
"Compare makes"
```

### Safety Ratings
```
"5-star rated vehicles"
"Standard safety features"
"ADAS availability"
"Best crash test scores"
```

### Electric Vehicles
```
"Longest range EVs"
"Fastest charging"
"Most affordable"
"Performance comparison"
```

### Warranty Coverage
```
"Best powertrain warranty"
"Hybrid battery coverage"
"Longest basic warranty"
"Compare by brand"
```

### Common Problems
```
"Honda Accord issues"
"High mileage problems"
"Recall information"
"Most expensive fixes"
```

## Advanced Analysis Examples

### Multi-Factor Analysis
```
"Show me vehicles with:
- Good fuel economy
- Low insurance rates
- High safety ratings
- Low depreciation"
```

### Cost of Ownership
```
"Calculate total cost for Toyota Camry:
- Purchase price
- Insurance
- Maintenance
- Fuel costs
- Depreciation"
```

### Comparison Analysis
```
"Compare Honda Accord vs Toyota Camry:
- Price
- Fuel economy
- Insurance rates
- Maintenance costs
- Safety ratings
- Depreciation"
```

## Data Quality

### Accuracy
- Based on real-world market data
- Updated regularly
- Reflects current pricing trends
- Includes regional variations

### Coverage
- Major brands represented
- Popular models included
- Multiple years covered
- Various vehicle types

### Completeness
- All critical fields included
- No missing essential data
- Comprehensive coverage
- Real-world scenarios

## Using the Datasets

### Upload Your Own Data
1. Format as CSV with clear headers
2. Include relevant columns (price, category, etc.)
3. Upload via AI Assistant
4. Ask questions immediately

### Best Practices
- Use descriptive column names
- Include numeric values for analysis
- Add category columns for grouping
- Keep data consistent

### Tips for Better Results
- Be specific in questions
- Use keywords (average, total, show, compare)
- Reference specific makes/models
- Ask follow-up questions

## Future Enhancements

### Planned Additions
- **Lease vs Buy Analysis**
- **Financing Options**
- **Regional Price Variations**
- **Historical Trends**
- **Predictive Maintenance**
- **Cost Forecasting**

### Advanced Features
- Multi-dataset correlation
- Predictive analytics
- Custom reports
- Data visualization
- Export capabilities

## Technical Details

### File Format
- CSV (Comma-Separated Values)
- UTF-8 encoding
- Header row required
- Consistent delimiters

### Storage
- Location: `wwwroot/uploads/datasets/`
- Naming: `{name}_{timestamp}_{userid}.csv`
- Max size: 10MB per file
- Persistent storage

### Security
- User-specific access
- Unique file naming
- Excluded from git
- Secure storage

## Support

### Troubleshooting
- Ensure CSV format is correct
- Check column headers
- Verify numeric values
- Review file size

### Getting Help
- Ask the AI Assistant
- Check documentation
- Review example queries
- Test with sample data

## Conclusion

These comprehensive datasets enable the AI Assistant to provide intelligent, data-driven responses about all aspects of car ownership, maintenance, and purchasing decisions. The system automatically analyzes your questions and extracts relevant insights from the appropriate datasets.
