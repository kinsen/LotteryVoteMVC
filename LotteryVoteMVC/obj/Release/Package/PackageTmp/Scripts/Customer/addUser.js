var onSuccess = function (result) {
    result = eval(result);
    if (result.IsSuccess) {
        showMessage("Success", result.Message);
        appendUser(result.Model);
    }
    else {
        showMessage("Error", result.Message);
    }
}
var appendUser = function (model) {
    var tbody = $(window.parent.document).find("table tbody");
    if (!tbody.length) return;

    var tr = $("<tr></tr>");
    tr.append($("<td></td>"));
    var name = $("<td>{0}</td>".format(model.Name)).appendTo(tr);
    var username = $("<td>{0}</td>".format(model.UserName)).appendTo(tr);
    var currency = $("<td>VND</td>").appendTo(tr);
    var gredit = $("<td>{0}</td>".format(model.GivenCredit)).appendTo(tr);
    var avalidableCredit = $("<td>{0}</td>".format(model.GivenCredit)).appendTo(tr);
    var sharerate = $("<td>{0}</td>".format(model.ShareRate)).appendTo(tr);
    tr.append($("<td></td>"));
    tr.append($("<td></td>"));
    var state = $("<td>{0}</td>".format(model.State)).appendTo(tr);
    tr.append($("<td></td>"));
    tbody.append(tr);
}