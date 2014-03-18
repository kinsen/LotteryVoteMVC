var onError = null;
var onSuccess = function (data) {
    try {
        data = eval(data);
        showMessage("result", data.Message);
    }
    catch (e) {
        showMessage("result", "Something wrong,please connection your agent!");
    }
}
var onComplete = closeLoadingMask;      //操作完成时发生，需要使覆盖即可
var beforePost = openLodingMask;        //表单提交前操作，需要时覆盖即可
var POSTDATA = new Array();
$(function () {
    $('form').submit(function () {
        var action = this.action;
        if ($.inArray(action, POSTDATA) >= 0)
            return false;
        if ($(this).valid()) {
            $.ajax({
                url: this.action,
                type: this.method,
                data: $(this).serialize(),
                success: onSuccess,
                error: onError,
                complete: function () {
                    onComplete();
                    var idx = $.inArray(action, POSTDATA);
                    if (idx != -1) POSTDATA.splice(idx, 1);
                }
            });
            beforePost();
            POSTDATA.push(action);
        }
        return false;
    });
});