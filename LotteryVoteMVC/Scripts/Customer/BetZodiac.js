$(document).ready(function () {
    bindAmountSlider("amount");
    $("input[type='text']").bind("keyup", CalcBetAmount).betAmount();
    $("input[type='checkbox']").bind("change", CalcBetAmount);
    $(":reset").bind("click", function () {
        $(":text").val("");
        $(":checkbox").removeAttr('checked');
        $(".turnover").html("");
    });
});
var getBetJson = function () {
    if (!isAmountEnoughBet())
        throw new Error(resource.HaventEnoughtCredit);
    return buildBetJson();
}

var buildBetJson = function () {
    var betTypeArr = new Array();
    $("input[type='text']").each(function (i, obj) {
        var betType = $(obj).attr("class");
        if (!containsItem(betTypeArr, betType))
            betTypeArr.push(betType);
    });
    var wagerArr = new Array();
    $(betTypeArr).each(function (i, bettype) {
        var wager = getBetTypeWager(bettype);
        if (!isNullOrEmpty(wager))
            wagerArr.push(wager);
    });
    return "[" + wagerArr.join() + "]";
}

var getBetTypeWager = function (bettype) {
    var amount = getNotNullAmount(bettype);
    var companys = getCheckCompany(bettype);
    if (amount.length == 0 || companys.length == 0)
        return "";
    var wagerJson = new Array();
    var betTypeId = $(amount[0]).attr("id").split("_")[1];
    wagerJson.push("{\"BetType\":" + betTypeId);
    var companyList = new Array();
    companys.each(function (i, c) {
        companyList.push($(c).attr("company"));
    });
    wagerJson.push("\"CompanyList\":[" + companyList.join() + "]");
    var wagerArr = new Array();
    amount.each(function (i, a) {
        var wagerValue = $(a).val();
        if (isNaN(wagerValue)) throw new Error(wagerValue + resource.NotValidNum);
        var playWay = $(a).attr("id").split("_")[2];
        wagerArr.push("{\"key\":" + playWay + ",\"value\":" + wagerValue + "}");
    });
    wagerJson.push("\"WagerList\":[" + wagerArr.join() + "]}");
    return wagerJson.join();
}
//-------------------------------------金额计算----------------------------------------
var calcAmount = function () {
    var wagerBox = $("#betbox").attr("target");
    $("#" + wagerBox).trigger("keyup");
}
var CalcBetAmount = function () {
    InitCompanyNumLen();
    var input = $(this);
    var className = input.attr("class");
    var classArr = className.split("_");
    var betType = classArr.length > 1 ? classArr[1] : className;
    var companys = getCheckCompany(betType);
    var totalAmount = 0;
    getNotNullAmount(betType).each(function (i, wager) {
        wager = $(wager);
        var nameInfo = wager.attr("id").split("_");
        var amount = wager.val();
        companys.each(function (t, com) {
            var companyId = com.attr("company");
            totalAmount += amount * getNumCount(companyId, nameInfo[nameInfo.length - 1]);
        });
    });
    totalAmount *= (input.parents("table").find(".betNum").length);
    $("#" + betType).text(totalAmount);
}
//获取指定下注类型选中的公司
var getCheckCompany = function (betType) {
    var companyArr = new Array();
    $(".Company_" + betType).each(function (i, obj) {
        obj = $(obj);
        if (isCheck(obj))
            companyArr.push(obj);
    });
    return $(companyArr);
}
//获取不为空的金额
var getNotNullAmount = function (betType) {
    var amountArr = new Array();
    $("." + betType).each(function (i, obj) {
        obj = $(obj);
        var wager = obj.val();
        if (!isNullOrEmpty(wager) && !isNaN(wager))
            amountArr.push(obj);
    });
    return $(amountArr);
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
var getNumCount = function (companyId, playWay) {
    switch (playWay) {
        case "Head": return getHeadNumCount(companyId);
        case "Last": return getLastNumCount();
        case "HeadAndLast": return getHeadAndLastNumCount(companyId);
        case "Roll": return getRollNumCount(companyId);
        default: throw new Error(resource.NotSupportGamePlay);
    }
}
var getHeadNumCount = function (companyId) {
    var companyNumLen = findCompanyNumLen(companyId);

    var numLen;
    $(companyNumLen.NumLenList).each(function (i, obj) {
        if (obj.Length == 2) {
            numLen = obj;
            return false;
        }
    });
    return numLen.Count;
}
var getLastNumCount = function () {
    return 1;
}
var getHeadAndLastNumCount = function (companyId) {
    return getHeadNumCount(companyId) + getLastNumCount();
}
var getRollNumCount = function (companyId) {
    var companyNumLen = findCompanyNumLen(companyId);

    var numLen=0;
    $(companyNumLen.NumLenList).each(function (i, obj) {
        if (obj.Length >= 2) {
            numLen += obj.Count;
        }
    });
    return numLen;
}