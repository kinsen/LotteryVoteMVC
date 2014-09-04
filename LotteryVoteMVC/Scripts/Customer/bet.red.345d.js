﻿var cols = { "3": [1, 2, 3, 4, 5], "4": [2, 4], "5": [2] };
$(function () {
    $(".num").betNumeral();
    $("input[type='text']").not(".num").betAmount();

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

    var numCheck = function () {
        var $this = $(this);
        var length = $this.val().length;
        var showCols = cols[length.toString()];
        $this.closest("tr").find("input[type='text']").not(".num").attr("disabled", "disabled");
        if (showCols && showCols.length > 0)
            for (var col in showCols)
                $this.closest("tr").children().eq(showCols[col]).find("input").removeAttr("disabled");

    };
    $(".num").keyup(numCheck).blur(numCheck);

    $(".cell-set").click(function () {
        var $this = $(this);
        var index = $this.closest("td").index();
        if (index < 0) {
            index = $this.closest("th").index();
            index++;
        }

        var amount = $this.closest("td,th").find("input[type='text']").val();
        $("#betTableContent tr").each(function () {
            var input = $(this).find("td").eq(index).find("input[type='text']");
            if (input.is(":disabled")) return;
            input.val(amount).trigger("blur");
        });
    });

    $("#betTable input[type='text']").not(".num").bind("keyup", calcAmount).bind("blur", calcAmount);
    $("#betTable input[type='checkbox']").bind("change", calcAmount);

    $("#chk_all_com_check").change(function () {
        var chk = $(this);
        var ischeck = isCheck(chk);
        $("input:checkbox").attr("checked", ischeck);
        calcAmount();
    });

    $(".chk_all_com").change(function () {
        var chk = $(this);
        var ischeck = isCheck(chk);
        chk.parent().find(":checkbox").attr("checked", ischeck);
        calcAmount();
    })

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
        for (var i = 1; i <= length; i++) {
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
                $(this).find(":checkbox").attr("checked", "");
        });
        calcAmount();
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

var calcAmount = function () {
    try {
        CalcBetAmount();
    }
    catch (e) {
        showMessage("Error", e);
    }
}

var getBetJson = function () {
    checkValidableFormat();
    if (!isAmountEnoughBet())
        throw new Error(resource.HaventEnoughtCredit);

    var json = getBetOrderJson();
    if (isNullOrEmpty(json))
        throw new Error(resource.PleaseCheckContent);
    return json;
}

//检查下注表单格式是否正确
var checkValidableFormat = function () {
    var nums = getNotNullNums();
    var isValidable = true;
    var errorRow;
    $(nums).each(function (i, num) {
        var rowId = num.attr("rowId");
        var wagers = $(".wager" + rowId);
        var hasWager = false;
        wagers.each(function (t, x) {
            var wg = $(x).val();
            if (!isNullOrEmpty(wg) && wg > 0) {
                hasWager = true;
                return false;
            }
        });
        if (!hasWager) {
            isValidable = false;
            errorRow = rowId;
            return false;
        }
        checkSameLenWager(num.val(), rowId);
    });
    if (!isValidable)
        throw new Error(resource.PleaseCheckRowContent.format(errorRow));
}
//检查是否拥有与该号码长度相匹配的下注额
var checkSameLenWager = function (num, row) {
    var numStr = num.toString();
    var numLen = numStr.length;

    if (/^\d{2}-\d{2}$/.exec(numStr)) numLen = 2;
    var wagers = $(".wager" + row);
    var lenWagers = new Array();
    var enableCols = cols[numLen.toString()];
    var startStr = "{0}_".format(numLen - 1);
    wagers.each(function (i, obj) {
        obj = $(obj);
        if (!isNullOrEmpty(obj.val()) && $.inArray(obj.closest("td").index(), enableCols) >= 0)
            lenWagers.push(obj);
    });
    if (lenWagers.length <= 0)
        throw new Error(resource.MustBetSpecificGameType.format(row, numLen));
}
var getNotNullNums = function () {
    allTxtBox = $("#betTable input[type='text']");        //获取所有文本框
    allChk = $("#betTable input[type='checkbox']");       //获取所有复选框
    var numTextbox = new Array();                       //存放要下注的号码
    //找到填写号码的文本框
    $(".num").each(function (i, obj) {
        obj = $(obj);
        var numValue = obj.val();
        if (!isNullOrEmpty(numValue))
            numTextbox.push(obj);
    });
    return numTextbox;
}

var checkNumFormat = function (numStr) {
    if (/^\d+$/.exec(numStr))
    { }
    else if (/^\d{2}-\d{2}$/.exec(numStr)) {    //2D连数；如11-49
    }
    else if (/^\d{3}-\d{3}$/.exec(numStr)) {    //3D
    }
    else if (/^\d{4}-\d{4}$/.exec(numStr)) {    //4D
    }
    else if (/^((\d\*)|(\*\d))$/.exec(numStr)) { }
    else if (/^\d*\*{1}\d*$/.exec(numStr)) {
    }
    else if (/^\d+\-\d*$/.exec(numStr)) { }
    else
        throw new Error(resource.PleaseInputCorrectNum);
}
//获取要下注的号码
var getBetNums = function (numStr) {
    var numArr = new Array();
    if (/^\d+$/.exec(numStr))
        numArr.push(numStr);
    else if (/^\d{2}-\d{2}$/.exec(numStr)) {    //2D连数；如11-49
        $(getNums(numStr)).each(function (i, x) { numArr.push(x); });
    }
    else if (/^\d{3}-\d{3}$/.exec(numStr)) {    //3D
        $(getNums(numStr)).each(function (i, x) { numArr.push(x); });
    }
    else if (/^\d{4}-\d{4}$/.exec(numStr)) {    //4D
        $(getNums(numStr)).each(function (i, x) { numArr.push(x); });
    }
    else if (/^((\d\*)|(\*\d))$/.exec(numStr)) {
        for (var i = 0; i < 10; i++) {
            numArr.push(numStr.replace("*", i.toString()));
        }
    }
    else if (/^\*\d{2}$/.exec(numStr)) {
        var subNum = numStr.split("*")[1];
        for (var i = 0; i < 10; i++) {
            numArr.push(i + subNum);
        }
    }
    else if (/^\d\*\d$/.exec(numStr)) {
        var subNum = numStr.split("*");
        for (var i = 0; i < 10; i++) {
            numArr.push(subNum[0] + i + subNum[1]);
        }
    }
    else if (/^\d{2}\*$/.exec(numStr)) {
        var subNum = numStr.split("*")[0];
        for (var i = 0; i < 10; i++) {
            numArr.push(i + subNum);
        }
    }
    else if (/^\d*\*{1}\d*$/.exec(numStr)) { }
    else if (/^\d+\-\d*$/.exec(numStr)) { }
    else
        throw new Error(resource.PleaseInputCorrectNum);
    return numArr;
}

//得到连数中所有的有效的数字
var getNums = function (numStr) {
    var numArr = new Array();
    var nums = numStr.split("-");
    var numA = eval(nums[0]);
    var numB = eval(nums[1]);
    var numLen = nums[1].length;
    if (numA <= numB) {
        var i;
        for (i = numA; i <= numB; i++)
            numArr.push(numToString(i, numLen));
    }
    return numArr;
}


//从所有选项中（公司）找到指定行选中的公司
var getCheckCompany = function (rowId) {
    var checkCompanyArr = new Array();
    $(".company" + rowId).each(function (i, obj) {
        obj = $(obj);
        if (isCheck(obj)) {
            var company = obj.attr("company");
            if (!isNullOrEmpty(company))
                checkCompanyArr.push(obj.attr("company"));
        }
    });
    return checkCompanyArr;
}
//获取玩法下注额
var getWagerJson = function (gameType, rowId) {
    var txtBoxArr = new Array();        //存放当前Row所有投注金额
    $(".wager" + rowId).each(function (i, obj) {
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
        //var chk = $("#" + obj.attr("id") + "_ck");
        //var isSelected = isCheck(chk);
        var sb = new Array();
        var gamePlayWayInfo = obj.attr("id").split("_");
        var isSelected = gamePlayWayInfo[3] == "1";
        var playWay = gamePlayWayInfo[1];
        var gameplaytype = window.GamePlayWays["{0}_{1}".format(gameType, playWay)];
        sb.push("{\"GamePlayTypeId\":" + gameplaytype);
        sb.push("\"IsFullPermutation\":" + isSelected);
        sb.push("\"Wager\":" + obj.val() + "}");
        wagerArr.push(sb.join());
    });
    wagerArr.push("]");
    return wagerArr.join("");
}
//从指定行中找到特定游戏玩法且选中的checkbox
var findCheckBox = function (gamePlayWay, rowId) {
    var allChk = $("#betTable input[type='checkbox']");
    var chk;
    allChk.each(function (i, obj) {
        obj = $(obj);
        if (obj.attr("gameplayway") == gamePlayWay && obj.attr("rowid") == rowId) {
            chk = obj;
            return false;
        }
    });
    return chk;
}

var getBetOrderJson = function () {
    var numTextbox = getNotNullNums();
    var betArr = new Array();
    numTextbox = $(numTextbox);
    numTextbox.each(function (i, obj) {
        var num = obj.val();
        var gameType = num.length - 1;
        var wagerStr = getWagerJson(gameType, $(obj).attr("rowid"));

        checkNumFormat(num);
        var company = getCheckCompany(obj.attr("rowid"));
        if (company.length == 0) return true;
        //var numArr = $(getBetNums(num));
        //numArr.each(function (i, number) {
        var sb = new Array();
        sb.push("{\"CompanyList\":[" + company.join() + "]");
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

//-----------------------Begin  金额计算------------------------------------------
//计算下注金额
var CalcBetAmount = function () {
    var numArr = $(getNotNullNums());   //获取不为空的号码textbox
    InitCompanyNumLen();                //初始化公司支持号码长度
    numArr.each(function (i, obj) {
        obj = $(obj);
        var rowId = obj.attr("rowid");
        var nums = getBetNums(obj.val());
        setWagerEnable(nums[0], rowId);

        var totalAmount = 0;
        var totalComm = 0;

        $(nums).each(function (t, x) {
            var rowAmount = CalcRowAmount(x, rowId);
            totalAmount += rowAmount.amount;
            totalComm += rowAmount.comm;
        });
        $("#amount" + rowId).html(totalAmount.toFixed(2));
        $("#comm" + rowId).html(totalComm.toFixed(2));
        $("#net" + rowId).html((totalAmount - totalComm).toFixed(2));
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
var enableWager = function () {
    try {
        var rowId = $(this).attr("rowid");
        var content = $(this).val();
        var nums = new Array();
        if (isNullOrEmpty(content)) nums.push("");
        else nums = getBetNums(content);
        setWagerEnable(nums[0], rowId);
    } catch (e) {
        showMessage(resource.Error, e);
    }
}
var getCommission = function (companyId, gameType) {
    $.getJSON("/Commission/GetCommission/{0}/{1}?ticker={3}".format(companyId, gameType, new Date().getTime()), function (e) {
        if (!window.Commission)
            window.Commission = {};
        window.Commission["{0}_{1}".format(companyId, gameType)] = e;
        CalcBetAmount();
    });
}
var CalcRowAmount = function (num, rowId) {
    var wagerArr = new Array();         //所有本行下注额
    $(".wager" + rowId).each(function (i, obj) {
        obj = $(obj);
        if (!isNullOrEmpty(obj.val()))
            wagerArr.push(obj);
    });
    //获取游戏类型
    var gameType = num
    .length - 1;
    var companys = getCheckCompany(rowId);
    var totalAmount = 0;
    var totalComm = 0;
    $(companys).each(function (t, x) {
        var comNumLen = findCompanyNumLen(x);
        $(wagerArr).each(function (i, obj) {
            //var chk = $("#" + obj.attr("id") + "_ck");
            //var isSelected = isCheck(chk);
            var gameplaywayInfo = obj.attr("id").split("_");
            var isSelected = gameplaywayInfo[3] == "1";
            var number = getParseNum(gameType, num);
            var numLen = getPlayWayNumLen(gameplaywayInfo[1], number, comNumLen);
            var commKey = "{0}_{1}".format(comNumLen.CompanyId, gameType);
            if (!window.Commission || !window.Commission[commKey]) {
                getCommission(comNumLen.CompanyId, gameType);
                return false; //终止循环
            }

            var amount = obj.val() * numLen * (isSelected ? countPermutation(number) : 1);
            var comm = amount * window.Commission[commKey];
            totalAmount += amount;
            totalComm += comm;
        });

    });
    return { amount: totalAmount, comm: totalComm };
}
var setWagerEnable = function (num, rowId) {
    var enableCols = cols[num.length.toString()];
    var enableInputs = new Array();
    var disableInputs = new Array();
    var wagerInputs = $(".wager" + rowId);
    var numLen = isNullOrEmpty(num) ? 0 : num.length;
    wagerInputs.each(function (i, obj) {
        if ($.inArray($(obj).closest("td").index(), enableCols) >= 0)
            enableInputs.push($(obj));
        else
            disableInputs.push($(obj));
    });
    $(enableInputs).each(function (i, obj) {
        obj.removeAttr('disabled');
    });
    $(disableInputs).each(function (i, obj) {
        obj.val("").attr("disabled", true);
    });
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
//-------------------------End  金额计算-----------------------------------------------