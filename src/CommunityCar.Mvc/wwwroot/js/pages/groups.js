/**
 * Groups Page JavaScript
 * Handles group interactions, tab navigation, and content loading
 */

(function() {
    'use strict';

    // Initialize when DOM is ready
    document.addEventListener('DOMContentLoaded', function() {
        initializeGroupTabs();
        initializeGroupActions();
        initializeGroupSearch();
    });

    /**
     * Initialize tab functionality for group details page
     */
    function initializeGroupTabs() {
        const tabButtons = document.querySelectorAll('#groupTabs button[data-bs-toggle="tab"]');
        
        tabButtons.forEach(button => {
            button.addEventListener('shown.bs.tab', function(event) {
                const targetId = event.target.getAttribute('data-bs-target');
                const tabName = targetId.replace('#', '');
                
                // Update URL without page reload
                if (history.pushState) {
                    const url = new URL(window.location);
                    url.searchParams.set('tab', tabName);
                    history.pushState({}, '', url);
                }
                
                // Track tab view
                console.log('Viewing tab:', tabName);
            });
        });
    }

    /**
     * Initialize group action buttons (Join, Leave, etc.)
     */
    function initializeGroupActions() {
        // Join/Leave confirmation
        const leaveButtons = document.querySelectorAll('button[onclick*="leave this group"]');
        leaveButtons.forEach(button => {
            button.addEventListener('click', function(e) {
                if (!confirm('Are you sure you want to leave this group? You will need to request to join again.')) {
                    e.preventDefault();
                    return false;
                }
            });
        });

        // Delete confirmation
        const deleteButtons = document.querySelectorAll('button[onclick*="delete this group"]');
        deleteButtons.forEach(button => {
            button.addEventListener('click', function(e) {
                if (!confirm('Are you sure you want to delete this group? This action cannot be undone.')) {
                    e.preventDefault();
                    return false;
                }
            });
        });

        // Remove member confirmation
        const removeMemberButtons = document.querySelectorAll('button[onclick*="Remove this member"]');
        removeMemberButtons.forEach(button => {
            button.addEventListener('click', function(e) {
                if (!confirm('Are you sure you want to remove this member from the group?')) {
                    e.preventDefault();
                    return false;
                }
            });
        });
    }

    /**
     * Initialize group search functionality
     */
    function initializeGroupSearch() {
        const searchInput = document.querySelector('input[name="query"]');
        if (!searchInput) return;

        let searchTimeout;
        searchInput.addEventListener('input', function() {
            clearTimeout(searchTimeout);
            const query = this.value.trim();

            if (query.length === 0) {
                // Clear search and show all groups
                window.location.href = window.location.pathname;
                return;
            }

            if (query.length < 2) return;

            searchTimeout = setTimeout(() => {
                performGroupSearch(query);
            }, 500);
        });
    }

    /**
     * Perform group search
     */
    function performGroupSearch(query) {
        const culture = document.documentElement.lang || 'en';
        const url = `/${culture}/Groups/Search?query=${encodeURIComponent(query)}`;
        window.location.href = url;
    }

    /**
     * Load group posts via AJAX
     */
    window.loadGroupPosts = function(groupId, page = 1) {
        const container = document.getElementById('groupPostsContainer');
        if (!container) return;

        const culture = document.documentElement.lang || 'en';
        
        // Show loading spinner
        container.innerHTML = `
            <div class="text-center py-5">
                <div class="spinner-border text-primary" role="status">
                    <span class="visually-hidden">Loading...</span>
                </div>
                <p class="mt-3 text-muted">Loading posts...</p>
            </div>
        `;

        fetch(`/${culture}/Posts?groupId=${groupId}&page=${page}&pageSize=10`)
            .then(response => {
                if (!response.ok) throw new Error('Failed to load posts');
                return response.text();
            })
            .then(html => {
                // For now, show placeholder message
                const isMember = container.dataset.isMember === 'true';
                container.innerHTML = `
                    <div class="alert alert-info">
                        <i class="fas fa-info-circle me-2"></i>
                        Posts for this group will be displayed here. 
                        ${isMember ? `<a href="/${culture}/Posts/Create?groupId=${groupId}" class="alert-link">Create the first post!</a>` : ''}
                    </div>
                `;
            })
            .catch(error => {
                console.error('Error loading posts:', error);
                container.innerHTML = `
                    <div class="alert alert-warning">
                        <i class="fas fa-exclamation-triangle me-2"></i>
                        Unable to load posts. Please try again later.
                    </div>
                `;
            });
    };

    /**
     * Load group questions via AJAX
     */
    window.loadGroupQuestions = function(groupId, page = 1) {
        const container = document.getElementById('groupQuestionsContainer');
        if (!container) return;

        const culture = document.documentElement.lang || 'en';
        
        // Show loading spinner
        container.innerHTML = `
            <div class="text-center py-5">
                <div class="spinner-border text-primary" role="status">
                    <span class="visually-hidden">Loading...</span>
                </div>
                <p class="mt-3 text-muted">Loading questions...</p>
            </div>
        `;

        fetch(`/${culture}/Questions?groupId=${groupId}&page=${page}&pageSize=10`)
            .then(response => {
                if (!response.ok) throw new Error('Failed to load questions');
                return response.text();
            })
            .then(html => {
                // For now, show placeholder message
                const isMember = container.dataset.isMember === 'true';
                container.innerHTML = `
                    <div class="alert alert-info">
                        <i class="fas fa-info-circle me-2"></i>
                        Questions for this group will be displayed here. 
                        ${isMember ? `<a href="/${culture}/Questions/Create?groupId=${groupId}" class="alert-link">Ask the first question!</a>` : ''}
                    </div>
                `;
            })
            .catch(error => {
                console.error('Error loading questions:', error);
                container.innerHTML = `
                    <div class="alert alert-warning">
                        <i class="fas fa-exclamation-triangle me-2"></i>
                        Unable to load questions. Please try again later.
                    </div>
                `;
            });
    };

    /**
     * Load group reviews via AJAX
     */
    window.loadGroupReviews = function(groupId, page = 1) {
        const container = document.getElementById('groupReviewsContainer');
        if (!container) return;

        const culture = document.documentElement.lang || 'en';
        
        // Show loading spinner
        container.innerHTML = `
            <div class="text-center py-5">
                <div class="spinner-border text-primary" role="status">
                    <span class="visually-hidden">Loading...</span>
                </div>
                <p class="mt-3 text-muted">Loading reviews...</p>
            </div>
        `;

        fetch(`/${culture}/Reviews?groupId=${groupId}&page=${page}&pageSize=10`)
            .then(response => {
                if (!response.ok) throw new Error('Failed to load reviews');
                return response.text();
            })
            .then(html => {
                // For now, show placeholder message
                const isMember = container.dataset.isMember === 'true';
                container.innerHTML = `
                    <div class="alert alert-info">
                        <i class="fas fa-info-circle me-2"></i>
                        Reviews for this group will be displayed here. 
                        ${isMember ? `<a href="/${culture}/Reviews/Create?groupId=${groupId}" class="alert-link">Write the first review!</a>` : ''}
                    </div>
                `;
            })
            .catch(error => {
                console.error('Error loading reviews:', error);
                container.innerHTML = `
                    <div class="alert alert-warning">
                        <i class="fas fa-exclamation-triangle me-2"></i>
                        Unable to load reviews. Please try again later.
                    </div>
                `;
            });
    };

    /**
     * Show toast notification
     */
    function showToast(message, type = 'success') {
        if (typeof toastr !== 'undefined') {
            toastr[type](message);
        } else {
            alert(message);
        }
    }

})();
