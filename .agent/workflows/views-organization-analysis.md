# Views Organization Analysis

## Current Issues

### 1. Duplicate Views
- **Info** and **Legal** folders contain duplicate Privacy and Terms views
- **Info/Support.cshtml** exists alongside **Support/** folder

### 2. Missing Controllers

Views exist but controllers are missing for:
- **Answers** (Views/Answers/Edit.cshtml)
- **Badges** (Views/Badges/)
- **Events** (Views/Events/)
- **Groups** (Views/Groups/)
- **Guides** (Views/Guides/)
- **Info** (Views/Info/)
- **Legal** (Views/Legal/)
- **Maps** (Views/Maps/)
- **News** (Views/News/)
- **Posts** (Views/Posts/)
- **Questions** (Views/Questions/)
- **Reviews** (Views/Reviews/)
- **Search** (Views/Search/)
- **Support** (Views/Support/)
- **Error** (Views/Error/)

### 3. Sidebar Organization
Multiple right sidebar partials exist:
- _RightSidebar_Chat.cshtml
- _RightSidebar_Dashboard.cshtml
- _RightSidebar_Events.cshtml
- _RightSidebar_Feed.cshtml (duplicate with _FeedSidebar.cshtml?)
- _RightSidebar_Generic.cshtml
- _RightSidebar_Guides.cshtml
- _RightSidebar_Maps.cshtml
- _RightSidebar_News.cshtml
- _RightSidebar_QA.cshtml
- _RightSidebar_Reviews.cshtml
- _RightSidebar_Social.cshtml

## Recommended Structure

### Views Organization
```
Views/
├── Community/           # Community features
│   ├── Posts/
│   ├── Questions/
│   ├── Answers/
│   ├── Events/
│   ├── Groups/
│   ├── Guides/
│   ├── News/
│   ├── Reviews/
│   ├── Maps/
│   └── Feed/
├── Social/              # Social features
│   ├── Friends/
│   ├── Chats/
│   └── Badges/
├── Common/              # Common pages
│   ├── Error/
│   ├── Search/
│   ├── Legal/          # Merge Info and Legal here
│   └── Support/
└── Shared/
    ├── Components/     # Reusable components
    │   ├── _CommentForm.cshtml
    │   ├── _CommentItem.cshtml
    │   └── _CommentList.cshtml
    └── Sidebars/
        ├── _LeftSidebar.cshtml
        └── _RightSidebar.cshtml (with sections)
```

### Controllers Organization
```
Controllers/
├── Community/
│   ├── PostsController.cs
│   ├── QuestionsController.cs
│   ├── AnswersController.cs
│   ├── EventsController.cs
│   ├── GroupsController.cs
│   ├── GuidesController.cs
│   ├── NewsController.cs
│   ├── ReviewsController.cs
│   ├── MapsController.cs
│   └── FeedController.cs ✓
├── Social/
│   ├── FriendsController.cs
│   ├── ChatsController.cs (move from Areas/Communications)
│   └── BadgesController.cs
└── Common/
    ├── ErrorController.cs
    ├── SearchController.cs
    ├── LegalController.cs
    └── SupportController.cs
```

## Action Plan

### Phase 1: Clean Up Duplicates
1. ✓ Merge Info and Legal folders into single Legal folder
2. ✓ Remove duplicate views
3. ✓ Update routes and links

### Phase 2: Create Missing Controllers
1. ✓ Create all Community controllers
2. ✓ Create Social controllers
3. ✓ Create Common controllers
4. ✓ Move ChatsController from Areas to main Controllers

### Phase 3: Reorganize Views (Optional - Breaking Change)
1. Group views by feature area
2. Update _ViewImports for each area
3. Update all route references

### Phase 4: Consolidate Sidebars
1. Create single _RightSidebar.cshtml with sections
2. Use ViewData to determine which sidebar content to show
3. Remove duplicate sidebar files
