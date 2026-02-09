/**
 * Header Logic - Handle Global Search & Live Results
 */
window.Header = {
    init: () => {
        const searchInput = document.getElementById('globalSearchInput');
        const resultsContainer = document.getElementById('liveSearchResults');
        let searchTimeout;

        if (searchInput && resultsContainer) {
            searchInput.addEventListener('input', (e) => {
                const query = e.target.value.trim();
                clearTimeout(searchTimeout);

                if (query.length < 2) {
                    resultsContainer.classList.add('d-none');
                    resultsContainer.innerHTML = '';
                    return;
                }

                // Show searching state if needed
                searchTimeout = setTimeout(() => {
                    fetch(`/Search/Live?query=${encodeURIComponent(query)}`)
                        .then(response => response.json())
                        .then(data => {
                            if (data.success && data.results.length > 0) {
                                Header.renderResults(data.results, resultsContainer);
                                resultsContainer.classList.remove('d-none');
                            } else {
                                Header.renderNoResults(resultsContainer);
                                resultsContainer.classList.remove('d-none');
                            }
                        })
                        .catch(err => {
                            console.error('Search error:', err);
                            resultsContainer.classList.add('d-none');
                        });
                }, 300);
            });

            // Close results when clicking outside
            document.addEventListener('click', (e) => {
                if (!searchInput.contains(e.target) && !resultsContainer.contains(e.target)) {
                    resultsContainer.classList.add('d-none');
                }
            });

            // Show current results on focus if they exist
            searchInput.addEventListener('focus', () => {
                const query = searchInput.value.trim();
                if (query.length >= 2 && resultsContainer.innerHTML.trim() !== '') {
                    resultsContainer.classList.remove('d-none');
                }
            });
        }
    },

    renderResults: (results, container) => {
        const searchInput = document.getElementById('globalSearchInput');
        const query = searchInput.value;
        const seeAllText = searchInput.dataset.seeAllLabel || 'See all results for';

        let html = '<div class="list-group list-group-flush py-2">';

        results.forEach(res => {
            const iconHtml = res.image
                ? `<img src="${res.image}" class="rounded-circle size-32" />`
                : `<i class="${res.icon} fa-fw"></i>`;

            html += `
                <a href="${res.url}" class="list-group-item list-group-item-action border-0 d-flex align-items-center gap-3">
                    <div class="result-icon text-primary-premium">
                        ${iconHtml}
                    </div>
                    <div class="result-info overflow-hidden">
                        <div class="fw-bold text-truncate">${res.title}</div>
                        <div class="text-xs text-muted fw-medium opacity-75">${res.type}</div>
                    </div>
                </a>
            `;
        });

        html += '</div>';

        // Add "See all results" link
        html += `
            <a href="/Search?query=${encodeURIComponent(query)}" class="see-all-results d-block border-top p-3 text-center text-decoration-none">
                <span class="text-xs fw-bold text-primary">
                    ${seeAllText} "${query}"
                </span>
            </a>
        `;

        container.innerHTML = html;
    },

    renderNoResults: (container) => {
        const searchInput = document.getElementById('globalSearchInput');
        const noResultsText = searchInput.dataset.noResultsLabel || 'No results found';

        container.innerHTML = `
            <div class="p-4 text-center text-muted">
                <i class="fas fa-magnifying-glass fa-2x mb-2 opacity-25"></i>
                <p class="mb-0 small fw-medium">${noResultsText}</p>
            </div>
        `;
    }
};

// Initialized by layout if needed, or manually here
document.addEventListener('DOMContentLoaded', () => {
    window.Header.init();
});
