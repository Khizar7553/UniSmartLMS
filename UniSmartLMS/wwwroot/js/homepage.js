// ========================================
// HOMEPAGE INTERACTIONS - UniSmartLMS
// ========================================

document.addEventListener('DOMContentLoaded', function () {

    // Book Click Handler
    const books = document.querySelectorAll('.orbit-book');
    books.forEach(book => {
        book.addEventListener('click', function () {
            const subject = this.querySelector('span').innerText;
            showSubjectMessage(subject);
        });
    });

    // Center Quran Click Handler
    const quranCenter = document.querySelector('.quran-center');
    if (quranCenter) {
        quranCenter.addEventListener('click', function () {
            showQuranMessage();
        });
    }

    // Search Functionality
    const searchBtn = document.querySelector('.search-btn');
    const searchInput = document.querySelector('.search-input');

    if (searchBtn && searchInput) {
        searchBtn.addEventListener('click', function () {
            performSearch(searchInput.value);
        });

        searchInput.addEventListener('keypress', function (e) {
            if (e.key === 'Enter') {
                performSearch(searchInput.value);
            }
        });
    }

    // Auto-focus search input
    if (searchInput) {
        setTimeout(() => searchInput.focus(), 500);
    }
});

function showSubjectMessage(subject) {
    const messages = {
        'CS/IT': '🔬 Explore Computer Science & IT collection with 5,000+ books',
        'Math': '📐 Mathematics section has 3,000+ books on Algebra, Calculus & more',
        'Physics': '⚡ Physics section - From Newton to Quantum Physics',
        'Science': '🧪 General Science collection with 4,000+ books',
        'Research': '📖 Research section - 2,000+ research papers & journals',
        'Programming': '💻 Programming books - C#, Python, Java, SQL & more',
        'AI/ML': '🤖 Artificial Intelligence & Machine Learning resources',
        'Urdu': '📚 اردو ادب کی 10,000+ کتب دستیاب ہیں'
    };

    const message = messages[subject] || `📚 ${subject} section has books available`;
    showNotification(message);
}

function showQuranMessage() {
    const message = '🕋 Holy Quran Section - Multiple translations & Tafseer available in Urdu, English & Arabic';
    showNotification(message);
}

function performSearch(query) {
    if (query.trim() === '') {
        showNotification('🔍 Please enter a search term');
        return;
    }
    showNotification(`🔍 Searching for "${query}"...`);
    // window.location.href = `/Search?q=${encodeURIComponent(query)}`;
}

function showNotification(message) {
    const existing = document.querySelector('.floating-notification');
    if (existing) existing.remove();

    const notification = document.createElement('div');
    notification.className = 'floating-notification';
    notification.innerHTML = message;
    document.body.appendChild(notification);

    notification.style.cssText = `
        position: fixed;
        bottom: 20px;
        left: 50%;
        transform: translateX(-50%);
        background: #0a2a1a;
        color: #ffc107;
        padding: 12px 24px;
        border-radius: 50px;
        font-size: 14px;
        font-weight: 500;
        z-index: 9999;
        border: 1px solid #ffc107;
        box-shadow: 0 4px 15px rgba(0,0,0,0.3);
        animation: slideUp 0.3s ease;
        white-space: nowrap;
    `;

    setTimeout(() => {
        notification.style.animation = 'slideDown 0.3s ease';
        setTimeout(() => notification.remove(), 300);
    }, 3000);
}