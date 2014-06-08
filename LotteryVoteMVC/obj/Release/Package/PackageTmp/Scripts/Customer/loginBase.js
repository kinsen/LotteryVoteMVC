$(document).ready(function () {
    var lang = $.cookie("Language");
    if (!isNullOrEmpty(lang)) {
        $("#language").val(lang);
    }
    $("#language").bind("change", function () {
        var langStr = $(this).children('option:selected').val();
        setLanguage(langStr);
        reloadPage();
    });
});
var onSuccess = function (data) {
    data = eval(data);
    if (data.IsSuccess) {
        if (data.Model != null && data.Model.ShowChangePwd) {
            $("#changePwdPanel").dialog({ modal: true, minWidth: 300, title: "修改密码" });
            $("#YourPassword").val(data.Model.YourPassword);
        }
        else
            $("#Agreement").dialog({ modal: true, minWidth: 990 });
    }
    else {
        showMessage("Error", data.Message);
        try {
            refreshVerifyCode();
        }
        catch (e) { }
    }
}
