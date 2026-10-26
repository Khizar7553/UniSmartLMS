// ========================================
// AI AVATAR - UniSmartLMS
// With Speech-to-Text & Text-to-Speech (Noise Reduced)
// ========================================

(function () {
    'use strict';

    let isOpen = false;
    let isLoading = false;
    let isRecording = false;
    let recognition = null;
    let synth = window.speechSynthesis;
    let retryCount = 0;

    // DOM Elements
    const chatWindow = document.getElementById('aiChatWindow');
    const chatBtn = document.getElementById('aiAvatarBtn');
    const chatClose = document.getElementById('aiChatClose');
    const chatInput = document.getElementById('aiChatInput');
    const chatSend = document.getElementById('aiChatSend');
    const chatMessages = document.getElementById('aiChatMessages');
    const voiceBtn = document.getElementById('aiVoiceBtn');

    // Initialize Speech Recognition with Noise Reduction
    function initSpeechRecognition() {
        if ('webkitSpeechRecognition' in window) {
            recognition = new webkitSpeechRecognition();
            recognition.continuous = false;
            recognition.interimResults = true;  // Show interim results for better feedback
            recognition.lang = 'en-US';
            recognition.maxAlternatives = 3;     // Get multiple alternatives for best match

            recognition.onresult = function (event) {
                let finalTranscript = '';
                let bestConfidence = 0;

                // Loop through results to find the most confident one
                for (let i = event.resultIndex; i < event.results.length; i++) {
                    if (event.results[i].isFinal) {
                        for (let j = 0; j < event.results[i].length; j++) {
                            const alternative = event.results[i][j];
                            if (alternative.confidence > bestConfidence) {
                                bestConfidence = alternative.confidence;
                                finalTranscript = alternative.transcript;
                            }
                        }
                    }
                }

                // Only accept if confidence is reasonable (> 0.5) or if it's the only option
                if (finalTranscript && (bestConfidence > 0.5 || finalTranscript.length > 0)) {
                    if (chatInput) {
                        chatInput.value = finalTranscript;
                        // Visual feedback - flash green for successful capture
                        if (voiceBtn) {
                            voiceBtn.style.backgroundColor = '#28a745';
                            setTimeout(() => {
                                voiceBtn.style.backgroundColor = '';
                            }, 500);
                        }
                        sendMessage();
                    }
                } else if (finalTranscript && bestConfidence <= 0.5) {
                    addMessage('🎤 I heard: "' + finalTranscript + '" (unclear). Please speak more clearly.', 'ai');
                } else {
                    addMessage('🎤 I couldn\'t understand. Please try again in a quiet environment.', 'ai');
                }
                stopRecording();
            };

            recognition.onerror = function (event) {
                console.error('Speech recognition error:', event.error);
                stopRecording();

                let errorMessage = '';
                switch (event.error) {
                    case 'no-speech':
                        errorMessage = '🎤 No speech detected. Please speak clearly into the microphone.';
                        break;
                    case 'audio-capture':
                        errorMessage = '🎤 Microphone not found. Please check your microphone.';
                        break;
                    case 'not-allowed':
                        errorMessage = '🎤 Microphone access denied. Please allow microphone permissions.';
                        break;
                    case 'network':
                        errorMessage = '🎤 Network error. Please check your connection.';
                        break;
                    default:
                        errorMessage = '🎤 Sorry, I couldn\'t hear you. Please try again.';
                }
                addMessage(errorMessage, 'ai');
            };

            recognition.onend = function () {
                stopRecording();
            };

            recognition.onstart = function () {
                console.log('🎤 Microphone active - speak now!');
            };

            recognition.onsoundstart = function () {
                console.log('🔊 Sound detected!');
                // Visual feedback - mic active
                if (voiceBtn) {
                    voiceBtn.style.backgroundColor = '#dc3545';
                }
            };

            recognition.onspeechstart = function () {
                console.log('🗣️ Speech detected!');
            };
        } else {
            console.log('Speech recognition not supported');
            if (voiceBtn) voiceBtn.style.display = 'none';
        }
    }

    function startRecording() {
        if (!recognition) {
            addMessage('Voice input not supported in this browser. Please use Chrome or Edge.', 'ai');
            return;
        }

        if (isRecording) {
            stopRecording();
            return;
        }

        // Reset retry count
        retryCount = 0;

        try {
            recognition.start();
            isRecording = true;
            if (voiceBtn) {
                voiceBtn.classList.add('recording');
                voiceBtn.innerHTML = '<i class="fas fa-microphone-slash"></i>';
                voiceBtn.title = 'Recording... Click to stop';
            }
            addMessage('🎤 Listening... Speak clearly and close to the microphone.', 'ai');

            // Auto-stop after 10 seconds to prevent hanging
            setTimeout(() => {
                if (isRecording) {
                    addMessage('⏱️ Time limit reached. Please try again.', 'ai');
                    stopRecording();
                }
            }, 10000);

        } catch (error) {
            console.error('Start error:', error);
            addMessage('Please click the microphone button and speak clearly.', 'ai');
            isRecording = false;
            if (voiceBtn) {
                voiceBtn.classList.remove('recording');
                voiceBtn.innerHTML = '<i class="fas fa-microphone"></i>';
            }
        }
    }

    function stopRecording() {
        if (!recognition) return;
        if (!isRecording) return;

        isRecording = false;
        if (voiceBtn) {
            voiceBtn.classList.remove('recording');
            voiceBtn.innerHTML = '<i class="fas fa-microphone"></i>';
            voiceBtn.title = 'Click to speak';
            voiceBtn.style.backgroundColor = '';
        }
        try {
            recognition.stop();
        } catch (e) {
            console.log('Recognition already stopped');
        }
    }

    // Text-to-Speech
    function speakText(text) {
        if (!synth) return;

        synth.cancel();

        // Clean text (remove emojis, markdown, and redirect commands)
        let cleanText = text.replace(/\[\[REDIRECT\]\].*$/, '');
        cleanText = cleanText.replace(/[🔍📚⚠️✅📅💰📊👋⭐❌✅]/g, '');
        cleanText = cleanText.replace(/\*/g, '');
        cleanText = cleanText.replace(/\n/g, '. ');
        cleanText = cleanText.trim();

        if (cleanText.length === 0) return;

        const utterance = new SpeechSynthesisUtterance(cleanText);
        utterance.rate = 0.9;
        utterance.pitch = 1;
        utterance.volume = 1;

        const voices = synth.getVoices();
        const femaleVoice = voices.find(v => v.name.includes('Google UK English Female') || v.name.includes('Samantha'));
        if (femaleVoice) utterance.voice = femaleVoice;

        synth.speak(utterance);
    }

    // Initialize if elements exist
    if (!chatBtn) return;

    initSpeechRecognition();

    // Event Listeners
    chatBtn.addEventListener('click', toggleChat);
    if (chatClose) chatClose.addEventListener('click', closeChat);
    if (chatSend) chatSend.addEventListener('click', sendMessage);
    if (voiceBtn) voiceBtn.addEventListener('click', startRecording);
    if (chatInput) {
        chatInput.addEventListener('keypress', (e) => {
            if (e.key === 'Enter') sendMessage();
        });
    }

    function toggleChat() {
        if (isOpen) {
            closeChat();
        } else {
            openChat();
        }
    }

    function openChat() {
        if (chatWindow) {
            chatWindow.style.display = 'flex';
            isOpen = true;
            if (chatInput) chatInput.focus();
            scrollToBottom();
        }
    }

    function closeChat() {
        if (chatWindow) {
            chatWindow.style.display = 'none';
            isOpen = false;
            if (synth) synth.cancel();
        }
    }

    // MAIN SEND MESSAGE FUNCTION
    async function sendMessage() {
        if (!chatInput) return;

        const message = chatInput.value.trim();
        if (!message || isLoading) return;

        addMessage(message, 'user');
        chatInput.value = '';

        showTypingIndicator();
        isLoading = true;

        try {
            const response = await fetch('/AIAssistant/Chat', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ query: message })
            });

            if (!response.ok) {
                throw new Error('Network response was not ok');
            }

            const data = await response.json();
            hideTypingIndicator();

            // Check for redirect command BEFORE adding message
            if (data.response && data.response.startsWith('[[REDIRECT]]')) {
                const url = data.response.replace('[[REDIRECT]]', '');
                const action = url.split('/').pop();

                // Speak before redirecting
                speakText(`Opening ${action} page for you.`);

                setTimeout(() => {
                    if (confirm('🤖 AI Assistant wants to open: ' + url + '\n\nContinue?')) {
                        window.location.href = url;
                    }
                }, 500);
                isLoading = false;
                scrollToBottom();
                return;
            }

            // Normal message
            addMessage(data.response, 'ai');
            speakText(data.response);

        } catch (error) {
            console.error('Error:', error);
            hideTypingIndicator();
            addMessage('Sorry, I encountered an error. Please try again.', 'ai');
        }

        isLoading = false;
        scrollToBottom();
    }

    function addMessage(text, sender) {
        if (!chatMessages) return;

        const messageDiv = document.createElement('div');
        messageDiv.className = `ai-message ${sender}`;

        const bubble = document.createElement('div');
        bubble.className = 'ai-message-bubble';

        if (sender === 'ai') {
            bubble.innerHTML = `
                <i class="fas fa-robot text-success me-1"></i> 
                <strong>AI Assistant</strong>
                <button class="ai-speaker-btn" onclick="speakThis('${escapeHtml(text).replace(/'/g, "\\'")}')">
                    <i class="fas fa-volume-up"></i>
                </button>
                <br/>${formatResponse(text)}
            `;
        } else {
            bubble.innerHTML = escapeHtml(text);
        }

        const time = document.createElement('div');
        time.className = 'ai-message-time';
        time.textContent = formatTime(new Date());

        messageDiv.appendChild(bubble);
        messageDiv.appendChild(time);
        chatMessages.appendChild(messageDiv);
        scrollToBottom();
    }

    window.speakThis = function (text) {
        speakText(text);
    };

    function formatResponse(text) {
        let formatted = text.replace(/\n/g, '<br/>');
        formatted = formatted.replace(/•/g, '• ');
        return formatted;
    }

    function escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    function formatTime(date) {
        return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    }

    function showTypingIndicator() {
        if (!chatMessages) return;

        const typingDiv = document.createElement('div');
        typingDiv.className = 'ai-message ai';
        typingDiv.id = 'aiTypingIndicator';
        typingDiv.innerHTML = `
            <div class="ai-typing-indicator">
                <span></span><span></span><span></span>
            </div>
        `;
        chatMessages.appendChild(typingDiv);
        scrollToBottom();
    }

    function hideTypingIndicator() {
        const indicator = document.getElementById('aiTypingIndicator');
        if (indicator) indicator.remove();
    }

    function scrollToBottom() {
        if (chatMessages) {
            chatMessages.scrollTop = chatMessages.scrollHeight;
        }
    }
})();