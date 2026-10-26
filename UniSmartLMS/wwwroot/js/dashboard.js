// UniSmartLMS - Dashboard Interactive Features
$(document).ready(function () {

    // Tooltips initialization
    $('[data-toggle="tooltip"]').tooltip();

    // Auto-hide alerts after 5 seconds
    setTimeout(function () {
        $('.alert').fadeOut('slow');
    }, 5000);

    // Add active class to current nav item
    var currentUrl = window.location.pathname;
    $('.nav-link, .dropdown-item').each(function () {
        if ($(this).attr('href') === currentUrl) {
            $(this).addClass('active');
        }
    });

    // Search box auto-focus on index pages
    if ($('.search-input').length) {
        $('.search-input').focus();
    }

    // Confirm delete
    $('.btn-delete').click(function (e) {
        if (!confirm('Are you sure you want to delete this record? This action cannot be undone.')) {
            e.preventDefault();
        }
    });

    // Table row click to details
    $('.clickable-row').click(function () {
        window.location = $(this).data('href');
    });

    // Print functionality
    $('.btn-print').click(function () {
        window.print();
    });

    // Refresh data button
    $('.btn-refresh').click(function () {
        location.reload();
    });
});