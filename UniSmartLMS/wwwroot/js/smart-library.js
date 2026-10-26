// ========================================
// Smart Library AI - Chat Interface
// Database + Role-Based Chat History
// ========================================

let isLoading = false;
let currentConversationId = null;
let isUserAtBottom = true;

// DOM Elements
const questionInput = document.getElementById('questionInput');
const sendBtn = document.getElementById('sendBtn');
const loadingOverlay = document.getElementById('loadingOverlay');
const messagesContainer = document.getElementById('messagesContainer');

function getChatMessages() {
    return document.getElementById('chatMessages');
}

function getWelcomeScreen() {
    return document.getElementById('welcomeScreen');
}

function forceScrollToBottom() {
    const messages = getChatMessages();
    if (messages) {
        messages.scrollTop = messages.scrollHeight;
    }
    if (messagesContainer) {
        messagesContainer.scrollTop = messagesContainer.scrollHeight;
    }
}

// ============================================================
// LOAD CONVERSATIONS FROM DATABASE
// ============================================================

async function loadConversations() {
    try {
        const response = await fetch('/SmartLibrary/GetConversations');
        const conversations = await response.json();

        const historyList = document.getElementById('chatHistoryList');
        if (historyList) {
            if (conversations.length > 0) {
                historyList.innerHTML = conversations.map(conv => `
                    <div class="history-item" data-id="${conv.id}" onclick="loadConversation(${conv.id})">
                        <i class="fas fa-comment-dots"></i>
                        <span>${escapeHtml(conv.title)}</span>
                    </div>
                `).join('');
            } else {
                historyList.innerHTML = '<div class="history-item">No conversations yet</div>';
            }
        }
        return conversations;
    } catch (e) {
        console.error('Error loading conversations:', e);
        return [];
    }
}

// ============================================================
// LOAD SPECIFIC CONVERSATION MESSAGES
// ============================================================

async function loadConversation(conversationId) {
    try {
        const response = await fetch(`/SmartLibrary/GetMessages?conversationId=${conversationId}`);
        const messages = await response.json();

        const container = getChatMessages();
        const welcomeScreen = getWelcomeScreen();

        if (container) {
            container.innerHTML = '';

            if (messages.length > 0) {
                messages.forEach(msg => {
                    // msg.role comes as "user" or "assistant" from API
                    const messageDiv = createMessageElement(msg.content, msg.role);
                    container.appendChild(messageDiv);
                });

                if (welcomeScreen) welcomeScreen.style.display = 'none';
                container.style.display = 'flex';
                currentConversationId = conversationId;
                setTimeout(() => forceScrollToBottom(), 100);
            } else {
                if (welcomeScreen) welcomeScreen.style.display = 'flex';
                container.style.display = 'none';
                currentConversationId = conversationId;
            }
        }

        // Highlight active conversation
        document.querySelectorAll('.history-item').forEach(item => {
            item.classList.remove('active');
            if (item.getAttribute('data-id') == conversationId) {
                item.classList.add('active');
            }
        });

    } catch (e) {
        console.error('Error loading messages:', e);
    }
}

// ============================================================
// LOAD LAST CONVERSATION (Auto-load on page refresh)
// ============================================================

async function loadLastConversation() {
    try {
        const response = await fetch('/SmartLibrary/GetConversations');
        const conversations = await response.json();
        if (conversations.length > 0) {
            await loadConversation(conversations[0].id);
        }
    } catch (e) {
        console.error('Error loading last conversation:', e);
    }
}

// ============================================================
// NEW CHAT (DO NOT DELETE OLD CONVERSATION)
// ============================================================

async function newChat() {
    const container = getChatMessages();
    const welcomeScreen = getWelcomeScreen();

    if (container) {
        container.innerHTML = '';
        container.style.display = 'none';
    }
    if (welcomeScreen) {
        welcomeScreen.style.display = 'flex';
    }

    currentConversationId = null;
    isLoading = false;

    if (questionInput) {
        questionInput.focus();
    }

    // Remove active highlight from sidebar
    document.querySelectorAll('.history-item').forEach(item => {
        item.classList.remove('active');
    });
}

