# Reviews UI Implementation Guide

## Quick Start - Add to Your Views

### 1. Star Rating Input (Create/Edit Forms)

```html
<!-- In Create.cshtml or Edit.cshtml -->
<div class="rating-input-group">
    <label class="rating-input-label" for="Rating">Rating *</label>
    <div class="rating-input-wrapper">
        <div class="star-rating" data-rating-input>
            <input type="hidden" asp-for="Rating" />
            <span class="star" data-value="1"></span>
            <span class="star" data-value="2"></span>
            <span class="star" data-value="3"></span>
            <span class="star" data-value="4"></span>
            <span class="star" data-value="5"></span>
        </div>
        <span class="rating-value-display">0.0</span>
    </div>
    <span asp-validation-for="Rating" class="text-danger"></span>
</div>
```

### 2. Star Rating Display (Read-Only)

```html
<!-- Show existing rating -->
<div class="rating-display">
    <div class="star-rating readonly">
        @for (int i = 1; i <= 5; i++)
        {
            var isFilled = i <= Math.Floor(Model.Rating);
            var isHalf = !isFilled && i == Math.Ceiling(Model.Rating);
            <span class="star @(isFilled ? "filled" : "") @(isHalf ? "half" : "")"></span>
        }
    </div>
    <span class="rating-number">@Model.Rating.ToString("0.0")</span>
    <span class="rating-count">(@Model.TotalReviews reviews)</span>
</div>
```

### 3. Rating Distribution Chart

```html
<div class="rating-distribution">
    <h3>Rating Breakdown</h3>
    @for (int stars = 5; stars >= 1; stars--)
    {
        var count = Model.RatingDistribution.GetValueOrDefault(stars, 0);
        var percentage = Model.TotalReviews > 0 ? (count * 100.0 / Model.TotalReviews) : 0;
        
        <div class="rating-bar">
            <span class="rating-bar-label">@stars ★</span>
            <div class="rating-bar-track">
                <div class="rating-bar-fill" style="width: @percentage%"></div>
            </div>
            <span class="rating-bar-count">@count</span>
        </div>
    }
</div>
```

### 4. Helpful Buttons

```html
<div class="helpful-actions">
    <span>Was this review helpful?</span>
    <button type="button" 
            class="helpful-btn @(Model.UserReaction == true ? "active" : "")"
            data-helpful-btn
            data-review-id="@Model.Id"
            data-helpful="true">
        👍 Yes (<span class="helpful-count">@Model.HelpfulCount</span>)
    </button>
    <button type="button" 
            class="helpful-btn @(Model.UserReaction == false ? "active" : "")"
            data-helpful-btn
            data-review-id="@Model.Id"
            data-helpful="false">
        👎 No (<span class="helpful-count">@Model.NotHelpfulCount</span>)
    </button>
</div>
```

### 5. Flag Review Button

```html
@if (User.Identity.IsAuthenticated && !Model.IsReviewer)
{
    <button type="button" 
            class="btn btn-link text-danger"
            data-flag-btn
            data-review-id="@Model.Id">
        🚩 Flag as inappropriate
    </button>
}
```

### 6. Delete Review Button

```html
@if (Model.IsReviewer)
{
    <button type="button" 
            class="btn btn-danger"
            data-delete-review-btn
            data-review-id="@Model.Id">
        Delete Review
    </button>
}
```

### 7. Comment Form

```html
<form data-comment-form data-review-id="@Model.Id">
    <div class="form-group">
        <label for="comment">Add a comment</label>
        <textarea class="form-control" 
                  name="content" 
                  rows="3" 
                  placeholder="Share your thoughts..."
                  required></textarea>
    </div>
    <button type="submit" class="btn btn-primary">Post Comment</button>
</form>
```

### 8. AJAX Review Form

