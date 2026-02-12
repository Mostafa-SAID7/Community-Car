# Views Organization Summary

## Changes Made

### 1. Removed Duplicate Views
- ✓ Deleted `Views/Info/Privacy.cshtml` (duplicate of `Views/Legal/Privacy.cshtml`)
- ✓ Deleted `Views/Info/Terms.cshtml` (duplicate of `Views/Legal/Terms.cshtml`)
- ✓ Moved `Views/Info/Support.cshtml` to `Views/Legal/Support.cshtml`

### 2. Current Views Structure

```
Views/
├── Answers/
│   └── Edit.cshtml
├── Badges/
│   ├── Index.cshtml
│   └── MyBadges.cshtml
├── Chats/
│   ├── Conversation.cshtml
│   └── Index.cshtml
├── Error/
│   ├── 400.cshtml, 401.cshtml, 403.cshtml
│   ├── 404.cshtml, 500.cshtml, 503.cshtml
├── Events/
│   ├── Create.cshtml, Details.cshtml, Edit.cshtml
│   ├── Index.cshtml, MyEvents.cshtml
├── Feed/
│   ├── _FeedFilters.cshtml, _FeedItem.cshtml
│   ├── Index.cshtml, Popular.cshtml, Trending.cshtml
├── Friends/
│   ├── _FriendCard.cshtml, _UserSearchCard.cshtml
│   ├── Blocked.cshtml, Index.cshtml
│   ├── Requests.cshtml, Search.cshtml
│   ├── SentRequests.cshtml, Suggestions.cshtml
├── Groups/
│   ├── Create.cshtml, Details.cshtml, Edit.cshtml
│   ├── Index.cshtml, Leave.cshtml, MyGroups.cshtml
├── Guides/
│   ├── Create.cshtml, Details.cshtml, Edit.cshtml
│   ├── Index.cshtml, MyGuides.cshtml
├── Legal/
│   ├── Privacy.cshtml
│   ├── Support.cshtml
│   └── Terms.cshtml
├── Maps/
│   ├── Create.cshtml, Details.cshtml, Edit.cshtml
│   ├── Featured.cshtml, Index.cshtml
│   ├── MyPoints.cshtml, Search.cshtml
├── News/
│   ├── Create.cshtml, Details.cshtml, Edit.cshtml
│   ├── Featured.cshtml, Index.cshtml, MyArticles.cshtml
├── Posts/
│   ├── Create.cshtml, Details.cshtml, Edit.cshtml
│   ├── Featured.cshtml, Index.cshtml, MyPosts.cshtml
├── Questions/
│   ├── _AnswerItem.cshtml, _QuestionList.cshtml
│   ├── Bookmarks.cshtml, Create.cshtml
│   ├── Details.cshtml, Edit.cshtml
│   ├── Index.cshtml, MyQuestions.cshtml
├── Reviews/
│   ├── Create.cshtml, Details.cshtml, Edit.cshtml
│   ├── Index.cshtml, MyReviews.cshtml
├── Search/
│   └── Index.cshtml
├── Shared/
│   ├── Sidebars/
│   │   ├── _FeedSidebar.cshtml
│   │   ├── _RightSidebar_Chat.cshtml
│   │   ├── _RightSidebar_Dashboard.cshtml
│   │   ├── _RightSidebar_Events.cshtml
│   │   ├── _RightSidebar_Feed.cshtml
│   │   ├── _RightSidebar_Generic.cshtml
│   │   ├── _RightSidebar_Guides.cshtml
│   │   ├── _RightSidebar_Maps.cshtml
│   │   ├── _RightSidebar_News.cshtml
│   │   ├── _RightSidebar_QA.cshtml
│   │   ├── _RightSidebar_Reviews.cshtml
│   │   └── _RightSidebar_Social.cshtml
│   ├── _CommentForm.cshtml
│   ├── _CommentItem.cshtml
│   ├── _CommentList.cshtml
│   ├── _EditCommentForm.cshtml
│   ├── _Footer.cshtml
│   ├── _Header.cshtml
│   ├── _InnerPageHeader.cshtml
│   ├── _Layout.cshtml
│   ├── _LeftSidebar.cshtml
│   ├── _NotificationDropdown.cshtml
│   ├── _ValidationScriptsPartial.cshtml
│   └── Error.cshtml
└── Support/
    ├── Contact.cshtml
    ├── FAQ.cshtml
    └── Index.cshtml
```

## Controllers Status