// ============================================================
// DELETE CONVERSATION (Only when user explicitly wants to delete)
// ============================================================

async function deleteConversation(conversationId) {
    if (!confirm('Delete this conversation?')) return;

    try {
        await fetch(`/SmartLibrary/DeleteConversation?conversationId=${conversationId}`, { method: 'DELETE' });
        await loadConversations();

        if (currentConversationId == conversationId) {
            await newChat();
        }
    } catch (e) {
        console.error('Error deleting conversation:', e);
    }
}

// ============================================================
// SEND MESSAGE
// ============================================================

async function sendMessage() {
    const question = questionInput.value.trim();

    if (!question) {
        showTempMessage('⚠️ Please enter a question', '#dc3545');
        return;
    }

    if (isLoading) return;

    addUserMessage(question);
    questionInput.value = '';
    questionInput.style.height = 'auto';
    questionInput.focus();
    forceScrollToBottom();

    isLoading = true;
    showLoading();

    try {
        const response = await fetch('/SmartLibrary/Ask', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(question)
        });

        const data = await response.json();
        hideLoading();

        if (data.success) {
            addAIMessage(data.answer);

            // Update conversation ID and refresh sidebar
            if (data.conversationId) {
                currentConversationId = data.conversationId;
                await loadConversations();

                // Highlight the current conversation
                document.querySelectorAll('.history-item').forEach(item => {
                    item.classList.remove('active');
                    if (item.getAttribute('data-id') == data.conversationId) {
                        item.classList.add('active');
                    }
                });
            }
        } else {
            addAIMessage("❌ **Error:** " + (data.error || "Please try again."));
        }

    } catch (error) {
        console.error('Error:', error);
        hideLoading();
        addAIMessage("🌐 **Network Error**\n\nPlease check your internet connection and try again.");
    } finally {
        isLoading = false;
    }
}

// ============================================================
// UI HELPER FUNCTIONS
// ============================================================

function addUserMessage(message) {
    const welcomeScreen = getWelcomeScreen();
    const chatMessagesContainer = getChatMessages();

    if (welcomeScreen && chatMessagesContainer) {
        welcomeScreen.style.display = 'none';
        chatMessagesContainer.style.display = 'flex';
    }

    const messageDiv = createMessageElement(escapeHtml(message), 'user');
    const container = getChatMessages();
    if (container) {
        container.appendChild(messageDiv);
        setTimeout(() => forceScrollToBottom(), 50);
    }
}

function addAIMessage(message) {
    const messageDiv = createMessageElement(message, 'ai');
    const container = getChatMessages();
    if (container) {
        container.appendChild(messageDiv);
        setTimeout(() => forceScrollToBottom(), 50);
    }
}

function createMessageElement(content, type) {
    const div = document.createElement('div');
    div.className = `message ${type}-message`;

    const avatar = document.createElement('div');
    avatar.className = 'message-avatar';
    avatar.innerHTML = type === 'user' ? '<i class="fas fa-user"></i>' : '<i class="fas fa-robot"></i>';

    const contentDiv = document.createElement('div');
    contentDiv.className = 'message-content';

    const header = document.createElement('div');
    header.className = 'message-header';
    header.innerHTML = `
        <span class="message-sender">${type === 'user' ? 'You' : 'AI Assistant'}</span>
        <span class="message-time">${formatTime(new Date())}</span>
    `;

    const textDiv = document.createElement('div');
    textDiv.className = 'message-text';

    if (type === 'ai') {
        // AI content already has HTML - render directly
        textDiv.innerHTML = content;
        addCopyButton(textDiv, content);
    } else {
        textDiv.textContent = content;
    }

    contentDiv.appendChild(header);
    contentDiv.appendChild(textDiv);
    div.appendChild(avatar);
    div.appendChild(contentDiv);

    return div;
}

function addCopyButton(textDiv, content) {
    const copyBtn = document.createElement('button');
    copyBtn.className = 'copy-btn';
    copyBtn.innerHTML = '<i class="fas fa-copy"></i>';
    copyBtn.title = 'Copy to clipboard';
    copyBtn.onclick = (e) => {
        e.stopPropagation();
        const plainText = content.replace(/<[^>]*>/g, '');
        navigator.clipboard.writeText(plainText);
        copyBtn.innerHTML = '<i class="fas fa-check"></i>';
        setTimeout(() => {
            copyBtn.innerHTML = '<i class="fas fa-copy"></i>';
        }, 2000);
    };
    textDiv.appendChild(copyBtn);
}

