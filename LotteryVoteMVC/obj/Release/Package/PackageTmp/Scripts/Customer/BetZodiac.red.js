$(function () {
    $(".chk_betype").change(function () {
        var $this = $(this);
        if ($this.is(":checked"))
            $this.closest("tr").find(".amount").removeAttr("disabled");
        else
            $this.closest("tr").find(".amount").attr("disabled", true);

    }).keyup(CalcBetAmount);

    $("input[type='reset']").bind("click", function () {
        $(".turnover").html("0.00");
        $(".commission").text("0.00");
        $(".netvalue").text("0.00");
        $("#total-amount").text("0.00");
        $("#total-comm").text("0.00");
        $("#total-net").text("0.00");
        $("input[type='text']").val("");
        $(":checkbox").not(".region").removeAttr('checked');
    });

    $(".allChecked").bind("change", function () {
        var chk = $(this);
        var companyId = chk.attr("id").split("_")[1];
        var companyArr = new Array();
        var length = chk.closest("table").find("tr").length;
        for (var i = 0; i <= length; i++) {
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

    $("input[type='checkbox']").bind("change", CalcBetAmount);
    $(".amount").keyup(CalcBetAmount).blur(CalcBetAmount);

    $("#all_tens").change(function () {
        $(".tens").attr("checked", $(this).is(":checked"));
    });
    $("#all_single").change(function () {
        $(".single").attr("checked", $(this).is(":checked"));
    });

    $(".tens").change(Calc2DBetAmount);
    $(".single").change(Calc2DBetAmount);
    $(".2d-company").change(Calc2DBetAmount);

    $(".2dAmount").keyup(Calc2DBetAmount).blur(Calc2DBetAmount);

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
    if (!isAmountEnoughBet())
        throw new Error(resource.HaventEnoughtCredit);
    if ($(event.srcElement).hasClass("2d"))
        return get2DBetOrderJson();
    return buildBetJson();
}

var buildBetJson = function () {
    var wagerArr = new Array();
    $(".chk_betype:checked").each(function () {
        var $this = $(this);
        var bettype = $this.attr("id");
        var rowId = $this.attr("rowid");
        var wager = getBetTypeWager(bettype, rowId);
        if (!isNullOrEmpty(wager))
            wagerArr.push(wager);
    });
    return "[" + wagerArr.join() + "]";
}

var getBetTypeWager = function (bettype, rowId) {
    var amount = getNotNullAmount(bettype);
    var companys = getCheckCompany(rowId);
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


var get2DBetOrderJson = function () {
    var nums = get2DBetNums();
    var betArr = new Array();
    $(nums).each(function () {
        var wagerStr = get2DWagerJson();
        var num = this;
        var companys = new Array();
        $(".2d-company:checked").each(function () {
            companys.push($(this).attr("company"));
        });
        if (companys.length == 0) return true;
        //var numArr = $(getBetNums(num));
        //numArr.each(function (i, number) {
        var sb = new Array();
        sb.push("{\"CompanyList\":[" + companys.join() + "]");
        sb.push("\"Num\":\"" + num + "\""); //number);
        sb.push("\"WargerList\":" + wagerStr + "}");
        betArr.push(sb.join());
        //});
    });
    var json = betArr.join();
    if (!isNullOrEmpty(json))
        json = "[" + json + "]";
    return json;
}

var get2DWagerJson = function () {
    var txtBoxArr = new Array();        //存放当前Row所有投注金额
    $(".2dAmount").each(function (i, obj) {
        obj = $(obj);
        if (!isNullOrEmpty(obj.val()))
            txtBoxArr.push(obj);
    });

    var wagerArr = new Array();
    wagerArr.push("[");
    var isFirst = true;
    $(txtBoxArr).each(function (i, obj) {
        if (isFirst)
            isFirst = false;
        else
            wagerArr.push(",");
        var sb = new Array();
        var gamePlayWayInfo = obj.attr("id").split("_");
        sb.push("{\"GamePlayTypeId\":" + gamePlayWayInfo[2]);
        sb.push("\"IsFullPermutation\":false");
        sb.push("\"Wager\":" + obj.val() + "}");
        wagerArr.push(sb.join());
    });
    wagerArr.push("]");
    return wagerArr.join("");
}

//-------------------------------------金额计算----------------------------------------
var CalcBetAmount = function () {
    InitCompanyNumLen();

    $(".chk_betype:checked").each(function () {
        var $this = $(this);
        var totalAmount = 0, totalComm = 0;
        var rowId = $this.attr("rowid");
        var betType = $this.attr("id");
        var numCount = parseInt($this.attr("nums"));
        var companys = getCheckCompany(rowId);
        getNotNullAmount(betType).each(function (i, wager) {
            wager = $(wager);
            var nameInfo = wager.attr("id").split("_");
            var amount = wager.val();
            var result = calcRowAmount(companys, numCount, amount, nameInfo[nameInfo.length - 1]);
            totalAmount += result.amount;
            totalComm += result.comm;
        });
        //totalAmount *= numCount;
        //totalComm *= numCount;
        $("#amount" + rowId).text(totalAmount.toFixed(2));
        $("#comm" + rowId).text(totalComm.toFixed(2));
        $("#net" + rowId).text((totalAmount - totalComm).toFixed(2));
    });

    var turnover = 0, commission = 0, netvalue = 0;
    $(".turnover").each(function () {
        turnover += parseFloat($(this).text());
    });
    $(".commission").each(function () {
        commission += parseFloat($(this).text());
    });
    $(".netvalue").each(function () {
        netvalue += parseFloat($(this).text());
    });
    $("#total-amount").text(turnover.toFixed(2));
    $("#total-comm").text(commission.toFixed(2));
    $("#total-net").text(netvalue.toFixed(2));
}

var getCommission = function (companyId) {
    var gameType = 1;
    $.getJSON("/Commission/GetCommission/{0}/{1}?ticker={3}".format(companyId, gameType, new Date().getTime()), function (e) {
        if (!window.Commission)
            window.Commission = {};
        window.Commission["{0}_{1}".format(companyId, gameType)] = e;
        CalcBetAmount();
    });
}

var calcRowAmount = function (betCompanys, numCount, wager, playWay) {
    var totalAmount = 0;
    var totalComm = 0;
    var gameType = 1;
    betCompanys.each(function () {
        var company = $(this).attr("company");
        var commKey = "{0}_{1}".format(company, gameType);
        if (!window.Commission || !window.Commission[commKey]) {
            getCommission(company, gameType);
            return false; //终止循环
        }
        var amount = wager * getNumCount(company, playWay);
        var comm = amount * window.Commission[commKey];
        totalAmount += amount * numCount;
        totalComm += comm * numCount;
    });
    return { amount: totalAmount, comm: totalComm };
}

var Calc2DBetAmount = function () {
    var nums = get2DBetNums();
    var companys = $(".2d-company:checked");
    var totalAmount = 0, totalComm = 0;
    $(nums).each(function () {
        var amount = Calc2DAmount(this, companys);
        totalAmount += amount.amount;
        totalComm += amount.comm;
    });

    $("#2d-turnover").text(totalAmount.toFixed(2));
    $("#2d-comm").text(totalComm.toFixed(2));
    $("#2d-net").text((totalAmount - totalComm).toFixed(2));
}

var get2DBetNums = function () {
    var nums = new Array();
    $(".tens:checked").each(function () {
        var tens = $(this).val();
        $(".single:checked").each(function () {
            var single = $(this).val();
            nums.push(tens + single);
        });
    });
    return nums;
}

var get2DCommission = function (companyId, gameType) {
    $.getJSON("/Commission/GetCommission/{0}/{1}?ticker={3}".format(companyId, gameType, new Date().getTime()), function (e) {
        if (!window.Commission)
            window.Commission = {};
        window.Commission["{0}_{1}".format(companyId, gameType)] = e;
        Calc2DBetAmount();
    });
}
var Calc2DAmount = function (num, companys) {
    var wagerArr = new Array();         //所有本行下注额
    $(".2dAmount").each(function (i, obj) {
        obj = $(obj);
        if (!isNullOrEmpty(obj.val()))
            wagerArr.push(obj);
    });

    var totalAmount = 0;
    var totalComm = 0;
    $(companys).each(function (t, x) {
        var comNumLen = findCompanyNumLen($(x).attr("company"));
        $(wagerArr).each(function (i, obj) {
            var gameplaywayInfo = obj.attr("id").split("_");
            var numLen = getPlayWayNumLen(gameplaywayInfo[1], num, comNumLen);
            var commKey = "{0}_{1}".format(comNumLen.CompanyId, gameplaywayInfo[0]);
            if (!window.Commission || !window.Commission[commKey]) {
                get2DCommission(comNumLen.CompanyId, gameplaywayInfo[0]);
                return false; //终止循环
            }

            var amount = obj.val() * numLen;
            var comm = amount * window.Commission[commKey];
            totalAmount += amount;
            totalComm += comm;
        });

    });
    return { amount: totalAmount, comm: totalComm };
}

//获取指定下注类型选中的公司
var getCheckCompany = function (rowId) {
    var companyArr = new Array();
    $(".company" + rowId).each(function (i, obj) {
        obj = $(obj);
        if (isCheck(obj))
            companyArr.push(obj);
    });
    return $(companyArr);
}
//获取不为空的金额
var getNotNullAmount = function (betType) {
    var amountArr = new Array();
    $("#" + betType).closest("tr").find(".amount").each(function (i, obj) {
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

    var numLen = 0;
    $(companyNumLen.NumLenList).each(function (i, obj) {
        if (obj.Length >= 2) {
            numLen += obj.Count;
        }
    });
    return numLen;
}
var getPlayWayNumLen = function (playWay, num, companyNumLen) {
    if (playWay == "Head")
        return getHead(num, companyNumLen);
    else if (playWay == "Last")
        return getLast();
    else if (playWay == "HeadAndLast")
        return getHeadAndLast(num, companyNumLen);
    else if (playWay == "Roll")
        return getRoll(num, companyNumLen);
    else if (playWay == "Roll7")
        return getRoll7(num, companyNumLen);
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
var getRoll7 = function (num, companyNumLen) {
    return 7;
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