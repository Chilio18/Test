/**
 * Wittebrug Salesperson Dashboard
 * Monitor and take over customer conversations
 */

// Configuration
const API_BASE_URL = window.location.origin;
let currentConversationId = null;
let isHumanMode = false;
let conversations = {};
let pollInterval = null;

// Initialize on page load
document.addEventListener('DOMContentLoaded', () => {
    loadConversations();
    // Poll for updates every 3 seconds
    pollInterval = setInterval(loadConversations, 3000);
});

// Load all conversations
async function loadConversations() {
    try {
        const response = await fetch(`${API_BASE_URL}/api/conversations`);
        if (!response.ok) return;

        const data = await response.json();
        conversations = {};

        // Convert to map
        data.conversations.forEach(conv => {
            conversations[conv.id] = conv;
        });

        renderConversationList();

        // Update current conversation if selected
        if (currentConversationId && conversations[currentConversationId]) {
            updateCurrentConversation(conversations[currentConversationId]);
        }
    } catch (error) {
        console.error('Error loading conversations:', error);
    }
}

// Render conversation list in sidebar
function renderConversationList() {
    const container = document.getElementById('conversationList');
    const emptyState = document.getElementById('emptyConversations');

    const conversationIds = Object.keys(conversations);

    if (conversationIds.length === 0) {
        emptyState.style.display = 'flex';
        return;
    }

    emptyState.style.display = 'none';

    // Sort by last activity (most recent first)
    conversationIds.sort((a, b) => {
        const timeA = conversations[a].last_activity || 0;
        const timeB = conversations[b].last_activity || 0;
        return timeB - timeA;
    });

    // Build HTML
    let html = '';
    conversationIds.forEach(id => {
        const conv = conversations[id];
        const isActive = id === currentConversationId;
        const qualLevel = conv.qualification_level || 'cold';

        html += `
            <div class="conversation-item ${isActive ? 'active' : ''} ${conv.unread ? 'unread' : ''}"
                 onclick="selectConversation('${id}')">
                <div class="conversation-avatar">${getInitials(conv.customer_name)}</div>
                <div class="conversation-content">
                    <div class="conversation-header">
                        <span class="conversation-name">${conv.customer_name || 'Nieuwe klant'}</span>
                        <span class="conversation-time">${formatTime(conv.last_activity)}</span>
                    </div>
                    <div class="conversation-preview">${conv.last_message || 'Geen berichten'}</div>
                    <div class="conversation-badges">
                        <span class="badge ${conv.human_mode ? 'human' : 'ai'}">
                            ${conv.human_mode ? 'Verkoper' : 'AI'}
                        </span>
                        <span class="badge ${qualLevel}">${getQualificationLabel(qualLevel)}</span>
                    </div>
                </div>
            </div>
        `;
    });

    // Only update if conversation list changed
    const existingItems = container.querySelectorAll('.conversation-item');
    if (existingItems.length !== conversationIds.length || !container.innerHTML.includes(html.substring(0, 100))) {
        container.innerHTML = html + emptyState.outerHTML;
    }

    // Update active state
    container.querySelectorAll('.conversation-item').forEach(item => {
        const id = item.getAttribute('onclick').match(/'([^']+)'/)[1];
        item.classList.toggle('active', id === currentConversationId);
    });
}

// Select a conversation
async function selectConversation(conversationId) {
    currentConversationId = conversationId;

    // Show conversation view
    document.getElementById('noConversationSelected').style.display = 'none';
    document.getElementById('activeConversation').style.display = 'flex';

    // Load full conversation data
    try {
        const [convResponse, leadResponse] = await Promise.all([
            fetch(`${API_BASE_URL}/api/conversation/${conversationId}`),
            fetch(`${API_BASE_URL}/api/lead/${conversationId}`)
        ]);

        if (convResponse.ok) {
            const convData = await convResponse.json();
            renderMessages(convData.messages);
            updateConversationHeader(convData);
        }

        if (leadResponse.ok) {
            const leadData = await leadResponse.json();
            updateLeadPanel(leadData);
        }
    } catch (error) {
        console.error('Error loading conversation:', error);
    }

    // Update sidebar
    renderConversationList();
}

// Update current conversation (called during polling)
function updateCurrentConversation(conv) {
    isHumanMode = conv.human_mode || false;
    updateUIForMode();
}