function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

function formatTime(date) {
    return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
}

function showTempMessage(message, color) {
    const tempMsg = document.createElement('div');
    tempMsg.className = 'temp-error';
    tempMsg.textContent = message;
    tempMsg.style.cssText = `position:fixed;bottom:100px;left:50%;transform:translateX(-50%);background:${color};color:white;padding:10px 20px;border-radius:50px;z-index:1000;font-size:13px;animation:fadeInOut 2s ease;`;
    document.body.appendChild(tempMsg);
    setTimeout(() => tempMsg.remove(), 2000);
}

function showLoading() {
    if (loadingOverlay) loadingOverlay.style.display = 'flex';
}

function hideLoading() {
    if (loadingOverlay) loadingOverlay.style.display = 'none';
}

// ============================================================
// EVENT LISTENERS
// ============================================================

document.addEventListener('DOMContentLoaded', async function () {
    await loadConversations();
    await loadLastConversation();

    if (questionInput) {
        questionInput.focus();
    }

    const chatMessages = getChatMessages();
    if (chatMessages) {
        chatMessages.addEventListener('scroll', function () {
            const distanceFromBottom = chatMessages.scrollHeight - chatMessages.scrollTop - chatMessages.clientHeight;
            isUserAtBottom = distanceFromBottom < 50;
        });
    }
});

if (sendBtn) sendBtn.addEventListener('click', sendMessage);

if (questionInput) {
    questionInput.addEventListener('keydown', function (e) {
        if (e.key === 'Enter' && !e.shiftKey) {
            e.preventDefault();
            sendMessage();
        }
    });
}

if (questionInput) {
    questionInput.addEventListener('input', function () {
        this.style.height = 'auto';
        this.style.height = Math.min(this.scrollHeight, 120) + 'px';
    });
}

// ============================================================
// GLOBAL FUNCTIONS FOR HTML BUTTONS
// ============================================================

window.sendQuickQuestion = function (question) {
    if (questionInput) {
        questionInput.value = question;
        sendMessage();
    }
};

window.newChat = newChat;
window.deleteConversation = deleteConversation;
window.loadConversation = loadConversation;

// Update HTML button to call newChat instead of resetChat
// Change your HTML: onclick="resetChat()" → onclick="newChat()"

// ============================================================
// CSS ANIMATIONS
// ============================================================

const animationStyle = document.createElement('style');
animationStyle.textContent = `
    @keyframes fadeInOut {
        0% { opacity: 0; transform: translateX(-50%) translateY(20px); }
        15% { opacity: 1; transform: translateX(-50%) translateY(0); }
        85% { opacity: 1; transform: translateX(-50%) translateY(0); }
        100% { opacity: 0; transform: translateX(-50%) translateY(-20px); }
    }
    .copy-btn {
        position: absolute;
        right: 10px;
        bottom: 10px;
        background: rgba(255,255,255,0.08);
        border: none;
        border-radius: 6px;
        padding: 4px 10px;
        cursor: pointer;
        color: #ffc107;
        font-size: 11px;
        transition: all 0.2s;
        opacity: 0;
    }
    .message-text {
        position: relative;
    }
    .message-text:hover .copy-btn {
        opacity: 1;
    }
    .copy-btn:hover {
        background: rgba(255,193,7,0.2);
        transform: scale(1.02);
    }
    .history-item.active {
        background: rgba(255,193,7,0.15);
        color: #ffc107;
        border-left: 3px solid #ffc107;
    }
    .timeout-card, .warning-card {
        background: #1a1a2e;
        border-radius: 12px;
        padding: 16px;
        text-align: center;
        border-left: 3px solid #ffc107;
    }
    .timeout-icon, .warning-icon { font-size: 32px; margin-bottom: 8px; }
    .timeout-text, .warning-text { color: #ffc107; }
`;
document.head.appendChild(animationStyle);