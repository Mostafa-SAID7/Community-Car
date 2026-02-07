// Feed functionality
(function() {
    'use strict';

    // Like button functionality
    document.addEventListener('DOMContentLoaded', function() {
        initializeLikeButtons();
        initializeInfiniteScroll();
        initializeShareButtons();
    });

    function initializeLikeButtons() {
        const likeButtons = document.querySelectorAll('.like-btn');
        
        likeButtons.forEach(button => {
            button.addEventListener('click', async function(e) {
                e.preventDefault();
                
                const itemId = this.dataset.itemId;
                const icon = this.querySelector('i');
                const countSpan = this.querySelector('.like-count');
                const isLiked = icon.classList.contains('fas');
                
                try {
                    const response = await fetch(`/api/feed/like/${itemId}`, {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json',
                            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
                        },
                        body: JSON.stringify({ isLike: !isLiked })
                    });

                    if (response.ok) {
                        const data = await response.json();
                        
                        // Toggle icon
                        if (isLiked) {
                            icon.classList.remove('fas');
                            icon.classList.add('far');
                        } else {
                            icon.classList.remove('far');
                            icon.classList.add('fas');
                            icon.classList.add('text-danger');
                        }
                        
                        // Update count
                        if (data.likeCount !== undefined) {
                            countSpan.textContent = data.likeCount;
                        } else {
                            const currentCount = parseInt(countSpan.textContent) || 0;
                            countSpan.textContent = isLiked ? currentCount - 1 : currentCount + 1;
                        }
                        
                        // Add animation
                        icon.classList.add('animate-like');
                        setTimeout(() => icon.classList.remove('animate-like'), 300);
                    } else if (response.status === 401) {
                        window.location.href = '/Identity/Account/Login?returnUrl=' + encodeURIComponent(window.location.pathname);
                    }
                } catch (error) {
                    console.error('Error liking item:', error);
                    showToast('Failed to like item. Please try again.', 'error');
                }
            });
        });
    }

    function initializeInfiniteScroll() {
        let isLoading = false;
        let currentPage = 1;
        const feedContainer = document.querySelector('.feed-items');
        
        if (!feedContainer) return;

        window.addEventListener('scroll', async function() {
            if (isLoading) return;
            
            const scrollPosition = window.innerHeight + window.scrollY;
            const threshold = document.documentElement.scrollHeight - 500;
            
            if (scrollPosition >= threshold) {
                isLoading = true;
                currentPage++;
                
                try {
                    const url = new URL(window.location.href);
                    url.searchParams.set('page', currentPage);
                    
                    const response = await fetch(url.toString(), {
                        headers: {
                            'X-Requested-With': 'XMLHttpRequest'
                        }
                    });
                    
                    if (response.ok) {
                        const html = await response.text();
                        const parser = new DOMParser();
                        const doc = parser.parseFromString(html, 'text/html');
                        const newItems = doc.querySelectorAll('.feed-item');
                        
                        if (newItems.length > 0) {
                            newItems.forEach(item => {
                                feedContainer.appendChild(item);
                            });
                            
                            // Reinitialize like buttons for new items
                            initializeLikeButtons();
                        } else {
                            // No more items
                            window.removeEventListener('scroll', arguments.callee);
                        }
                    }
                } catch (error) {
                    console.error('Error loading more items:', error);
                } finally {
                    isLoading = false;
                }
            }
        });
    }

    function initializeShareButtons() {
        document.addEventListener('click', function(e) {
            if (e.target.closest('.share-btn')) {
                e.preventDefault();
                const button = e.target.closest('.share-btn');
                const url = button.dataset.url || window.location.href;
                const title = button.dataset.title || document.title;
                
                if (navigator.share) {
                    navigator.share({
                        title: title,
                        url: url
                    }).catch(err => console.log('Error sharing:', err));
                } else {
                    // Fallback: copy to clipboard
                    copyToClipboard(url);
                    showToast('Link copied to clipboard!', 'success');
                }
            }
        });
    }

    function copyToClipboard(text) {
        const textarea = document.createElement('textarea');
        textarea.value = text;
        textarea.style.position = 'fixed';
        textarea.style.opacity = '0';
        document.body.appendChild(textarea);
        textarea.select();
        document.execCommand('copy');
        document.body.removeChild(textarea);
    }

    function showToast(message, type = 'info') {
        // Check if toastr is available
        if (typeof toastr !== 'undefined') {
            toastr[type](message);
        } else {
            // Fallback to alert
            alert(message);
        }
    }

    // Filter form auto-submit on change
    const filterForm = document.querySelector('form[action*="Index"]');
    if (filterForm) {
        const selects = filterForm.querySelectorAll('select');
        selects.forEach(select => {
            select.addEventListener('change', function() {
                filterForm.submit();
            });
        });
    }

    // Add CSS animation for like button
    const style = document.createElement('style');
    style.textContent = `
        .animate-like {
            animation: likeAnimation 0.3s ease;
        }
        
        @keyframes likeAnimation {
            0%, 100% { transform: scale(1); }
            50% { transform: scale(1.3); }
        }
        
        .feed-item {
            transition: box-shadow 0.3s ease;
        }
        
        .feed-item:hover {
            box-shadow: 0 4px 12px rgba(0,0,0,0.1);
        }
        
        .like-btn {
            transition: color 0.2s ease;
        }
        
        .like-btn:hover {
            color: #dc3545 !important;
        }
    `;
    document.head.appendChild(style);
})();
