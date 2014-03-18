$(document).ready(function () {
    //多语言切换
    $(".flag").bind("click", function () {
        var lang = $(this).attr("lang");
        var currentLang = getCurrentLanguage();
        if (!isNullOrEmpty(lang) && lang != currentLang) {
            setLanguage(lang);
            reloadPage();
        }
    });
});
$(document).ready(function () {
    // 找到所有菜单, 并添加显示和隐藏菜单事件
    $('#menus > li').each(function () {
        $(this).hover(
        // 显示菜单
            function () {
                $(this).find('ul:eq(0)').show();
            },
        // 隐藏菜单
            function () {
                $(this).find('ul:eq(0)').hide();
            });
    });
    $(".children a").bind("click", function () {
        $(this).parents(".children").hide();
    });
});
var updatePwd = function (userId) {
    buildFrame("UpdatePwd", 230, 350, "/Account/ChangePassword{0}".format(isNullOrEmpty(userId) ? "" : "/" + userId));
}
var editComLimit = function () {
    buildFrame("editComLimit", 550, 800, "/Limit/Index/1", "EditUserLimit");
    $("#editComLimit").bind("load", function () {
        $("#editComLimit").contents().find("body").css("width", "780px");
    });
}