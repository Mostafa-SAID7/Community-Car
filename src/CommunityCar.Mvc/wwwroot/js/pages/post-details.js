/**
 * Post Details Page - Interactive Features
 * Handles pin, like tooltip with viewers, share with counts, and reactions
 */

(function() {
    'use strict';

    // Initialize on page load
    document.addEventListener('DOMContentLoaded', function() {
        initializeLikeTooltip();
        initializeReactions();
        initializeCommentForm();
        initializeShareModal();
    });

    /**
     * Initialize Like Button with Tooltip showing who liked
     */
    function initializeLikeTooltip() {
        const likeButtons = document.querySelectorAll('[id^="likeBtn-"]');
        
        likeButtons.forEach(btn => {
            const postId = btn.id.replace('likeBtn-', '');
            
            // Initialize Bootstrap tooltip
            let tooltipInstance = null;
            
            btn.addEventListener('mouseenter', async function() {
                try {
                    const culture = CultureHelper.getCurrentCulture();
                    const response = await fetch(`/${culture}/Posts/GetLikers/${postId}`);
                    const data = await response.json();
                    
                    if (data.success && data.likers && data.likers.length > 0) {
                        const tooltipContent = generateLikersTooltip(data.likers, data.total);
                        
                        // Destroy existing tooltip if any
                        if (tooltipInstance) {
                            tooltipInstance.dispose();
                        }
                        
                        // Create new tooltip with dynamic content
                        tooltipInstance = new bootstrap.Tooltip(btn, {
                            title: tooltipContent,
                            html: true,
                            placement: 'top',
                            customClass: 'likes-tooltip',
                            trigger: 'manual'
                        });
                        
                        tooltipInstance.show();
                    }
                } catch (error) {
                    console.error('Error loading likers:', error);
                }
            });
            
            btn.addEventListener('mouseleave', function() {
                if (tooltipInstance) {
                    tooltipInstance.hide();
                }
            });
        });
    }

    /**
     * Generate HTML for likers tooltip
     */
    function generateLikersTooltip(likers, total) {
        const maxDisplay = 5;
        const displayLikers = likers.slice(0, maxDisplay);
        
        let html = '<div class="likers-tooltip-content">';
        
        displayLikers.forEach(liker => {
            html += `
                <div class="liker-item d-flex align-items-center mb-2">
                    <img src="${liker.userAvatar}" alt="${liker.userName}" class="rounded-circle me-2" style="width: 32px; height: 32px; object-fit: cover;">
                    <span class="fw-bold">${liker.userName}</span>
                </div>
            `;
        });
        
        if (total > maxDisplay) {
            html += `<div class="text-muted small mt-2">and ${total - maxDisplay} more...</div>`;
        }
        
        html += '</div>';
        return html;
    }

    /**
     * Toggle Pin Post
     */
    window.togglePin = async function(postId) {
        try {
            const culture = CultureHelper.getCurrentCulture();
            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
            
            const response = await fetch(`/${culture}/Posts/TogglePin/${postId}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': token
                }
            });
            
            const data = await response.json();
            
            if (data.success) {
                const btn = document.querySelector(`button[onclick="togglePin('${postId}')"]`);
                if (data.isPinned) {
                    btn.classList.remove('btn-outline-warning');
                    btn.classList.add('btn-warning');
                    btn.innerHTML = '<i class="fas fa-thumbtack me-2"></i>Unpin Post';
                } else {
                    btn.classList.remove('btn-warning');
                    btn.classList.add('btn-outline-warning');
                    btn.innerHTML = '<i class="fas fa-thumbtack me-2"></i>Pin Post';
                }
                
                showToast(data.message, 'success');
            } else {
                showToast(data.message || 'Failed to toggle pin', 'error');
            }
        } catch (error) {
            console.error('Error toggling pin:', error);
            showToast('An error occurred', 'error');
        }
    };

    /**
     * Toggle Like
     */
    window.toggleLike = async function(postId) {
        try {
            const culture = CultureHelper.getCurrentCulture();
            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
            
            const response = await fetch(`/${culture}/Posts/ToggleLike/${postId}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': token
                }
            });
            
            const data = await response.json();
            
            if (data.success) {
                // Update all like buttons and counts
                const likeButtons = document.querySelectorAll(`button[onclick="toggleLike('${postId}')"]`);
                const likeCounts = document.querySelectorAll('.like-count');
                
                likeButtons.forEach(btn => {
                    if (data.isLiked) {
                        btn.classList.remove('btn-outline-danger');
                        btn.classList.add('btn-danger');
                        const icon = btn.querySelector('i');
                        if (icon) icon.classList.add('fas');
                    } else {
                        btn.classList.remove('btn-danger');
                        btn.classList.add('btn-outline-danger');
                        const icon = btn.querySelector('i');
                        if (icon) icon.classList.remove('fas');
                    }
                });
                
                likeCounts.forEach(count => {
                    count.textContent = data.totalLikes;
                });
            } else {
                showToast(data.message || 'Failed to toggle like', 'error');
            }
        } catch (error) {
            console.error('Error toggling like:', error);
            showToast('An error occurred', 'error');
        }
    };

    /**
     * Initialize Reactions
     */
    function initializeReactions() {
        const reactionButtons = document.querySelectorAll('.reaction-btn');
        
        reactionButtons.forEach(btn => {
            btn.addEventListener('click', function() {
                // Add animation
                this.classList.add('reaction-animate');
                setTimeout(() => {
                    this.classList.remove('reaction-animate');
                }, 300);
            });
        });
    }

    /**
     * Add Reaction
     */
    window.addReaction = async function(postId, reactionType) {
        try {
            const culture = CultureHelper.getCurrentCulture();
            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
            
            const response = await fetch(`/${culture}/Posts/AddReaction/${postId}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': token
                },
                body: JSON.stringify({ reactionType })
            });
            
            const data = await response.json();
            
            if (data.success) {
                // Update like count
                const likeCounts = document.querySelectorAll('.like-count');
                likeCounts.forEach(count => {
                    count.textContent = data.totalLikes;
                });
                
                showToast(`Reacted with ${reactionType}!`, 'success');
            } else {
                showToast(data.message || 'Failed to add reaction', 'error');
            }
        } catch (error) {
            console.error('Error adding reaction:', error);
            showToast('An error occurred', 'error');
        }
    };

    /**
     * Initialize Share Modal
     */
    function initializeShareModal() {
        const shareModal = document.getElementById('shareModal');
        if (shareModal) {
            shareModal.addEventListener('show.bs.modal', function() {
                // Track modal open
                console.log('Share modal opened');
            });
        }
    }

    /**
     * Share to Social Media
     */
    window.shareToSocial = async function(platform, postId, postTitle) {
        const culture = CultureHelper.getCurrentCulture();
        const postUrl = encodeURIComponent(window.location.href);
        const title = encodeURIComponent(postTitle);
        
        let shareUrl = '';
        
        switch(platform) {
            case 'facebook':
                shareUrl = `https://www.facebook.com/sharer/sharer.php?u=${postUrl}`;
                break;
            case 'twitter':
                shareUrl = `https://twitter.com/intent/tweet?url=${postUrl}&text=${title}`;
                break;
            case 'whatsapp':
                shareUrl = `https://wa.me/?text=${title}%20${postUrl}`;
                break;
            case 'linkedin':
                shareUrl = `https://www.linkedin.com/sharing/share-offsite/?url=${postUrl}`;
                break;
            default:
                return;
        }
        
        // Open share window
        window.open(shareUrl, '_blank', 'width=600,height=400');
        
        // Increment share count
        try {
            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
            const response = await fetch(`/${culture}/Posts/Share/${postId}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': token
                }
            });
            
            const data = await response.json();
            if (data.success) {
                // Update share count
                const shareCounts = document.querySelectorAll('.share-count');
                shareCounts.forEach(count => {
                    const currentCount = parseInt(count.textContent) || 0;
                    count.textContent = currentCount + 1;
                });
            }
        } catch (error) {
            console.error('Error incrementing share count:', error);
        }
        
        // Close modal
        const modal = bootstrap.Modal.getInstance(document.getElementById('shareModal'));
        if (modal) {
            modal.hide();
        }
    };

    /**
     * Copy Link
     */
    window.copyLink = async function(postId) {
        try {
            const url = window.location.href;
            await navigator.clipboard.writeText(url);
            
            showToast('Link copied to clipboard!', 'success');
            
            // Increment share count
            const culture = CultureHelper.getCurrentCulture();
            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
            await fetch(`/${culture}/Posts/Share/${postId}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': token
                }
            });
            
            // Update share count
            const shareCounts = document.querySelectorAll('.share-count');
            shareCounts.forEach(count => {
                const currentCount = parseInt(count.textContent) || 0;
                count.textContent = currentCount + 1;
            });
            
            // Close modal
            const modal = bootstrap.Modal.getInstance(document.getElementById('shareModal'));
            if (modal) {
                modal.hide();
            }
        } catch (error) {
            console.error('Error copying link:', error);
            showToast('Failed to copy link', 'error');
        }
    };

    /**
     * Initialize Comment Form
     */
    function initializeCommentForm() {
        const commentForm = document.getElementById('commentForm');
        if (!commentForm) return;
        
        commentForm.addEventListener('submit', async function(e) {
            e.preventDefault();
            
            const formData = new FormData(commentForm);
            const postId = formData.get('postId');
            const content = formData.get('content');
            
            if (!content || content.trim() === '') {
                showToast('Please enter a comment', 'error');
                return;
            }
            
            try {
                const culture = CultureHelper.getCurrentCulture();
                const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
                
                const response = await fetch(`/${culture}/Posts/AddComment`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/x-www-form-urlencoded',
                        'RequestVerificationToken': token
                    },
                    body: new URLSearchParams({
                        postId: postId,
                        content: content
                    })
                });
                
                const data = await response.json();
                
                if (data.success) {
                    showToast(data.message || 'Comment added successfully', 'success');
                    commentForm.reset();
                    
                    // Reload page to show new comment
                    setTimeout(() => {
                        window.location.reload();
                    }, 1000);
                } else {
                    showToast(data.message || 'Failed to add comment', 'error');
                }
            } catch (error) {
                console.error('Error adding comment:', error);
                showToast('An error occurred', 'error');
            }
        });
    }

    /**
     * Show Toast Notification
     */
    function showToast(message, type = 'info') {
        // Use SweetAlert2 Toast if available
        if (typeof Swal !== 'undefined') {
            const Toast = Swal.mixin({
                toast: true,
                position: 'top-end',
                showConfirmButton: false,
                timer: 3000,
                timerProgressBar: true,
                didOpen: (toast) => {
                    toast.addEventListener('mouseenter', Swal.stopTimer);
                    toast.addEventListener('mouseleave', Swal.resumeTimer);
                }
            });

            const iconMap = {
                'success': 'success',
                'error': 'error',
                'danger': 'error',
                'info': 'info',
                'warning': 'warning'
            };

            Toast.fire({
                icon: iconMap[type] || 'info',
                title: message
            });
        } else if (typeof bootstrap !== 'undefined' && bootstrap.Toast) {
            // Fallback to Bootstrap Toast
            const toastContainer = document.querySelector('.toast-container') || createToastContainer();
            const toastId = 'toast-' + Date.now();
            const bgClass = type === 'success' ? 'bg-success' : type === 'error' || type === 'danger' ? 'bg-danger' : 'bg-info';
            
            const toastHtml = `
                <div id="${toastId}" class="toast align-items-center text-white ${bgClass} border-0" role="alert" aria-live="assertive" aria-atomic="true">
                    <div class="d-flex">
                        <div class="toast-body">${message}</div>
                        <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
                    </div>
                </div>
            `;
            
            toastContainer.insertAdjacentHTML('beforeend', toastHtml);
            const toastElement = document.getElementById(toastId);
            const toast = new bootstrap.Toast(toastElement);
            toast.show();
            
            toastElement.addEventListener('hidden.bs.toast', () => {
                toastElement.remove();
            });
        } else {
            // Final fallback to alert
            alert(message);
        }
    }

    /**
     * Create Toast Container if it doesn't exist
     */
    function createToastContainer() {
        const container = document.createElement('div');
        container.className = 'toast-container position-fixed top-0 end-0 p-3';
        container.style.zIndex = '9999';
        document.body.appendChild(container);
        return container;
    }

    /**
     * Culture Helper (if not already defined)
     */
    if (typeof window.CultureHelper === 'undefined') {
        window.CultureHelper = {
            getCurrentCulture: function() {
                const pathParts = window.location.pathname.split('/');
                return pathParts[1] || 'en';
            },
            addCultureToUrl: function(url) {
                const culture = this.getCurrentCulture();
                return `/${culture}${url}`;
            }
        };
    }

})();
