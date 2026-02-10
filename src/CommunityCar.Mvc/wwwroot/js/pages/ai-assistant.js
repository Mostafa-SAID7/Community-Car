document.addEventListener('DOMContentLoaded', function () {
    const chatForm = document.getElementById('chatForm');
    const userInput = document.getElementById('userInput');
    const chatMessages = document.getElementById('chatMessages');
    const clearChat = document.getElementById('clearChat');
    const uploadFile = document.getElementById('uploadFile');
    const uploadDatasetBtn = document.getElementById('uploadDatasetBtn');
    const fileInput = document.getElementById('fileInput');
    const filePreview = document.getElementById('filePreview');
    const fileName = document.getElementById('fileName');
    const fileSize = document.getElementById('fileSize');
    const removeFile = document.getElementById('removeFile');
    
    let uploadedFile = null;

    if (!chatForm) return;

    // Quick action buttons
    document.querySelectorAll('.quick-action-btn').forEach(btn => {
        btn.addEventListener('click', function() {
            const prompt = this.getAttribute('data-prompt');
            userInput.value = prompt;
            chatForm.dispatchEvent(new Event('submit'));
        });
    });

    // Suggested topics
    document.querySelectorAll('.suggested-topic').forEach(btn => {
        btn.addEventListener('click', function() {
            const prompt = this.getAttribute('data-prompt');
            userInput.value = prompt;
            chatForm.dispatchEvent(new Event('submit'));
        });
    });

    // Upload file button
    if (uploadFile) {
        uploadFile.addEventListener('click', () => fileInput.click());
    }
    
    if (uploadDatasetBtn) {
        uploadDatasetBtn.addEventListener('click', () => fileInput.click());
    }

    // File input change
    fileInput.addEventListener('change', function(e) {
        const file = e.target.files[0];
        if (file) {
            uploadedFile = file;
            fileName.textContent = file.name;
            fileSize.textContent = formatFileSize(file.size);
            filePreview.style.display = 'flex';
            
            // Auto-upload and analyze
            uploadDataset(file);
        }
    });

    // Remove file
    removeFile.addEventListener('click', function() {
        uploadedFile = null;
        fileInput.value = '';
        filePreview.style.display = 'none';
    });

    // Chat form submit
    chatForm.addEventListener('submit', async function (e) {
        e.preventDefault();
        const message = userInput.value.trim();
        if (!message) return;

        // Add user message to UI
        appendMessage('user', message);
        userInput.value = '';

        // Add typing indicator
        const typingId = addTypingIndicator();

        try {
            const url = CultureHelper.addCultureToUrl('/AI/Assistant/SendMessage');
            const response = await fetch(url, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                },
                body: JSON.stringify({ message: message })
            });

            const data = await response.json();
            removeTypingIndicator(typingId);

            if (data.success) {
                appendMessage('assistant', data.response);
            } else {
                appendMessage('assistant', data.message || 'Sorry, I couldn\'t process that. Please try again.');
            }
        } catch (error) {
            console.error('Chat error:', error);
            removeTypingIndicator(typingId);
            appendMessage('assistant', 'An error occurred. Please check your connection and try again.');
        }
    });

    // Clear chat
    clearChat.addEventListener('click', function () {
        if (confirm('Clear all messages?')) {
            // Keep only the welcome message
            const welcomeMsg = chatMessages.querySelector('.message.assistant');
            const quickActions = chatMessages.querySelector('.quick-actions-container');
            chatMessages.innerHTML = '';
            if (welcomeMsg) chatMessages.appendChild(welcomeMsg.cloneNode(true));
            if (quickActions) chatMessages.appendChild(quickActions.cloneNode(true));
            
            // Re-attach event listeners for quick actions
            document.querySelectorAll('.quick-action-btn').forEach(btn => {
                btn.addEventListener('click', function() {
                    const prompt = this.getAttribute('data-prompt');
                    userInput.value = prompt;
                    chatForm.dispatchEvent(new Event('submit'));
                });
            });
        }
    });

    // Upload dataset function
    async function uploadDataset(file) {
        const formData = new FormData();
        formData.append('file', file);

        // Add user message about upload
        appendMessage('user', `📎 Uploaded dataset: ${file.name}`);
        
        // Add typing indicator
        const typingId = addTypingIndicator();

        try {
            const url = CultureHelper.addCultureToUrl('/AI/Assistant/UploadDataset');
            const response = await fetch(url, {
                method: 'POST',
                body: formData
            });

            const data = await response.json();
            removeTypingIndicator(typingId);

            if (data.success) {
                appendMessage('assistant', `✅ Dataset "${data.fileName}" (${data.fileSize}) uploaded successfully!\n\n${data.response}`);
                // Clear file preview after successful upload
                setTimeout(() => {
                    uploadedFile = null;
                    fileInput.value = '';
                    filePreview.style.display = 'none';
                }, 2000);
            } else {
                appendMessage('assistant', `❌ ${data.message}`);
            }
        } catch (error) {
            console.error('Upload error:', error);
            removeTypingIndicator(typingId);
            appendMessage('assistant', 'Failed to upload dataset. Please try again.');
        }
    }

    // Helper functions
    function appendMessage(sender, text) {
        const time = new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
        const div = document.createElement('div');
        div.className = `message ${sender === 'user' ? 'sent' : 'received'} ${sender}`;
        
        // Format text with line breaks
        const formattedText = text.replace(/\n/g, '<br>');
        
        div.innerHTML = `
            <div class="message-bubble">${formattedText}</div>
            <div class="message-time">${time}</div>
        `;
        chatMessages.appendChild(div);
        chatMessages.scrollTop = chatMessages.scrollHeight;
    }

    function addTypingIndicator() {
        const id = 'typing-' + Date.now();
        const div = document.createElement('div');
        div.id = id;
        div.className = 'message received assistant';
        div.innerHTML = `
            <div class="message-bubble">
                <div class="typing">
                    <span></span><span></span><span></span>
                </div>
            </div>
        `;
        chatMessages.appendChild(div);
        chatMessages.scrollTop = chatMessages.scrollHeight;
        return id;
    }

    function removeTypingIndicator(id) {
        const el = document.getElementById(id);
        if (el) el.remove();
    }

    function formatFileSize(bytes) {
        if (bytes === 0) return '0 Bytes';
        const k = 1024;
        const sizes = ['Bytes', 'KB', 'MB', 'GB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
    }
});
