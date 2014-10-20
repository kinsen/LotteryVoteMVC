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

    selectNav();
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

var editGroupLimit = function (groupId) {
    buildFrame("editGroupLimit", 550, 800, "/ShareRateGroup/Limit/" + groupId, "EditGroupLimit");
    $("#editGroupLimit").bind("load", function () {
        $("#editGroupLimit").contents().find("body").css("width", "780px");
    });
}

var selectNav = function () {
    var rootPath = location.pathname.split('/')[1].toLowerCase();
    var secondPath = location.pathname.split('/')[2];
    secondPath = secondPath ? secondPath.toLowerCase() : "";
    path = "{0}:{1}".format(rootPath, secondPath);
    var navDict = {
        ":": 0,
        "bet:rule": 1,
        "log:loginfailed": 0,
        "bet:": 1,
        "bet:2d": 1,
        "bet:345d": 1,
        "bet:roll": 1,
        "bet:rollparlay": 1,
        "bet:abrollparlay": 1,
        "bet:zodiac": 1,
        "bet:fastbet2d": 1,
        "bet:fastbet3d": 1,
        "sheet:bet": 2,
        "report:statement": 3,
        "log:dropwater": 4,
        "company:": 5,
        "company:lotteryresult": 5,

        "account:": 1,
        "account:shadow": 1,
        "commission:": 2,
        "sheet:": 3,
        "order:all": 3,
        "report:companyamountranking": 3,
        "report:numamountranking": 3,
        "report:threednumamountranking": 3,
        "report:cancel": 3,
        "report:eachlevel": 3,
        "report:outcome":4,
        "report:memberwinlost": 4,
        "report:shareratewl": 4,
        "limit:uppermonitor": 5,
        "dropwater:today": 5,
        "dropwater:tomorrow": 5,
        "dropwater:": 5,
        "dropwater:betauto": 5,
        "limit:stopupperlimits": 5,
        "limit:defaultupper": 5,
        "sharerategroup:": 5,
        "settle:": 6
    };
    var index = navDict[path];
    if (rootPath == "company" && $(".menubar > ul > li").length > 9)
        index = 7;
    $($(".menubar > ul > li")[index]).addClass("current");
};

