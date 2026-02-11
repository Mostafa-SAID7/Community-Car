namespace CommunityCar.Domain.Models.AI
{
    public class ConversationContext
    {
        public string UserId { get; set; }
        public List<ConversationMessage> Messages { get; set; } = new();
        public ConversationState State { get; set; } = ConversationState.Idle;
        public Dictionary<string, object> Data { get; set; } = new();
        public DateTime LastActivity { get; set; } = DateTime.UtcNow;
        
        public void AddMessage(string role, string content)
        {
            Messages.Add(new ConversationMessage
            {
                Role = role,
                Content = content,
                Timestamp = DateTime.UtcNow
            });
            LastActivity = DateTime.UtcNow;
        }
        
        public string GetLastUserMessage()
        {
            return Messages.LastOrDefault(m => m.Role == "user")?.Content;
        }
        
        public string GetLastAssistantMessage()
        {
            return Messages.LastOrDefault(m => m.Role == "assistant")?.Content;
        }
        
        public List<ConversationMessage> GetRecentMessages(int count = 5)
        {
            return Messages.TakeLast(count).ToList();
        }
        
        public void SetState(ConversationState state, Dictionary<string, object> data = null)
        {
            State = state;
            if (data != null)
            {
                foreach (var kvp in data)
                {
                    Data[kvp.Key] = kvp.Value;
                }
            }
        }
        
        public T GetData<T>(string key, T defaultValue = default)
        {
            if (Data.TryGetValue(key, out var value) && value is T typedValue)
            {
                return typedValue;
            }
            return defaultValue;
        }
        
        public void ClearContext()
        {
            State = ConversationState.Idle;
            Data.Clear();
        }
    }
    
    public class ConversationMessage
    {
        public string Role { get; set; } // "user" or "assistant"
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
    }
    
    public enum ConversationState
    {
        Idle,
        AskingAboutVehicle,
        AskingForYear,
        AskingForDetails,
        AskingForMaintenance,
        AskingForPrice,
        ComparingVehicles,
        AnalyzingData
    }
}
