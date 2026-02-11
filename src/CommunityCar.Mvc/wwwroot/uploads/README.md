# Uploads Directory

This directory stores user-uploaded files for the CommunityCar application.

## Directory Structure

- `posts/` - Post images and videos
  - `images/` - Post images
  - `videos/` - Post videos
- `profiles/` - User profile pictures
- `datasets/` - AI Assistant dataset files (CSV, JSON, Excel)

## Static File Serving

Files in this directory are served by ASP.NET Core's static file middleware configured in `Program.cs`:

```csharp
app.UseStaticFiles();
```

This serves all files from the `wwwroot` directory, including subdirectories.

## File Access

Files can be accessed via URL:
- `http://localhost:5000/uploads/posts/images/example.jpg`
- `http://localhost:5000/uploads/profiles/user-avatar.png`
- `http://localhost:5000/uploads/datasets/data.csv`

## Upload Limits

The application is configured to accept files up to 50 MB:
- Configured in `Program.cs` via Kestrel options
- `MaxRequestBodySize = 52428800` (50 MB)

## Security Notes

- Validate file types before upload
- Sanitize file names to prevent path traversal attacks
- Consider implementing virus scanning for uploaded files
- Set appropriate file permissions on the server

## Git Tracking

`.gitkeep` files are used to track empty directories in git while ignoring uploaded content.
