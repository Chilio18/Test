/**
 * Wittebrug Chat Widget
 * Customer-facing chat interface for the Car Sales AI Agent
 */

// Configuration
const API_BASE_URL = window.location.origin;
let conversationId = null;
let isTyping = false;
let isHumanMode = false;

// Generate unique conversation ID
function generateConversationId() {
    return 'cust_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);
}

// Initialize on page load
document.addEventListener('DOMContentLoaded', () => {
    // Get or create conversation ID from session storage
    conversationId = sessionStorage.getItem('chatConversationId');
    if (!conversationId) {
        conversationId = generateConversationId();
        sessionStorage.setItem('chatConversationId', conversationId);
    }

    // Start polling for status updates (to detect human takeover)
    setInterval(checkConversationStatus, 5000);
});

// Toggle chat window
function toggleChat() {
    const widget = document.getElementById('chatWidget');
    widget.classList.toggle('open');

    if (widget.classList.contains('open')) {
        document.getElementById('chatInput').focus();
    }
}

// Auto-grow textarea
function autoGrow(element) {
    element.style.height = 'auto';
    element.style.height = Math.min(element.scrollHeight, 120) + 'px';
}

// Handle keyboard events
function handleKeyDown(event) {
    if (event.key === 'Enter' && !event.shiftKey) {
        event.preventDefault();
        sendMessage();
    }
}

// Send quick reply
function sendQuickReply(text) {
    document.getElementById('chatInput').value = text;
    sendMessage();

    // Hide quick replies after use
    document.getElementById('quickReplies').style.display = 'none';
}

// Send message to API
async function sendMessage() {
    const input = document.getElementById('chatInput');
    const message = input.value.trim();

    if (!message || isTyping) return;

    // Clear input
    input.value = '';
    input.style.height = 'auto';

    // Hide quick replies
    document.getElementById('quickReplies').style.display = 'none';

    // Add user message to chat
    addMessage(message, 'user');

    // Show typing indicator
    showTypingIndicator();

    try {
        const response = await fetch(`${API_BASE_URL}/api/chat`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                conversation_id: conversationId,
                message: message,
                channel: 'website'
            })
        });

        const data = await response.json();

        // Hide typing indicator
        hideTypingIndicator();

        if (data.response) {
            addMessage(data.response, 'agent');

            // Check if human has taken over
            if (data.human_mode) {
                setHumanMode(true);
            }
        } else if (data.error) {
            addMessage('Sorry, er ging iets mis. Probeer het later opnieuw.', 'agent');
        }
    } catch (error) {
        console.error('Error sending message:', error);
        hideTypingIndicator();
        addMessage('Sorry, er ging iets mis met de verbinding. Probeer het later opnieuw.', 'agent');
    }
}

// Add message to chat
function addMessage(text, sender) {
    const messagesContainer = document.getElementById('chatMessages');
    const messageDiv = document.createElement('div');
    messageDiv.className = `message ${sender}`;

    const now = new Date();
    const timeString = now.toLocaleTimeString('nl-NL', { hour: '2-digit', minute: '2-digit' });

    // Convert markdown-like formatting to HTML
    let formattedText = text
        .replace(/\*\*(.+?)\*\*/g, '<strong>$1</strong>')
        .replace(/\*(.+?)\*/g, '<em>$1</em>')
        .replace(/\n/g, '<br>');

    messageDiv.innerHTML = `
        <div class="message-bubble">${formattedText}</div>
        <div class="message-meta">${sender === 'user' ? 'U' : (isHumanMode ? 'Wittebrug Medewerker' : 'Wittebrug Assistent')} • ${timeString}</div>
    `;

    messagesContainer.appendChild(messageDiv);
    scrollToBottom();
}

// Show typing indicator
function showTypingIndicator() {
    isTyping = true;
    const messagesContainer = document.getElementById('chatMessages');

    const typingDiv = document.createElement('div');
    typingDiv.id = 'typingIndicator';
    typingDiv.className = 'typing-indicator';
    typingDiv.innerHTML = '<span></span><span></span><span></span>';

    messagesContainer.appendChild(typingDiv);
    scrollToBottom();

    // Disable send button
    document.getElementById('sendBtn').disabled = true;
}

// Hide typing indicator
function hideTypingIndicator() {
    isTyping = false;
    const typingDiv = document.getElementById('typingIndicator');
    if (typingDiv) {
        typingDiv.remove();
    }

    // Enable send button
    document.getElementById('sendBtn').disabled = false;
}

// Scroll to bottom of messages
function scrollToBottom() {
    const messagesContainer = document.getElementById('chatMessages');
    messagesContainer.scrollTop = messagesContainer.scrollHeight;
}

// Check conversation status (for human takeover detection)
async function checkConversationStatus() {
    if (!conversationId) return;

    try {
        const response = await fetch(`${API_BASE_URL}/api/conversation/${conversationId}/status`);
        if (response.ok) {
            const data = await response.json();
            setHumanMode(data.human_mode || false);
        }
    } catch (error) {
        // Silently fail - not critical
    }
}

// Set human mode (when salesperson takes over)
function setHumanMode(enabled) {
    isHumanMode = enabled;
    const statusDot = document.getElementById('statusDot');
    const statusText = document.getElementById('statusText');
    const humanBanner = document.getElementById('humanBanner');
    const requestHumanSection = document.getElementById('requestHumanSection');

    if (enabled) {
        statusDot.classList.add('human');
        statusText.textContent = 'Medewerker online';
        if (humanBanner) humanBanner.style.display = 'flex';
        if (requestHumanSection) requestHumanSection.style.display = 'none';
    } else {
        statusDot.classList.remove('human');
        statusText.textContent = 'Online';
        if (humanBanner) humanBanner.style.display = 'none';
        if (requestHumanSection) requestHumanSection.style.display = 'block';
    }
}

// Request human assistance
function requestHuman() {
    sendQuickReply('Ik wil graag met een medewerker spreken');
}