```html
<!-- Add data-review-form attribute to enable AJAX -->
<form asp-action="Create" 
      method="post" 
      data-review-form>
    @Html.AntiForgeryToken()
    
    <!-- Rating input (see #1 above) -->
    
    <div class="form-group">
        <label asp-for="Title"></label>
        <input asp-for="Title" class="form-control" />
        <span asp-validation-for="Title" class="text-danger"></span>
    </div>
    
    <div class="form-group">
        <label asp-for="Content"></label>
        <textarea asp-for="Content" class="form-control" rows="5"></textarea>
        <span asp-validation-for="Content" class="text-danger"></span>
    </div>
    
    <div class="form-group">
        <label asp-for="Pros"></label>
        <textarea asp-for="Pros" class="form-control" rows="3"></textarea>
    </div>
    
    <div class="form-group">
        <label asp-for="Cons"></label>
        <textarea asp-for="Cons" class="form-control" rows="3"></textarea>
    </div>
    
    <div class="form-check">
        <input asp-for="IsRecommended" class="form-check-input" />
        <label asp-for="IsRecommended" class="form-check-label">
            I recommend this
        </label>
    </div>
    
    <button type="submit" class="btn btn-primary">Submit Review</button>
</form>
```

## Required References

### In _Layout.cshtml (or specific pages)

```html
<!-- CSS -->
<link rel="stylesheet" href="~/css/components/star-rating.css" />

<!-- JavaScript (before closing </body>) -->
<script src="~/js/pages/reviews.js"></script>
```

## C# Helper Methods (Optional)

Add to your view or create a helper class:

```csharp
@functions {
    public static string GetStarClass(decimal rating, int starPosition)
    {
        if (rating >= starPosition)
            return "filled";
        if (rating >= starPosition - 0.5m)
            return "half";
        return "";
    }
}

<!-- Usage -->
<span class="star @GetStarClass(Model.Rating, 1)"></span>
<span class="star @GetStarClass(Model.Rating, 2)"></span>
<span class="star @GetStarClass(Model.Rating, 3)"></span>
<span class="star @GetStarClass(Model.Rating, 4)"></span>
<span class="star @GetStarClass(Model.Rating, 5)"></span>
```

## Complete Example: Reviews/Details.cshtml

