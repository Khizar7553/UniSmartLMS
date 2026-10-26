// ========================================
// Student Dashboard Specific Functions
// ========================================

(function () {
    'use strict';

    // Renew book
    window.renewBook = function (loanId, event) {
        if (event) event.stopPropagation();

        if (confirm('Are you sure you want to renew this book? The due date will be extended by 14 days.')) {
            showLoading('loan-row-' + loanId);

            fetch(`/Issuance/Renew/${loanId}`, { method: 'POST' })
                .then(response => response.json())
                .then(data => {
                    if (data.success) {
                        showToast('Book renewed successfully! New due date: ' + formatDate(data.newDueDate), 'success');
                        setTimeout(() => location.reload(), 1500);
                    } else {
                        showToast(data.message || 'Renewal failed', 'error');
                        location.reload();
                    }
                })
                .catch(error => {
                    showToast('Error renewing book', 'error');
                    location.reload();
                });
        }
    };

    // Pay fine
    window.payFine = function (fineId, amount, event) {
        if (event) event.stopPropagation();

        if (confirm(`Pay fine of ${formatCurrency(amount)}?`)) {
            showLoading('fine-row-' + fineId);

            fetch(`/FineTransaction/Pay`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ fineId: fineId, amount: amount, method: 'Online' })
            })
                .then(response => response.json())
                .then(data => {
                    if (data.success) {
                        showToast('Payment successful!', 'success');
                        setTimeout(() => location.reload(), 1500);
                    } else {
                        showToast(data.message || 'Payment failed', 'error');
                    }
                })
                .catch(error => {
                    showToast('Error processing payment', 'error');
                });
        }
    };

    // Cancel reservation
    window.cancelReservation = function (reservationId, event) {
        if (event) event.stopPropagation();

        if (confirm('Are you sure you want to cancel this reservation?')) {
            fetch(`/ReserveItem/Delete/${reservationId}`, { method: 'POST' })
                .then(() => {
                    showToast('Reservation cancelled', 'success');
                    location.reload();
                })
                .catch(error => {
                    showToast('Error cancelling reservation', 'error');
                });
        }
    };

    // View book details
    window.viewBookDetails = function (bookId) {
        window.location.href = `/ItemInfo/Details/${bookId}`;
    };

    // Reserve book
    window.reserveBook = function (bookId, event) {
        if (event) event.stopPropagation();

        fetch(`/ReserveItem/Create?bookId=${bookId}`, { method: 'POST' })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    showToast('Book reserved successfully!', 'success');
                } else {
                    showToast(data.message || 'Reservation failed', 'error');
                }
            })
            .catch(error => {
                showToast('Error reserving book', 'error');
            });
    };

    // Load more books
    let currentPage = 1;
    window.loadMoreBooks = function () {
        currentPage++;
        const container = document.getElementById('booksContainer');
        const loadingIndicator = document.getElementById('loadingIndicator');

        loadingIndicator.style.display = 'block';

        fetch(`/ItemInfo/GetBooks?page=${currentPage}`)
            .then(response => response.json())
            .then(data => {
                loadingIndicator.style.display = 'none';
                if (data.html) {
                    container.insertAdjacentHTML('beforeend', data.html);
                }
                if (!data.hasMore) {
                    document.getElementById('loadMoreBtn').style.display = 'none';
                }
            });
    };
})();