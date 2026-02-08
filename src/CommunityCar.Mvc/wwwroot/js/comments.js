// Comments functionality
(function() {
    'use strict';

    document.addEventListener('DOMContentLoaded', function() {
        initializeCommentActions();
        initializeCommentForm();
        initializeSignalR();
    });

    function initializeCommentActions() {
        // Edit comment
        document.addEventListener('click', function(e) {
            if (e.target.closest('.edit-comment-btn')) {
                e.preventDefault();
                const btn = e.target.closest('.edit-comment-btn');
                const commentId = btn.dataset.commentId;
                loadEditForm(commentId);
            }
        });

        // Delete comment
        document.addEventListener('click', function(e) {
            if (e.target.closest('.delete-comment-btn')) {
                e.preventDefault();
                const btn = e.target.closest('.delete-comment-btn');
                const commentId = btn.dataset.commentId;
                deleteComment(commentId);
            }
        });

        // Vote comment
        document.addEventListener('click', function(e) {
            if (e.target.closest('.vote-comment-btn')) {
                e.preventDefault();
                const btn = e.target.closest('.vote-comment-btn');
                const commentId = btn.dataset.commentId;
                const voteType = btn.dataset.voteType;
                voteComment(commentId, voteType === 'up');
            }
        });

        // Report comment
        document.addEventListener('click', function(e) {
            if (e.target.closest('.report-comment-btn')) {
                e.preventDefault();
                const btn = e.target.closest('.report-comment-btn');
                const commentId = btn.dataset.commentId;
                reportComment(commentId);
            }
        });

        // Cancel edit
        document.addEventListener('click', function(e) {
            if (e.target.closest('.cancel-edit-btn')) {
                e.preventDefault();
                const form = e.target.closest('.edit-comment-form');
                const commentItem = form.closest('.comment-item');
                const commentId = commentItem.dataset.commentId;
                reloadComment(commentId);
            }
        });
    }

    function initializeCommentForm() {
        const commentForms = document.querySelectorAll('.comment-form');
        
        commentForms.forEach(form => {
            form.addEventListener('submit', async function(e) {
                e.preventDefault();
                
                const formData = new FormData(form);
                const submitBtn = form.querySelector('button[type="submit"]');
                const originalText = submitBtn.innerHTML;
                
                try {
                    submitBtn.disabled = true;
                    submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin me-1"></i>Posting...';
                    
                    const response = await fetch(form.action, {
                        method: 'POST',
                        body: formData,
                        headers: {
                            'X-Requested-With': 'XMLHttpRequest'
                        }
                    });

                    const data = await response.json();
                    
                    if (data.success) {
                        // Clear form
                        form.reset();
                        
                        // Show success message
                        showToast(data.message, 'success');
                        
                        // Reload comments if needed
                        if (data.html) {
                            const commentsList = form.closest('.answer-comments').querySelector('.comments-list');
                            if (commentsList) {
                                const tempDiv = document.createElement('div');
                                tempDiv.innerHTML = data.html;
                                commentsList.appendChild(tempDiv.firstElementChild);
                            }
                        }
                    } else {
                        showToast(data.message || 'Failed to post comment', 'error');
                    }
                } catch (error) {
                    console.error('Error posting comment:', error);
                    showToast('Failed to post comment. Please try again.', 'error');
                } finally {
                    submitBtn.disabled = false;
                    submitBtn.innerHTML = originalText;
                }
            });
        });
    }

    async function loadEditForm(commentId) {
        try {
            const response = await fetch(`/Comments/Edit/${commentId}`, {
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });

            if (response.ok) {
                const html = await response.text();
                const commentItem = document.querySelector(`[data-comment-id="${commentId}"]`);
                if (commentItem) {
                    const contentDiv = commentItem.querySelector('.comment-content');
                    contentDiv.innerHTML = html;
                    
                    // Initialize form submission
                    const form = contentDiv.querySelector('.edit-comment-form');
                    if (form) {
                        form.addEventListener('submit', async function(e) {
                            e.preventDefault();
                            await submitEditForm(form, commentId);
                        });
                    }
                }
            }
        } catch (error) {
            console.error('Error loading edit form:', error);
            showToast('Failed to load edit form', 'error');
        }
    }

    async function submitEditForm(form, commentId) {
        const formData = new FormData(form);
        const submitBtn = form.querySelector('button[type="submit"]');
        const originalText = submitBtn.innerHTML;
        
        try {
            submitBtn.disabled = true;
            submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin me-1"></i>Saving...';
            
            const response = await fetch(form.action, {
                method: 'POST',
                body: formData,
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });

            const data = await response.json();
            
            if (data.success) {
                showToast(data.message, 'success');
                await reloadComment(commentId);
            } else {
                showToast(data.message || 'Failed to update comment', 'error');
            }
        } catch (error) {
            console.error('Error updating comment:', error);
            showToast('Failed to update comment', 'error');
        } finally {
            submitBtn.disabled = false;
            submitBtn.innerHTML = originalText;
        }
    }

    async function deleteComment(commentId) {
        if (!confirm('Are you sure you want to delete this comment?')) {
            return;
        }

        try {
            const response = await fetch(`/Comments/Delete/${commentId}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
                }
            });

            const data = await response.json();
            
            if (data.success) {
                const commentItem = document.querySelector(`[data-comment-id="${commentId}"]`);
                if (commentItem) {
                    commentItem.style.opacity = '0';
                    setTimeout(() => commentItem.remove(), 300);
                }
                showToast(data.message, 'success');
            } else {
                showToast(data.message || 'Failed to delete comment', 'error');
            }
        } catch (error) {
            console.error('Error deleting comment:', error);
            showToast('Failed to delete comment', 'error');
        }
    }

    async function voteComment(commentId, isUpvote) {
        try {
            const url = CultureHelper.addCultureToUrl(`/Comments/Vote/${commentId}?isUpvote=${isUpvote}`);
            const response = await fetch(url, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
                }
            });

            const data = await response.json();
            
            if (data.success) {
                const commentItem = document.querySelector(`[data-comment-id="${commentId}"]`);
                if (commentItem) {
                    const voteCount = commentItem.querySelector('.vote-count');
                    if (voteCount) {
                        voteCount.textContent = data.voteCount;
                    }
                }
            } else if (response.status === 401) {
                window.location.href = CultureHelper.addCultureToUrl('/Identity/Account/Login?returnUrl=' + encodeURIComponent(window.location.pathname));
            } else {
                showToast(data.message || 'Failed to vote', 'error');
            }
        } catch (error) {
            console.error('Error voting comment:', error);
            showToast('Failed to vote', 'error');
        }
    }

    async function reportComment(commentId) {
        const reason = prompt('Please provide a reason for reporting this comment:');
        if (!reason) return;

        try {
            const response = await fetch(`/Comments/Report/${commentId}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
                },
                body: JSON.stringify({ reason })
            });

            const data = await response.json();
            showToast(data.message, data.success ? 'success' : 'error');
        } catch (error) {
            console.error('Error reporting comment:', error);
            showToast('Failed to report comment', 'error');
        }
    }

    async function reloadComment(commentId) {
        try {
            const response = await fetch(`/Comments/GetCommentCard/${commentId}`);
            if (response.ok) {
                const html = await response.text();
                const commentItem = document.querySelector(`[data-comment-id="${commentId}"]`);
                if (commentItem) {
                    const tempDiv = document.createElement('div');
                    tempDiv.innerHTML = html;
                    commentItem.replaceWith(tempDiv.firstElementChild);
                }
            }
        } catch (error) {
            console.error('Error reloading comment:', error);
        }
    }

    function initializeSignalR() {
        if (typeof signalR === 'undefined') return;

        const connection = new signalR.HubConnectionBuilder()
            .withUrl("/hubs/question")
            .build();

        connection.on("ReceiveComment", function (data) {
            const commentsList = document.querySelector(`[data-answer-id="${data.answerId}"] .comments-list`);
            if (commentsList && data.comment) {
                // Add new comment to the list
                fetch(`/Comments/GetCommentCard/${data.comment.Id}`)
                    .then(response => response.text())
                    .then(html => {
                        const tempDiv = document.createElement('div');
                        tempDiv.innerHTML = html;
                        commentsList.appendChild(tempDiv.firstElementChild);
                    });
            }
        });

        connection.on("CommentUpdated", function (data) {
            reloadComment(data.commentId);
        });

        connection.on("CommentDeleted", function (data) {
            const commentItem = document.querySelector(`[data-comment-id="${data.commentId}"]`);
            if (commentItem) {
                commentItem.style.opacity = '0';
                setTimeout(() => commentItem.remove(), 300);
            }
        });

        connection.start().catch(err => console.error('SignalR connection error:', err));
    }

    function showToast(message, type = 'info') {
        if (typeof toastr !== 'undefined') {
            toastr[type](message);
        } else {
            alert(message);
        }
    }

    // Add CSS for smooth transitions
    const style = document.createElement('style');
    style.textContent = `
        .comment-item {
            transition: opacity 0.3s ease;
        }
        
        .comment-content {
            transition: all 0.3s ease;
        }
        
        .vote-comment-btn:hover {
            color: #007bff !important;
        }
        
        .vote-comment-btn.voted {
            color: #007bff !important;
        }
    `;
    document.head.appendChild(style);
})();