```html
@model ReviewDetailsViewModel
@{
    ViewData["Title"] = Model.Review.Title;
}

<!-- Include CSS -->
<link rel="stylesheet" href="~/css/components/star-rating.css" />

<div class="review-details">
    <!-- Header -->
    <div class="review-header">
        <h1>@Model.Review.Title</h1>
        
        <!-- Star Rating Display -->
        <div class="rating-display">
            <div class="star-rating readonly">
                @for (int i = 1; i <= 5; i++)
                {
                    var isFilled = i <= Math.Floor(Model.Review.Rating);
                    var isHalf = !isFilled && i == Math.Ceiling(Model.Review.Rating);
                    <span class="star @(isFilled ? "filled" : "") @(isHalf ? "half" : "")"></span>
                }
            </div>
            <span class="rating-number">@Model.Review.Rating.ToString("0.0")</span>
        </div>
        
        <!-- Reviewer Info -->
        <div class="reviewer-info">
            <img src="@Model.Review.ReviewerAvatar" alt="@Model.Review.ReviewerName" />
            <span>@Model.Review.ReviewerName</span>
            <span class="review-date">@Model.Review.CreatedAt.ToString("MMM dd, yyyy")</span>
            @if (Model.Review.IsVerifiedPurchase)
            {
                <span class="badge badge-success">✓ Verified Purchase</span>
            }
        </div>
    </div>
    
    <!-- Review Content -->
    <div class="review-content">
        <p>@Model.Review.Content</p>
        
        @if (!string.IsNullOrEmpty(Model.Review.Pros))
        {
            <div class="review-pros">
                <h4>👍 Pros</h4>
                <p>@Model.Review.Pros</p>
            </div>
        }
        
        @if (!string.IsNullOrEmpty(Model.Review.Cons))
        {
            <div class="review-cons">
                <h4>👎 Cons</h4>
                <p>@Model.Review.Cons</p>
            </div>
        }
        
        @if (Model.Review.IsRecommended)
        {
            <div class="review-recommendation">
                <strong>✓ I recommend this</strong>
            </div>
        }
    </div>
    
    <!-- Helpful Actions -->
    @if (User.Identity.IsAuthenticated)
    {
        <div class="helpful-actions">
            <span>Was this review helpful?</span>
            <button type="button" 
                    class="helpful-btn"
                    data-helpful-btn
                    data-review-id="@Model.Review.Id"
                    data-helpful="true">
                👍 Yes (<span class="helpful-count">@Model.Review.HelpfulCount</span>)
            </button>
            <button type="button" 
                    class="helpful-btn"
                    data-helpful-btn
                    data-review-id="@Model.Review.Id"
                    data-helpful="false">
                👎 No (<span class="helpful-count">@Model.Review.NotHelpfulCount</span>)
            </button>
        </div>
    }
    
    <!-- Comments Section -->
    <div class="review-comments">
        <h3>Comments (@Model.Comments.TotalCount)</h3>
        
        @foreach (var comment in Model.Comments.Items)
        {
            <div class="comment">
                <div class="comment-author">
                    <strong>@comment.UserName</strong>
                    <span class="comment-date">@comment.CreatedAt.ToString("MMM dd, yyyy")</span>
                </div>
                <p>@comment.Content</p>
            </div>
        }
        
        <!-- Add Comment Form -->
        @if (User.Identity.IsAuthenticated)
        {
            <form data-comment-form data-review-id="@Model.Review.Id">
                <div class="form-group">
                    <textarea class="form-control" 
                              name="content" 
                              rows="3" 
                              placeholder="Add a comment..."
                              required></textarea>
                </div>
                <button type="submit" class="btn btn-primary">Post Comment</button>
            </form>
        }
    </div>
    
    <!-- Sidebar: Rating Distribution -->
    <aside class="review-sidebar">
        <div class="rating-summary">
            <div class="average-rating">
                <span class="rating-number-large">@Model.AverageRating.ToString("0.0")</span>
                <div class="star-rating readonly lg">
                    @for (int i = 1; i <= 5; i++)
                    {
                        var isFilled = i <= Math.Floor(Model.AverageRating);
                        var isHalf = !isFilled && i == Math.Ceiling(Model.AverageRating);
                        <span class="star @(isFilled ? "filled" : "") @(isHalf ? "half" : "")"></span>
                    }
                </div>
                <span class="rating-count">@Model.TotalReviews reviews</span>
            </div>
            
            <div class="rating-distribution">
                @for (int stars = 5; stars >= 1; stars--)
                {
                    var count = Model.RatingDistribution.GetValueOrDefault(stars, 0);
                    var percentage = Model.TotalReviews > 0 ? (count * 100.0 / Model.TotalReviews) : 0;
                    
                    <div class="rating-bar">
                        <span class="rating-bar-label">@stars ★</span>
                        <div class="rating-bar-track">
                            <div class="rating-bar-fill" style="width: @percentage%"></div>
                        </div>
                        <span class="rating-bar-count">@count</span>
                    </div>
                }
            </div>
        </div>
    </aside>
</div>

<!-- Include JavaScript -->
<script src="~/js/pages/reviews.js"></script>
```

## Customization

### Change Star Colors

In `star-rating.css`:

```css
.star.filled::before {
    color: #ff6b6b; /* Your custom color */
}
```

### Change Rating Limits

In `Rating.cs`:

```csharp
public const decimal Min = 1m;  // Start from 1 instead of 0
public const decimal Max = 10m; // Use 1-10 scale
public const decimal Step = 1m; // Only whole numbers
```

### Adjust Rate Limits

In `ReviewsController.cs`:

```csharp
[RateLimit("CreateReview", maxRequests: 5, timeWindowSeconds: 600)] // 5 per 10 minutes
```

## Testing Your Implementation

1. **Create a review** with 3.5 stars
2. **Click helpful** button
3. **Add a comment**
4. **Try to create duplicate** (should fail)
5. **Exceed rate limit** (create 4 reviews quickly)
6. **Check responsive** design on mobile
7. **Test dark mode** (if supported)

## Troubleshooting

### Stars not showing?
- Check if `star-rating.css` is loaded
- Verify star HTML structure matches examples

### AJAX not working?
- Check if `reviews.js` is loaded
- Verify `data-review-form` attribute is present
- Check browser console for errors

### Rate limiting not working?
- Ensure `IMemoryCache` is registered in DI
- Check if user is authenticated (rate limiting requires user ID)

### Validation errors?
- Ensure FluentValidation is registered
- Check if validators are in correct namespace
- Verify model binding is working

## Next Steps

1. Add these UI components to your existing views
2. Test all functionality
3. Customize styling to match your theme
4. Add localization for multi-language support
5. Consider adding image upload for reviews
