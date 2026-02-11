# AI Assistant - Dataset Priority System

## Overview
The AI Assistant now prioritizes information from uploaded datasets over generic knowledge, providing accurate, context-specific responses based on YOUR data.

## How It Works

### Priority Order
1. **Uploaded Dataset Information** (Highest Priority)
2. **Generic Knowledge** (Fallback)

### Example Scenario

**Without Dataset:**
```
User: "Tell me about Toyota Corolla price"
AI: "Toyota Corolla prices typically range from $20,000-$30,000 USD..."
```

**With Uploaded Dataset (Egypt Market):**
```
User: "Tell me about Toyota Corolla price"
AI: "🚗 Vehicle Details (from your uploaded data):

🏢 Brand: Toyota
📛 Model: Corolla
📅 Year: 2024
💰 Price: 1,250,000 EGP

⚙️ Specifications:
• Body Type: Sedan
• Fuel Type: Petrol
• Transmission: Automatic
• Engine: 1600 cc
• Power: 120 HP

📦 Availability: Available
🌍 Origin: Imported
📍 Market: Egypt

💡 This information is from your uploaded dataset!"
```

## Supported Dataset Formats

### JSON Structure
```json
{
  "metadata": {
    "country": "Egypt",
    "currency": "EGP",
    "lastUpdated": "2026-01-01"
  },
  "cars": [
    {
      "brand": "Toyota",
      "model": "Corolla",
      "year": 2024,
      "price": 1250000,
      "bodyType": "Sedan",
      "fuelType": "Petrol",
      "transmission": "Automatic",
      "engineCc": 1600,
      "horsePower": 120,
      "availability": "Available",
      "origin": "Imported"
    }
  ]
}
```

### CSV Structure
```csv
Brand,Model,Year,Price,BodyType,FuelType,Transmission,EngineCc,HorsePower
Toyota,Corolla,2024,1250000,Sedan,Petrol,Automatic,1600,120
```

## Query Detection

### Specific Vehicle Queries
The AI detects when you ask about a specific vehicle:
- "Toyota Corolla price"
- "Tell me about Nissan Sunny"
- "How much is the Hyundai Elantra"
- "Kia Sportage specifications"

### General Queries
For general questions, the AI analyzes all data:
- "What's the average price?"
- "Show me all vehicles"
- "Which is the cheapest car?"
- "Compare all brands"

## Features

### 1. Specific Vehicle Lookup
```
User: "Toyota Corolla"
AI: Returns complete details from YOUR dataset
```

### 2. Price Analysis
```
User: "What's the average price?"
AI: Calculates from YOUR data:
• Average: 1,482,000 EGP
• Minimum: 780,000 EGP (Chevrolet Optra)
• Maximum: 2,200,000 EGP (Peugeot 3008)
```

### 3. Brand Comparison
```
User: "Compare brands"
AI: Shows distribution from YOUR data:
• Toyota: 1 model
• Nissan: 1 model
• Hyundai: 1 model
• Kia: 1 model
• MG: 1 model
```

### 4. Availability Check
```
User: "What's available?"
AI: Lists vehicles with "Available" status from YOUR data
```

## Metadata Support

### Country-Specific Information
```json
"metadata": {
  "country": "Egypt",
  "currency": "EGP"
}
```

The AI automatically:
- Uses the correct currency symbol
- Mentions the market location
- Formats prices according to locale

### Last Updated Tracking
```json
"metadata": {
  "lastUpdated": "2026-01-01"
}
```

Shows when the data was last updated for accuracy.

## Query Examples

### Specific Vehicle
```
User: "Tell me about Toyota Corolla"
AI: Shows complete Toyota Corolla details from dataset
```

### Price Comparison
```
User: "What's the cheapest car?"
AI: "💵 Cheapest: Chevrolet Optra - 780,000 EGP"
```

### Specifications
```
User: "Show me SUVs"
AI: Lists all vehicles with bodyType: "SUV" from dataset
```

### Availability
```
User: "What can I buy now?"
AI: Lists vehicles with availability: "Available"
```

## Benefits

### 1. Accurate Information
- No generic estimates
- Real market data
- Your specific context

### 2. Market-Specific
- Local currency
- Local availability
- Local pricing

### 3. Up-to-Date
- Based on your latest upload
- Reflects current market
- No outdated information

### 4. Contextual
- Understands your market
- Relevant comparisons
- Appropriate recommendations

## How to Use

### Step 1: Upload Dataset
1. Click upload button in AI Assistant
2. Select your JSON or CSV file
3. Wait for analysis

### Step 2: Ask Questions
```
"Tell me about Toyota Corolla"
"What's the average price?"
"Show me all SUVs"
"Which is the cheapest?"
```

### Step 3: Get Accurate Answers
The AI responds with information from YOUR dataset, not generic knowledge.

## Advanced Features

### Multi-Dataset Support
Upload multiple datasets:
- Car prices
- Maintenance costs
- Insurance rates
- Fuel economy

The AI analyzes the most relevant dataset for each query.

### Smart Fallback
If no dataset matches your query, the AI falls back to generic knowledge:
```
User: "How do I change oil?" (No dataset for this)
AI: Provides generic oil change guide
```

### Context Retention
The AI remembers your uploaded datasets throughout the conversation:
```
User: "Upload car_prices.json"
AI: "Dataset uploaded!"

User: "What's the Toyota Corolla price?"
AI: Uses car_prices.json data

User: "Compare with Nissan Sunny"
AI: Still uses car_prices.json data
```

## Data Privacy

### Your Data
- Stored securely
- User-specific access
- Not shared between users
- Can be deleted anytime

### File Naming
Files are saved with unique identifiers:
```
dataset_20260211_123512_7042ecb2.json
```
- Timestamp included
- User ID included
- Original name preserved

## Best Practices

### 1. Structure Your Data
Use clear field names:
- `brand`, `model`, `year`
- `price`, `cost`, `amount`
- `bodyType`, `fuelType`, `transmission`

### 2. Include Metadata
Add context to your dataset:
```json
"metadata": {
  "country": "Egypt",
  "currency": "EGP",
  "lastUpdated": "2026-01-01",
  "source": "Official Dealer Prices"
}
```

### 3. Keep Data Updated
- Upload new versions regularly
- Remove outdated files
- Verify accuracy

### 4. Use Descriptive Names
- `egypt_car_prices_2026.json` ✅
- `dataset.json` ❌

## Troubleshooting

### AI Not Using My Data
**Check:**
1. File uploaded successfully?
2. Query matches data (e.g., "Toyota" in dataset)?
3. File format correct (JSON/CSV)?

### Wrong Information
**Verify:**
1. Dataset contains correct data
2. Field names are standard
3. Values are properly formatted

### Generic Response
**Possible Reasons:**
1. No matching data in dataset
2. Query too vague
3. Dataset not uploaded

## Future Enhancements

### Planned Features
- [ ] Multi-dataset correlation
- [ ] Historical data tracking
- [ ] Price trend analysis
- [ ] Automatic data updates
- [ ] Data validation
- [ ] Export analyzed results

## Conclusion

The AI Assistant now provides reliable, accurate information based on YOUR uploaded data, making it a powerful tool for market-specific queries and analysis. Upload your datasets and get precise answers tailored to your context!
