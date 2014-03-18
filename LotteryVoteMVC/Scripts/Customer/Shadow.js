$(document).ready(function () {
    $("dt :checkbox").each(function (i, chk) {
        chk = $(chk);
        var isCheck = chk.parent().next("dd").find(":checkbox[checked=false]").length == 0;
        if (isCheck)
            chk.attr("checked", 'checked');
        else
            chk.removeAttr('checked');
    });
    $("dt :checkbox").change(function () {
        if ($(this).attr("checked") == true) {
            $(this).parent().next("dd").find(":checkbox").attr('checked', 'checked');
        }
        else {
            $(this).parent().next("dd").find(":checkbox").removeAttr('checked');
        }
    });
    $(".advance").click(function () {
        $(this).parent().next("dd").show();
    });
});
var onSuccess = function (data) {
    data = eval(data);
    showMessage("Result", data.Message);
}
