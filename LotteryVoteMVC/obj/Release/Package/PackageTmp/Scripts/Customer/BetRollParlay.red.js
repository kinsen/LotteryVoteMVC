$(function () {
    registGamePlayWay();
    $("#betTable input[type='text']").betAmount();

    $("input[name='pl']").change(function () {
        var pl = parseInt($("input[name='pl']:checked").val());
        $("input[type='reset']").click();
        var length = pl == 2 ? 5 : 8;
        $(".num").attr("maxlength", pl);
    })

    $("input[type='reset']").bind("click", function () {
        $(".turnover").html("0.00");
        $(".commission").text("0.00");
        $(".netvalue").text("0.00");
        $("#total-amount").text("0.00");
        $("#total-comm").text("0.00");
        $("#total-net").text("0.00");
        $("input[type='text']").val("");
        $(".amount").attr("disabled", true);
        $(":checkbox").not(".region").removeAttr('checked');
    });

    $(".num").keyup(function () {
        var $this = $(this);
        var amount = $this.closest("td").next("td").find("input.amount");
        if (!amount.is(":disabled"))
            amount.attr("disabled", true);

        var numStr = $this.val();
        var pl = parseInt($("input[name='pl']:checked").val());
        if (pl == 2) {
            if (/^\d{2}#\d{2}$/.exec(numStr))
                amount.removeAttr("disabled");
            else if (/^\d$/.exec(numStr)) return;
            else if (/^\d{2}#\d+$/.exec(numStr)) return;
            else if (/^\d{2}$/.exec(numStr)) return;
            else if (/^\d{2}#$/.exec(numStr)) return;
            else
                $this.val("");
        }
        else {
            if (/^\d{2}#\d{2}#\d{2}$/.exec(numStr))
                amount.removeAttr("disabled");
            else if (/^\d$/.exec(numStr)) return;
            else if (/^\d{2}$/.exec(numStr)) return;
            else if (/^\d{2}#$/.exec(numStr)) return;
            else if (/^\d{2}#\d{1,2}$/.exec(numStr)) return;
            else if (/^\d{2}#\d{2}#$/.exec(numStr)) return;
            else if (/^\d{2}#\d{2}#\d+$/.exec(numStr)) return;
            else
                $this.val("");
        }

    });
    $(".selector").change(function () {
        var $this = $(this);
        var isCheck = $this.is(":checked");
        if (!isCheck) return;
        var num = $this.attr("num");
        var numObj = null;
        var numObjVal = null;
        var pl = parseInt($("input[name='pl']:checked").val());
        var length = pl == 2 ? 5 : 8;

        $(".num").each(function () {
            var obj = $(this);
            var objVal = obj.val();
            if (isNullOrEmpty(objVal) || objVal.length < length) {
                numObj = obj;
                numObjVal = objVal;
                return false;
            }
        });
        var amount = numObj.closest("td").next("td").find("input.amount");
        if (!amount.is(":disabled"))
            amount.attr("disabled", true);

        if (isNullOrEmpty(numObjVal))
            numObj.val(num);
        else if (numObjVal == num) return;
        else if ($.inArray(num, numObjVal.split("#")) >= 0) return;
        else {
            numObj.val("{0}#{1}".format(numObjVal, num));
            $(".selector:checked").attr("checked", false);
            if (numObj.val().length == length)
                amount.removeAttr("disabled");
        }
    });

    $(".cell-set").click(function () {
        var $this = $(this);
        var index = $this.closest("td").index();
        if (index < 0) {
            index = $this.closest("th").index();
            index++;
        }

        var amount = $this.closest("td,th").find("input[type='text']").val();
        $(".amount:not(:disabled)").val(amount).trigger("keyup");
    });

    $(".amount").bind("keyup", calcAmount);
    $("#betTable input[type='checkbox']").bind("change", calcAmount);

    $(".region").bind("change", function () {
        var chk = $(this);
        var region = chk.attr("name");
        var companyArr = new Array();
        companyArr.push($(".allCheck_" + region));
        var length = chk.closest("table").find("tr").length;
        for (var i = 1; i <= length; i++) {
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
        var length = chk.closest("table").find("tr").length;
        for (var i = 0; i <= length; i++) {
            $(".company" + i).each(function (i, obj) {
                obj = $(obj);
                if ($.trim(obj.attr("company")) == companyId) {
                    companyArr.push(obj.parent("span"));
                }
            });
        }

        var ischeck = isCheck(chk);
        $(companyArr).each(function (i, obj) {
            if (ischeck)
                $(this).find(":checkbox").attr("checked", true);
            else
                $(this).find(":checkbox").removeAttr("checked");
        });
    });

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
        var num = obj.closest("tr").find(".num").val();
        if (!(/^\d{2}#\d{2}$/.exec(num) || /^\d{2}#\d{2}#\d{2}$/.exec(num))) {
            returnVal = false;
            errorRow = rowId;
            return false;
        }
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
        var num = obj.closest("tr").find(".num").val();

        if (!(/^\d{2}#\d{2}$/.exec(num) || /^\d{2}#\d{2}#\d{2}$/.exec(num))) {
            returnVal = false;
            errorRow = rowId;
            return true;
        }

        var company = $(".company" + rowId);
        var checkCompany = new Array();     //获取选中的公司
        company.each(function (x, chk) {
            chk = $(chk);
            if (!isCheck(chk)) return true;
            checkCompany.push(chk.attr("company"));
        });
        var companyStr = "[" + checkCompany.join() + "]";
        var wargerStr = "[" + buildWarger(num.split("#").length, rowId) + "]";
        var subJson = "{\"CompanyList\":" + companyStr + ",\"Num\":\"" + num + "\",\"WargerList\":" + wargerStr + "}";
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
        var num = obj.closest("tr").find(".num");
        var numStr = num.val();
        if (!(/^\d{2}#\d{2}$/.exec(numStr) || /^\d{2}#\d{2}#\d{2}$/.exec(numStr))) return true; //包组过关格式必须是两个2D或3个2D

        var betCompanys = getBetCompany(rowId);
        var numCount = calcBetNumCount(betCompanys);
        //var totalAmount = numStr.split("#").length * numCount.count * obj.val();
        //var totalComm = calcComm(betCompanys, numCount, numStr);
        var amounts = calcRowAmount(betCompanys, numCount, numStr, obj.val());
        $("#amount" + rowId).text(amounts.amount.toFixed(2));
        $("#comm" + rowId).html(amounts.comm.toFixed(2));
        $("#net" + rowId).html((amounts.amount - amounts.comm).toFixed(2));
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
var getCommission = function (companyId, gameType) {
    $.getJSON("/Commission/GetCommission/{0}/{1}?ticker={3}".format(companyId, gameType, new Date().getTime()), function (e) {
        if (!window.Commission)
            window.Commission = {};
        window.Commission["{0}_{1}".format(companyId, gameType)] = e;
        CalcBetAmount();
    });
}
var calcRowAmount = function (betCompanys, numCount, numStr, warge) {
    var totalAmount = 0;
    var totalComm = 0;
    var gameType = numStr.length == 5 ? 5 : 6;
    betCompanys.each(function () {
        var company = $(this).attr("company");
        var commKey = "{0}_{1}".format(company, gameType);
        if (!window.Commission || !window.Commission[commKey]) {
            getCommission(company, gameType);
            return false; //终止循环
        }
        var amount = numStr.split("#").length * numCount.companys[company] * warge;
        var comm = amount * window.Commission[commKey];
        totalAmount += amount;
        totalComm += comm;
    });
    return { amount: totalAmount, comm: totalComm };
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
    var companys = {};
    betCompanys.each(function (i, obj) {
        var company = obj.attr("company");
        var companyNumLen = findCompanyNumLen(company);
        companys[company] = 0;
        $(companyNumLen.NumLenList).each(function (t, x) {
            count += x.Count;
            companys[company] += x.Count;
        });
    });
    return { count: count, companys: companys };
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