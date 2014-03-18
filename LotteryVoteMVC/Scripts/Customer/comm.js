//select option是否选中
var isCheck = function (chkItem) {
    var checkedVal = $(chkItem).attr("checked");
    return !(checkedVal == undefined || eval(checkedVal) == false)
}

//字符串是否空
var isNullOrEmpty = function (str) {
    return str == null || str == "" || str == undefined;
}

var getCookie = function (name)//读取cookies函数        
{
    var arr = document.cookie.match(new RegExp("(^| )" + name + "=([^;]*)(;|$)"));
    if (arr != null) return unescape(arr[2]); return null;

}
var setCookie = function (name, value, expiredays) {
    var exdate = new Date();
    exdate.setDate(exdate.getDate() + expiredays);
    // 使设置的有效时间正确。增加toGMTString()
    document.cookie = name + "=" + escape(value) + ((expiredays == null) ? "" : ";expires=" + exdate.toGMTString());
}
//计算出一个数的全排列可能
var countPermutation = function (num) {
    var rptNumDic = new Array();
    var number = num.toString();
    for (var i = 0; i < number.length; i++) {
        var ch = number.substring(i, i + 1);
        if (isNullOrEmpty(rptNumDic[ch]))
            rptNumDic[ch] = 1;
        else
            rptNumDic[ch] = rptNumDic[ch] + 1;
    }
    var totalFac = factorial(number.length);
    $(rptNumDic).each(function (i, obj) {
        if (isNullOrEmpty(obj) || obj <= 1) return true;
        totalFac /= factorial(obj);
    });
    return totalFac;
}
//计算阶乘.
var factorial = function (num) {
    if (num == 1) return num;
    return num * factorial(--num);
}
//将数字转化成字符串
var numToString = function (num, len) {
    var numStr = num.toString();
    var zeroArr = new Array();
    for (var i = numStr.length; i < len; i++)
        zeroArr.push("0");
    zeroArr.push(numStr);
    return zeroArr.join("");
}
//初始化公司号码长度信息
var InitCompanyNumLen = function () {
    CompanyNumLen = eval($.base64.decode(getCookie("CompanyNumLen")));
}
//字符串是否以指定字符串开头
//source:原字符串
//partten:要包含的字符串
var isStartWith = function (source, partten) {
    if (source.length < partten.length) return false;
    var len = partten.length;
    var subStr = source.subString(0, len);
    return subStr == partten;
}
//获取QueryString
var getQuery = function (name) {
    var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)", "i");
    var r = window.location.search.substr(1).match(reg);
    if (r != null)
        return unescape(r[2]);
    return null;
}
var reloadPage = function () {
    location.reload();
}
//调用JQuery Dialog
var showMessage = function (title, content) {
    var htmlArr = new Array();
    htmlArr.push("<div title=\"" + title + "\">");
    htmlArr.push("<p>" + content + "</p>");
    htmlArr.push("</div>");
    var msgBox = $(htmlArr.join(""));
    msgBox.appendTo($(document));
    msgBox.dialog({ modal: true, buttons: { "OK": function () {
        $(this).dialog("close");
        //if (isIe6Or7()) reloadPage();
    }
    }
    });
}
var isIe6Or7 = function () {
    if ($.browser.msie) {
        if ($.browser.version == "6.0" || $.browser.version == "7.0") return true;
    }
    return false;
}
//更新显示语言
var setLanguage = function (language) {
    $.cookie("Language", language, { expires: 31, path: '/' });
}
//获取当前显示语言
var getCurrentLanguage = function () {
    return $.cookie("Language");
}
//集合对象中是否包含指定项
var containsItem = function (source, item) {
    var isContains = false;
    $(source).each(function (i, obj) {
        if (obj == item) {
            isContains = true;
            return false;
        }
    });
    return isContains;
}
//string.format
//用法："{0} is dead, but {1} is alive! {0} {2}".format("ASP", "ASP.NET")
String.prototype.format = function () {
    var args = arguments;
    return this.replace(/{(\d+)}/g, function (match, number) {
        return typeof args[number] != 'undefined'
      ? args[number]
      : match
    ;
    });
};
//startsWith
String.prototype.startsWith = function (str) {
    return this.indexOf(str) == 0;
};
//endsWith
String.prototype.endsWith = function (str) {
    return this.slice(-str.length) == str;
};
/* 只能输入数字
例如：$(input[type='text']).numeral();
*/
$.fn.numeral = function (isSupportZero) {
    $(this).css("ime-mode", "disabled");
    this.bind("keypress", function () {
        if (event.keyCode == 46) {
            if (this.value.indexOf(".") != -1) {
                return false;
            }
        } else {
            return event.keyCode >= 46 && event.keyCode <= 57;
        }
    });
    this.bind("blur", function () {
        if (this.value.lastIndexOf(".") == (this.value.length - 1)) {
            this.value = this.value.substr(0, this.value.length - 1);
        } else if (isNaN(this.value)) {
            this.value = "";
        }
    });
    this.bind("paste", function () {
        var s = clipboardData.getData('text');
        if (!/\D/.test(s));
        if (!isSupportZero)
            value = s.replace(/^0*/, '');
        return false;
    });
    this.bind("dragenter", function () {
        return false;
    });
    this.bind("keyup", function () {
        if (/(^0+)/.test(this.value)) {
            if (!isSupportZero)
                this.value = this.value.replace(/^0*/, '');
        }
    });
};

