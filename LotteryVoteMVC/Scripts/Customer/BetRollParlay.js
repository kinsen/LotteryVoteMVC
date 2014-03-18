$(document).ready(function () {
    registGamePlayWay();
    bindClassAmountSilder("amount");
    $("input[type='reset']").bind("click", function () {
        $(".turnover").html("");
        $("input[type='text']").val("");
        $(":checkbox").not(".region").removeAttr('checked');
    });
    $("#betTable input[type='text']").betAmount();
    $(".region").bind("change", function () {
        var chk = $(this);
        var region = chk.attr("name");
        var companyArr = new Array();
        companyArr.push($(".allCheck_" + region));
        for (var i = 1; i <= 10; i++) {
            $(".company" + i).each(function (i, obj) {
                obj = $(obj);
                if (obj.attr("region") == region) {
                    companyArr.push(obj.parent("span"));
                }
            });
        }

        var ischeck = isCheck(chk);
        $(companyArr).each(function (i, obj) {
            $(this).find(":checkbox").attr("checked", "");
            if (ischeck)
                $(obj).show();
            else
                $(obj).hide();
        });

        $.cookie(region, ischeck, { expires: 1 });
    });
    $(".allChecked").bind("change", function () {
        var chk = $(this);
        var companyId = chk.attr("id").split("_")[1];
        var companyArr = new Array();
        for (var i = 1; i <= 10; i++) {
            $(".company" + i).each(function (i, obj) {
                obj = $(obj);
                if (obj.attr("company") == companyId) {
                    companyArr.push(obj.parent("span"));
                }
            });
        }

        var ischeck = isCheck(chk);
        $(companyArr).each(function (i, obj) {
            if (ischeck)
                $(this).find(":checkbox").attr("checked", true);
            else
                $(this).find(":checkbox").attr("checked", "");
        });
    });
    $(".amount").bind("keyup", calcAmount);
    $("#betTable input[type='checkbox']").bind("change", calcAmount);

    if ($.cookie("North") == "false") {
        $("#North").trigger("click").trigger("change");
    }
    if ($.cookie("South") == "false") {
        $("#South").trigger("click").trigger("change");
    }
    if ($.cookie("Middle") == "false") {
        $("#Middle").trigger("click").trigger("change");
    }
});
var getBetJson = function () {
    checkBetOrderFormat();
    return getBetOrderJson();
}
var checkBetOrderFormat = function () {
    var amounts = getNotNullAmount();
    var returnVal = amounts.length > 0;
    var errorRow;
    $(amounts).each(function (i, obj) {
        obj = $(obj);
        var rowId = obj.attr("rowid");
        var nums = $(".num" + rowId);
        if (nums.length < 3) {
            returnVal = false;
            errorRow = rowId;
            return false;
        }
        var numArr = new Array();
        $(nums).each(function (t, x) {
            var num = $(x).val();
            if (isNullOrEmpty(num)) {
                return true;        //contine
            }
            if (isNaN(num)) {
                returnVal = false;
                errorRow = rowId;
            }

            if (num.length == 2)    // && !containsItem(numArr, num))
                numArr.push(num);
            else {
                returnVal = false;
                errorRow = rowId;
                return false;
            }
        });
        returnVal = returnVal && (numArr.length >= 2);
    });
    if (!returnVal) {
        if (isNullOrEmpty(errorRow))
            throw new Error(resource.PleaseCheckContent);
        else
            throw new Error(resource.PleaseCheckRowContent.format(errorRow));
    }
}

var getBetOrderJson = function () {
    if (!isAmountEnoughBet())
        throw new Error(resource.HaventEnoughtCredit);
    return parseToRollParlay(getNotNullAmount());
}

