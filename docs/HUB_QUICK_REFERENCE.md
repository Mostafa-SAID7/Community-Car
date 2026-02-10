# SignalR Hub Quick Reference Guide

## Available Hubs

| Hub | Endpoint | Purpose | Service Interface |
|-----|----------|---------|-------------------|
| QuestionHub | `/hubs/question` | Q&A real-time updates | `IQuestionHubService` |
| PostHub | `/hubs/post` | Post notifications | `IPostHubService` |
| FriendHub | `/hubs/friend` | Friend activities | `IFriendHubService` |
| NotificationHub | `/hubs/notification` | General notifications | `INotificationHubService` |
| GenericHub | `/hubs/generic` | Legacy/Mult