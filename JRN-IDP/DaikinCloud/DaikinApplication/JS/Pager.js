function ASPSnippetsPager($container, pagerData) {
    const linkTemplate = (text, page) =>
        `<a style="cursor:pointer; padding:5px;" class="page" page="${page}">${text}</a>`
    const currentPageTemplate = (text) => `<span>${text}</span>`;
    const totalPage = Math.ceil(pagerData.RecordCount / pagerData.PageSize);
    let pageIndex = Math.min(pagerData.PageIndex, totalPage);

    if (totalPage <= 1) {
        $container.html('');
        return;
    }

    const maxPagesToShow = 10;
    let startPage = Math.max(1, pageIndex - Math.floor(maxPagesToShow / 2));
    let endPage = Math.min(totalPage, startPage + maxPagesToShow + 1);

    startPage = Math.max(1, endPage - maxPagesToShow + 1);

    const startRecord = (pageIndex - 1) * pagerData.PageSize + 1;
    const endRecord = Math.min(pageIndex * pagerData.PageSize, pagerData.RecordCount);
    let html = `<b>Records ${startRecord} - ${endRecord} of ${pagerData.RecordCount}</b> `;

    if (pageIndex > 1) {
        html += linkTemplate("<<", 1);
        html += linkTemplate("<", pageIndex - 1);
    }

    for (let p = startPage; p <= endPage; p++) {
        html += p === pageIndex ? currentPageTemplate(p) : linkTemplate(p, p);
    }

    if (pageIndex < totalPage) {
        html += linkTemplate(">", pageIndex + 1);
        html += linkTemplate(">>", totalPage);
    }

    $container.html(html);
    try {
        $container[0].disabled = false;
    } catch (e) { }
};

(function (a) {
    a.fn.ASPSnippets_Pager = function (b) {
        let c = {};
        let b = a.extend(c, b);
        return this.each(function () {
            ASPSnippetsPager(a(this), b)
        })
    }
})(jQuery);


// -------------------- Helper Functions --------------------
function calculatePageRange(current, total, maxPages) {
    let start = Math.max(1, current - Math.floor(maxPages / 2));
    let end = Math.min(total, start + maxPages - 1);
    start = Math.max(1, end - maxPages + 1);
    return { startPage: start, endPage: end };
};

function calculateRecordRange(pageIndex, pageSize, recordCount) {
    const startRecord = (pageIndex - 1) * pageSize + 1;
    const endRecord = Math.min(pageIndex * pageSize, recordCount);
    return { startRecord, endRecord };
};

function renderNavigationButtons(pageIndex, totalPage) {
    if (pageIndex <= 1) return '';
    return link("<<", 1) + link("<", pageIndex - 1);
};

function renderNextButtons(pageIndex, totalPage) {
    if (pageIndex >= totalPage) return '';
    return link(">", pageIndex + 1) + link(">>", totalPage);
};

function renderPageLinks(start, end, current) {
    let html = '';
    for (let p = start; p <= end; p++) {
        html += p === current ? currentPage(p) : link(p, p);
    }
    return html;
};

function link(text, page) {
    return `<a style="cursor:pointer; padding:5px;" class="page" page="${page}">${text}</a>`;
};

function currentPage(text) {
    return `<span>${text}</span>`;
};