$(document).ready(function () {
    bindAmountSlider("none");
    bindColChkBox();
    bindRowChkBox();
    bindNumChange();
    bindCalcAmount();
    bindPages();
    bindReset();
})
var bindReset = function () {
    $("input[type='reset']").bind("click", function () {
        $("input[type='text']").val("");
        $(":checkbox").removeAttr('checked');
        $(".blue").removeClass("blue");
        $("#totalAmount").text("");
    });
}
var bindNumChange = function () {
    $(".num").bind("change", function () {
        var chk = $(this);
        var ischeck = isCheck(chk);
        chk.next("label").attr("class", ischeck ? "blue" : "");
        var col = $(this).parent().parent().prevAll().length + 3
        var row = $(this).parent().prevAll().length + 1;
    });
}
var bindColChkBox = function () {
    $(".ck_cols").bind("change", function () {
        var chk = $(this);
        var col = $(this).parent().parent().prevAll().length + 3
        var ischeck = isCheck(chk);

        $("tr:eq({0})".format(col)).find(":checkbox").attr("checked", ischeck ? true : "").next("label").attr("class", ischeck ? "blue" : "");
    })
}
var bindRowChkBox = function () {
    $(".ck_row").bind("change", function () {
        var chk = $(this);
        var row = $(this).parent().prevAll().length + 1;
        var ischeck = isCheck(chk);
        var chkArr = new Array();
        $(':checkbox').each(function (i, ck) {//搜寻表格里的每一个区间
            if ($(ck).attr("col") == row - 1) {
                chkArr.push(ck);
            }
        });
        $(chkArr).attr("checked", ischeck ? true : "").next("label").attr("class", ischeck ? "blue" : "");
    })
}
var bindCalcAmount = function () {
    $(".ck_cols").bind("change", calcAmount);
    $(".ck_row").bind("change", calcAmount);
    $(".gpw").bind("change", calcAmount);
    $(".company").bind("change", calcAmount);
    $(".num").bind("change", calcAmount);
    $("#amount").bind("keyup", calcAmount).betAmount();
}
var bindPages = function () {
    $(".pages span").bind("click", function () {
        var page = $(this).attr("page");
        var currentPage = $(".pages").attr("currentpage");
        if (page == currentPage) return;
        $(".num").each(function (i, num) {
            num = $(num);
            var n = num.val();
            var newNum = page + n.substring(1, n.length);
            num.val(newNum);
            num.next("label").text(newNum);
        })
        $(".pages span").removeClass("active");
        $(this).addClass("active");
        $(".pages").attr("currentpage", page);
    })
}
var calcAmount = function () {
    try {
        CalcBetAmount();
    }
    catch (e) {
        showMessage("Error", e);
    }
}
var getBetJson = function () {
    var nums = getCheckNums();
    var companys = getCheckCompanys();
    var gpws = getCheckGPWs();
    var amount = $("#amount").val();
    checkValidableFormat(nums, companys, gpws, amount);
    var sb = new Array();
    sb.push("{\"NumList\":[" + nums.join() + "]");
    sb.push("\"Companys\":[" + companys.join() + "]");
    sb.push("\"GamePlayWays\":[" + gpws.join() + "]");
    sb.push("\"Wager\":{0}".format(amount));
    sb.push("\"IsFullPermutation\":false}");
    return sb.join();
}

var checkValidableFormat = function (nums, coms, gpws, amount) {
    var isValidable = true;
    var msg = "";
    if (nums.length == 0) {
        isValidable = false;
        msg = "最少必须选择一个数字！";
    }
    if (isValidable && coms.length == 0) {
        isValidable = false;
        msg = "至少必须选择一个下注公司！";
    }
    if (isValidable && gpws.length == 0) {
        isValidable = false;
        msg = "至少必须选择一个玩法！"
    }
    if (isValidable && (isNullOrEmpty(amount) || amount <= 0)) {
        isValidable = false;
        msg = "请输入正确的投注金额!";
    }
    if (!isValidable)
        throw new Error(msg);
}

