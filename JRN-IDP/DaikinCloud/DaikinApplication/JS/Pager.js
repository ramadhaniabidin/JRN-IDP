function ASPSnippetsPager(a, b) {
    //console.log(a, b, 'Pager');
    var c = '<a style = "cursor:pointer; padding:5px;" class="page" page = "{1}">{0}</a>';
    var d = "<span>{0}</span>";
    var e, f, g;
    var g = 10;
    var TotalPage = Math.ceil(b.RecordCount / b.PageSize);
    //console.log('TotalPage', TotalPage);
    //console.log('b.PageIndex', b.PageIndex);
    if (b.PageIndex > TotalPage) {
        b.PageIndex = TotalPage
    }
    var i = "";
    if (TotalPage > 1) {
        f = TotalPage > g ? g : TotalPage;
        e = b.PageIndex > 1 && b.PageIndex + g - 1 < g ? b.PageIndex : 1;
        if (b.PageIndex > g % 2) {
            if (b.PageIndex == 2) f = 5;
            else f = b.PageIndex + 2
        } else {
            f = g - b.PageIndex + 1
        }
        if (f - (g - 1) > e) {
            e = f - (g - 1)
        }
        if (f > TotalPage) {
            f = TotalPage;
            e = f - g + 1 > 0 ? f - g + 1 : 1
        }
        var j = (b.PageIndex - 1) * b.PageSize + 1;
        var k = j + b.PageSize - 1;
        if (k > b.RecordCount) {
            k = b.RecordCount
        }
        i = "<b>Records " + (j == 0 ? 1 : j) + " - " + k + " of " + b.RecordCount + "</b> ";
        if (b.PageIndex > 1) {
            i += c.replace("{0}", "<<").replace("{1}", "1");
            i += c.replace("{0}", "<").replace("{1}", b.PageIndex - 1)
        }
        for (var l = e; l <= f; l++) {
            if (l == b.PageIndex) {
                i += d.replace("{0}", l)
            } else {
                i += c.replace("{0}", l).replace("{1}", l)
            }
        }
        if (b.PageIndex < TotalPage) {
            i += c.replace("{0}", ">").replace("{1}", b.PageIndex + 1);
            i += c.replace("{0}", ">>").replace("{1}", TotalPage)
        }
    }
    a.html(i);
    try {
        a[0].disabled = false
    } catch (m) { }
} (function (a) {
    a.fn.ASPSnippets_Pager = function (b) {
        var c = {};
        var b = a.extend(c, b);
        return this.each(function () {
            ASPSnippetsPager(a(this), b)
        })
    }
})(jQuery);