// PostHub - SignalR client for real-time post notifications and updates

const PostHubConnection = (function() {
    'use strict';

    let connection = null;
    let isConnected = false;
    let reconnectAttempts = 0;
    const maxReconnectAttempts = 5;

    // Initialize connection
    function init() {
        // Skip initialization on login/register pages
        const currentPath = window.location.pathname.toLowerCase();
        if (currentPath.includes('/login') || 
            currentPath.includes('/register') ||
            currentPath.includes('/forgotpassword') ||
            currentPath.includes('/resetpassword') ||
            currentPath.includes('/account/')) {
            console.log('PostHub skipped on authentication pages');
            return;
        }

        if (connection) {
            console.log('PostHub already initialized');
            return;
        }

        // SignalR hubs don't use culture-prefixed URLs
        const hubUrl = '/postHub';
        
        connection = new signalR.HubConnectionBuilder()
            .withUrl(hubUrl)
            .withAutomaticReconnect({
                nextRetryDelayInMilliseconds: retryContext => {
                    if (retryContext.previousRetryCount < 5) {
                        return Math.min(1000 * Math.pow(2, retryContext.previousRetryCount), 30000);
                    }
                    return null;
                }
            })
            .configureLogging(signalR.LogLevel.Information)
            .build();

        setupEventHandlers();
        startConnection();
    }

    // Setup all event handlers
    function setupEventHandlers() {
        // Connection events
        connection.onclose(onDisconnected);
        connection.onreconnecting(onReconnecting);
        connection.onreconnected(onReconnected);

        // Post creation/update/delete events
        connection.on('PostCreated', handlePostCreated);
        connection.on('PostUpdated', handlePostUpdated);
        connection.on('PostDeleted', handlePostDeleted);

        // Friend notifications
        connection.on('FriendPublishedPost', handleFriendPublishedPost);

        // Engagement events
        connection.on('PostLiked', handlePostLiked);
        connection.on('PostCommented', handlePostCommented);
        connection.on('PostShared', handlePostShared);
        connection.on('PostEngagementUpdated', handlePostEngagementUpdated);

        // Comment events
        connection.on('NewCommentAdded', handleNewCommentAdded);
        connection.on('CommentUpdated', handleCommentUpdated);
        connection.on('CommentDeleted', handleCommentDeleted);
        connection.on('CommentReplyReceived', handleCommentReply);

        // Status events
        connection.on('PostStatusChanged', handlePostStatusChanged);
        connection.on('PostPinStatusChanged', handlePostPinStatusChanged);

        // Milestone events
        connection.on('PostMilestoneReached', handlePostMilestone);

        // System announcements
        connection.on('PostSystemAnnouncement', handleSystemAnnouncement);
    }

    // Start connection
    async function startConnection() {
        try {
            await connection.start();
            isConnected = true;
            reconnectAttempts = 0;
            console.log('PostHub connected successfully');
        } catch (err) {
            isConnected = false;
            console.error('PostHub connection error:', err);
            
            reconnectAttempts++;
            if (reconnectAttempts < maxReconnectAttempts) {
                const delay = Math.min(1000 * Math.pow(2, reconnectAttempts), 30000);
                console.log(`Retrying connection in ${delay}ms...`);
                setTimeout(startConnection, delay);
            }
        }
    }

    // Connection state handlers
    function onDisconnected(error) {
        isConnected = false;
        console.log('PostHub disconnected', error);
    }

    function onReconnecting(error) {
        isConnected = false;
        console.log('PostHub reconnecting...', error);
    }

    function onReconnected(connectionId) {
        isConnected = true;
        reconnectAttempts = 0;
        console.log('PostHub reconnected', connectionId);
    }

    // Event Handlers

    function handlePostCreated(data) {
        console.log('Post created:', data);
        if (window.Toast) {
            window.Toast.show(data.Message || 'Post created successfully!', 'success', 'Post Created');
        }
        
        // Trigger custom event for other components
        document.dispatchEvent(new CustomEvent('postCreated', { detail: data }));
        
        // Optionally refresh the feed
        if (typeof refreshFeed === 'function') {
            refreshFeed();
        }
    }

    function handlePostUpdated(data) {
        console.log('Post updated:', data);
        if (window.Toast) {
            window.Toast.show(data.Message || 'Post updated successfully!', 'success', 'Post Updated');
        }
        
        // Trigger custom event
        document.dispatchEvent(new CustomEvent('postUpdated', { detail: data }));
        
        // Update the post in the DOM if it exists
        updatePostInDOM(data.Post);
    }

    function handlePostDeleted(data) {
        console.log('Post deleted:', data);
        if (window.Toast) {
            window.Toast.show(data.Message || 'Post deleted successfully!', 'success', 'Post Deleted');
        }
        
        // Trigger custom event
        document.dispatchEvent(new CustomEvent('postDeleted', { detail: data }));
        
        // Remove post from DOM
        removePostFromDOM(data.PostId);
    }

    function handleFriendPublishedPost(data) {
        console.log('Friend published post:', data);
        
        // Show toast notification
        if (window.Toast) {
            window.Toast.show('Your friend published a new post!', 'info', 'New Post');
        }
        
        // Trigger custom event
        document.dispatchEvent(new CustomEvent('friendPublishedPost', { detail: data }));
        
        // Show badge on feed icon
        updateFeedBadge();
    }

    function handlePostLiked(data) {
        console.log('Post liked:', data);
        
        if (window.Toast) {
            window.Toast.show(`${data.LikerName} liked your post "${data.PostTitle}"`, 'success', 'New Like');
        }
    }

    function handlePostCommented(data) {
        console.log('Post commented:', data);
        
        if (window.Toast) {
            window.Toast.show(`${data.CommenterName} commented on your post "${data.PostTitle}"`, 'info', 'New Comment');
        }
    }

    function handlePostShared(data) {
        console.log('Post shared:', data);
        
        if (window.Toast) {
            window.Toast.show(`${data.SharerName} shared your post "${data.PostTitle}"`, 'success', 'Post Shared');
        }
    }

    function handlePostEngagementUpdated(data) {
        console.log('Post engagement updated:', data);
        
        // Update engagement stats in DOM
        updateEngagementStats(data.PostId, {
            likes: data.LikeCount,
            comments: data.CommentCount,
            shares: data.ShareCount,
            views: data.ViewCount
        });
    }

    function handleNewCommentAdded(data) {
        console.log('New comment added:', data);
        
        // Add comment to DOM if on post details page
        if (window.location.pathname.includes('/Posts/Details')) {
            addCommentToDOM(data.Comment);
        }
        
        // Trigger custom event
        document.dispatchEvent(new CustomEvent('newCommentAdded', { detail: data }));
    }

    function handleCommentUpdated(data) {
        console.log('Comment updated:', data);
        
        // Update comment in DOM
        updateCommentInDOM(data.CommentId, data.NewContent);
    }

    function handleCommentDeleted(data) {
        console.log('Comment deleted:', data);
        
        // Remove comment from DOM
        removeCommentFromDOM(data.CommentId);
    }

    function handleCommentReply(data) {
        console.log('Comment reply received:', data);
        
        if (window.Toast) {
            window.Toast.show(`${data.ReplierName} replied to your comment on "${data.PostTitle}"`, 'info', 'New Reply');
        }
    }

    function handlePostStatusChanged(data) {
        console.log('Post status changed:', data);
        
        if (window.Toast) {
            window.Toast.show(`Post status changed to ${data.NewStatus}`, 'info');
        }
        
        // Update status in DOM
        updatePostStatus(data.PostId, data.NewStatus);
    }

    function handlePostPinStatusChanged(data) {
        console.log('Post pin status changed:', data);
        
        if (window.Toast) {
            window.Toast.show(data.Message, 'success');
        }
        
        // Update pin status in DOM
        updatePostPinStatus(data.PostId, data.IsPinned);
    }

    function handlePostMilestone(data) {
        console.log('Post milestone reached:', data);
        
        if (window.Toast) {
            window.Toast.show(data.Message, 'success', 'Milestone Reached! 🎉');
        }
    }

    function handleSystemAnnouncement(data) {
        console.log('System announcement:', data);
        
        if (window.Toast) {
            window.Toast.show(data.Message, data.Type || 'info', 'System Announcement');
        }
    }

    // Helper Functions

    function showToast(message, type = 'info', title = '') {
        if (window.Toast) {
            window.Toast.show(message, type, title);
        } else {
            console.log(`Toast [${type}]: ${message}`);
        }
    }

    function updatePostInDOM(post) {
        const postElement = document.querySelector(`[data-post-id="${post.Id}"]`);
        if (postElement) {
            // Update post content
            const titleElement = postElement.querySelector('.post-title');
            if (titleElement) titleElement.textContent = post.Title;
            
            const contentElement = postElement.querySelector('.post-content');
            if (contentElement) contentElement.textContent = post.Content;
        }
    }

    function removePostFromDOM(postId) {
        const postElement = document.querySelector(`[data-post-id="${postId}"]`);
        if (postElement) {
            postElement.style.transition = 'opacity 0.3s ease';
            postElement.style.opacity = '0';
            setTimeout(() => postElement.remove(), 300);
        }
    }

    function updateEngagementStats(postId, stats) {
        const postElement = document.querySelector(`[data-post-id="${postId}"]`);
        if (!postElement) return;

        const likeCount = postElement.querySelector('.like-count');
        if (likeCount) likeCount.textContent = stats.likes;

        const commentCount = postElement.querySelector('.comment-count');
        if (commentCount) commentCount.textContent = stats.comments;

        const shareCount = postElement.querySelector('.share-count');
        if (shareCount) shareCount.textContent = stats.shares;

        const viewCount = postElement.querySelector('.view-count');
        if (viewCount) viewCount.textContent = stats.views;
    }

    function addCommentToDOM(comment) {
        const commentsContainer = document.querySelector('#comments-container');
        if (!commentsContainer) return;

        // Create comment element (simplified)
        const commentHTML = `
            <div class="comment" data-comment-id="${comment.Id}">
                <div class="comment-author">${comment.AuthorName}</div>
                <div class="comment-content">${comment.Content}</div>
                <div class="comment-time">${new Date(comment.CreatedAt).toLocaleString()}</div>
            </div>
        `;
        
        commentsContainer.insertAdjacentHTML('afterbegin', commentHTML);
    }

    function updateCommentInDOM(commentId, newContent) {
        const commentElement = document.querySelector(`[data-comment-id="${commentId}"]`);
        if (commentElement) {
            const contentElement = commentElement.querySelector('.comment-content');
            if (contentElement) contentElement.textContent = newContent;
        }
    }

    function removeCommentFromDOM(commentId) {
        const commentElement = document.querySelector(`[data-comment-id="${commentId}"]`);
        if (commentElement) {
            commentElement.style.transition = 'opacity 0.3s ease';
            commentElement.style.opacity = '0';
            setTimeout(() => commentElement.remove(), 300);
        }
    }

    function updatePostStatus(postId, newStatus) {
        const postElement = document.querySelector(`[data-post-id="${postId}"]`);
        if (postElement) {
            const statusBadge = postElement.querySelector('.post-status');
            if (statusBadge) {
                statusBadge.textContent = newStatus;
                statusBadge.className = `post-status status-${newStatus.toLowerCase()}`;
            }
        }
    }

    function updatePostPinStatus(postId, isPinned) {
        const postElement = document.querySelector(`[data-post-id="${postId}"]`);
        if (postElement) {
            const pinIcon = postElement.querySelector('.pin-icon');
            if (pinIcon) {
                pinIcon.style.display = isPinned ? 'inline-block' : 'none';
            }
        }
    }

    function updateFeedBadge() {
        const feedBadge = document.querySelector('#feed-notification-badge');
        if (feedBadge) {
            const currentCount = parseInt(feedBadge.textContent) || 0;
            feedBadge.textContent = currentCount + 1;
            feedBadge.style.display = 'inline-block';
        }
    }

    // Public API
    return {
        init: init,
        isConnected: () => isConnected,
        getConnection: () => connection
    };
})();

// Auto-initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    PostHubConnection.init();
});
