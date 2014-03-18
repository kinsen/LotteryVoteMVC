$(document).ready(function () {
    $(":submit").bind("click", function () {
        var url = $(this).attr("action");
        if (!isNullOrEmpty(url)) {
            var form = $(this).parents("form");
            form.attr("action", url);
        }
    });
});