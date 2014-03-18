//全局变量，用来标记是否进行搜索
IsSearch = true;
$(function () {
    $('form').submit(function () {
        if (IsSearch) {
            var data = $(this).serialize();
            var url = this.action;
            if (!isNullOrEmpty(data))
                data += "&Search=true";
            visit(url, data, true);
            return false;
        }
    });
});
$(document).ready(function () {
    bindSearchValue();
});
var bindSearchValue = function () {
    var url = decodeURIComponent(document.URL);
    if (url.indexOf("?") > 0) {
        //获取所有参数对值
        var queryString = url.substr(url.indexOf("?") + 1, url.length);
        //分组
        var queryStringArr = queryString.split("&");
        $(queryStringArr).each(function (i, param) {
            var paramInfo = param.split("=");
            var ele = $("#searchBar #{0}".format(paramInfo[0]));
            if (ele.length) {
                ele.val(paramInfo[1]);
            }
        });
    }
}