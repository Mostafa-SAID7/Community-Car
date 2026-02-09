document.addEventListener('DOMContentLoaded', function () {
    const chatForm = document.getElementById('chatForm');
    const userInput = document.getElementById('userInput');
    const chatMessages = document.getElementById('chatMessages');
    const clearChat = document.getElementById('clearChat');
    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

    if (!chatForm) return;

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
            const response = await fetch('/AI/Assistant/SendMessage', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': token
                },
                body: JSON.stringify({ message: message })
            });

            const data = await response.json();
            removeTypingIndicator(typingId);

            if (data.success) {
                appendMessage('assistant', data.response);
            } else {
                appendMessage('assistant', 'Sorry, I couldn\'t process that. Please try again.');
            }
        } catch (error) {
            console.error('Chat error:', error);
            removeTypingIndicator(typingId);
            appendMessage('assistant', 'An error occurred. check your connection.');
        }
    });

    clearChat.addEventListener('click', function () {
        if (confirm('Clear all messages?')) {
            chatMessages.innerHTML = '';
            appendMessage('assistant', 'Conversation cleared. How else can I help?');
        }
    });

    function appendMessage(sender, text) {
        const time = new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
        const div = document.createElement('div');
        div.className = `message ${sender}`;
        div.innerHTML = `
            <div class="message-bubble">${text}</div>
            <div class="message-time">${time}</div>
        `;
        chatMessages.appendChild(div);
        chatMessages.scrollTop = chatMessages.scrollHeight;
    }

    function addTypingIndicator() {
        const id = 'typing-' + Date.now();
        const div = document.createElement('div');
        div.id = id;
        div.className = 'message assistant';
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
});
