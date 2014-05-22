$(document).ready(function () {
    //var date = calcTime(7);
    var date = new Date(Date.parse($.trim($("#currentTime").html()).replace(/-/g, "/")));
    $("#dateTime").html(parseDateFormat(date));
    $("#currentTime").html(parseTimeFormat(date));
    $("#dayOfWeek").html(getDateDayOfWeek(date));
    SetNowTime();
})

function SetNowTime() {
    //var date = calcTime(7);
    var dateStr = $.trim($("#dateTime").html()) + "   " + $.trim($("#currentTime").html());
    var date = new Date(Date.parse(dateStr.replace(/-/g, "/")));
    date.setSeconds(date.getSeconds() + 1);
    $("#currentTime").html(parseTimeFormat(date));
    setTimeout(SetNowTime, 1000);
}
//检查金额是否足够下注单
var isAmountEnoughBet = function () {
    var betCredit = Number($.trim($("#betCredit").text()).replace(/,/g, ""));
    var totalAmount = 0;
    $(".turnover").each(function (i, obj) {
        var val = $.trim($(obj).text());
        if (!isNaN(val))
            totalAmount += Number(val);
    });
    totalAmount = Number(totalAmount);
    return totalAmount <= betCredit;
}
//Ajax 提交表单
var onError = null;
var onSuccess = function (data) {
    var result = eval(data);
    if (result.IsSuccess) {
        updateAmountBoard();
        var model = result.Model;
        var msgArr = new Array();
        msgArr.push("<strong>{0}:<font color='blue'>{2}</font>,{1}:<font color='red'>{3}</font></strong>".format(resource.Bet, resource.Accept, model.ExceptTurnover, model.ActualTurnover));
        if (model.Drops.length > 0) {
            msgArr.push("<strong>{0}</strong>:".format(resource.DropWater));
            $(model.Drops).each(function (i, x) {
                msgArr.push("{0} {3} {4}(<font color='red'>-{1}</font>)<font color='blue'>{2}</font>".format(x.Num, x.Drop, x.Net, x.Company, x.GameType));
            });
        }
        if (model.NotAccept.length > 0) {
            msgArr.push("<strong style='color:red'>{0}</strong>:".format(resource.NotAccept));
            $(model.NotAccept).each(function (i, x) {
                msgArr.push("<font color='red'>{0}({1} {2})</font>".format(x.Num, x.Company, x.GameType));
            });
        }
        showMessage(resource.Success, msgArr.join("<br/>"));
        $("input[type='reset']").trigger("click");
        try {
            $(".num").trigger("keyup");
        }
        catch (err) { }
    }
    else {
        showMessage("Error", data.Message);
    }
}
//更新金额面板
var updateAmountBoard = function () {
    $.getJSON("/Bet/AmountBoard?{0}".format(new Date().getTime()), function (data) {
        $("#betCredit").html(data.Model.BetCredit.toString());
        $("#outstanding").html(data.Model.Outstanding.toString());
    });
}
var onComplete = closeLoadingMask;      //操作完成时发生，需要使覆盖即可
var beforePost = openLodingMask;        //表单提交前操作，需要时覆盖即可
var getBetJson = null;                  //提交的Json数据获取方法，必须复写
$(function () {
    $('form').submit(function () {
        try {
            var json = getBetJson();
            $.ajax({
                url: this.action,
                type: this.method,
                data: json,
                dataType: "json",
                success: onSuccess,
                error: onError,
                complete: onComplete
            });
            beforePost();
        }
        catch (e) {
            showMessage("Error", e.message);
        }
        return false;
    });
});

var bindClassAmountSilder = function (className) {
    $("." + className).bind("focus", showBetBox);
}
var bindAmountSlider = function (notClass) {
    $("input[type='text']:not(." + notClass + ")").bind("focus", showBetBox);
}
var showBetBox = function (obj) {
    var offset = $(this).offset();
    var betBox = getBetBox();
    betBox.attr("target", $(this).attr("id"));
    betBox.css("top", offset.top - ($.browser.msie ? 120 : 140)).css("left", offset.left - 30).css("z-index", "999").fadeIn("fast");
}
var getBetBox = function () {
    var betBox = $("#betbox");
    if (betBox == null || betBox.length == 0) {
        betBox = createBetBox();
    }
    return betBox;
}
var createBetBox = function () {
    var arrowBox = $("<div class='arrow_box' id='betbox'></div>").css("position", "absolute").css("width", "130px");
    var table = $('<table cellpadding="0" cellspacing="0"></table>');
    for (var i = 0; i < 4; i++) {
        table.append($("<tr><td></td><td></td></tr>"));
    }
    var tdList = table.find("td");
    var wagers = "10,50,100,500,1000,2000,5000".split(",");
    tdList.each(function (i, td) {
        if (i == 0) {
            var clearButton = $("<input type='button' value='" + resource.Clear + "' />").bind("click", function () {
                var target = $(this).parents("div").attr("target");
                $("#" + target).val("0");
                calcAmount();
            });
            $(td).append($(clearButton));
        }
        else {
            var button = $("<input type='button' value='+" + wagers[i - 1] + "' />").bind("click", function () {
                var target = $(this).parents("div").attr("target");
                var txtbox = $("#" + target);
                var oldAmount = isNullOrEmpty(txtbox.val()) ? "0" : txtbox.val();
                var amount = eval(oldAmount) + eval(wagers[i - 1]);
                txtbox.val(amount);
                calcAmount();
            });
            $(td).append(button);
        }
    });
    table.appendTo(arrowBox);
    arrowBox.appendTo($("body"));
    arrowBox.hide();
    $(document).mousedown(hideBetBox).focus(hideBetBox);
    return arrowBox;
}
var hideBetBox = function (e) {
    var clickEle = $(e.srcElement || e.target);
    var canHide = true; //代表是否能隐藏betBoard
    if (clickEle != null && clickEle.length > 0) {
        var eleEvents = clickEle.data("events");
        if (eleEvents) {
            var checkEvents = ["focus", "click", "blur"];
            for (var i = 0; i < checkEvents.length; i++) {
                if (!canHide) break;
                var handlers = eleEvents[checkEvents[i]];
                if (handlers) {
                    for (var j = 0; j < handlers.length; j++) {
                        if (handlers[j].handler == showBetBox) {
                            canHide = false;
                            break;
                        }
                    }
                }
            }
        }

        if (clickEle.attr("id") == $("#betbox").attr("target") || clickEle.attr("id") == "betbox" || clickEle.parents("#betbox").length > 0)
            canHide = false;
    }
    if (canHide)
        $("#betbox").hide();
}

