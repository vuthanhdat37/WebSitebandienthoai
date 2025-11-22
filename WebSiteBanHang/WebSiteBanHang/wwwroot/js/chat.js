document.addEventListener('DOMContentLoaded', () => {
    const chatIcon = document.getElementById('chat-icon');
    const chatWindow = document.getElementById('chat-window');
    const closeChatBtn = document.getElementById('close-chat-btn');
    const resetChatBtn = document.getElementById('reset-chat-btn');
    const sendChatBtn = document.getElementById('send-chat-btn');
    const chatInput = document.getElementById('chat-input');
    const chatBody = document.getElementById('chat-body');

    // --- State & Constants ---
    const CHAT_HISTORY_KEY = 'chatHistory';
    const CHAT_TIMESTAMP_KEY = 'chatHistoryTimestamp';
    const HISTORY_EXPIRATION_MS = 60 * 60 * 1000; // 1 hour

    // Toggle chat window
    chatIcon.addEventListener('click', () => {
        chatWindow.classList.remove('hidden');
        chatIcon.classList.add('hidden');
    });

    closeChatBtn.addEventListener('click', () => {
        chatWindow.classList.add('hidden');
        chatIcon.classList.remove('hidden');
    });

    resetChatBtn.addEventListener('click', resetChat);

    // Send message on button click
    sendChatBtn.addEventListener('click', handleSendMessage);

    // Send message on Enter key press
    chatInput.addEventListener('keypress', (e) => {
        if (e.key === 'Enter') {
            e.preventDefault();
            handleSendMessage();
        }
    });

    async function handleSendMessage() {
        const message = chatInput.value.trim();
        if (message === '') return;

        appendAndSaveUserMessage(message);
        chatInput.value = '';
        showLoadingIndicator();

        try {
            const response = await fetch('/api/chat', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ message: message }),
            });
            if (!response.ok) throw new Error('Network response was not ok');
            
            const data = await response.json();
            hideLoadingIndicator();
            
            if (data.introduction) {
                appendAndSaveBotMessage(data.introduction);
            }

            if (data.recommendations && data.recommendations.length > 0) {
                for (const pair of data.recommendations) {
                    await sleep(500); // Add a small delay for a more natural flow
                    appendAndSaveBotMessage(pair.explanation);
                    appendAndSaveProductCard(pair.product);
                }
            }

        } catch (error) {
            hideLoadingIndicator();
            appendAndSaveBotMessage('Rất xin lỗi, đã có lỗi xảy ra. Vui lòng thử lại.');
            console.error('Error:', error);
        }
    }

    function appendAndSaveUserMessage(content) {
        appendMessage(content, 'user');
        saveHistory({ type: 'user_text', content });
    }

    function appendAndSaveBotMessage(content) {
        appendMessage(content, 'bot');
        saveHistory({ type: 'bot_text', content });
    }

    function appendAndSaveProductCard(product) {
        appendProductCard(product);
        saveHistory({ type: 'card', content: product });
    }

    function appendMessage(content, sender) {
        const messageDiv = document.createElement('div');
        messageDiv.classList.add('chat-message', sender);
        const p = document.createElement('p');
        p.textContent = content;
        messageDiv.appendChild(p);
        chatBody.appendChild(messageDiv);
        scrollToBottom();
    }

    function appendProductCard(product) {
        const card = createProductCard(product);
        chatBody.appendChild(card);
        scrollToBottom();
    }

    function createProductCard(product) {
        const card = document.createElement('div');
        card.className = 'chat-product-card chat-message bot';
        const imageUrl = product.imageUrl && product.imageUrl !== "/images/placeholder.png"
            ? product.imageUrl
            : '/images/iphone-15-plus_1__1.png';

        card.innerHTML = `
            <img src="${imageUrl}" alt="${product.name}" class="chat-product-card-img">
            <div class="chat-product-card-info">
                <p class="product-name">${product.name}</p>
                <p class="product-price">${new Intl.NumberFormat('vi-VN').format(product.price)} đ</p>
            </div>
            <a href="/Product/Details/${product.id}" class="chat-product-card-link" target="_blank">Xem</a>
        `;
        return card;
    }

    function showLoadingIndicator() {
        if (document.getElementById('loading-indicator')) return;
        const loadingDiv = document.createElement('div');
        loadingDiv.id = 'loading-indicator';
        loadingDiv.classList.add('chat-message', 'bot');
        loadingDiv.innerHTML = `<div class="loading-dots"><span></span><span></span><span></span></div>`;
        chatBody.appendChild(loadingDiv);
        scrollToBottom();
    }

    function hideLoadingIndicator() {
        const indicator = document.getElementById('loading-indicator');
        if (indicator) indicator.remove();
    }

    function scrollToBottom() {
        chatBody.scrollTop = chatBody.scrollHeight;
    }

    function sleep(ms) {
        return new Promise(resolve => setTimeout(resolve, ms));
    }

    function saveHistory(entry) {
        const history = JSON.parse(localStorage.getItem(CHAT_HISTORY_KEY) || '[]');
        history.push(entry);
        localStorage.setItem(CHAT_HISTORY_KEY, JSON.stringify(history));
        localStorage.setItem(CHAT_TIMESTAMP_KEY, new Date().getTime().toString());
    }

    function loadHistory() {
        const timestamp = localStorage.getItem(CHAT_TIMESTAMP_KEY);
        const history = JSON.parse(localStorage.getItem(CHAT_HISTORY_KEY) || '[]');

        if (!timestamp || !history.length || (new Date().getTime() - parseInt(timestamp)) > HISTORY_EXPIRATION_MS) {
            resetChat();
            return;
        }

        chatBody.innerHTML = '';
        history.forEach(entry => {
            if (entry.type === 'user_text') {
                appendMessage(entry.content, 'user');
            } else if (entry.type === 'bot_text') {
                appendMessage(entry.content, 'bot');
            } else if (entry.type === 'card') {
                appendProductCard(entry.content);
            }
        });
    }

    function resetChat() {
        localStorage.removeItem(CHAT_HISTORY_KEY);
        localStorage.removeItem(CHAT_TIMESTAMP_KEY);
        chatBody.innerHTML = '';
        const welcomeMessage = 'Chào bạn! Tôi là trợ lý ảo của Tech World. Bạn đang tìm kiếm sản phẩm nào?';
        appendAndSaveBotMessage(welcomeMessage);
    }

    // --- Initial Load ---
    loadHistory();
}); 