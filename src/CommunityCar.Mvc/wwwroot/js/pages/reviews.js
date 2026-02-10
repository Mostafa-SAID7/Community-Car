/**
 * Reviews Page - AJAX functionality for reviews
 * Handles: Create, Edit, Delete, Rate, Comment, Flag
 */

const ReviewsModule = (() => {
    'use strict';

    // Configuration
    const config = {
        ratingStep: 0.5,
        minRating: 0,
        maxRating: 5,
        debounceDelay: 300
    };

    // Initialize star rating component
    function initStarRating() {
        const ratingContainers = document.querySelectorAll('[data-rating-input]');
        
        ratingContainers.forEach(container => {
            const input = container.querySelector('input[type="hidden"], input[type="number"]');
            const stars = container.querySelectorAll('.star');
            
            if (!input || !stars.length) return;

            // Set initial state
            updateStarDisplay(stars, parseFloat(input.value) || 0);

            // Handle star clicks
            stars.forEach((star, index) => {
                star.addEventListener('click', (e) => {
                    const rect = star.getBoundingClientRect();
                    const clickX = e.clientX - rect.left;
                    const isHalf = clickX < rect.width / 2;
                    
                    const rating = index + (isHalf ? 0.5 : 1);
                    input.value = rating;
                    updateStarDisplay(stars, rating);
                });

                // Hover preview
                star.addEventListener('mouseenter', (e) => {
                    const rect = star.getBoundingClientRect();
                    star.addEventListener('mousemove', function moveHandler(e) {
                        const clickX = e.clientX - rect.left;
                        const isHalf = clickX < rect.width / 2;
                        const rating = index + (isHalf ? 0.5 : 1);
                        updateStarDisplay(stars, rating, true);
                    });
                });

                star.addEventListener('mouseleave', () => {
                    updateStarDisplay(stars, parseFloat(input.value) || 0);
                });
            });
        });
    }

    // Update star display (filled, half, empty)
    function updateStarDisplay(stars, rating, isPreview = false) {
        stars.forEach((star, index) => {
            star.classList.remove('filled', 'half', 'preview');
            
            if (rating >= index + 1) {
                star.classList.add('filled');
            } else if (rating >= index + 0.5) {
                star.classList.add('half');
            }

            if (isPreview) {
                star.classList.add('preview');
            }
        });
    }

    // Submit review via AJAX
    function submitReview(form) {
        const formData = new FormData(form);
        const url = form.action;

        fetch(url, {
            method: 'POST',
            body: formData,
            headers: {
                'X-Requested-With': 'XMLHttpRequest',
                'RequestVerificationToken': formData.get('__RequestVerificationToken')
            }
        })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                showNotification(data.message, 'success');
                
                // Redirect to review details if slug provided
                if (data.slug) {
                    setTimeout(() => {
                        window.location.href = `/Reviews/Details/${data.slug}`;
                    }, 1500);
                } else {
                    form.reset();
                }
            } else {
                if (data.errors && Array.isArray(data.errors)) {
                    data.errors.forEach(error => showNotification(error, 'error'));
                } else {
                    showNotification(data.message || 'Failed to submit review', 'error');
                }
            }
        })
        .catch(error => {
            console.error('Error submitting review:', error);
            showNotification('An error occurred. Please try again.', 'error');
        });
    }

    // Mark review as helpful/not helpful
    function markHelpful(reviewId, isHelpful) {
        const url = `/Reviews/MarkHelpful/${reviewId}`;
        
        fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'X-Requested-With': 'XMLHttpRequest'
            },
            body: JSON.stringify({ isHelpful })
        })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                showNotification(data.message, 'success');
                updateHelpfulCount(reviewId, isHelpful);
            } else {
                showNotification(data.message, 'error');
            }
        })
        .catch(error => {
            console.error('Error marking review:', error);
            showNotification('Failed to submit feedback', 'error');
        });
    }

    // Update helpful count in UI
    function updateHelpfulCount(reviewId, isHelpful) {
        const countElement = document.querySelector(`[data-review-id="${reviewId}"] .helpful-count`);
        if (countElement) {
            const currentCount = parseInt(countElement.textContent) || 0;
            countElement.textContent = currentCount + (isHelpful ? 1 : -1);
        }
    }

    // Add comment to review
    function addComment(reviewId, content) {
        const url = '/Reviews/AddComment';
        
        fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'X-Requested-With': 'XMLHttpRequest'
            },
            body: JSON.stringify({ reviewId, content })
        })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                showNotification(data.message, 'success');
                // Reload comments section
                location.reload();
            } else {
                showNotification(data.message, 'error');
            }
        })
        .catch(error => {
            console.error('Error adding comment:', error);
            showNotification('Failed to add comment', 'error');
        });
    }

    // Flag review
    function flagReview(reviewId, reason) {
        const url = `/Reviews/Flag/${reviewId}`;
        
        fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'X-Requested-With': 'XMLHttpRequest'
            },
            body: JSON.stringify({ reason })
        })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                showNotification(data.message, 'success');
            } else {
                showNotification(data.message, 'error');
            }
        })
        .catch(error => {
            console.error('Error flagging review:', error);
            showNotification('Failed to flag review', 'error');
        });
    }

    // Delete review
    function deleteReview(reviewId) {
        if (!confirm('Are you sure you want to delete this review?')) {
            return;
        }

        const url = `/Reviews/Delete/${reviewId}`;
        
        fetch(url, {
            method: 'POST',
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            }
        })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                showNotification(data.message, 'success');
                setTimeout(() => {
                    window.location.href = '/Reviews';
                }, 1500);
            } else {
                showNotification(data.message, 'error');
            }
        })
        .catch(error => {
            console.error('Error deleting review:', error);
            showNotification('Failed to delete review', 'error');
        });
    }

    // Show notification
    function showNotification(message, type = 'info') {
        // Use existing notification system if available
        if (window.NotificationModule && typeof window.NotificationModule.show === 'function') {
            window.NotificationModule.show(message, type);
        } else {
            // Fallback to alert
            alert(message);
        }
    }

    // Initialize event listeners
    function initEventListeners() {
        // Review form submission
        const reviewForms = document.querySelectorAll('[data-review-form]');
        reviewForms.forEach(form => {
            form.addEventListener('submit', (e) => {
                e.preventDefault();
                submitReview(form);
            });
        });

        // Helpful buttons
        document.addEventListener('click', (e) => {
            if (e.target.matches('[data-helpful-btn]')) {
                const reviewId = e.target.dataset.reviewId;
                const isHelpful = e.target.dataset.helpful === 'true';
                markHelpful(reviewId, isHelpful);
            }

            // Flag button
            if (e.target.matches('[data-flag-btn]')) {
                const reviewId = e.target.dataset.reviewId;
                const reason = prompt('Please provide a reason for flagging this review:');
                if (reason) {
                    flagReview(reviewId, reason);
                }
            }

            // Delete button
            if (e.target.matches('[data-delete-review-btn]')) {
                const reviewId = e.target.dataset.reviewId;
                deleteReview(reviewId);
            }
        });

        // Comment form
        const commentForms = document.querySelectorAll('[data-comment-form]');
        commentForms.forEach(form => {
            form.addEventListener('submit', (e) => {
                e.preventDefault();
                const reviewId = form.dataset.reviewId;
                const content = form.querySelector('textarea').value;
                if (content.trim()) {
                    addComment(reviewId, content);
                }
            });
        });
    }

    // Initialize module
    function init() {
        initStarRating();
        initEventListeners();
    }

    // Public API
    return {
        init,
        markHelpful,
        addComment,
        flagReview,
        deleteReview
    };
})();

// Auto-initialize on DOM ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', ReviewsModule.init);
} else {
    ReviewsModule.init();
}
