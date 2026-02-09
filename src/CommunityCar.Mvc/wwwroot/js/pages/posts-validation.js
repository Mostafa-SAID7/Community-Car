// Post File Upload Validation
(function() {
    'use strict';

    const maxImageSize = 10 * 1024 * 1024; // 10 MB
    const maxVideoSize = 50 * 1024 * 1024; // 50 MB
    const allowedImageTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/webp', 'image/bmp'];
    const allowedVideoTypes = ['video/mp4', 'video/webm', 'video/ogg', 'video/quicktime', 'video/x-msvideo'];

    function validateImageFile(file) {
        if (!file) return { valid: true };

        // Validate file type
        if (!allowedImageTypes.includes(file.type)) {
            return {
                valid: false,
                message: 'Invalid image format. Only JPG, PNG, GIF, WebP, and BMP are allowed.'
            };
        }

        // Validate file size
        if (file.size > maxImageSize) {
            return {
                valid: false,
                message: `Image file size cannot exceed ${maxImageSize / 1024 / 1024} MB. Your file is ${(file.size / 1024 / 1024).toFixed(2)} MB.`
            };
        }

        return { valid: true };
    }

    function validateVideoFile(file) {
        if (!file) return { valid: true };

        // Validate file type
        if (!allowedVideoTypes.includes(file.type)) {
            return {
                valid: false,
                message: 'Invalid video format. Only MP4, WebM, OGG, MOV, and AVI are allowed.'
            };
        }

        // Validate file size
        if (file.size > maxVideoSize) {
            return {
                valid: false,
                message: `Video file size cannot exceed ${maxVideoSize / 1024 / 1024} MB. Your file is ${(file.size / 1024 / 1024).toFixed(2)} MB.`
            };
        }

        return { valid: true };
    }

    function showValidationError(message) {
        if (typeof Swal !== 'undefined') {
            Swal.fire({
                icon: 'error',
                title: 'Validation Error',
                text: message,
                confirmButtonColor: '#d33'
            });
        } else {
            alert(message);
        }
    }

    // Initialize validation on document ready
    $(document).ready(function() {
        // Image file validation
        $('#imageFileInput').on('change', function(e) {
            const file = e.target.files[0];
            const validation = validateImageFile(file);
            
            if (!validation.valid) {
                showValidationError(validation.message);
                $(this).val('');
                $('#imagePreview').removeClass('active');
                return;
            }

            // If valid, show preview
            if (file) {
                const reader = new FileReader();
                reader.onload = function(e) {
                    $('#imagePreviewImg').attr('src', e.target.result);
                    $('#imagePreview').addClass('active');
                };
                reader.readAsDataURL(file);
            }
        });

        // Video file validation
        $('#videoFileInput').on('change', function(e) {
            const file = e.target.files[0];
            const validation = validateVideoFile(file);
            
            if (!validation.valid) {
                showValidationError(validation.message);
                $(this).val('');
                $('#videoPreview').removeClass('active');
                return;
            }

            // If valid, show preview
            if (file) {
                const url = URL.createObjectURL(file);
                $('#videoPreviewPlayer').attr('src', url);
                $('#videoPreview').addClass('active');
            }
        });

        // Form submission validation
        $('form[action*="Create"], form[action*="Edit"]').on('submit', function(e) {
            const imageFile = $('#imageFileInput')[0]?.files[0];
            const videoFile = $('#videoFileInput')[0]?.files[0];

            // Validate image if present
            if (imageFile) {
                const imageValidation = validateImageFile(imageFile);
                if (!imageValidation.valid) {
                    e.preventDefault();
                    showValidationError(imageValidation.message);
                    return false;
                }
            }

            // Validate video if present
            if (videoFile) {
                const videoValidation = validateVideoFile(videoFile);
                if (!videoValidation.valid) {
                    e.preventDefault();
                    showValidationError(videoValidation.message);
                    return false;
                }
            }

            // Show loading indicator for large files
            if ((imageFile && imageFile.size > 5 * 1024 * 1024) || (videoFile && videoFile.size > 10 * 1024 * 1024)) {
                if (typeof Swal !== 'undefined') {
                    Swal.fire({
                        title: 'Uploading...',
                        html: 'Please wait while your files are being uploaded. This may take a few moments for large files.',
                        allowOutsideClick: false,
                        allowEscapeKey: false,
                        didOpen: () => {
                            Swal.showLoading();
                        }
                    });
                }
            }

            return true;
        });
    });
})();