var getCheckNums = function () {
    var nums = $(".num");
    var numArr = new Array();
    nums.each(function (i, num) {
        num = $(num);
        if (isCheck(num))
            numArr.push(num.val());
    });
    return numArr;
}
var getCheckGPWs = function () {
    var gpws = $(".gpw");
    var gpwArr = new Array();
    gpws.each(function (i, gpw) {
        gpw = $(gpw);
        if (isCheck(gpw))
            gpwArr.push(gpw.attr("gpw"));
    });
    return gpwArr;
}
var getCheckGPWChkBoxs = function () {
    var gpws = $(".gpw");
    var gpwArr = new Array();
    gpws.each(function (i, gpw) {
        gpw = $(gpw);
        if (isCheck(gpw))
            gpwArr.push(gpw);
    });
    return gpwArr;
}
var getCheckCompanys = function () {
    var companys = $(".company");
    var companyArr = new Array();
    companys.each(function (i, com) {
        com = $(com);
        if (isCheck(com))
            companyArr.push(com.attr("company"));
    });
    return companyArr;
}

var CalcBetAmount = function () {
    var nums = getCheckNums();
    var companys = getCheckCompanys();
    var gpws = getCheckGPWChkBoxs();
    var wager = $("#amount").val();
    InitCompanyNumLen();                //初始化公司支持号码长度
    var totalAmount = CalcRowAmount(nums, companys, gpws, wager);
    $("#totalAmount").text(totalAmount);
}

var CalcRowAmount = function (nums, coms, gpws, wager) {
    var totalAmount = 0;
    var isPermutation = false;
    $(nums).each(function (n, num) {
        $(coms).each(function (t, x) {
            var comNumLen = findCompanyNumLen(x);
            $(gpws).each(function (g, gpw) {
                var gameplaywayInfo = gpw.attr("id").split("_");
                var number = getParseNum(gameplaywayInfo[1], num);
                var numLen = getPlayWayNumLen(gameplaywayInfo[2], number, comNumLen);
                var amount = wager * numLen * (isPermutation ? countPermutation(number) : 1);
                totalAmount += amount;
            });
        });
    });
    return totalAmount;
}
var getParseNum = function (gameType, num) {
    var number = num.toString();
    //2D
    if (gameType == "1") {
        if (number.length > 2) {
            num = number.substring(number.length - 2);
        }
        else if (number.length < 2)
            throw new Error(num + resource.Not2DNum);
    }
    //3D
    else if (gameType == "2") {
        if (number.length > 3) {
            num = number.substring(number.length - 3);
        }
        else if (number.length < 3)
            throw new Error(num + resource.Not3DNum);
    }
    //4D
    else if (gameType == "3") {
        if (number.length > 4)
            num = number.substring(number.length - 4);
        else if (number.length < 4)
            throw new Error(num + resource.Not4DNum);
    }
    //5D
    else if (gameType == "4") {
        if (num.length != 5)
            throw new Error(num + resource.Not5DNum);
    }
    else throw new Error(resource.InvalidNum);
    return num;
}
var getPlayWayNumLen = function (playWay, num, companyNumLen) {
    if (playWay == "1")
        return getHead(num, companyNumLen);
    else if (playWay == "2")
        return getLast();
    else if (playWay == "3")
        return getHeadAndLast(num, companyNumLen);
    else if (playWay == "4")
        return getRoll(num, companyNumLen);
    else
        return;

}
var getHead = function (num, companyNumLen) {
    return findNumLen(num.toString().length, companyNumLen).Count;
}
var getLast = function () {
    return 1;
}
var getHeadAndLast = function (num, companyNumLen) {
    return getHead(num, companyNumLen) + getLast();
}
var getRoll = function (num, companyNumLen) {
    var len = 0;
    $(companyNumLen.NumLenList).each(function (i, obj) {
        if (obj.Length >= num.length)
            len += obj.Count;
    });
    return len;
}
//根据公司Id获取号码长度
var findCompanyNumLen = function (companyId) {
    var numLen;
    $(CompanyNumLen).each(function (i, obj) {
        if (obj.CompanyId == companyId) {
            numLen = obj;
            return false;
        }
    });
    return numLen;
}
var findNumLen = function (length, companyNumLen) {
    var numLen;
    $(companyNumLen.NumLenList).each(function (i, obj) {
        if (obj.Length == length) {
            numLen = obj;
            return false;
        }
    });
    return numLen;
}