### ✓ Existing Controllers
- FeedController (Community)
- ChatsController (Areas/Communications)
- AccountController (Areas/Identity)
- ProfilesController (Areas/Identity)
- NotificationsController (Areas/Communications)
- All Dashboard Controllers

### ✗ Missing Controllers (Need to be Created)

#### Community Controllers
- PostsController
- QuestionsController
- AnswersController
- EventsController
- GroupsController
- GuidesController
- NewsController
- ReviewsController
- MapsController

#### Social Controllers
- FriendsController
- BadgesController

#### Common Controllers
- ErrorController
- SearchController
- LegalController
- SupportController

## Sidebar Organization

### Current Sidebars
1. **_FeedSidebar.cshtml** - Feed-specific stats (left sidebar)
2. **_RightSidebar_Feed.cshtml** - Trending topics, suggested friends
3. **_RightSidebar_Chat.cshtml** - Chat-specific sidebar
4. **_RightSidebar_Dashboard.cshtml** - Dashboard widgets
5. **_RightSidebar_Events.cshtml** - Upcoming events
6. **_RightSidebar_Generic.cshtml** - Generic content
7. **_RightSidebar_Guides.cshtml** - Popular guides
8. **_RightSidebar_Maps.cshtml** - Map locations
9. **_RightSidebar_News.cshtml** - Latest news
10. **_RightSidebar_QA.cshtml** - Top questions
11. **_RightSidebar_Reviews.cshtml** - Recent reviews
12. **_RightSidebar_Social.cshtml** - Social features

### Sidebar Usage Pattern
Views use `@section RightSidebar` to specify which sidebar to render:
```razor
@section RightSidebar {
    <partial name="Sidebars/_FeedSidebar" />
}
```

## Recommendations

### Immediate Actions Needed

1. **Create Missing Controllers**
   - Priority: Community controllers (Posts, Questions, Events, etc.)
   - These are core features with existing views

2. **Consolidate Legal/Support**
   - Merge Support views into Legal folder
   - Create single LegalController for all legal/support pages

3. **Standardize Sidebar Naming**
   - Keep current structure (it's working well)
   - Ensure consistent naming: `_RightSidebar_{Feature}.cshtml`

### Future Improvements

1. **Group Views by Feature Area** (Breaking Change)
   - Move to Areas for better organization
   - Example: `Areas/Community/Views/Posts/`

2. **Create Shared Components**
   - Extract common patterns into ViewComponents
   - Example: FeedItemComponent, CommentComponent

3. **Implement View Models Consistently**
   - Ensure all views have corresponding ViewModels
   - Follow naming convention: `{Feature}{Action}ViewModel`

## View Naming Conventions

### Current Patterns
- **Index.cshtml** - List/Browse view
- **Details.cshtml** - Single item view
- **Create.cshtml** - Create new item
- **Edit.cshtml** - Edit existing item
- **My{Feature}.cshtml** - User's own items
- **_{Component}.cshtml** - Partial views/components

### Consistency Check
✓ All features follow this pattern consistently
✓ Partial views prefixed with underscore
✓ Feature-specific views in feature folders

## Areas Structure

### Current Areas
```
Areas/
├── AI/
│   ├── Controllers/AssistantController.cs
│   └── Views/Assistant/Index.cshtml
├── Communications/
│   ├── Controllers/
│   │   ├── ChatsController.cs
│   │   └── notifications/NotificationsController.cs
│   ├── ViewModels/
│   └── Views/
├── Dashboard/
│   ├── Controllers/ (Multiple organized by feature)
│   ├── ViewModels/
│   └── Views/
└── Identity/
    ├── Controllers/
    │   ├── Account/AccountController.cs
    │   └── Profiles/ProfilesController.cs
    ├── ViewModels/
    ├── Validators/
    └── Views/
```

### Area Organization
✓ Dashboard - Well organized with subfolders
✓ Identity - Proper structure
✓ Communications - Could be moved to main Controllers
✓ AI - Good separation

## Next Steps

1. Create missing controllers (see list above)
2. Implement services for each controller
3. Add proper routing and authorization
4. Test all views render correctly
5. Add localization for all views
6. Implement validation for forms

## Files Modified
- Deleted: `Views/Info/Privacy.cshtml`
- Deleted: `Views/Info/Terms.cshtml`
- Moved: `Views/Info/Support.cshtml` → `Views/Legal/Support.cshtml`

## Files to Create
See "Missing Controllers" section above for complete list.