// Render messages in chat area
function renderMessages(messages) {
    const container = document.getElementById('dashboardMessages');

    if (!messages || messages.length === 0) {
        container.innerHTML = '<p style="text-align: center; color: var(--gray-500);">Nog geen berichten</p>';
        return;
    }

    let html = '';
    messages.forEach(msg => {
        const isUser = msg.role === 'user';
        const isToolUse = msg.is_tool_use;

        if (isToolUse) {
            // Show tool use as a system message
            html += `
                <div style="text-align: center; font-size: 12px; color: var(--gray-500); padding: 8px;">
                    <em>AI gebruikt tool: ${msg.tool_name}</em>
                </div>
            `;
        } else {
            html += `
                <div class="message ${isUser ? 'user' : 'agent'}">
                    <div class="message-bubble">${formatMessageContent(msg.content)}</div>
                    <div class="message-meta">
                        ${isUser ? 'Klant' : (msg.human_response ? 'Verkoper' : 'AI Assistent')}
                        • ${formatTime(msg.timestamp)}
                    </div>
                </div>
            `;
        }
    });

    container.innerHTML = html;
    container.scrollTop = container.scrollHeight;
}

// Update conversation header
function updateConversationHeader(conv) {
    document.getElementById('conversationCustomerName').textContent = conv.customer_name || 'Klant';

    isHumanMode = conv.human_mode || false;
    updateUIForMode();
}

// Update lead panel with qualification data
function updateLeadPanel(lead) {
    if (!lead || lead.error) {
        return;
    }

    // Contact info
    document.getElementById('leadName').textContent = lead.name || 'Onbekend';
    document.getElementById('leadSource').textContent = `Bron: ${formatSource(lead.source)}`;
    document.getElementById('leadPhone').textContent = lead.phone || '-';
    document.getElementById('leadEmail').textContent = lead.email || '-';
    document.getElementById('leadContactPref').textContent = lead.preferred_contact_method || '-';

    // Qualification scores
    const qual = lead.qualification || {};
    updateScoreBar('budget', qual.budget?.score || 0);
    updateScoreBar('authority', qual.authority?.score || 0);
    updateScoreBar('need', qual.need?.score || 0);
    updateScoreBar('timeline', qual.timeline?.score || 0);

    // Total score
    const total = qual.total_score || 0;
    document.getElementById('totalScore').textContent = `${total}/40`;

    // Qualification badge
    const qualBadge = document.getElementById('qualificationBadge');
    const level = qual.level || 'cold';
    qualBadge.className = `badge ${level}`;
    qualBadge.textContent = getQualificationLabel(level);

    // Vehicle interests
    const vehicles = lead.vehicle_interests || [];
    document.getElementById('leadVehicles').textContent = vehicles.length > 0 ? vehicles.join(', ') : '-';

    // Trade-in
    document.getElementById('leadTradeIn').textContent = lead.has_trade_in ? 'Ja' : 'Nee';

    // Notes
    const notesContainer = document.getElementById('leadNotes');
    const notes = lead.notes || [];
    if (notes.length > 0) {
        notesContainer.innerHTML = notes.map(note => `
            <div class="note-item">
                <div class="note-time">${note.substring(1, 17)}</div>
                <div>${note.substring(19)}</div>
            </div>
        `).join('');
    } else {
        notesContainer.innerHTML = '<p style="font-size: 13px; color: var(--gray-500);">Nog geen notities</p>';
    }
}

// Update score bar
function updateScoreBar(type, score) {
    const percentage = (score / 10) * 100;
    document.getElementById(`${type}Score`).style.width = `${percentage}%`;
    document.getElementById(`${type}Value`).textContent = score;
}

// Take over conversation
async function takeoverConversation() {
    if (!currentConversationId) return;

    try {
        const response = await fetch(`${API_BASE_URL}/api/conversation/${currentConversationId}/takeover`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' }
        });

        if (response.ok) {
            isHumanMode = true;
            updateUIForMode();
            showNotification('U heeft het gesprek overgenomen', 'success');
        }
    } catch (error) {
        console.error('Error taking over conversation:', error);
        showNotification('Kon gesprek niet overnemen', 'error');
    }
}

// Hand back to AI
async function handbackToAI() {
    if (!currentConversationId) return;

    try {
        const response = await fetch(`${API_BASE_URL}/api/conversation/${currentConversationId}/handback`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' }
        });

        if (response.ok) {
            isHumanMode = false;
            updateUIForMode();
            showNotification('Gesprek teruggegeven aan AI', 'success');
        }
    } catch (error) {
        console.error('Error handing back to AI:', error);
        showNotification('Kon gesprek niet teruggeven', 'error');
    }
}

// Update UI based on human/AI mode
function updateUIForMode() {
    const modeBadge = document.getElementById('conversationModeBadge');
    const takeoverBtn = document.getElementById('takeoverBtn');
    const handbackBtn = document.getElementById('handbackBtn');
    const takeoverBanner = document.getElementById('takeoverBanner');
    const input = document.getElementById('dashboardInput');
    const sendBtn = document.getElementById('dashboardSendBtn');
    const inputHint = document.getElementById('inputHint');

    if (isHumanMode) {
        modeBadge.className = 'badge human';
        modeBadge.textContent = 'Verkoper Modus';
        takeoverBtn.style.display = 'none';
        handbackBtn.style.display = 'flex';
        takeoverBanner.classList.add('active');
        input.disabled = false;
        sendBtn.disabled = false;
        inputHint.textContent = 'Druk op Enter om te versturen';
    } else {
        modeBadge.className = 'badge ai';
        modeBadge.textContent = 'AI Modus';
        takeoverBtn.style.display = 'flex';
        handbackBtn.style.display = 'none';
        takeoverBanner.classList.remove('active');
        input.disabled = true;
        sendBtn.disabled = true;
        inputHint.textContent = 'Neem het gesprek over om berichten te versturen';
    }
}