var parseToRollParlay = function (notNullAmount) {
    notNullAmount = $(notNullAmount);
    var betArr = new Array();
    notNullAmount.each(function (i, obj) {
        obj = $(obj);
        var rowId = obj.attr("rowid");
        var nums = $(".num" + rowId);
        if (nums.length != 3) return true;     //号码个数必须是3个，否则跳过

        var notNullNumArr = new Array();
        nums.each(function (x, num) {
            num = $(num);
            var numVal = num.val();
            if (!isNullOrEmpty(numVal) && !isNaN(numVal))    //不能为空
                notNullNumArr.push(numVal);
        });

        //包组过关最少需有两个数字
        if (notNullNumArr.length < 2)
            return true;

        var company = $(".company" + rowId);
        var checkCompany = new Array();     //获取选中的公司
        company.each(function (x, chk) {
            chk = $(chk);
            if (!isCheck(chk)) return true;
            checkCompany.push(chk.attr("company"));
        });
        var numStr = notNullNumArr.join("#");
        var companyStr = "[" + checkCompany.join() + "]";
        var wargerStr = "[" + buildWarger(notNullNumArr.length, rowId) + "]";
        var subJson = "{\"CompanyList\":" + companyStr + ",\"Num\":\"" + numStr + "\",\"WargerList\":" + wargerStr + "}";
        betArr.push(subJson);
    });
    return "[" + betArr.join() + "]";
}
var buildWarger = function (numCount, rowId) {
    var gamePlayType = numCount == 2 ? PL2 : PL3;
    var amount;
    $(".amount").each(function (i, obj) {
        obj = $(obj);
        if (obj.attr("rowid") == rowId) {
            amount = obj.val();
            return false;
        }
    });
    var json = "{\"GamePlayTypeId\":" + gamePlayType + ",\"IsFullPermutation\":false,\"Wager\":" + amount + "}";
    return json;
}

//---------------------------------金额计算---------------------------------------------
var calcAmount = function () {
    try {
        CalcBetAmount();
    }
    catch (e) {
        showMessage("Error", e);
    }
}
//获取不为空的金额
var getNotNullAmount = function () {
    var notNullAmountArr = new Array(); //金额不为空输入框
    $(".amount").each(function (i, obj) {
        obj = $(obj);
        var amount = obj.val();
        if (!isNullOrEmpty(amount) && !isNaN(amount))
            notNullAmountArr.push(obj);
    });
    return notNullAmountArr;
}
//计算下注金额
var CalcBetAmount = function () {
    InitCompanyNumLen();                //初始化公司支持号码长度
    var notNullAmountArr = getNotNullAmount();

    $(notNullAmountArr).each(function (i, obj) {
        var rowId = obj.attr("rowid");
        var nums = $(".num" + rowId);
        if (nums.length < 2)    //包组过关至少需要两个号码
            return true;
        var numArr = new Array();
        nums.each(function (t, x) {
            x = $(x);
            var num = x.val();
            if (!isNullOrEmpty(num)) {
                if (!isNaN(num) && num.length == 2)
                    numArr.push(num);
                else {
                    $("#amount_" + rowId).text("");
                    throw new Error(num + resource.NotValidNum);
                }
            }
        });
        //包组过关至少需要两个号码
        if (numArr.length < 2) return true;

        var betCompanys = getBetCompany(rowId);
        var numCount = calcBetNumCount(betCompanys);
        var totalAmount = numArr.length * numCount * obj.val();
        $("#amount_" + rowId).text(totalAmount);
    });
}
//获取要投注的公司
var getBetCompany = function (rowId) {
    var companyArr = new Array();
    $(".company" + rowId).each(function (i, obj) {
        obj = $(obj);
        if (isCheck(obj))
            companyArr.push(obj);
    });
    return $(companyArr);
}
//计算所有公司号码数
var calcBetNumCount = function (betCompanys) {
    var count = 0;
    betCompanys.each(function (i, obj) {
        var companyNumLen = findCompanyNumLen(obj.attr("company"));
        $(companyNumLen.NumLenList).each(function (t, x) {
            count += x.Count;
        });
    });
    return count;
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
var PL2 = null;
var PL3 = null;
var registGamePlayWay = function () {
    var pl2 = $("#PL2").val();
    var pl3 = $("#PL3").val();
    PL2 = pl2;
    PL3 = pl3;
}