$(document).ready(function () {
    $("input[type='text']").keydown(function () {
        switch (event.keyCode) {
            case 38: moveUp(this); break;
            case 40: moveDown(this); break;
            case 37: moveLeft(this); break;
            case 39: moveRight(this); break;
            default: return;
        }
    });
});
var moveUp = function (obj) {
    obj = $(obj);
    var index = getTextBoxIndex(obj);
    var tr = obj.parents("tr").prev("tr");
    var textbox = tr.find(":text").eq(index);
    if (textbox.length) {
        if (textbox.attr("disabled"))
            textbox = getLastAbleTextBox(tr);
        textbox.focus();
    }
}
var moveDown = function (obj) {
    obj = $(obj);
    var index = getTextBoxIndex(obj);
    var tr = obj.parents("tr").next("tr");
    var textbox = tr.find(":text").eq(index);
    if (textbox.length) {
        if (textbox.attr("disabled"))
            textbox = getLastAbleTextBox(tr);
        textbox.focus();
    }
}
var moveLeft = function (obj) {
    obj = $(obj);
    var textbox = obj.parent().prev().find(":text");
    if (textbox.length) {
        if (textbox.attr("disabled"))
            return;
        textbox.focus();
    }
}
var moveRight = function (obj) {
    obj = $(obj);
    var textbox = obj.parent().next().find(":text");
    if (textbox.length) {
        if (textbox.attr("disabled"))
            return;
        textbox.focus();
    }
}
var getLastAbleTextBox = function (tr) {
    var ableTB = tr.find(":text:not(:disabled):last");
    return ableTB;
}
var getTextBoxIndex = function (obj) {
    obj = $(obj);
    var originalId = obj.attr("id");
    var tr = obj.parents("tr");
    var textboxs = tr.find(":text");
    var index = 0;
    textboxs.each(function (i, x) {
        if ($(x).attr("id") == originalId) {
            index = i;
            return false;
        }
    });
    return index;
}

/* 只能输入数字
例如：$(input[type='text']).betNumeral();
*/
$.fn.betNumeral = function () {
    $(this).css("ime-mode", "disabled");
    this.bind("keypress", function () {
        if (event.keyCode == 46) {
            if (this.value.indexOf(".") != -1) {
                return false;
            }
        } else {
            return (event.keyCode >= 48 && event.keyCode <= 57) || event.keyCode == 42 || event.keyCode == 45;
        }
    });
    this.bind("blur", function () {
        if (this.value.lastIndexOf(".") == (this.value.length - 1)) {
            this.value = this.value.substr(0, this.value.length - 1);
        } else if (isNaN(this.value) && !(/^\d*\*{1}\d*$/.exec(this.value)) && !(/^\d+\-\d*$/.exec(this.value))) {
            this.value = "";
        }
    });
    this.bind("paste", function () {
        var s = clipboardData.getData('text');
        if (!/\D/.test(s))
            return false;
    });
    this.bind("dragenter", function () {
        return false;
    });
};

$.fn.betAmount = function () {
    $(this).css("ime-mode", "disabled");
    this.bind("keypress", function () {
        if (this.value.lastIndexOf(".") > 0 && this.value.length - this.value.lastIndexOf(".") > 2)
            return this.value.length - 1 - this.value.lastIndexOf(".") < 2;
        return true;
    });
    this.bind("blur", function () {
        if (this.value.lastIndexOf(".") == (this.value.length - 1)) {
            this.value = this.value.substr(0, this.value.length - 1);
        }
        else if (this.value.lastIndexOf(".") > 0 && this.value.length - 1 - this.value.lastIndexOf(".") > 2) {
            this.value = this.value.substr(0, this.value.lastIndexOf(".") + 3);
        } else if (isNaN(this.value) && !(/^\d*\*{1}\d*$/.exec(this.value)) && !(/^\d+\-\d*$/.exec(this.value))) {
            this.value = "";
        }
    });
    this.bind("paste", function () {
        var s = clipboardData.getData('text');
        if (!/\D/.test(s))
            return false;
    });
    this.bind("dragenter", function () {
        return false;
    });
};