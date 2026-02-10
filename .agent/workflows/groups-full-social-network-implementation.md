# Groups - Full Social Network Implementation

## Overview
The Groups feature now functions as a complete social network where members can create and share content (Posts, Questions, Reviews) within group contexts, similar to Facebook Groups, LinkedIn Groups, or Reddit communities.

## Features Implemented

### 1. Group Management
- **Create Groups**: Users can create public or private groups
- **Join/Leave Groups**: Members can join public groups or leave groups they're part of
- **Member Roles**: Admin, Moderator, and Member roles with different permissions
- **Group Discovery**: Search and browse available groups
- **Member Management**: Admins can remove members and update roles

### 2. Group-Scoped Content

#### Posts in Groups
- Members can create posts within a group context
- Posts have `GroupId` field to associate them with groups
- Group posts appear in the group's feed
- Supports all post types: Discussion, Question, Article, News, Event, Poll

#### Questions (Q&A) in Groups
- Members can ask questions within a group
- Questions have `GroupId` and `Group` navigation property
- Group-specific Q&A helps members get answers from the community
- Supports categories, tags, voting, and accepted answers

#### Reviews in Groups
- Members can write reviews within a group context
- Reviews have `GroupId` and `Group` navigation property
- Useful for product reviews, service reviews, or location reviews within interest groups
- Supports ratings, pros/cons, helpful votes

### 3. Database Schema

#### Entities with Group Support
```csharp
// Post Entity
public class Post : AggregateRoot
{
    public Guid? GroupId { get; set; }
    // ... other properties
}

// Question Entity
public class Question : AggregateRoot
{
    public Guid? GroupId { get; private set; }
    public virtual CommunityGroup? Group { get; private set; }
    // ... other properties
}

// Review Entity
public class Review : AggregateRoot
{
    public Guid? GroupId { get; private set; }
    public virtual CommunityGroup? Group { get; private set; }
    // ... other properties
}
```

### 4. DTOs with Group Information
All content DTOs now include:
- `GroupId`: The ID of the associated group (nullable)
- `GroupName`: The name of the group for display purposes

### 5. AutoMapper Profiles
Created comprehensive AutoMapper profiles:
- **GroupProfile**: Maps `CommunityGroup` → `GroupDto` and `GroupMember` → `GroupMemberDto`
- **QuestionProfile**: Updated to map `GroupId` and `GroupName`
- **ReviewProfile**: Updated to map `GroupId` and `GroupName`
- **PostProfile**: Already had group mapping support
- **MapProfile**: Maps `MapPoint` → `MapPointDto` with location flattening

### 6. Service Layer Support
Services support filtering by GroupId:
- `GetPostsAsync(... groupId ...)`: Get posts for a specific group
- `GetQuestionsAsync(... groupId ...)`: Get questions for a specific group
- `GetReviewsAsync(... groupId ...)`: Get reviews for a specific group

## Usage Examples

### Creating Content in a Group

#### Create a Post in a Group
```csharp
var post = await _postService.CreatePostAsync(
    title: "Welcome to our group!",
    content: "Let's discuss...",
    type: PostType.Discussion,
    authorId: userId,
    category: PostCategory.General,
    groupId: groupId  // Associate with group
);
```

#### Create a Question in a Group
```csharp
var question = new Question(
    title: "How do I...?",
    content: "I need help with...",
    authorId: userId,
    categoryId: categoryId,
    groupId: groupId  // Associate with group
);
```

#### Create a Review in a Group
```csharp
var review = new Review(
    entityId: productId,
    entityType: "Product",
    type: ReviewType.Product,
    reviewerId: userId,
    rating: 5,
    title: "Great product!",
    comment: "I love it because...",
    groupId: groupId  // Associate with group
);
```

### Filtering Content by Group

#### Get Group Posts
```csharp
var posts = await _postService.GetPostsAsync(
    parameters: new QueryParameters { PageNumber = 1, PageSize = 20 },
    status: PostStatus.Published,
    type: null,
    groupId: groupId,  // Filter by group
    currentUserId: userId
);
```

## Benefits

### For Users
1. **Organized Discussions**: Content is organized by interest groups
2. **Targeted Audience**: Share content with specific communities
3. **Community Building**: Build relationships with like-minded members
4. **Focused Content**: See only content relevant to groups you're interested in

### For the Platform
1. **Increased Engagement**: Users spend more time in groups they care about
2. **Content Organization**: Better content categorization and discovery
3. **Moderation**: Group admins can moderate their own communities
4. **Scalability**: Groups can grow independently

## Real-World Comparisons

### Similar to Facebook Groups
- Create and join groups
- Post discussions, questions, and media
- Member roles and permissions
- Group discovery and search

### Similar to LinkedIn Groups
- Professional networking within groups
- Share articles and insights
- Q&A within professional communities
- Member management

### Similar to Reddit Communities
- Subreddit-like group structure
- Upvoting/downvoting (through reactions)
- Moderation by group admins
- Community-driven content

## Technical Implementation

### Database Migrations
- Groups already have proper database schema
- Posts, Questions, and Reviews have `GroupId` foreign key columns
- Indexes on `GroupId` for efficient querying

### Navigation Properties
- Entities have `Group` navigation property for eager loading
- Enables efficient queries with `.Include(x => x.Group)`

### AutoMapper Configuration
- Automatic mapping of group information to DTOs
- Handles null groups gracefully
- Maps group names for display

### Service Layer
- All content services support group filtering
- Group membership validation
- Permission checks for group content

## Future Enhancements

### Potential Features
1. **Group Feeds**: Dedicated feed showing only group content
2. **Group Notifications**: Notify members of new content in their groups
3. **Group Analytics**: Track engagement metrics per group
4. **Group Invitations**: Invite users to join private groups
5. **Group Rules**: Custom rules and guidelines per group
6. **Group Events**: Schedule events within groups
7. **Group Files**: Share files and documents within groups
8. **Group Polls**: Create polls for group decisions

## Conclusion
The Groups feature is now a fully functional social network component that enables community-driven content creation and sharing. Members can create posts, ask questions, and write reviews within group contexts, making the platform more engaging and organized.