//增加回发参数
var addAction = function (action/*参数名*/, value/*参数值*/, clear/*是否清空参数*/) {
    //获取当前页Url
    var formUrl = $("form")[0].action;
    formUrl = addActionToUrl(formUrl, action, value, clear);
    $("form")[0].action = formUrl;
}

var addActionToUrl = function (formUrl, action/*参数名*/, value/*参数值*/, clear/*是否清空参数*/) {
    //如果Url中含有参数
    if (formUrl.indexOf("?") > 0) {
        //清空参数
        if (clear)
            formUrl = formUrl.substr(0, formUrl.indexOf("?") + 1);
        //Url中含有要添加的参数对
        if (formUrl.indexOf(action) > 0) {
            //获取所有参数对值
            var queryString = formUrl.substr(formUrl.indexOf("?") + 1, formUrl.length);
            //分组
            var queryStringArr = queryString.split("&");
            //不包含参数的Url
            formUrl = formUrl.substr(0, formUrl.indexOf("?") + 1);
            var isFirst = true;
            //遍历参数对
            for (var i = 0; i < queryStringArr.length; i++) {
                //如果参数对不等于要添加的Action.则添加到Url中
                if (queryStringArr[i].indexOf(action) < 0) {
                    if (isFirst) isFirst = false;
                    else formUrl += "&";
                    formUrl += queryStringArr[i];
                }
            }
        }
        //避免只有一个参数,并且参数相同时出现的?&情况
        if (formUrl.indexOf("?") + 1 != formUrl.length)
            formUrl += "&";
    }
    else {
        formUrl += "?";
    }

    formUrl += action + "=" + value;
    return formUrl;
}
var visit = function (url, params, clear) {
    if (!isNullOrEmpty(params)) {
        var paramArr = params.split('&');
        var isFirst = true;
        $(paramArr).each(function (i, param) {
            if (isFirst) isFirst = false;
            else {
                if (clear) clear = false;
            }
            var paramInfo = param.split('=');
            url = addActionToUrl(url, paramInfo[0], paramInfo[1], clear);
        });
    }
    window.location.href = url;
}
//本页回发
var postBack = function (action/*参数名*/, value/*参数值*/, clear/*是否清空参数*/) {
    if (action != null && value != null && value != undefined && value != "undefined")
        addAction(action, value, clear);
    var formUrl = $("form")[0].action;
    window.location.href = formUrl;
}

var buildFrame = function (frameId, height, width, src, title, onClosed, modal, draggable) {
    title = isNullOrEmpty(title) ? "" : title;
    var oldVersion = $("#" + frameId);
    if (oldVersion.length)
        oldVersion.remove();
    var div = $("<div title=\"" + title + "\" id='win" + frameId + "'></div>");
    var iframe = $("<iframe id=\"" + frameId + "\" Width=\"" + width + "px\" height=\"" + height + "px\" frameborder=\"0\" src=\"" + src + "\" </iframe>").appendTo(div);
    div.appendTo($(document));
    var height = $("#frame").attr("height");
    if (onClosed == null || onClosed == undefined || onClosed == "")
        onClosed = function (event, ui) {
            $("#" + frameId).dialog("destroy");
        };
    if (modal == null || modal == undefined)
        modal = true;
    if (draggable == null || draggable == undefined)
        draggable = false
    div.dialog({ modal: modal, minWidth: width, minHeight: height, close: onClosed, draggable: draggable });
    $("#" + frameId).bind("load", function () {
        $("#" + frameId).contents().find("body").css("width", "{0}px".format(width - 30));
    });
}



var calcTime = function (offset) {
    var d = new Date();
    var utc = d.getTime() + (d.getTimezoneOffset() * 60000);
    var nd = new Date(utc + (3600000 * offset));
    return nd;
}
var parseDateFormat = function (date) {
    return date.getFullYear() + "-" + (date.getMonth() + 1) + "-" + date.getDate();
}
var parseTimeFormat = function (date) {
    return numToString(date.getHours(), 2) + ":" + numToString(date.getMinutes(), 2) + ":" + numToString(date.getSeconds(), 2);
}
var getDateDayOfWeek = function (date) {
    var dayOfWeek = {
        0: resource.Sunday,
        1: resource.Monday,
        2: resource.Tuesday,
        3: resource.Wednesday,
        4: resource.Thursday,
        5: resource.Friday,
        6: resource.Saturday
    };
    return dayOfWeek[date.getDay()];
}
//开启加载中遮罩层
var openLodingMask = function () {
    var mask = $("#loadingmask");
    if (!mask.length) {
        mask = $("<div id='loadingmask' class='alpha'></div>");
        mask.css("height", "100%").css("width", "100%").css("z-index", "999")
                .css("position", "fixed").css("position", "_position").css("top", "0px").css("left", "0px")
                .css("background", "#ccc").css("text-align", "center");
        var loadingImg = $("<img src='/Content/Images/ajax-loader.gif' />")
                .css("position", "relative").css("top", "48%");
        loadingImg.appendTo(mask);
        $("body").append(mask);
    }
    mask.show();
}
//关闭加载中遮罩层
var closeLoadingMask = function () {
    var mask = $("#loadingmask");
    if (mask.length)
        mask.hide();
}