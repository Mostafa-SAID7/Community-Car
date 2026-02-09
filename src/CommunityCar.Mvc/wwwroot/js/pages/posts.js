// Posts Page - Centralized JavaScript

const PostsModule = (function() {
    'use strict';

    // Configuration
    const config = {
        animationDelay: 100,
        animationDuration: 500,
        previewMaxHeight: 300,
        searchDebounceDelay: 500
    };

    // State
    let searchTimeout = null;
    let currentSearchTerm = '';
    let currentFilter = '';

    // Card Animation on Scroll
    function initCardAnimations() {
        const observerOptions = {
            threshold: 0.1,
            rootMargin: '0px 0px -50px 0px'
        };

        const observer = new IntersectionObserver(function(entries) {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.style.opacity = '0';
                    entry.target.style.transform = 'translateY(20px)';
                    setTimeout(() => {
                        entry.target.style.transition = `all ${config.animationDuration}ms ease`;
                        entry.target.style.opacity = '1';
                        entry.target.style.transform = 'translateY(0)';
                    }, config.animationDelay);
                    observer.unobserve(entry.target);
                }
            });
        }, observerOptions);

        document.querySelectorAll('.post-card').forEach(card => {
            observer.observe(card);
        });
    }

    // Post Type Selector
    function initPostTypeSelector() {
        const postTypeCards = $('.post-type-card');
        const postTypeInput = $('#postTypeInput');
        const imageFields = $('#imageFields');
        const videoFields = $('#videoFields');
        const linkFields = $('#linkFields');

        if (postTypeCards.length === 0) return;

        // Initialize with default or current type
        const currentType = postTypeInput.val() || 'Text';
        updatePostType(currentType);

        postTypeCards.on('click', function() {
            const type = $(this).data('type');
            updatePostType(type);
        });

        function updatePostType(type) {
            postTypeInput.val(type);
            postTypeCards.removeClass('active');
            postTypeCards.filter('[data-type="' + type + '"]').addClass('active');

            // Show/hide relevant fields
            imageFields.hide();
            videoFields.hide();
            linkFields.hide();

            switch(type) {
                case 'Image':
                    imageFields.show();
                    break;
                case 'Video':
                    videoFields.show();
                    break;
                case 'Link':
                    linkFields.show();
                    break;
            }
        }
    }

    // Post Status Selector
    function initPostStatusSelector() {
        const postStatusCards = $('.post-status-card');
        const postStatusInput = $('#postStatusInput');

        if (postStatusCards.length === 0) return;

        // Initialize with default or current status
        const currentStatus = postStatusInput.val() || 'Published';
        updatePostStatus(currentStatus);

        postStatusCards.on('click', function() {
            const status = $(this).data('status');
            updatePostStatus(status);
        });

        function updatePostStatus(status) {
            postStatusInput.val(status);
            postStatusCards.removeClass('active');
            postStatusCards.filter('[data-status="' + status + '"]').addClass('active');
        }
    }

    // Post Status Selector
    function initPostStatusSelector() {
        const postStatusCards = $('.post-status-card');
        const postStatusInput = $('#postStatusInput');

        if (postStatusCards.length === 0) return;

        // Initialize with default or current status
        const currentStatus = postStatusInput.val() || 'Published';
        updatePostStatus(currentStatus);

        postStatusCards.on('click', function() {
            const status = $(this).data('status');
            updatePostStatus(status);
        });

        function updatePostStatus(status) {
            postStatusInput.val(status);
            postStatusCards.removeClass('active');
            postStatusCards.filter('[data-status="' + status + '"]').addClass('active');
        }
    }

    // File Preview Functionality
    function initFilePreview() {
        // Image file preview with validation
        $('#imageFileInput').on('change', function(e) {
            const file = e.target.files[0];
            const $errorSpan = $(this).siblings('.text-danger');
            
            if (file) {
                // Validate file type
                const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/webp', 'image/bmp'];
                if (!allowedTypes.includes(file.type)) {
                    $errorSpan.text('Only JPG, JPEG, PNG, GIF, WEBP, and BMP images are allowed');
                    $(this).val('');
                    $('#imagePreview').removeClass('active');
                    return;
                }
                
                // Validate file size (10 MB)
                const maxSize = 10 * 1024 * 1024;
                if (file.size > maxSize) {
                    $errorSpan.text('Image file size cannot exceed 10 MB');
                    $(this).val('');
                    $('#imagePreview').removeClass('active');
                    return;
                }
                
                // Clear error and show preview
                $errorSpan.text('');
                const reader = new FileReader();
                reader.onload = function(e) {
                    $('#imagePreviewImg').attr('src', e.target.result);
                    $('#imagePreview').addClass('active');
                };
                reader.readAsDataURL(file);
            } else {
                $('#imagePreview').removeClass('active');
                $errorSpan.text('');
            }
        });

        // Video file preview with validation
        $('#videoFileInput').on('change', function(e) {
            const file = e.target.files[0];
            const $errorSpan = $(this).siblings('.text-danger');
            
            if (file) {
                // Validate file type
                const allowedTypes = ['video/mp4', 'video/webm', 'video/ogg', 'video/quicktime', 'video/x-msvideo'];
                if (!allowedTypes.includes(file.type)) {
                    $errorSpan.text('Only MP4, WEBM, OGG, MOV, and AVI videos are allowed');
                    $(this).val('');
                    $('#videoPreview').removeClass('active');
                    return;
                }
                
                // Validate file size (50 MB)
                const maxSize = 50 * 1024 * 1024;
                if (file.size > maxSize) {
                    $errorSpan.text('Video file size cannot exceed 50 MB');
                    $(this).val('');
                    $('#videoPreview').removeClass('active');
                    return;
                }
                
                // Clear error and show preview
                $errorSpan.text('');
                const url = URL.createObjectURL(file);
                $('#videoPreviewPlayer').attr('src', url);
                $('#videoPreview').addClass('active');
            } else {
                $('#videoPreview').removeClass('active');
                $errorSpan.text('');
            }
        });

        // Remove image preview
        $('#removeImageBtn').on('click', function() {
            $('#imageFileInput').val('');
            $('#existingImageUrl').val('');
            $('#imagePreview').removeClass('active');
            $('#imagePreview').find('img').remove();
        });

        // Remove video preview
        $('#removeVideoBtn').on('click', function() {
            $('#videoFileInput').val('');
            $('#existingVideoUrl').val('');
            $('#videoPreview').removeClass('active');
            $('#videoPreview').find('video').remove();
        });
    }

    // Delete Post Function
    function deletePost(id) {
        if (typeof Swal !== 'undefined') {
            Swal.fire({
                title: 'Are you sure?',
                text: "You won't be able to revert this!",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#d33',
                cancelButtonColor: '#3085d6',
                confirmButtonText: 'Yes, delete it!'
            }).then((result) => {
                if (result.isConfirmed) {
                    performDelete(id);
                }
            });
        } else {
            if (confirm('Are you sure you want to delete this post? This action cannot be undone.')) {
                performDelete(id);
            }
        }
    }

    function performDelete(id) {
        const url = CultureHelper.addCultureToUrl('/Posts/Delete/' + id);
        $.post(url, { 
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val() 
        }, function(res) {
            if (res.success) {
                window.location.href = CultureHelper.addCultureToUrl('/Posts');
            } else {
                alert('Failed to delete post: ' + (res.message || 'Unknown error'));
            }
        }).fail(function() {
            alert('An error occurred while deleting the post');
        });
    }

    // Pagination Scroll to Top
    function initPaginationScroll() {
        $('.pagination a').on('click', function() {
            $('html, body').animate({ scrollTop: 0 }, 300);
        });
    }

    // Comment Form Handler
    function initCommentForm() {
        $('#commentForm').on('submit', function(e) {
            e.preventDefault();
            const $form = $(this);
            const postId = $form.find('input[name="postId"]').val();
            const content = $form.find('textarea[name="content"]').val();
            const parentCommentId = $form.find('input[name="parentCommentId"]').val() || null;

            if (!content.trim()) {
                showError('Please enter a comment');
                return;
            }

            addComment(postId, content, parentCommentId);
        });
    }

    // Live Search Functionality
    function initLiveSearch() {
        const $searchInput = $('#postSearchInput');
        const $searchSpinner = $('#searchSpinner');
        
        if ($searchInput.length === 0) return;

        $searchInput.on('input', function() {
            const searchTerm = $(this).val().trim();
            
            // Clear previous timeout
            if (searchTimeout) {
                clearTimeout(searchTimeout);
            }

            // Show spinner
            $searchSpinner.removeClass('d-none');

            // Debounce search
            searchTimeout = setTimeout(function() {
                currentSearchTerm = searchTerm;
                performSearch();
            }, config.searchDebounceDelay);
        });

        // Clear search on ESC key
        $searchInput.on('keydown', function(e) {
            if (e.key === 'Escape') {
                $(this).val('');
                currentSearchTerm = '';
                performSearch();
            }
        });
    }

    // Filter Buttons Handler
    function initFilterButtons() {
        $('.filter-btn').on('click', function(e) {
            e.preventDefault();
            const filterType = $(this).data('type');
            
            // Update active state
            $('.filter-btn').removeClass('active btn-purple').addClass('btn-outline-purple');
            $(this).addClass('active btn-purple').removeClass('btn-outline-purple');
            
            // Update current filter
            currentFilter = filterType;
            
            // Perform search with filter
            performSearch();
        });
    }

    // Perform AJAX Search
    function performSearch() {
        const $postsGrid = $('#postsGrid');
        const $resultsCount = $('#resultsCount');
        const $searchSpinner = $('#searchSpinner');
        const $paginationNav = $('#paginationNav');

        // Build URL with parameters
        const params = new URLSearchParams();
        if (currentSearchTerm) params.append('search', currentSearchTerm);
        if (currentFilter) params.append('type', currentFilter);
        params.append('page', 1);
        params.append('pageSize', 12);

        const url = CultureHelper.addCultureToUrl(`/Posts/Index?${params.toString()}`);

        // Show loading state
        $postsGrid.addClass('loading');

        $.ajax({
            url: url,
            type: 'GET',
            dataType: 'html',
            success: function(response) {
                // Parse the response to extract posts grid
                const $response = $(response);
                const $newPostsGrid = $response.find('#postsGrid');
                const $newPagination = $response.find('#paginationNav');
                const $newResultsCount = $response.find('#resultsCount');

                // Update posts grid
                if ($newPostsGrid.length) {
                    $postsGrid.html($newPostsGrid.html());
                    initCardAnimations();
                }

                // Update results count
                if ($newResultsCount.length) {
                    $resultsCount.html($newResultsCount.html());
                }

                // Update pagination
                if ($newPagination.length) {
                    $paginationNav.html($newPagination.html());
                    initPaginationHandlers();
                } else {
                    $paginationNav.html('');
                }

                // Remove loading state
                $postsGrid.removeClass('loading');
                $searchSpinner.addClass('d-none');
            },
            error: function() {
                showError('Failed to load posts. Please try again.');
                $postsGrid.removeClass('loading');
                $searchSpinner.addClass('d-none');
            }
        });
    }

    // Pagination AJAX Handler
    function initPaginationHandlers() {
        $('#paginationNav .page-link').on('click', function(e) {
            e.preventDefault();
            const page = $(this).data('page');
            
            if (!page || $(this).parent().hasClass('disabled')) return;

            loadPage(page);
            $('html, body').animate({ scrollTop: 0 }, 300);
        });
    }

    // Load Specific Page
    function loadPage(page) {
        const $postsGrid = $('#postsGrid');
        const $resultsCount = $('#resultsCount');
        const $paginationNav = $('#paginationNav');

        // Build URL with parameters
        const params = new URLSearchParams();
        if (currentSearchTerm) params.append('search', currentSearchTerm);
        if (currentFilter) params.append('type', currentFilter);
        params.append('page', page);
        params.append('pageSize', 12);

        const url = CultureHelper.addCultureToUrl(`/Posts/Index?${params.toString()}`);

        // Show loading state
        $postsGrid.addClass('loading');

        $.ajax({
            url: url,
            type: 'GET',
            dataType: 'html',
            success: function(response) {
                const $response = $(response);
                const $newPostsGrid = $response.find('#postsGrid');
                const $newPagination = $response.find('#paginationNav');
                const $newResultsCount = $response.find('#resultsCount');

                if ($newPostsGrid.length) {
                    $postsGrid.html($newPostsGrid.html());
                    initCardAnimations();
                }

                if ($newResultsCount.length) {
                    $resultsCount.html($newResultsCount.html());
                }

                if ($newPagination.length) {
                    $paginationNav.html($newPagination.html());
                    initPaginationHandlers();
                } else {
                    $paginationNav.html('');
                }

                $postsGrid.removeClass('loading');
            },
            error: function() {
                showError('Failed to load page. Please try again.');
                $postsGrid.removeClass('loading');
            }
        });
    }

    // Live Search Functionality
    function initLiveSearch() {
        const $searchInput = $('#postSearchInput');
        const $clearBtn = $('#clearSearch');
        const $loadingIndicator = $('#loadingIndicator');
        const $postsContainer = $('#postsContainer');
        let searchTimeout;

        if ($searchInput.length === 0) return;

        $searchInput.on('input', function() {
            const query = $(this).val().trim();
            
            // Show/hide clear button
            if (query.length > 0) {
                $clearBtn.show();
            } else {
                $clearBtn.hide();
            }

            // Debounce search
            clearTimeout(searchTimeout);
            searchTimeout = setTimeout(() => {
                performSearch(query);
            }, 500);
        });

        $clearBtn.on('click', function() {
            $searchInput.val('').focus();
            $(this).hide();
            performSearch('');
        });

        function performSearch(query) {
            if (query.length === 0) {
                // Reset to show all posts
                $('.post-item').show().addClass('fade-in');
                return;
            }

            if (query.length < 2) return;

            // Show loading
            $loadingIndicator.show();

            // Client-side filtering (for current page)
            const $posts = $('.post-item');
            let visibleCount = 0;

            $posts.each(function() {
                const $post = $(this);
                const title = $post.find('.post-title').text().toLowerCase();
                const content = $post.find('.post-content').text().toLowerCase();
                const tags = $post.find('.tag-badge').map(function() {
                    return $(this).text().toLowerCase();
                }).get().join(' ');
                const author = $post.find('.author-name').text().toLowerCase();

                const searchText = `${title} ${content} ${tags} ${author}`;
                
                if (searchText.includes(query.toLowerCase())) {
                    $post.show().addClass('slide-up');
                    visibleCount++;
                } else {
                    $post.hide();
                }
            });

            // Hide loading
            setTimeout(() => {
                $loadingIndicator.hide();
                
                // Show no results message if needed
                if (visibleCount === 0) {
                    showNoResults(query);
                } else {
                    hideNoResults();
                }
            }, 300);
        }

        function showNoResults(query) {
            if ($('#noResultsMessage').length === 0) {
                const message = `
                    <div id="noResultsMessage" class="col-12">
                        <div class="card border-0 shadow-sm empty-state">
                            <div class="card-body text-center py-5">
                                <i class="fas fa-search fa-4x text-muted mb-4"></i>
                                <h4 class="text-muted mb-3">No Posts Found</h4>
                                <p class="text-muted mb-4">
                                    No posts match your search for "<strong>${escapeHtml(query)}</strong>".
                                    <br>Try different keywords or clear the search.
                                </p>
                                <button type="button" class="btn btn-purple text-white" onclick="$('#clearSearch').click()">
                                    <i class="fas fa-times me-2"></i>Clear Search
                                </button>
                            </div>
                        </div>
                    </div>
                `;
                $postsContainer.append(message);
            }
        }

        function hideNoResults() {
            $('#noResultsMessage').remove();
        }

        function escapeHtml(text) {
            const map = {
                '&': '&amp;',
                '<': '&lt;',
                '>': '&gt;',
                '"': '&quot;',
                "'": '&#039;'
            };
            return text.replace(/[&<>"']/g, m => map[m]);
        }
    }

    // Like/Unlike Post
    function toggleLike(postId) {
        const url = CultureHelper.addCultureToUrl(`/Posts/ToggleLike/${postId}`);
        const $likeBtn = $(`[data-post-id="${postId}"]`).find('.like-btn');
        const $likeCount = $(`[data-post-id="${postId}"]`).find('.like-count');

        $.post(url, { 
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val() 
        })
        .done(function(res) {
            if (res.success) {
                $likeBtn.toggleClass('liked');
                $likeCount.text(res.totalLikes);
                
                if (res.isLiked) {
                    $likeBtn.find('i').removeClass('far').addClass('fas');
                } else {
                    $likeBtn.find('i').removeClass('fas').addClass('far');
                }
            } else {
                showError(res.message || 'Failed to toggle like');
            }
        })
        .fail(function() {
            showError('An error occurred while toggling like');
        });
    }

    // Share Post
    function sharePost(postId, title) {
        // Try native share API first
        if (navigator.share) {
            navigator.share({
                title: title || 'Check out this post',
                url: window.location.href
            }).then(() => {
                // Count the share
                const url = CultureHelper.addCultureToUrl(`/Posts/Share/${postId}`);
                $.post(url, { 
                    __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val() 
                });
            }).catch((error) => {
                console.log('Share cancelled or failed:', error);
            });
        } else {
            // Fallback to clipboard
            navigator.clipboard.writeText(window.location.href).then(() => {
                showSuccess('Link copied to clipboard!');
                
                // Count the share
                const url = CultureHelper.addCultureToUrl(`/Posts/Share/${postId}`);
                $.post(url, { 
                    __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val() 
                })
                .done(function(res) {
                    if (res.success) {
                        const $shareCount = $(`[data-post-id="${postId}"]`).find('.share-count');
                        const currentCount = parseInt($shareCount.text()) || 0;
                        $shareCount.text(currentCount + 1);
                    }
                });
            }).catch(() => {
                showError('Failed to copy link');
            });
        }
    }

    // Add Comment
    function addComment(postId, content, parentCommentId = null) {
        const url = CultureHelper.addCultureToUrl('/Posts/AddComment');
        
        $.post(url, {
            postId: postId,
            content: content,
            parentCommentId: parentCommentId,
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
        })
        .done(function(res) {
            if (res.success) {
                showSuccess(res.message || 'Comment added successfully');
                // Reload comments section or append new comment
                location.reload();
            } else {
                showError(res.message || 'Failed to add comment');
            }
        })
        .fail(function() {
            showError('An error occurred while adding comment');
        });
    }

    // Pin/Unpin Post
    function togglePin(postId) {
        const url = CultureHelper.addCultureToUrl(`/Posts/Pin/${postId}`);
        
        $.post(url, { 
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val() 
        })
        .done(function(res) {
            if (res.success) {
                showSuccess(res.message);
                location.reload();
            } else {
                showError(res.message || 'Failed to pin post');
            }
        })
        .fail(function() {
            showError('An error occurred while pinning post');
        });
    }

    // Lock/Unlock Post
    function toggleLock(postId) {
        const url = CultureHelper.addCultureToUrl(`/Posts/Lock/${postId}`);
        
        $.post(url, { 
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val() 
        })
        .done(function(res) {
            if (res.success) {
                showSuccess(res.message);
                location.reload();
            } else {
                showError(res.message || 'Failed to lock post');
            }
        })
        .fail(function() {
            showError('An error occurred while locking post');
        });
    }

    // Publish Post
    function publishPost(postId) {
        const url = CultureHelper.addCultureToUrl(`/Posts/Publish/${postId}`);
        
        $.post(url, { 
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val() 
        })
        .done(function(res) {
            if (res.success) {
                showSuccess(res.message);
                location.reload();
            } else {
                showError(res.message || 'Failed to publish post');
            }
        })
        .fail(function() {
            showError('An error occurred while publishing post');
        });
    }

    // Helper Functions
    function showSuccess(message) {
        if (typeof Swal !== 'undefined') {
            Swal.fire({
                icon: 'success',
                title: 'Success',
                text: message,
                timer: 2000,
                showConfirmButton: false
            });
        } else {
            alert(message);
        }
    }

    function showError(message) {
        if (typeof Swal !== 'undefined') {
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: message
            });
        } else {
            alert(message);
        }
    }

    // Public API
    return {
        init: function() {
            $(document).ready(function() {
                initCardAnimations();
                initPostTypeSelector();
                initPostStatusSelector();
                initFilePreview();
                initPaginationScroll();
                initCommentForm();
                initLiveSearch();
                initFilterButtons();
                initPaginationHandlers();
            });
        },
        deletePost: deletePost,
        toggleLike: toggleLike,
        sharePost: sharePost,
        addComment: addComment,
        togglePin: togglePin,
        toggleLock: toggleLock,
        publishPost: publishPost
    };
})();

// Initialize module
PostsModule.init();

// Expose functions globally for onclick handlers
window.deletePost = PostsModule.deletePost;
window.toggleLike = PostsModule.toggleLike;
window.sharePost = PostsModule.sharePost;
window.addComment = PostsModule.addComment;
window.togglePin = PostsModule.togglePin;
window.toggleLock = PostsModule.toggleLock;
window.publishPost = PostsModule.publishPost;
