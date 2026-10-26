// ========================================
// Faculty Dashboard Specific Functions
// ========================================

(function () {
    'use strict';

    // Manage course reserve
    window.manageCourseReserve = function (courseId) {
        window.location.href = `/Course/ManageReserves/${courseId}`;
    };

    // Recommend book
    window.recommendBook = function () {
        window.location.href = '/ItemRequisition/Create';
    };

    // View publication
    window.viewPublication = function (publicationId) {
        window.location.href = `/ProfessorPublication/Details/${publicationId}`;
    };

    // Submit research request
    window.submitResearchRequest = function () {
        window.location.href = '/Research/Request';
    };

    // Download research paper
    window.downloadPaper = function (paperId) {
        showToast('Download started', 'success');
        // window.location.href = `/DigitalResource/Download/${paperId}`;
    };

    // Refresh stats
    window.refreshStats = function () {
        showLoading('statsContainer');
        location.reload();
    };
})();