// Send message from dashboard
async function sendDashboardMessage() {
    if (!currentConversationId || !isHumanMode) return;

    const input = document.getElementById('dashboardInput');
    const message = input.value.trim();

    if (!message) return;

    input.value = '';
    input.style.height = 'auto';

    try {
        const response = await fetch(`${API_BASE_URL}/api/conversation/${currentConversationId}/message`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                message: message,
                from_salesperson: true
            })
        });

        if (response.ok) {
            // Reload messages
            await selectConversation(currentConversationId);
        }
    } catch (error) {
        console.error('Error sending message:', error);
        showNotification('Kon bericht niet versturen', 'error');
    }
}

// Handle keyboard in dashboard input
function handleDashboardKeyDown(event) {
    if (event.key === 'Enter' && !event.shiftKey) {
        event.preventDefault();
        sendDashboardMessage();
    }
}

// Auto-grow dashboard textarea
function autoGrowDashboard(element) {
    element.style.height = 'auto';
    element.style.height = Math.min(element.scrollHeight, 150) + 'px';
}

// Filter conversations
function filterConversations() {
    const searchTerm = document.getElementById('searchInput').value.toLowerCase();
    const items = document.querySelectorAll('.conversation-item');

    items.forEach(item => {
        const name = item.querySelector('.conversation-name').textContent.toLowerCase();
        const preview = item.querySelector('.conversation-preview').textContent.toLowerCase();
        const matches = name.includes(searchTerm) || preview.includes(searchTerm);
        item.style.display = matches ? 'flex' : 'none';
    });
}

// Add note modal
function addNote() {
    document.getElementById('noteModal').style.display = 'flex';
    document.getElementById('noteInput').focus();
}

function closeNoteModal() {
    document.getElementById('noteModal').style.display = 'none';
    document.getElementById('noteInput').value = '';
}

async function saveNote() {
    if (!currentConversationId) return;

    const note = document.getElementById('noteInput').value.trim();
    if (!note) return;

    try {
        const response = await fetch(`${API_BASE_URL}/api/lead/${currentConversationId}/note`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ note: note })
        });

        if (response.ok) {
            closeNoteModal();
            // Reload lead data
            const leadResponse = await fetch(`${API_BASE_URL}/api/lead/${currentConversationId}`);
            if (leadResponse.ok) {
                const leadData = await leadResponse.json();
                updateLeadPanel(leadData);
            }
            showNotification('Notitie toegevoegd', 'success');
        }
    } catch (error) {
        console.error('Error saving note:', error);
        showNotification('Kon notitie niet opslaan', 'error');
    }
}

// View lead details (toggle panel on mobile)
function viewLeadDetails() {
    const panel = document.getElementById('leadPanel');
    panel.style.display = panel.style.display === 'none' ? 'block' : 'none';
}

// Helper functions
function getInitials(name) {
    if (!name) return '?';
    return name.split(' ').map(n => n[0]).join('').substring(0, 2).toUpperCase();
}

function formatTime(timestamp) {
    if (!timestamp) return '';
    const date = new Date(timestamp);
    const now = new Date();
    const isToday = date.toDateString() === now.toDateString();

    if (isToday) {
        return date.toLocaleTimeString('nl-NL', { hour: '2-digit', minute: '2-digit' });
    }
    return date.toLocaleDateString('nl-NL', { day: 'numeric', month: 'short' });
}

function formatSource(source) {
    const sources = {
        'website': 'Website',
        'whatsapp': 'WhatsApp',
        'email': 'E-mail',
        'lead_system': 'Lead Systeem',
        'phone': 'Telefoon'
    };
    return sources[source] || source || 'Onbekend';
}

function formatMessageContent(content) {
    if (!content) return '';
    return content
        .replace(/\*\*(.+?)\*\*/g, '<strong>$1</strong>')
        .replace(/\*(.+?)\*/g, '<em>$1</em>')
        .replace(/\n/g, '<br>');
}

function getQualificationLabel(level) {
    const labels = {
        'hot': 'Warm',
        'warm': 'Lauwwarm',
        'cool': 'Koel',
        'cold': 'Koud'
    };
    return labels[level] || level;
}

// Show notification
function showNotification(message, type = 'success') {
    const notification = document.createElement('div');
    notification.className = `notification ${type}`;
    notification.innerHTML = `<span>${message}</span>`;
    document.body.appendChild(notification);

    setTimeout(() => {
        notification.remove();
    }, 3000);
}
